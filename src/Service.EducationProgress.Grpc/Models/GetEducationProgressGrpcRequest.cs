﻿using System.Runtime.Serialization;
using Service.Education.Structure;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class GetEducationProgressGrpcRequest
	{
		[DataMember(Order = 1)]
		public string UserId { get; set; }

		[DataMember(Order = 2)]
		public EducationTutorial? Tutorial { get; set; }

		[DataMember(Order = 3)]
		public int? Unit { get; set; }
	}
}