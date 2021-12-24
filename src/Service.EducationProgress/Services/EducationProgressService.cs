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
				return result;

			float[] progressValues = items
				.WhereIf(request.Tutorial != null, val => val.Tutorial == request.Tutorial)
				.WhereIf(request.Unit != null, val => val.Unit == request.Unit)
				.WhereIf(request.Task != null, val => val.Task == request.Task)
				.Select(val => val.Value)
				.ToArray();

			if (progressValues.IsNullOrEmpty())
			{
				_logger.LogError("Error while set education progress for user: {userId}, no progress found.", userId);
				return result;
			}

			result.Value = (int) Math.Round(progressValues.Average());

			return result;
		}

		public async ValueTask<CommonGrpcResponse> SetProgressAsync(SetEducationProgressGrpcRequest request)
		{
			Guid? userId = request.UserId;
			float taskValue = request.Value;

			if (taskValue > 100 || taskValue < 0)
				return GetFailResponse($"Error while set education progress for user: {userId}, progress value is not valid: {taskValue}.");

			EducationProgressDto[] progressDtos = await GetProgress(userId);
			if (progressDtos.IsNullOrEmpty())
				return CommonGrpcResponse.Fail;

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
			if (!items.IsNullOrEmpty())
				return CommonGrpcResponse.Fail;

			EducationProgressDto[] progressDtos = EducationStructure.GetProjections()
				.Select(item => new EducationProgressDto(item.Tutorial, item.Unit, item.Task, 0f))
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
			if (value != null)
			{
				EducationProgressDto[] progressDtos = JsonSerializer.Deserialize<EducationProgressDto[]>(value);
				if (!progressDtos.IsNullOrEmpty())
					return progressDtos;
			}

			_logger.LogError("No education progress record where found in ServerKeyValue storage for user: {userId}", userId);

			return await ValueTask.FromResult<EducationProgressDto[]>(null);
		}

		private CommonGrpcResponse GetFailResponse(string message)
		{
			_logger.LogError(message);

			return CommonGrpcResponse.Fail;
		}
	}
}