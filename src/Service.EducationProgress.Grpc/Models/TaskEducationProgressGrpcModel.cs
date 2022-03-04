using System;
using System.Runtime.Serialization;
using Service.EducationProgress.Domain.Models;

namespace Service.EducationProgress.Grpc.Models
{
	[DataContract]
	public class TaskEducationProgressGrpcModel
	{
		public TaskEducationProgressGrpcModel()
		{
		}

		public TaskEducationProgressGrpcModel(EducationProgressDto progress)
		{
			HasProgress = progress is { HasProgress: true };

			if (progress is not { HasProgress: true }) 
				return;

			Value = progress.GetValue();
			Date = progress.Date;
			Task = progress.Task;
		}

		[DataMember(Order = 1)]
		public int Value { get; set; }

		[DataMember(Order = 2)]
		public bool HasProgress { get; set; }

		[DataMember(Order = 3)]
		public DateTime? Date { get; set; }

		[DataMember(Order = 4)]
		public int Task { get; set; }
	}
}