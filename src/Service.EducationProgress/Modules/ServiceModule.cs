using Autofac;
using Service.ServerKeyValue.Client;
using Service.UserHabit.Client;
using Service.UserKnowledge.Client;

namespace Service.EducationProgress.Modules
{
	public class ServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterKeyValueClient(Program.Settings.ServerKeyValueServiceUrl);
			builder.RegisterUserKnowledgeClient(Program.Settings.UserKnowledgeServiceUrl);
			builder.RegisterUserHabitClient(Program.Settings.UserHabitServiceUrl);
		}
	}
}