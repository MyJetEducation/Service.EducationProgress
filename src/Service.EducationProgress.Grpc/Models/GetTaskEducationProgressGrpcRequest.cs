using System;
using System.Runtime.Serialization;
using Service.Core.Domain.Models.Education;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class GetTaskEducationProgressGrpcRequest
	{
		[DataMember(Order = 1)]
		public Guid? UserId { get; set; }

		[DataMember(Order = 2)]
		public EducationTutorial Tutorial { get; set; }

		[DataMember(Order = 3)]
		public int Unit { get; set; }

		[DataMember(Order = 4)]
		public int Task { get; set; }
	}
}