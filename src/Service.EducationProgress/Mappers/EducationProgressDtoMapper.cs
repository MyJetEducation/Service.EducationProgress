using Service.EducationProgress.Domain.Models;
using Service.EducationProgress.Grpc.Models;

namespace Service.EducationProgress.Mappers
{
	public static class EducationProgressDtoMapper
	{
		public static EducationProgressTaskDataGrpcModel ToGrpcModel(this EducationProgressDto dto) => new EducationProgressTaskDataGrpcModel
		{
			Tutorial = dto.Tutorial,
			Unit = dto.Unit,
			Task = dto.Task,
			Value = dto.Value,
			Retries = dto.Retries,
			Date = dto.Date
		};
	}
}