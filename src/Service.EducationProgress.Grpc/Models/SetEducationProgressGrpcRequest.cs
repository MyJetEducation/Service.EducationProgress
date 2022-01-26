using System;
using System.Runtime.Serialization;
using Service.Core.Client.Education;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class SetEducationProgressGrpcRequest
	{
		[DataMember(Order = 1)]
		public Guid? UserId { get; set; }

		[DataMember(Order = 2)]
		public EducationTutorial Tutorial { get; set; }

		[DataMember(Order = 3)]
		public int Unit { get; set; }

		[DataMember(Order = 4)]
		public int Task { get; set; }

		[DataMember(Order = 5)]
		public int Value { get; set; }

		[DataMember(Order = 6)]
		public TimeSpan Duration { get; set; }

		[DataMember(Order = 7)]
		public bool IsRetry { get; set; }
	}
}