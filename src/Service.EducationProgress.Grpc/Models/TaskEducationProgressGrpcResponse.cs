using System.Runtime.Serialization;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class TaskEducationProgressGrpcResponse
	{
		[DataMember(Order = 1)]
		public TaskEducationProgressGrpcModel Progress { get; set; }
	}
}