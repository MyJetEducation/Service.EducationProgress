using System;
using Service.Core.Domain.Models.Education;

namespace Service.EducationProgress.Domain.Models
{
	public class EducationProgressDto
	{
		public EducationProgressDto(EducationTutorial tutorial, int unit, int task)
		{
			Tutorial = tutorial;
			Unit = unit;
			Task = task;
		}

		public EducationTutorial Tutorial { get; set; }

		public int Unit { get; set; }

		public int Task { get; set; }

		public float? Value { get; set; }

		public DateTime? WhenFinished { get; set; }
	}
}