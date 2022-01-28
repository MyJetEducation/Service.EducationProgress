using Service.EducationProgress.Grpc.Models;
using Service.ServiceBus.Models;

namespace Service.EducationProgress.Mappers
{
	public static class SetProgressInfoMapper
	{
		public static SetProgressInfoServiceBusModel ToBusModel(this SetEducationProgressGrpcRequest request, bool newRequestHasResult) => new SetProgressInfoServiceBusModel
		{
			UserId = request.UserId,
			Tutorial = request.Tutorial,
			Unit = request.Unit,
			Task = request.Task,
			SetUserProgress = newRequestHasResult,
			Duration = request.Duration,
			IsRetry = request.IsRetry
		};
	}
}