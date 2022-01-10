using System.Threading.Tasks;
using DotNetCoreDecorators;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.TcpClient;
using Service.EducationProgress.Domain.Models;

namespace Service.EducationProgress.Services
{
	public class MyServiceBusPublisher : IPublisher<ISetProgressInfo>
	{
		private readonly MyServiceBusTcpClient _client;

		public MyServiceBusPublisher(MyServiceBusTcpClient client)
		{
			_client = client;
			_client.CreateTopicIfNotExists(SetProgressInfoServiceBusModel.TopicName);
		}

		public ValueTask PublishAsync(ISetProgressInfo valueToPublish)
		{
			byte[] bytesToSend = valueToPublish.ServiceBusContractToByteArray();

			Task task = _client.PublishAsync(SetProgressInfoServiceBusModel.TopicName, bytesToSend, false);

			return new ValueTask(task);
		}
	}
}