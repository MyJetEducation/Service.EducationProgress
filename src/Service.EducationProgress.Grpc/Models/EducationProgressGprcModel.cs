using System.Runtime.Serialization;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class EducationProgressGprcModel
	{
		[DataMember(Order = 1)]
		public int Value { get; set; }
	}
}