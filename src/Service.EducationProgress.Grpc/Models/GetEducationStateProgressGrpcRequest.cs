﻿using System.Runtime.Serialization;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class GetEducationStateProgressGrpcRequest
	{
		[DataMember(Order = 1)]
		public string UserId { get; set; }
	}
}