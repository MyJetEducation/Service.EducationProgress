using Service.Core.Domain.Models.Education;

namespace Service.EducationProgress.Domain.Models
{
	public class EducationProgressDto
	{
		public EducationProgressDto(EducationTutorial tutorial, int unit, int task, float? value)
		{
			Tutorial = tutorial;
			Unit = unit;
			Task = task;
			Value = value;
		}

		public EducationTutorial Tutorial { get; set; }

		public int Unit { get; set; }

		public int Task { get; set; }

		public float? Value { get; set; }
	}
}