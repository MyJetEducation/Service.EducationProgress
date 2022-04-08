using System;
using System.Threading.Tasks;
using Service.Core.Client.Models;
using Service.EducationProgress.Domain.Models;

namespace Service.EducationProgress.Services
{
	public interface IDtoRepository
	{
		ValueTask<EducationProgressDto[]> GetEducationProgress(string userId);

		ValueTask<CommonGrpcResponse> SetEducationProgress(string userId, EducationProgressDto[] prcDtos);

		ValueTask<TestTasks100PrcDto> GetTestTasks100Prc(string userId);

		ValueTask<CommonGrpcResponse> SetTestTasks100Prc(string userId, TestTasks100PrcDto prcDto);
	}
}