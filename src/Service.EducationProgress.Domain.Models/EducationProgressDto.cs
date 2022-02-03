using System;
using System.Text.Json.Serialization;
using Service.Core.Client.Education;

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

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? Value { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public DateTime? Date { get; set; }

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public int? Retries { get; set; }

		[JsonIgnore]
		public bool HasProgress => Value != null;

		public void Clear()
		{
			Value = null;
			Date = null;
			Retries = null;
		}
	}
}