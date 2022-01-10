using Service.EducationProgress.Grpc.Models;
using Service.UserHabit.Grpc.Models;

namespace Service.EducationProgress.Mappers
{
	public static class SetHabitMapper
	{
		public static SetHabitGrpcRequset ToGrpcModel(this SetEducationProgressGrpcRequest request) => new SetHabitGrpcRequset
		{
			UserId = request.UserId,
			Tutorial = request.Tutorial,
			Unit = request.Unit,
			Task = request.Task
		};
	}
}