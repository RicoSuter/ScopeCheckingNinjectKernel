namespace ScopeCheckingNinjectKernel.Services
{
    public class Service1Singleton : IService1
    {
        public Service1Singleton(IService2 service2)
        {
            Service2 = service2;
        }

        public IService2 Service2 { get; private set; }

        // This is thread safe
    }
}