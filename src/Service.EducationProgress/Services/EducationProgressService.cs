using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.Core.Domain.Extensions;
using Service.Core.Domain.Models.Education;
using Service.Core.Grpc.Models;
using Service.EducationProgress.Domain.Models;
using Service.EducationProgress.Grpc;
using Service.EducationProgress.Grpc.Models;
using Service.ServerKeyValue.Grpc;
using Service.ServerKeyValue.Grpc.Models;

namespace Service.EducationProgress.Services
{
	public class EducationProgressService : IEducationProgressService
	{
		private static readonly string KeyEducationProgress = Program.Settings.KeyEducationProgress;

		private readonly IServerKeyValueService _serverKeyValueService;
		private readonly ILogger<EducationProgressService> _logger;

		public EducationProgressService(IServerKeyValueService serverKeyValueService, ILogger<EducationProgressService> logger)
		{
			_serverKeyValueService = serverKeyValueService;
			_logger = logger;
		}

		public async ValueTask<EducationProgressGrpcResponse> GetProgressAsync(GetEducationProgressGrpcRequest request)
		{
			var result = new EducationProgressGrpcResponse();
			Guid? userId = request.UserId;

			EducationProgressDto[] items = await GetProgress(userId);
			if (items == null)
			{
				_logger.LogError("No education progress record where found in ServerKeyValue storage for user: {userId}", userId);
				return result;
			}

			EducationProgressDto[] dtos = FilterProgressData(items, request.Tutorial, request.Unit);

			float[] progressValues = dtos
				.Select(val => val.Value ?? 0f)
				.ToArray();

			if (dtos.IsNullOrEmpty())
			{
				_logger.LogError("Error while set education progress for user: {userId}, no progress found.", userId);
				return result;
			}

			result.Progress = new EducationProgressGprcModel
			{
				Value = (int) Math.Round(progressValues.Average())
			};

			return result;
		}

		private static EducationProgressDto[] FilterProgressData(EducationProgressDto[] items, EducationTutorial? tutorial, int? unit, int? task = null) => items
			.WhereIf(tutorial != null, val => val.Tutorial == tutorial)
			.WhereIf(unit != null, val => val.Unit == unit)
			.WhereIf(task != null, val => val.Task == task)
			.ToArray();

		public async ValueTask<TaskEducationProgressGrpcResponse> GetTaskProgressAsync(GetTaskEducationProgressGrpcRequest request)
		{
			var result = new TaskEducationProgressGrpcResponse();
			Guid? userId = request.UserId;

			EducationProgressDto[] items = await GetProgress(userId);
			if (items == null)
			{
				_logger.LogError("No education progress record where found in ServerKeyValue storage for user: {userId}", userId);
				return result;
			}

			float? progressValue = FilterProgressData(items, request.Tutorial, request.Unit, request.Task)
				.Select(dto => dto.Value)
				.FirstOrDefault();

			result.Progress = new TaskEducationProgressGrpcModel
			{
				Value = (int) Math.Round(progressValue.GetValueOrDefault()),
				HasProgress = progressValue != null
			};

			return result;
		}

		public async ValueTask<CommonGrpcResponse> SetProgressAsync(SetEducationProgressGrpcRequest request)
		{
			Guid? userId = request.UserId;
			float taskValue = request.Value;

			if (taskValue > 100 || taskValue < 0)
				return GetFailResponse($"Error while set education progress for user: {userId}, progress value is not valid: {taskValue}.");

			EducationProgressDto[] progressDtos = await GetProgress(userId);
			if (progressDtos == null)
				return GetFailResponse($"No education progress record where found in ServerKeyValue storage for user: {userId}");

			EducationProgressDto task = progressDtos
				.Where(dto => dto.Tutorial == request.Tutorial)
				.Where(dto => dto.Unit == request.Unit)
				.FirstOrDefault(dto => dto.Task == request.Task);

			if (task == null)
				return GetFailResponse($"Error while set education progress for user: {userId}, progress for task not exists.");

			if (task.Value.Equals(taskValue))
				return CommonGrpcResponse.Success;

			task.Value = taskValue;

			return await SetProgress(request.UserId, progressDtos);
		}

		public async ValueTask<CommonGrpcResponse> InitProgressAsync(InitEducationProgressGrpcRequest request)
		{
			Guid? userId = request.UserId;

			EducationProgressDto[] items = await GetProgress(userId);
			if (items != null)
				return GetFailResponse($"Error while init education progress record in ServerKeyValue storage for user: {userId}, progress already exists.");

			EducationProgressDto[] progressDtos = EducationStructure.GetProjections()
				.Select(item => new EducationProgressDto(item.Tutorial, item.Unit, item.Task, null))
				.ToArray();

			return await SetProgress(request.UserId, progressDtos);
		}

		private async Task<CommonGrpcResponse> SetProgress(Guid? userId, EducationProgressDto[] progressDtos) => await _serverKeyValueService.Put(new ItemsPutGrpcRequest
		{
			UserId = userId,
			Items = new[]
			{
				new KeyValueGrpcModel
				{
					Key = KeyEducationProgress,
					Value = JsonSerializer.Serialize(progressDtos)
				}
			}
		});

		private async ValueTask<EducationProgressDto[]> GetProgress(Guid? userId)
		{
			ItemsGrpcResponse getResponse = await _serverKeyValueService.Get(new ItemsGetGrpcRequest
			{
				UserId = userId,
				Keys = new[] {KeyEducationProgress}
			});

			string value = getResponse.Items?.FirstOrDefault(model => model.Key == KeyEducationProgress)?.Value;
			if (value == null)
				return await ValueTask.FromResult<EducationProgressDto[]>(null);

			EducationProgressDto[] progressDtos = JsonSerializer.Deserialize<EducationProgressDto[]>(value);
			if (!progressDtos.IsNullOrEmpty())
				return progressDtos;

			return await ValueTask.FromResult<EducationProgressDto[]>(null);
		}

		private CommonGrpcResponse GetFailResponse(string message)
		{
			_logger.LogError(message);

			return CommonGrpcResponse.Fail;
		}
	}
}