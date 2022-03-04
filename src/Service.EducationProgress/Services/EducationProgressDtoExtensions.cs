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
		public static bool IsPassed(this EducationProgressDto[] dtos) => HasProgress(dtos) && CountProgress(dtos).IsOkProgress();

		public static bool IsPassed(this IGrouping<EducationTutorial, EducationProgressDto> grouping) => grouping.ToArray().IsPassed();

		public static bool HasProgress(this EducationProgressDto[] dtos) => dtos.All(dto => dto.HasProgress);

		public static bool HasProgress(this IGrouping<EducationTutorial, EducationProgressDto> grouping) => grouping.ToArray().HasProgress();

		public static bool IsProgressTask(this EducationProgressDto dto) => new[] {EducationTaskType.Test, EducationTaskType.TrueFalse}.Contains(GetTaskType(dto));

		public static EducationTaskType GetTaskType(this EducationProgressDto dto) => EducationHelper.GetTask(dto.Tutorial, dto.Unit, dto.Task).TaskType;

		public static int CountProgress(this EducationProgressDto[] dtos)
		{
			if (!dtos.Any())
				return 0;

			EducationProgressDto[] progressDtos = dtos.Where(IsProgressTask).ToArray();

			return progressDtos.Any()
				? (int) Math.Round(progressDtos.Average(dto => dto.GetValue()))
				: 0;
		}
	}
}