using Autofac;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.TcpClient;
using Service.EducationProgress.Services;
using Service.ServerKeyValue.Client;
using Service.ServiceBus.Models;

namespace Service.EducationProgress.Modules
{
	public class ServiceModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterKeyValueClient(Program.Settings.ServerKeyValueServiceUrl);
			builder.RegisterType<DtoRepository>().AsImplementedInterfaces().SingleInstance();

			var tcpServiceBus = new MyServiceBusTcpClient(() => Program.Settings.ServiceBusWriter, "MyJetEducation Service.EducationProgress");

			builder
				.Register(context => new MyServiceBusPublisher<SetProgressInfoServiceBusModel>(tcpServiceBus, SetProgressInfoServiceBusModel.TopicName, false))
				.As<IServiceBusPublisher<SetProgressInfoServiceBusModel>>()
				.SingleInstance();

			tcpServiceBus.Start();
		}
	}
}