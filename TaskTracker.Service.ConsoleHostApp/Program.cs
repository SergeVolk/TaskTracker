using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

using TaskTracker.ExceptionUtils;
using TaskTracker.Repository.Sql;

namespace TaskTracker.Service.ConsoleHostApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new TaskTrackerServiceHost(typeof(TaskTrackerService)))
            {
                host.Open();
                Console.WriteLine("TaskTracker service is running.");
                Console.WriteLine("<Press Enter to stop>");
                Console.ReadLine();
            }
        }
    }   

    public class DependencyInjectionInstanceProvider : IInstanceProvider
    {
        private Type serviceType;
        private static string dbConnectionString;

        static DependencyInjectionInstanceProvider()
        {
            dbConnectionString = ConfigurationManager.ConnectionStrings["TaskTrackerDB"].ConnectionString;
        }

        public DependencyInjectionInstanceProvider(Type serviceType)
        {
            ArgumentValidation.ThrowIfNull(serviceType, nameof(serviceType));
            this.serviceType = serviceType;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            var repoFactory = new SqlRepositoryFactory(false);
            return new TaskTrackerService(repoFactory.CreateRepositoryQueries(dbConnectionString), repoFactory.CreateRepositoryCommands(dbConnectionString));  
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        { }
    }

    public class DependencyInjectionServiceBehavior : IServiceBehavior
    {
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            ArgumentValidation.ThrowIfNull(serviceDescription, nameof(serviceDescription));
            ArgumentValidation.ThrowIfNull(serviceHostBase, nameof(serviceHostBase));

            foreach (ChannelDispatcherBase cdb in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher cd = cdb as ChannelDispatcher;
                if (cd != null)
                {
                    foreach (EndpointDispatcher ed in cd.Endpoints)
                    {
                        ed.DispatchRuntime.InstanceProvider =
                            new DependencyInjectionInstanceProvider(serviceDescription.ServiceType);
                    }
                }
            }
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
                Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        { }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase) { }
    }

    public class TaskTrackerServiceHost : ServiceHost
    {
        public TaskTrackerServiceHost(Type serviceType, params Uri[] baseAddresses) : base(serviceType, baseAddresses)
        { }

        protected override void OnOpening()
        {
            this.Description.Behaviors.Add(new DependencyInjectionServiceBehavior());
            base.OnOpening();
        }
    }
}
