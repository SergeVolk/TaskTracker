using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;

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
                Console.WriteLine("TaskTracker service is open.");
                Console.WriteLine("<Press Enter to close>");
                Console.ReadLine();
            }
        }
    }   

    public class DependencyInjectionInstanceProvider : IInstanceProvider
    {
        private Type _serviceType;
        private static string DbConnectionString;

        static DependencyInjectionInstanceProvider()
        {
            DbConnectionString = ConfigurationManager.ConnectionStrings["TaskTrackerModelContainer"].ConnectionString;
        }

        public DependencyInjectionInstanceProvider(Type serviceType)
        {
            _serviceType = serviceType;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return GetInstance(instanceContext, null);
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {            
            return new TaskTrackerService(new SqlRepositoryFactory().CreateRepository(DbConnectionString));            
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance) { }
    }

    public class DependencyInjectionServiceBehavior : IServiceBehavior
    {
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
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
        public TaskTrackerServiceHost(Type serviceType, params Uri[] baseAddresses) : base(serviceType, baseAddresses) { }

        protected override void OnOpening()
        {
            this.Description.Behaviors.Add(new DependencyInjectionServiceBehavior());
            base.OnOpening();
        }
    }
}
