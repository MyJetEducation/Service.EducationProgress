using System.Runtime.Serialization;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class TaskShortEducationProgressGrpcModel
	{
		[DataMember(Order = 1)]
		public int Value { get; set; }

		[DataMember(Order = 2)]
		public int Unit { get; set; }
	}
}