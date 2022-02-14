using System.Runtime.Serialization;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class UnitEducationProgressGrpcResponse
	{
		[DataMember(Order = 1)]
		public TaskEducationProgressGrpcModel[] Progress { get; set; }
	}
}