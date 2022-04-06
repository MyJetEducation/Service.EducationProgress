using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Service.Core.Client.Models;
using Service.EducationProgress.Domain.Models;
using Service.Grpc;
using Service.ServerKeyValue.Grpc;
using Service.ServerKeyValue.Grpc.Models;

namespace Service.EducationProgress.Services
{
	public class DtoRepository : IDtoRepository
	{
		private readonly IGrpcServiceProxy<IServerKeyValueService> _serverKeyValueService;

		public DtoRepository(IGrpcServiceProxy<IServerKeyValueService> serverKeyValueService) => _serverKeyValueService = serverKeyValueService;

		public async ValueTask<EducationProgressDto[]> GetEducationProgress(string userId) =>
			await GetData<EducationProgressDto>(Program.ReloadedSettings(model => model.KeyEducationProgress), userId);

		public async ValueTask<CommonGrpcResponse> SetEducationProgress(string userId, EducationProgressDto[] prcDtos) =>
			await SetData(Program.ReloadedSettings(model => model.KeyEducationProgress), userId, prcDtos);

		public async ValueTask<TestTasks100PrcDto> GetTestTasks100Prc(string userId) =>
			(await GetData<TestTasks100PrcDto>(Program.ReloadedSettings(model => model.KeyTestTasks100Prc), userId)).FirstOrDefault()
				?? new TestTasks100PrcDto();

		public async ValueTask<CommonGrpcResponse> SetTestTasks100Prc(string userId, TestTasks100PrcDto prcDto) =>
			await SetData(Program.ReloadedSettings(model => model.KeyTestTasks100Prc), userId, new[] {prcDto});

		private async ValueTask<TDto[]> GetData<TDto>(Func<string> settingsKeyFunc, string userId)
		{
			string value = (await _serverKeyValueService.Service.GetSingle(new ItemsGetSingleGrpcRequest
			{
				UserId = userId,
				Key = settingsKeyFunc.Invoke()
			}))?.Value;

			return value == null
				? Array.Empty<TDto>()
				: JsonSerializer.Deserialize<TDto[]>(value);
		}

		private async ValueTask<CommonGrpcResponse> SetData<TDto>(Func<string> settingsKeyFunc, string userId, IEnumerable<TDto> dtos) => await _serverKeyValueService.TryCall(service => service.Put(new ItemsPutGrpcRequest
		{
			UserId = userId,
			Items = new[]
			{
				new KeyValueGrpcModel
				{
					Key = settingsKeyFunc.Invoke(),
					Value = JsonSerializer.Serialize(dtos)
				}
			}
		}));
	}
}