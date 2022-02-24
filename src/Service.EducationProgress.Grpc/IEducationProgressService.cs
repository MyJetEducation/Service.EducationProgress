using System.ServiceModel;
using System.Threading.Tasks;
using Service.Core.Client.Models;
using Service.EducationProgress.Grpc.Models;

namespace Service.EducationProgress.Grpc
{
	[ServiceContract]
	public interface IEducationProgressService
	{
		/// <summary>
		///     Progress value for tutorial/unit
		/// </summary>
		[OperationContract]
		ValueTask<EducationProgressGrpcResponse> GetProgressAsync(GetEducationProgressGrpcRequest request);
		
		/// <summary>
		///     Progress info for single task
		/// </summary>
		[OperationContract]
		ValueTask<TaskEducationProgressGrpcResponse> GetTaskProgressAsync(GetTaskEducationProgressGrpcRequest request);

		/// <summary>
		///     Progress type and value for multiple tasks
		/// </summary>
		[OperationContract]
		ValueTask<TaskTypeProgressGrpcResponse> GetTaskTypeProgressAsync(GetTaskTypeProgressGrpcRequest request);

		/// <summary>
		///     Progress info for unit tasks
		/// </summary>
		[OperationContract]
		ValueTask<UnitEducationProgressGrpcResponse> GetUnitProgressAsync(GetUnitEducationProgressGrpcRequest request);
		
		/// <summary>
		///     Progress info for tutorial units
		/// </summary>
		[OperationContract]
		ValueTask<TutorialEducationProgressGrpcResponse> GetTutorialProgressAsync(GetTutorialEducationProgressGrpcRequest request);
		
		/// <summary>
		///     Progress info for all tutorials
		/// </summary>
		[OperationContract]
		ValueTask<EducationStateProgressGrpcResponse> GetEducationStateProgressAsync(GetEducationStateProgressGrpcRequest request);

		/// <summary>
		///     Save progress info
		/// </summary>
		[OperationContract]
		ValueTask<CommonGrpcResponse> SetProgressAsync(SetEducationProgressGrpcRequest request);

		/// <summary>
		///     Clear progress for user
		/// </summary>
		[OperationContract]
		ValueTask<CommonGrpcResponse> InitProgressAsync(InitEducationProgressGrpcRequest request);
	}
}