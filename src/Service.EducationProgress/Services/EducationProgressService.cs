﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.ServiceBus;
using Service.Core.Client.Extensions;
using Service.Core.Client.Models;
using Service.Education.Constants;
using Service.Education.Extensions;
using Service.Education.Helpers;
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

		public EducationProgressService(ILogger<EducationProgressService> logger, IServiceBusPublisher<SetProgressInfoServiceBusModel> publisher, IDtoRepository dtoRepository)
		{
			_logger = logger;
			_publisher = publisher;
			_dtoRepository = dtoRepository;
		}

		public async ValueTask<EducationProgressGrpcResponse> GetProgressAsync(GetEducationProgressGrpcRequest request)
		{
			var result = new EducationProgressGrpcResponse();
			Guid? userId = request.UserId;

			EducationProgressDto[] items = await _dtoRepository.GetEducationProgress(userId);
			if (items.IsNullOrEmpty())
			{
				_logger.LogError("No education progress record where found in ServerKeyValue storage for user: {userId}", userId);
				return result;
			}

			EducationProgressDto[] dtos = FilterProgressData(items, request.Tutorial, request.Unit);
			if (dtos.IsNullOrEmpty())
			{
				_logger.LogError("Error while get education progress for user: {userId}, no progress found.", userId);
				return result;
			}

			result.Value = (int) Math.Round(dtos
				.Select(val => val.Value.GetValueOrDefault())
				.ToArray().Average());

			return result;
		}

		private static EducationProgressDto[] FilterProgressData(IEnumerable<EducationProgressDto> items, EducationTutorial? tutorial, int? unit, int? task = null) => items
			.WhereIf(tutorial != null, val => val.Tutorial == tutorial)
			.WhereIf(unit != null, val => val.Unit == unit)
			.WhereIf(task != null, val => val.Task == task)
			.ToArray();

		public async ValueTask<TaskEducationProgressGrpcResponse> GetTaskProgressAsync(GetTaskEducationProgressGrpcRequest request)
		{
			var result = new TaskEducationProgressGrpcResponse();
			Guid? userId = request.UserId;

			EducationProgressDto[] items = await _dtoRepository.GetEducationProgress(userId);
			if (items.IsNullOrEmpty())
			{
				_logger.LogError("No education progress record where found in ServerKeyValue storage for user: {userId}", userId);
				return result;
			}

			EducationProgressDto progress = FilterProgressData(items, request.Tutorial, request.Unit, request.Task).FirstOrDefault();

			result.Progress = new TaskEducationProgressGrpcModel(progress);

			return result;
		}

		public async ValueTask<UnitEducationProgressGrpcResponse> GetUnitProgressAsync(GetUnitEducationProgressGrpcRequest request)
		{
			var result = new UnitEducationProgressGrpcResponse();
			Guid? userId = request.UserId;

			EducationProgressDto[] items = await _dtoRepository.GetEducationProgress(userId);
			if (items.IsNullOrEmpty())
			{
				_logger.LogError("No education progress record where found in ServerKeyValue storage for user: {userId}", userId);
				return result;
			}

			result.Progress = items
				.Where(dto => dto.Tutorial == request.Tutorial)
				.Where(dto => dto.Unit == request.Unit)
				.Select(dto => new TaskEducationProgressGrpcModel(dto)).ToArray();

			return result;
		}

		public async ValueTask<TutorialEducationProgressGrpcResponse> GetTutorialProgressAsync(GetTutorialEducationProgressGrpcRequest request)
		{
			Guid? userId = request.UserId;
			EducationTutorial tutorial = request.Tutorial;

			var result = new TutorialEducationProgressGrpcResponse
			{
				Tutorial = tutorial,
				Progress = new List<ShortUnitEducationProgressGrpcResponse>(5)
			};

			EducationProgressDto[] items = await _dtoRepository.GetEducationProgress(userId);
			if (items.IsNullOrEmpty())
			{
				_logger.LogError("No education progress record where found in ServerKeyValue storage for user: {userId}", userId);
				return result;
			}

			IGrouping<int, EducationProgressDto>[] groupings = items
				.Where(dto => dto.Tutorial == tutorial)
				.GroupBy(dto => dto.Unit, dto => dto)
				.OrderBy(dto => dto.Key)
				.ToArray();

			//By units
			foreach (IGrouping<int, EducationProgressDto> unitDtos in groupings)
			{
				var item = new ShortUnitEducationProgressGrpcResponse
				{
					Unit = unitDtos.Key,
					Progress = new List<ShortTaskEducationProgressGrpcModel>(6)
				};

				//By tasks
				foreach (EducationProgressDto dto in unitDtos)
				{
					item.Progress.Add(new ShortTaskEducationProgressGrpcModel
					{
						Task = dto.Task,
						TaskScore = dto.Value.GetValueOrDefault(),
						HasProgress = dto.Value != null
					});
				}

				item.HasProgress = item.Progress.Any(model => model.HasProgress);
				item.Finished = item.Progress.All(model => model.HasProgress);
				item.TaskScore = (int) Math.Round((double) (item.Progress.Sum(model => model.TaskScore) / item.Progress.Count));

				result.Progress.Add(item);
			}

			result.Finished = result.Progress.All(model => model.Finished);
			result.TaskScore = (int)Math.Round((double)(result.Progress.Sum(model => model.TaskScore) / result.Progress.Count));

			return result;
		}

		public async ValueTask<EducationStateProgressGrpcResponse> GetEducationStateProgressAsync(GetEducationStateProgressGrpcRequest request)
		{
			Guid? userId = request.UserId;

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
					Started = dtos.Any(dto => dto.HasProgress) && dtos.Key == EducationTutorial.PersonalFinance,
					Finished = dtos.All(dto => dto.HasProgress) && dtos.All(dto => dto.Value.GetValueOrDefault().IsOkProgress())
				})
				.OrderBy(arg => arg.Tutorial)
				.ToArray();

			return result;
		}

		public async ValueTask<CommonGrpcResponse> SetProgressAsync(SetEducationProgressGrpcRequest request)
		{
			Guid? userId = request.UserId;
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
				await _publisher.PublishAsync(request.ToBusModel(newRequestHasResult));
			}

			return commonGrpcResponse;
		}

		private async void SaveTestTasks100Prc(EducationProgressDto task, Guid? userId, bool isRetry, int taskScore)
		{
			if (EducationHelper.GetTask(task.Tutorial, task.Unit, task.Task).TaskType != EducationTaskType.Test)
				return;

			TestTasks100PrcDto prcDto = await _dtoRepository.GetTestTasks100Prc(userId);

			int newCount = isRetry || !taskScore.IsOkProgress() ? 0 : prcDto.Count + 1;
			if (prcDto.Count == newCount)
				return;

			prcDto.Count = newCount;

			CommonGrpcResponse result = await _dtoRepository.SetTestTasks100Prc(userId, prcDto);

			if (!result.IsSuccess)
				_logger.LogError("Error while set TestTasks100Prc value for user: {userId}.", userId);
		}

		public async ValueTask<CommonGrpcResponse> InitProgressAsync(InitEducationProgressGrpcRequest request)
		{
			Guid? userId = request.UserId;

			EducationProgressDto[] items = null;

			if (request.Tutorial != null)
				items = await _dtoRepository.GetEducationProgress(userId);

			if (items.IsNullOrEmpty())
				items = EducationHelper.GetProjections()
					.Select(item => new EducationProgressDto(item.Tutorial, item.Unit, item.Task))
					.ToArray();

			EducationProgressDto[] itemsToInit = items
				.WhereIf(request.Tutorial != null, dto => dto.Tutorial == request.Tutorial)
				.WhereIf(request.Unit != null, dto => dto.Unit == request.Unit)
				.WhereIf(request.Task != null, dto => dto.Task == request.Task)
				.ToArray();

			foreach (EducationProgressDto dto in itemsToInit)
				dto.Clear();

			return await _dtoRepository.SetEducationProgress(userId, items);
		}

		private CommonGrpcResponse GetFailResponse(string message)
		{
			_logger.LogError(message);

			return CommonGrpcResponse.Fail;
		}

		public async ValueTask<TaskTypeProgressGrpcResponse> GetTaskTypeProgressAsync(GetTaskTypeProgressGrpcRequest request)
		{
			Guid? userId = request.UserId;
			int? unit = request.Unit;
			var result = new TaskTypeProgressGrpcResponse();

			EducationProgressDto[] progressDtos = await _dtoRepository.GetEducationProgress(userId);
			if (progressDtos.IsNullOrEmpty())
			{
				_logger.LogError("No education progress record where found in ServerKeyValue storage for user: {userId}", userId);
				return result;
			}

			EducationProgressDto[] tasks = progressDtos
				.Where(dto => dto.Tutorial == request.Tutorial)
				.WhereIf(unit != null, dto => dto.Unit == unit)
				.ToArray();

			result.TaskTypeProgress = tasks
				.GroupBy(dto => EducationHelper.GetTask(dto.Tutorial, dto.Unit, dto.Task).TaskType, dto => dto.Value)
				.Select(dto => new TaskTypeProgressGrpcModel
				{
					TaskType = dto.Key,
					Values = dto.Select(i => i.GetValueOrDefault()).ToArray()
				}).ToArray();

			return result;
		}
	}
}