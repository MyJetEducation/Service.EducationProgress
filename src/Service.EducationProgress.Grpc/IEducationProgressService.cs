using System.ServiceModel;
using System.Threading.Tasks;
using Service.Core.Grpc.Models;
using Service.EducationProgress.Grpc.Models;

namespace Service.EducationProgress.Grpc
{
	[ServiceContract]
	public interface IEducationProgressService
	{
		[OperationContract]
		ValueTask<EducationProgressGrpcResponse> GetProgressAsync(GetEducationProgressGrpcRequest request);

		[OperationContract]
		ValueTask<CommonGrpcResponse> SetProgressAsync(SetEducationProgressGrpcRequest request);

		[OperationContract]
		ValueTask<CommonGrpcResponse> InitProgressAsync(InitEducationProgressGrpcRequest request);
	}
}