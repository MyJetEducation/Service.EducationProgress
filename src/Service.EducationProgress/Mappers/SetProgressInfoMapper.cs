using Service.EducationProgress.Domain.Models;
using Service.EducationProgress.Grpc.Models;

namespace Service.EducationProgress.Mappers
{
	public static class SetProgressInfoMapper
	{
		public static SetProgressInfoServiceBusModel ToBusModel(this SetEducationProgressGrpcRequest request) => new SetProgressInfoServiceBusModel
		{
			UserId = request.UserId,
			Tutorial = request.Tutorial,
			Unit = request.Unit,
			Task = request.Task
		};

		public static SetProgressInfoServiceBusModel ToBusModel(this ISetProgressInfo info) => new SetProgressInfoServiceBusModel
		{
			UserId = info.UserId,
			Tutorial = info.Tutorial,
			Unit = info.Unit,
			Task = info.Task
		};
	}
}