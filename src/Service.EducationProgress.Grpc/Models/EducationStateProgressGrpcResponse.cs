using System.Runtime.Serialization;
using Service.Education.Structure;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class EducationStateProgressGrpcResponse
	{
		[DataMember(Order = 1)]
		public EducationStateTutorialGrpcModel[] Tutorials { get; set; }
	}

	[DataContract]
	public class EducationStateTutorialGrpcModel
	{
		[DataMember(Order = 1)]
		public EducationTutorial Tutorial { get; set; }

		[DataMember(Order = 2)]
		public bool Started { get; set; }

		[DataMember(Order = 3)]
		public bool Finished { get; set; }
	}
}