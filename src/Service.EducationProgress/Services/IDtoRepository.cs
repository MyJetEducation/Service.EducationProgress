using System;
using System.Threading.Tasks;
using Service.Core.Grpc.Models;
using Service.EducationProgress.Domain.Models;

namespace Service.EducationProgress.Services
{
	public interface IDtoRepository
	{
		ValueTask<EducationProgressDto[]> GetEducationProgress(Guid? userId);

		ValueTask<CommonGrpcResponse> SetEducationProgress(Guid? userId, EducationProgressDto[] prcDtos);

		ValueTask<TestTasks100PrcDto> GetTestTasks100Prc(Guid? userId);

		ValueTask<CommonGrpcResponse> SetTestTasks100Prc(Guid? userId, TestTasks100PrcDto prcDto);

		//ValueTask<UnitsFinishedTodayDto> GetUnitsFinishedToday(Guid? userId);

		//ValueTask<CommonGrpcResponse> SetUnitsFinishedToday(Guid? userId, UnitsFinishedTodayDto dto);
	}
}