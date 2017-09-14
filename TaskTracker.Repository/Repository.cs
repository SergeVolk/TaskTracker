using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
//using System.Threading.Tasks;

using TaskTracker.Model;

// Workaround: otherwise "EntityFramework.SqlServer.dll" is not copyied to client-app's "bin" along with "Repository.dll"
using SqlProviderServices = System.Data.Entity.SqlServer.SqlProviderServices;

namespace TaskTracker.Repository
{
    public delegate void RepositoryOperations(IRepository repository);

    public interface IRepository
    {
        IEnumerable<User> GetUsers();

        IEnumerable<Project> GetProjects();

        IEnumerable<TaskType> GetTaskTypes();

        IQueryable<Task> GetTasks(Expression<Func<Task, bool>> taskCondition);        

        Task FindTask(int taskId);

        void Add(Task task);

        void Add(Activity activity);

        void Update(Task task);

        void Update(Activity acvtivity);

        void GroupOperations(RepositoryOperations operations);
    }

    public class RepositoryFactory
    {
        public IRepository CreateRepository(string connectionString)
        {
            return new Repository(connectionString);
        }
    }

    public partial class TaskTrackerModelContainer : DbContext
    {
        public TaskTrackerModelContainer(string connectionString) : base(connectionString)
        {
        }
    }

    internal static class ConnectionStringManager
    {
        private static readonly string EFDir = @"d:\Projects\sv-soft\Projects\TaskTracker\TaskTracker.EF";

        public static string EmdAwareConnectionString(string srcConnectionString)
        {
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

            entityBuilder.Provider = "System.Data.SqlClient"; 
            entityBuilder.ProviderConnectionString = srcConnectionString;
            entityBuilder.Metadata = $@"{EFDir}\TaskTrackerModel.csdl|" + 
                                     $@"{EFDir}\TaskTrackerModel.ssdl|" +
                                     $@"{EFDir}\TaskTrackerModel.msl";
            return entityBuilder.ToString();            
        }
    }

    internal class Repository : IRepository
    {
        private delegate void ContextOperation(TaskTrackerModelContainer context);
        private delegate TResult ContextOperation<TResult>(TaskTrackerModelContainer context);

        private static bool isInitialized = false;
        private static List<Project> projects;
        private static List<TaskType> taskTypes;
        private static List<User> users;
        private static User defaultUser;
        private static Status defaultStatus = Status.Open;

        private string connectionString;
        private TaskTrackerModelContainer context;

        private Repository(TaskTrackerModelContainer context)
        {
            this.context = context;
        }

        public Repository(string connectionString)
        {
            this.connectionString = connectionString;
            Init(connectionString);
        }

        public IEnumerable<Project> GetProjects()
        {
            return projects;
        }

        public IEnumerable<User> GetUsers()
        {
            return users;
        }

        public IEnumerable<TaskType> GetTaskTypes()
        {
            return taskTypes;
        }

        public List<Task> GetTasksAsList(Expression<Func<Task, bool>> taskCondition)
        {
            return DoContextOperations(ctx => GetTasks(taskCondition, ctx).ToList()); 
        }

        public IQueryable<Task> GetTasks(Expression<Func<Task, bool>> taskCondition)
        {
            return DoContextOperations(ctx => GetTasks(taskCondition, ctx));
        }

        public Task FindTask(int taskId)
        {
            return DoContextOperations(ctx => GetTasks(t => t.Id == taskId, ctx).First());
        }

        public void Add(Task task)
        {
            DoContextOperations(ctx => Add(task, ctx));            
        }

        public void Add(Activity activity)
        {
            DoContextOperations(ctx => Add(activity, ctx));            
        }

        public void Update(Task task)
        {
            DoContextOperations(ctx => Update(task, ctx));            
        }

        public void Update(Activity activity)
        {
            DoContextOperations(ctx => Update(activity, ctx));            
        }

        public void GroupOperations(RepositoryOperations operations)
        {
            using (var taskTrackerContainer = CreateContext(connectionString))
            {
                operations(new Repository(taskTrackerContainer));                
            }
        }

        private static TaskTrackerModelContainer CreateContext(string connectionString)
        {
            return new TaskTrackerModelContainer(ConnectionStringManager.EmdAwareConnectionString(connectionString));
        }

        private void DoContextOperations(ContextOperation operations)
        {
            if (context != null)
            {
                operations(context);
            }
            else
            {
                using (var taskTrackerContainer = CreateContext(connectionString))
                {
                    operations(taskTrackerContainer);
                }
            }
        }

        private TResult DoContextOperations<TResult>(ContextOperation<TResult> operations)
        {
            if (context != null)
            {
                return operations(context);
            }
            else
            {
                using (var taskTrackerContainer = CreateContext(connectionString))
                {
                    return operations(taskTrackerContainer);
                }
            }
        }

        private IQueryable<Task> GetTasks(Expression<Func<Task, bool>> taskCondition, TaskTrackerModelContainer ctx)
        {
            return ctx.TaskSet.Where(taskCondition).Select(t => t);
                /*from t in ctx.TaskSet
                where taskCondition
                select t;*/
        }

        private void Add(Task task, TaskTrackerModelContainer ctx)
        {
            ctx.ProjectSet.Attach(task.Project);
            ctx.UserSet.Attach(task.Assignee);
            ctx.UserSet.Attach(task.Creator);
            ctx.TaskTypeSet.Attach(task.TaskType);

            ctx.TaskSet.Add(task);
            ctx.SaveChanges();            
        }

        private void Add(Activity activity, TaskTrackerModelContainer ctx)
        {            
            ctx.UserSet.Attach(activity.User);
            ctx.TaskSet.Attach(activity.Task);

            ctx.ActivitySet.Add(activity);
            ctx.SaveChanges();            
        }

        private void Update<TEntity>(TEntity entity, TaskTrackerModelContainer ctx) 
            where TEntity : class
        {
            var taskEntry = ctx.Entry(entity);
            taskEntry.State = EntityState.Modified;

            ctx.SaveChanges();
        }        

        private static void AddTask(string summary, string desc, string prio, string assignee, string tt, string project, double? estimation,
          TaskTrackerModelContainer ctx)
        {
            var task = new Task()
            {
                Summary = summary,
                Description = desc,
                Priority = (Priority)Enum.Parse(typeof(Priority), prio),
                Creator = ctx.UserSet.FirstOrDefault(u => u.Name == "Admin"),
                Assignee = ctx.UserSet.FirstOrDefault(u => u.Name == assignee),
                TaskType = ctx.TaskTypeSet.FirstOrDefault(ttype => ttype.Name == tt),
                Project = ctx.ProjectSet.FirstOrDefault(p => p.Name == project),
                Estimation = estimation,
                Status = defaultStatus
            };

            ctx.TaskSet.Add(task);
        }       

        private static void EnsureTasksGenerated(TaskTrackerModelContainer ctx)
        {
            if (ctx.TaskSet.Count() != 0)
                return;

            AddTask("DO 1", "Do 1.1 ... do 1.2", "Normal", "Serge", "Accomplishable", "Project 1", 12, ctx);
            AddTask("DO 2", "Do 2.1 ... do 2.2", "High", "Admin", "Continuous", "Project 2", 1, ctx);
            AddTask("DO 3", "Do 3.1 ... do 3.2", "Low", "Serge", "Accomplishable", "Project 3", 120, ctx);
            AddTask("DO 4", "Do 4.1 ... do 4.2", "High", "Serge", "Accomplishable", "Project 1", null, ctx);
            AddTask("DO 5", "Do 5.1 ... do 5.2", "High", "Serge", "Accomplishable", "Project 1", 1, ctx);
            AddTask("DO 6", "Do 6.1 ... do 6.2", "Normal", "Serge", "Accomplishable", "Project 2", 1, ctx);

            ctx.SaveChanges();
        }

        private static void EnsureProjectsGenerated(TaskTrackerModelContainer ctx)
        {
            if (ctx.ProjectSet.Count() == 0)
            {
                projects = new List<Project>()
                {
                    new Project()
                    {
                        Name = "Project 1",
                        ShortName = "PRJ1"
                    },
                    new Project()
                    {
                        Name = "Project 2",
                        ShortName = "PRJ2"
                    },
                    new Project()
                    {
                        Name = "Project 3",
                        ShortName = "PRJ3"
                    }
                };
                ctx.ProjectSet.AddRange(projects);
                ctx.SaveChanges();
            }
            projects = ctx.ProjectSet.ToList();            
        }

        private static void EnsureUsersGenerated(TaskTrackerModelContainer ctx)
        {
            if (ctx.UserSet.Count() == 0)
            {
                defaultUser = new User { Name = "Admin" };
                users = new List<User>()
                {
                    defaultUser,
                    new User() { Name = "Serge" }
                };
                ctx.UserSet.AddRange(users);
                ctx.SaveChanges();
            }
            users = ctx.UserSet.ToList();
            defaultUser = users.Find(u => u.Name == "Admin");            
        }

        private static void EnsureTaskTypesGenerated(TaskTrackerModelContainer ctx)
        {
            if (ctx.TaskTypeSet.Count() == 0)
            {
                taskTypes = new List<TaskType>()
                {
                    new TaskType { Name = "Accomplishable" },
                    new TaskType { Name = "Continuous" }
                };
                ctx.TaskTypeSet.AddRange(taskTypes);
                ctx.SaveChanges();
            }
            taskTypes = ctx.TaskTypeSet.ToList();            
        }

        private static bool Cond(bool c)
        {
            return !c;
        }

        private static void Test(Predicate<Task> p, string connectionString)
        {
            using (var ctx = CreateContext(connectionString))
            {
                var q = ctx.TaskSet.AsEnumerable().Where(t => p(t)).Select(t => t);
                /*var q = from t in ctx.TaskSet
                        where p(t)
                        select t;*/

                var l = q.ToList();
            }
        }

        private static void Init(string connectionString)
        {
            //Test(t => true, connectionString);

            if (isInitialized)
                return;

            using (var taskTrackerContainer = CreateContext(connectionString))
            {
                EnsureProjectsGenerated(taskTrackerContainer);
                EnsureUsersGenerated(taskTrackerContainer);
                EnsureTaskTypesGenerated(taskTrackerContainer);
                EnsureTasksGenerated(taskTrackerContainer);
            }
            isInitialized = true;
        }
    }
}
