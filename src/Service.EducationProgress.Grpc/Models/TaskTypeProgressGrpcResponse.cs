using System.Runtime.Serialization;
using Service.Education.Structure;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class TaskTypeProgressGrpcResponse
	{
		[DataMember(Order = 1)]
		public TaskTypeProgressGrpcModel[] TaskTypeProgress { get; set; }
	}

	[DataContract]
	public class TaskTypeProgressGrpcModel
	{
		[DataMember(Order = 1)]
		public int[] Values { get; set; }

		[DataMember(Order = 2)]
		public EducationTaskType TaskType { get; set; }
	}
}