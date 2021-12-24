using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.EducationProgress.Grpc;

namespace Service.EducationProgress.Client
{
    [UsedImplicitly]
    public class EducationProgressClientFactory : MyGrpcClientFactory
    {
        public EducationProgressClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public IEducationProgressService GetEducationProgressService() => CreateGrpcService<IEducationProgressService>();
    }
}
