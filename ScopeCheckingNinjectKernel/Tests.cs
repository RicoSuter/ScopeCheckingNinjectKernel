using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;
using ScopeCheckingNinjectKernel.Services;

namespace ScopeCheckingNinjectKernel
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void When_injecting_singleton_scoped_into_singleton_scoped_then_success()
        {
            var kernel = new ScopeCheckingStandardKernel();
            kernel.Bind<IService1>().To<Service1Singleton>().InSingletonScope();
            kernel.Bind<IService2>().To<Service2Transient>().InSingletonScope();
            var service = kernel.Get<IService1>(); // no exception
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void When_injecting_thread_scoped_into_singleton_scoped_then_exception()
        {
            var kernel = new ScopeCheckingStandardKernel();
            kernel.Bind<IService1>().To<Service1Singleton>().InSingletonScope();
            kernel.Bind<IService2>().To<Service2Transient>().InThreadScope();
            var service = kernel.Get<IService1>(); // exception
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void When_injecting_transient_scoped_into_singleton_scoped_then_exception()
        {
            var kernel = new ScopeCheckingStandardKernel();
            kernel.Bind<IService1>().To<Service1Singleton>().InSingletonScope();
            kernel.Bind<IService2>().To<Service2Transient>().InTransientScope();
            var service = kernel.Get<IService1>(); // exception
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void When_injecting_transient_scoped_into_thread_scoped_then_exception()
        {
            var kernel = new ScopeCheckingStandardKernel();
            kernel.Bind<IService1>().To<Service1Singleton>().InThreadScope();
            kernel.Bind<IService2>().To<Service2Transient>().InTransientScope();
            var service = kernel.Get<IService1>(); // exception
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void When_injecting_unscoped_into_singleton_scoped_then_exception()
        {
            var kernel = new ScopeCheckingStandardKernel();
            kernel.Bind<IService1>().To<Service1Singleton>().InSingletonScope();
            kernel.Bind<IService2>().To<Service2Transient>();
            var service = kernel.Get<IService1>(); // exception
        }
    }
}