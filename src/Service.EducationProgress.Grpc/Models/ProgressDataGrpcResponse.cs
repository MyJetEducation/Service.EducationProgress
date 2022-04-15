using System;
using System.Runtime.Serialization;
using Service.Education.Structure;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class ProgressDataGrpcResponse
	{
		[DataMember(Order = 1)]
		public EducationProgressTaskDataGrpcModel[] Items { get; set; }
	}

	[DataContract]
	public class EducationProgressTaskDataGrpcModel
	{
		[DataMember(Order = 1)]
		public EducationTutorial Tutorial { get; set; }

		[DataMember(Order = 2)]
		public int Unit { get; set; }

		[DataMember(Order = 3)]
		public int Task { get; set; }

		[DataMember(Order = 4)]
		public int? Value { get; set; }

		[DataMember(Order = 5)]
		public DateTime? Date { get; set; }

		[DataMember(Order = 6)]
		public int? Retries { get; set; }

		public bool HasProgress() => Value != null;
	}
}