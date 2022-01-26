using System;

namespace Service.EducationProgress.Models
{
	public class UnitsFinishedTodayDto
	{
		public UnitsFinishedTodayDto() => Date = DateTime.UtcNow;

		public DateTime Date { get; set; }

		public int Count { get; set; }
	}
}