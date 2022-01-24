using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Service.Core.Grpc.Models;
using Service.EducationProgress.Domain.Models;
using Service.ServerKeyValue.Grpc;
using Service.ServerKeyValue.Grpc.Models;

namespace Service.EducationProgress.Services
{
	public class DtoRepository : IDtoRepository
	{
		private readonly IServerKeyValueService _serverKeyValueService;

		public DtoRepository(IServerKeyValueService serverKeyValueService) => _serverKeyValueService = serverKeyValueService;

		public async ValueTask<EducationProgressDto[]> GetEducationProgress(Guid? userId) =>
			await GetData<EducationProgressDto>(Program.ReloadedSettings(model => model.KeyEducationProgress), userId);

		public async ValueTask<CommonGrpcResponse> SetEducationProgress(Guid? userId, EducationProgressDto[] prcDtos) =>
			await SetData(Program.ReloadedSettings(model => model.KeyEducationProgress), userId, prcDtos);

		public async ValueTask<TestTasks100PrcDto> GetTestTasks100Prc(Guid? userId) =>
			(await GetData<TestTasks100PrcDto>(Program.ReloadedSettings(model => model.KeyTestTasks100Prc), userId)).FirstOrDefault()
				?? new TestTasks100PrcDto();

		public async ValueTask<CommonGrpcResponse> SetTestTasks100Prc(Guid? userId, TestTasks100PrcDto prcDto) =>
			await SetData(Program.ReloadedSettings(model => model.KeyTestTasks100Prc), userId, new[] {prcDto});

		//public async ValueTask<UnitsFinishedTodayDto> GetUnitsFinishedToday(Guid? userId) =>
		//	(await GetData<UnitsFinishedTodayDto>(Program.ReloadedSettings(model => model.KeyUnitsFinishedToday), userId)).FirstOrDefault()
		//		?? new UnitsFinishedTodayDto();

		//public async ValueTask<CommonGrpcResponse> SetUnitsFinishedToday(Guid? userId, UnitsFinishedTodayDto dto) =>
		//	await SetData(Program.ReloadedSettings(model => model.KeyUnitsFinishedToday), userId, new[] {dto});

		private async ValueTask<TDto[]> GetData<TDto>(Func<string> settingsKeyFunc, Guid? userId)
		{
			string value = (await _serverKeyValueService.GetSingle(new ItemsGetSingleGrpcRequest
			{
				UserId = userId,
				Key = settingsKeyFunc.Invoke()
			}))?.Value;

			return value == null
				? Array.Empty<TDto>()
				: JsonSerializer.Deserialize<TDto[]>(value);
		}

		private async ValueTask<CommonGrpcResponse> SetData<TDto>(Func<string> settingsKeyFunc, Guid? userId, IEnumerable<TDto> dtos) => await _serverKeyValueService.Put(new ItemsPutGrpcRequest
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
		});
	}
}