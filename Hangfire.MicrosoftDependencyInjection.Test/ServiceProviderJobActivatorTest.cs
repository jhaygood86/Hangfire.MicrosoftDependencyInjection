using System;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Hangfire.MicrosoftDependencyInjection.Test
{
    [TestClass]
    public class ServiceProviderJobActivatorTest
    {
        private readonly IServiceCollection _serviceCollection;

        public ServiceProviderJobActivatorTest()
        {
            _serviceCollection = new ServiceCollection();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_Should_Throw_When_ServiceProvider_Is_Null()
        {
            var activator = new ServiceProviderJobActivator(null);
            Assert.Fail("This should not be reached");
        }

        [TestMethod]
        public void Class_Is_Based_On_JobActivator()
        {
            var activator = CreateActivator();
            Assert.IsInstanceOfType(activator,typeof(JobActivator));
        }

        [TestMethod]
        public void ActivateJob_Calls_StructureMap()
        {
            _serviceCollection.AddTransient<string>((sp) => "called");

            var activator = CreateActivator();
            var result = activator.ActivateJob(typeof(string));

            Assert.AreEqual("called", result);
        }

        [TestMethod]
        public void Instance_Registered_With_Transient_Scope_Is_Disposed_On_Scope_Disposal()
        {
            var disposable = new BackgroundJobDependency();
            _serviceCollection.AddTransient<BackgroundJobDependency>(sp => disposable);
            var activator = CreateActivator();

            using (var scope = activator.BeginScope())
            {
                var instance = scope.Resolve(typeof(BackgroundJobDependency));
                Assert.AreSame(instance, disposable);
                Assert.IsFalse(((BackgroundJobDependency)instance).Disposed);
            }

            Assert.IsTrue(disposable.Disposed);
        }

        [TestMethod]
        public void Instance_Registered_With_Singleton_Scope_Is_Disposed_On_Scope_Disposal()
        {
            var disposable = new BackgroundJobDependency();
            _serviceCollection.AddSingleton<BackgroundJobDependency>(sp => disposable);
            var activator = CreateActivator();

            using (var scope = activator.BeginScope())
            {
                var instance = scope.Resolve(typeof(BackgroundJobDependency));
                Assert.AreSame(instance, disposable);
                Assert.IsFalse(((BackgroundJobDependency)instance).Disposed);
            }

            Assert.IsFalse(disposable.Disposed);
        }

        [TestMethod]
        public void In_BackgroundJobScope_Registers_Same_Service_Instance_For_The_Same_Scope_Instance()
        {
            _serviceCollection.AddScoped<object>(sp => new object());
            var activator = CreateActivator();

            using (var scope = activator.BeginScope())
            {
                var instance1 = scope.Resolve(typeof(object));
                var instance2 = scope.Resolve(typeof(object));

                Assert.AreSame(instance1,instance2);
            }
        }

        [TestMethod]
        public void In_BackgroundJobScope_Registers_Different_Service_Instances_For_Different_Scope_Instances()
        {
            _serviceCollection.AddScoped<object>();
            var activator = CreateActivator();

            object instance1;
            using (var scope1 = activator.BeginScope()) instance1 = scope1.Resolve(typeof(object));
            object instance2;
            using (var scope2 = activator.BeginScope()) instance2 = scope2.Resolve(typeof(object));

            Assert.AreNotSame(instance1, instance2);
        }

        [TestMethod]
        public void Instance_Registered_With_BackgroundJobScope_Is_Disposed_On_Scope_Disposal()
        {
            BackgroundJobDependency disposable;
            _serviceCollection.AddScoped<BackgroundJobDependency>();
            var activator = CreateActivator();

            using (var scope = activator.BeginScope())
            {
                disposable = (BackgroundJobDependency) scope.Resolve(typeof(BackgroundJobDependency));
                Assert.IsFalse(disposable.Disposed);
            }

            Assert.IsTrue(disposable.Disposed);
        }

        [TestMethod]
        public void Instance_Registered_With_BackgroundJobScope_Is_Reused_For_Other_Objects()
        {
            _serviceCollection.AddScoped<TestJob>();
            _serviceCollection.AddScoped<UniqueDependency>();
            _serviceCollection.AddScoped<ObjectDependsOnSameDependency>();
            _serviceCollection.AddScoped<BackgroundJobDependency>();
            var activator = CreateActivator();

            using (var scope = activator.BeginScope())
            {
                var instance = (TestJob) scope.Resolve(typeof(TestJob));
                Assert.AreSame(instance.BackgroundJobDependency, instance.SameDependencyObject.BackgroundJobDependency);
            }
        }

        [TestMethod]
        public void Instance_Registered_With_TransientScope_Is_Not_Reused_For_Other_Objects()
        {
            _serviceCollection.AddScoped<TestJob>();
            _serviceCollection.AddTransient<UniqueDependency>();
            _serviceCollection.AddScoped<ObjectDependsOnSameDependency>();
            _serviceCollection.AddScoped<BackgroundJobDependency>();

            var activator = CreateActivator();

            using (var scope = activator.BeginScope())
            {
                var instance = (TestJob)scope.Resolve(typeof(TestJob));
                Assert.AreNotSame(instance.UniqueDependency, instance.SameDependencyObject.UniqueDependency);
            }
        }

        private ServiceProviderJobActivator CreateActivator(Action<IServiceProvider> serviceProviderAction = null)
        {
            var serviceProvider = _serviceCollection.BuildServiceProvider();

            serviceProviderAction?.Invoke(serviceProvider);

            return new ServiceProviderJobActivator(serviceProvider);
        }
    }
}
