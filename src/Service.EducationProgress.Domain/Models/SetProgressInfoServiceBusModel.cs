using System;
using System.Runtime.Serialization;
using Service.Core.Domain.Models.Education;

namespace Service.EducationProgress.Domain.Models
{
	[DataContract]
	public class SetProgressInfoServiceBusModel : ISetProgressInfo
	{
		public const string TopicName = "myjeteducation-set-progress";

		public Guid? UserId { get; set; }

		public EducationTutorial Tutorial { get; set; }

		public int Unit { get; set; }

		public int Task { get; set; }
	}
}