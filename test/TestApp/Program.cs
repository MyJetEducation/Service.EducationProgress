using System;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;
using Service.EducationProgress.Client;
using Service.EducationProgress.Grpc;

namespace TestApp
{
	public class Program
	{
		private static async Task Main(string[] args)
		{
			GrpcClientFactory.AllowUnencryptedHttp2 = true;

			Console.Write("Press enter to start");
			Console.ReadLine();

			var factory = new EducationProgressClientFactory("http://localhost:5001");
			IEducationProgressService client = factory.GetEducationProgressService();

			Console.WriteLine("End");
			Console.ReadLine();
		}
	}
}