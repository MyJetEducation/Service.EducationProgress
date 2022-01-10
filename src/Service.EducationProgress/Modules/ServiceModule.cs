using Autofac;
using DotNetCoreDecorators;
using MyServiceBus.TcpClient;
using Service.EducationProgress.Domain.Models;
using Service.EducationProgress.Services;
using Service.ServerKeyValue.Client;
using Service.UserHabit.Client;

namespace Service.EducationProgress.Modules
{
	public class ServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterKeyValueClient(Program.Settings.ServerKeyValueServiceUrl);
			builder.RegisterUserHabitClient(Program.Settings.UserHabitServiceUrl);

			var tcpServiceBus = new MyServiceBusTcpClient(() => Program.Settings.ServiceBusWriter, "MyJetEducation Service.EducationProgress");
			IPublisher<ISetProgressInfo> clientRegisterPublisher = new MyServiceBusPublisher(tcpServiceBus);
			builder.Register(context => clientRegisterPublisher);
			tcpServiceBus.Start();
		}
	}
}