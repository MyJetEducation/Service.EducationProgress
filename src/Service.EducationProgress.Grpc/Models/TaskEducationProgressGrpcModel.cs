using System;
using System.Runtime.Serialization;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class TaskEducationProgressGrpcModel
	{
		[DataMember(Order = 1)]
		public int Value { get; set; }

		[DataMember(Order = 2)]
		public bool HasProgress { get; set; }

		[DataMember(Order = 3)]
		public DateTime? Date { get; set; }
	}
}