﻿using System.Runtime.Serialization;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class EducationProgressGrpcResponse
	{
		[DataMember(Order = 1)]
		public EducationProgressGprcModel Progress { get; set; }
	}
}