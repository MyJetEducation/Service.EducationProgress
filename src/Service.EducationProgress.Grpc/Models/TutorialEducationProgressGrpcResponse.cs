using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Education.Structure;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class TutorialEducationProgressGrpcResponse
	{
		[DataMember(Order = 1)]
		public EducationTutorial Tutorial { get; set; }

		[DataMember(Order = 2)]
		public bool Finished { get; set; }

		[DataMember(Order = 3)]
		public int TaskScore { get; set; }

		[DataMember(Order = 4)]
		public List<ShortUnitEducationProgressGrpcResponse> Units { get; set; }
	}

	[DataContract]
	public class ShortUnitEducationProgressGrpcResponse
	{
		[DataMember(Order = 1)]
		public int Unit { get; set; }

		[DataMember(Order = 2)]
		public bool Finished { get; set; }

		[DataMember(Order = 3)]
		public int TaskScore { get; set; }

		[DataMember(Order = 4)]
		public bool HasProgress { get; set; }

		[DataMember(Order = 5)]
		public List<ShortTaskEducationProgressGrpcModel> Tasks { get; set; }
	}

	[DataContract]
	public class ShortTaskEducationProgressGrpcModel
	{
		[DataMember(Order = 1)]
		public int Value { get; set; }

		[DataMember(Order = 2)]
		public bool HasProgress { get; set; }

		[DataMember(Order = 3)]
		public int Task { get; set; }

		[DataMember(Order = 4)]
		public DateTime? Date { get; set; }
	}
}