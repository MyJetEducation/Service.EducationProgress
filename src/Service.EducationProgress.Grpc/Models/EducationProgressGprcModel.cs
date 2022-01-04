using System;
using System.Runtime.Serialization;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class EducationProgressGprcModel
	{
		[DataMember(Order = 1)]
		public int Value { get; set; }

		[DataMember(Order = 2)]
		public TimeSpan? Duration { get; set; }
	}
}