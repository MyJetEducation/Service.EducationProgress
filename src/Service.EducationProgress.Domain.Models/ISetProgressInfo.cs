using System;
using Service.Core.Domain.Models.Education;

namespace Service.EducationProgress.Domain.Models
{
	public interface ISetProgressInfo
	{
		public Guid? UserId { get; set; }

		public EducationTutorial Tutorial { get; set; }

		public int Unit { get; set; }

		public int Task { get; set; }
	}
}