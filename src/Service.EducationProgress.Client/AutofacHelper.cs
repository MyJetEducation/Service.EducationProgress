using Autofac;
using Service.EducationProgress.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.EducationProgress.Client
{
    public static class AutofacHelper
    {
        public static void RegisterEducationProgressClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new EducationProgressClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetEducationProgressService()).As<IEducationProgressService>().SingleInstance();
        }
    }
}
