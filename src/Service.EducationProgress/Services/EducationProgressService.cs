using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.Core.Client.Extensions;
using Service.Core.Client.Models;
using Service.Education.Constants;
using Service.Education.Extensions;
using Service.Education.Structure;
using Service.EducationProgress.Domain.Models;
using Service.EducationProgress.Grpc;
using Service.EducationProgress.Grpc.Models;
using Service.EducationProgress.Mappers;
using Service.ServiceBus.Models;

namespace Service.EducationProgress.Services
{
	public class EducationProgressService : IEducationProgressService
	{
		private readonly ILogger<EducationProgressService> _logger;
		private readonly IServiceBusPublisher<SetProgressInfoServiceBusModel> _publisher;
		private readonly IDtoRepository _dtoRepository;

		public EducationProgressService(ILogger<EducationProgressService> logger,
			IServiceBusPublisher<SetProgressInfoServiceBusModel> publisher,
			IDtoRepository dtoRepository)
		{
			_logger = logger;
			_publisher = publisher;
			_dtoRepository = dtoRepository;
		}

		public async ValueTask<EducationProgressGrpcResponse> GetProgressAsync(GetEducationProgressGrpcRequest request)
		{
			var result = new EducationProgressGrpcResponse();
			string userId = request.UserId;

			EducationProgressDto[] progressDtos = await GetProgressDtos(userId, request.Tutorial, request.Unit);
			if (progressDtos.IsNullOrEmpty())
				return result;

			result.TestScore = progressDtos.CountTestScore();
			result.TaskScore = progressDtos.CountTaskScore();
			result.TasksPassed = progressDtos.Count(dto => dto.GetValue().IsOkProgress());
			result.TutorialsPassed = progressDtos
				.GroupBy(dto => dto.Tutorial, dto => dto)
				.Count(value => value.IsPassed());

			return result;
		}

		public async ValueTask<TaskEducationProgressGrpcResponse> GetTaskProgressAsync(GetTaskEducationProgressGrpcRequest request)
		{
			var result = new TaskEducationProgressGrpcResponse();
			string userId = request.UserId;

			EducationProgressDto[] items = await GetProgressDtos(userId, request.Tutorial, request.Unit, request.Task);
			EducationProgressDto progress = items.FirstOrDefault();
			result.Progress = new TaskEducationProgressGrpcModel(progress);

			return result;
		}

		public async ValueTask<UnitEducationProgressGrpcResponse> GetUnitProgressAsync(GetUnitEducationProgressGrpcRequest request)
		{
			var result = new UnitEducationProgressGrpcResponse {Unit = request.Unit};

			EducationProgressDto[] items = await GetProgressDtos(request.UserId, request.Tutorial, request.Unit);
			result.Progress = items.Select(dto => new TaskEducationProgressGrpcModel(dto)).ToArray();

			return result;
		}

		public async ValueTask<TutorialEducationProgressGrpcResponse> GetTutorialProgressAsync(GetTutorialEducationProgressGrpcRequest request)
		{
			string userId = request.UserId;
			EducationTutorial tutorial = request.Tutorial;

			var result = new TutorialEducationProgressGrpcResponse
			{
				Tutorial = tutorial,
				Units = new List<ShortUnitEducationProgressGrpcResponse>(5)
			};

			EducationProgressDto[] items = await GetProgressDtos(userId, tutorial);

			IGrouping<int, EducationProgressDto>[] groupings = items
				.GroupBy(dto => dto.Unit, dto => dto)
				.OrderBy(dto => dto.Key)
				.ToArray();

			//By units
			foreach (IGrouping<int, EducationProgressDto> unitDtos in groupings)
			{
				var unitItem = new ShortUnitEducationProgressGrpcResponse
				{
					Unit = unitDtos.Key,
					Tasks = new List<ShortTaskEducationProgressGrpcModel>(6)
				};

				//By tasks
				foreach (EducationProgressDto dto in unitDtos)
				{
					unitItem.Tasks.Add(new ShortTaskEducationProgressGrpcModel
					{
						Task = dto.Task,
						Value = dto.GetValue(),
						HasProgress = dto.HasProgress,
						Date = dto.Date
					});
				}

				unitItem.HasProgress = unitItem.Tasks.Any(model => model.HasProgress);
				unitItem.Finished = unitItem.Tasks.ForAll(model => model.Value.IsOkProgress());
				unitItem.TaskScore = unitDtos.ToArray().CountTaskScore();

				result.Units.Add(unitItem);
			}

			result.Finished = result.Units.ForAll(model => model.Finished);
			result.TaskScore = (int) Math.Round(result.Units.Average(unit => unit.TaskScore));

			return result;
		}

		public async ValueTask<EducationStateProgressGrpcResponse> GetEducationStateProgressAsync(GetEducationStateProgressGrpcRequest request)
		{
			string userId = request.UserId;

			var result = new EducationStateProgressGrpcResponse();

			EducationProgressDto[] items = await _dtoRepository.GetEducationProgress(userId);
			if (items.IsNullOrEmpty())
			{
				_logger.LogError("No education progress record where found in ServerKeyValue storage for user: {userId}", userId);
				return result;
			}

			result.Tutorials = items.GroupBy(dto => dto.Tutorial, dto => dto)
				.Select(dtos => new EducationStateTutorialGrpcModel
				{
					Tutorial = dtos.Key,
					Started = dtos.HasProgress() || dtos.Key == EducationTutorial.PersonalFinance,
					Finished = dtos.IsPassed()
				})
				.OrderBy(arg => arg.Tutorial)
				.ToArray();

			return result;
		}

		public async ValueTask<CommonGrpcResponse> SetProgressAsync(SetEducationProgressGrpcRequest request)
		{
			string userId = request.UserId;
			int taskScore = request.Value;

			if (taskScore > Progress.MaxProgress || taskScore < 0)
				return GetFailResponse($"Error while set education progress for user: {userId}, progress value is not valid: {taskScore}.");

			EducationProgressDto[] progressDtos = await _dtoRepository.GetEducationProgress(userId);
			if (progressDtos.IsNullOrEmpty())
				return GetFailResponse($"No education progress record where found in ServerKeyValue storage for user: {userId}");

			EducationProgressDto task = progressDtos
				.Where(dto => dto.Tutorial == request.Tutorial)
				.Where(dto => dto.Unit == request.Unit)
				.FirstOrDefault(dto => dto.Task == request.Task);

			if (task == null)
				return GetFailResponse($"Error while set education progress for user: {userId}, progress for task not exists.");

			int? oldScore = task.Value.GetValueOrDefault();
			if (request.IsRetry)
				task.Retries = task.Retries.GetValueOrDefault() + 1;

			task.Value = taskScore;
			task.Date = DateTime.UtcNow;

			CommonGrpcResponse commonGrpcResponse = await _dtoRepository.SetEducationProgress(request.UserId, progressDtos);
			if (commonGrpcResponse.IsSuccess)
			{
				SaveTestTasks100Prc(task, request.UserId, request.IsRetry, taskScore);

				//Обновить прогресс пользователя
				//Только при успешном прохождении таска
				//Исключаем повторное обновление с попытки
				//Либо включаем попытку, если до этого результат был не успешный
				bool newRequestHasResult = taskScore >= Progress.OkProgress && (!request.IsRetry || oldScore < Progress.OkProgress);
				await _publisher.PublishAsync(request.ToBusModel(newRequestHasResult, taskScore));
			}

			return commonGrpcResponse;
		}

		private async void SaveTestTasks100Prc(EducationProgressDto task, string userId, bool isRetry, int taskScore)
		{
			if (task.GetTaskType() != EducationTaskType.Test)
				return;

			TestTasks100PrcDto prcDto = await _dtoRepository.GetTestTasks100Prc(userId);

			int newCount = isRetry || !taskScore.IsOkProgress() ? 0 : prcDto.Count + 1;
			if (prcDto.Count == newCount)
				return;

			prcDto.Count = newCount;

			CommonGrpcResponse result = await _dtoRepository.SetTestTasks100Prc(userId, prcDto);

			if (!result.IsSuccess)
				_logger.LogError("Error while set TestTasks100Prc value for user: {userId}, dto: {@dto}.", userId, prcDto);
		}

		public async ValueTask<CommonGrpcResponse> InitProgressAsync(InitEducationProgressGrpcRequest request)
		{
			string userId = request.UserId;
			EducationProgressDto[] items;

			if (request.Tutorial == null)
				items = DtoRepository.GetEmptyProgress();
			else
			{
				items = await GetProgressDtos(userId);

				EducationProgressDto[] itemsToInit = items
					.Where(dto => dto.Tutorial == request.Tutorial)
					.WhereIf(request.Unit != null, dto => dto.Unit == request.Unit)
					.WhereIf(request.Task != null, dto => dto.Task == request.Task)
					.ToArray();

				foreach (EducationProgressDto dto in itemsToInit)
					dto.Clear();
			}

			return await _dtoRepository.SetEducationProgress(userId, items);
		}

		private async ValueTask<EducationProgressDto[]> GetProgressDtos(string userId, EducationTutorial? tutorial = null, int? unit = null, int? task = null)
		{
			EducationProgressDto[] items = await _dtoRepository.GetEducationProgress(userId);

			if (items.IsNullOrEmpty())
			{
				_logger.LogError("Error while get education progress for user: {userId}, no progress found.", userId);

				return Array.Empty<EducationProgressDto>();
			}

			return items
				.WhereIf(tutorial != null, dto => dto.Tutorial == tutorial)
				.WhereIf(unit != null, dto => dto.Unit == unit)
				.WhereIf(task != null, dto => dto.Task == task)
				.ToArray();
		}

		private CommonGrpcResponse GetFailResponse(string message)
		{
			_logger.LogError(message);

			return CommonGrpcResponse.Fail;
		}

		public async ValueTask<TaskTypeProgressGrpcResponse> GetTaskTypeProgressAsync(GetTaskTypeProgressGrpcRequest request)
		{
			string userId = request.UserId;
			int? unit = request.Unit;
			var result = new TaskTypeProgressGrpcResponse();

			EducationProgressDto[] tasks = await GetProgressDtos(userId, request.Tutorial, unit);

			result.TaskTypeProgress = tasks
				.GroupBy(dto => dto.GetTaskType(), dto => dto.GetValue())
				.Select(dto => new TaskTypeProgressGrpcModel
				{
					TaskType = dto.Key,
					Values = dto.ToArray()
				}).ToArray();

			return result;
		}

		public async ValueTask<ProgressDataGrpcResponse> GetProgressData(GetProgressDataGrpcRequest request)
		{
			EducationProgressDto[] items = await GetProgressDtos(request.UserId, request.Tutorial, request.Unit, request.Task);

			return new ProgressDataGrpcResponse
			{
				Items = items.Select(dto => dto.ToGrpcModel()).ToArray()
			};
		}

		public async ValueTask<CommonGrpcResponse> ChangeTaskDate(ChangeTaskDateGrpcRequest request)
		{
			string userId = request.UserId;

			EducationProgressDto[] progressDtos = await _dtoRepository.GetEducationProgress(userId);
			if (progressDtos.IsNullOrEmpty())
				return GetFailResponse($"No education progress record where found in ServerKeyValue storage for user: {userId}");

			EducationProgressDto task = progressDtos
				.Where(dto => dto.Tutorial == request.Tutorial)
				.Where(dto => dto.Unit == request.Unit)
				.FirstOrDefault(dto => dto.Task == request.Task);

			if (task == null)
				return GetFailResponse($"Error while set education progress for user: {userId}, progress for task not exists.");

			task.Date = request.Date;

			return await _dtoRepository.SetEducationProgress(request.UserId, progressDtos);
		}
	}
}