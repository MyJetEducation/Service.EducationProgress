using System;
using System.Linq;
using Service.Education.Extensions;
using Service.Education.Helpers;
using Service.Education.Structure;
using Service.EducationProgress.Domain.Models;

namespace Service.EducationProgress.Services
{
	public static class EducationProgressDtoExtensions
	{
		public static bool IsPassed(this EducationProgressDto[] dtos) => AllHasProgress(dtos) && CountTestScore(dtos).IsOkProgress();

		public static bool IsPassed(this IGrouping<EducationTutorial, EducationProgressDto> grouping) => grouping.ToArray().IsPassed();

		public static bool AllHasProgress(this EducationProgressDto[] dtos) => dtos.All(dto => dto.HasProgress);

		public static bool AllHasProgress(this IGrouping<EducationTutorial, EducationProgressDto> grouping) => grouping.ToArray().AllHasProgress();

		public static bool HasProgress(this EducationProgressDto[] dtos) => dtos.Any(dto => dto.HasProgress);

		public static bool HasProgress(this IGrouping<EducationTutorial, EducationProgressDto> grouping) => grouping.ToArray().HasProgress();

		public static bool IsTestTask(this EducationProgressDto dto) => new[] {EducationTaskType.Test, EducationTaskType.TrueFalse}.Contains(GetTaskType(dto));

		public static EducationTaskType GetTaskType(this EducationProgressDto dto) => EducationHelper.GetTask(dto.Tutorial, dto.Unit, dto.Task).TaskType;

		public static int CountTestScore(this EducationProgressDto[] dtos)
		{
			if (!dtos.Any())
				return 0;

			EducationProgressDto[] testDtos = dtos.Where(IsTestTask).ToArray();

			return testDtos.Any()
				? (int) Math.Round(testDtos.Average(dto => dto.GetValue()))
				: 0;
		}

		public static int CountTaskScore(this EducationProgressDto[] dtos)
		{
			return dtos.Any() 
				? (int) Math.Round(dtos.ToArray().Average(dto => dto.GetValue())) 
				: 0;
		}
	}
}