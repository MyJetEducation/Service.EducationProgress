using System.Runtime.Serialization;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class EducationProgressGrpcResponse
	{
		[DataMember(Order = 1)]
		public int TaskScore { get; set; }

		[DataMember(Order = 2)]
		public int TestScore { get; set; }

		[DataMember(Order = 3)]
		public int TutorialsPassed { get; set; }

		[DataMember(Order = 4)]
		public int TasksPassed { get; set; }
	}
}