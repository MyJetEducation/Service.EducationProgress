using System.Runtime.Serialization;
using Service.Education.Structure;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class EducationStateProgressGrpcResponse
	{
		public EducationStateTutorialGrpcModel[] Tutorials { get; set; }
	}

	public class EducationStateTutorialGrpcModel
	{
		public EducationTutorial Tutorial { get; set; }
		public bool Started { get; set; }
		public bool Finished { get; set; }
	}
}