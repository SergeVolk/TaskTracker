using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Globalization;
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

        IEnumerable<Task> GetTasks(Expression<Func<Task, bool>> taskCondition = null);

        IEnumerable<Task> GetOpenTasksOfUser(int userId);

        IEnumerable<Task> GetOpenTasksOfProject(int projectId);

        IEnumerable<Stage> GetStages(int level);

        Task FindTask(int taskId);

        Stage FindStage(int stageId);

        TaskType FindTaskType(int taskTypeId);

        void Add(Task task);

        void Add(Activity activity);

        void Add(Stage stage);

        void Update(Task task);

        void Update(Activity acvtivity);

        void Update(Stage stage);

        void SetTaskStatus(int taskId, Status newStatus);

        void AddTaskToStage(int taskId, int stageId);
        void RemoveTaskFromStage(int taskId, int stageId);

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
            return DoContextOperations(ctx => ctx.ProjectSet.ToList());
        }

        public IEnumerable<User> GetUsers()
        {
            return DoContextOperations(ctx => ctx.UserSet.ToList());
        }

        public IEnumerable<TaskType> GetTaskTypes()
        {
            return DoContextOperations(ctx => ctx.TaskTypeSet.ToList());
        }

        public IEnumerable<Task> GetOpenTasksOfUser(int userId)
        {
            return DoContextOperations(ctx => ctx.GetOpenTasksOfUser(userId));
        }

        public IEnumerable<Task> GetOpenTasksOfProject(int projectId)
        {
            return DoContextOperations(ctx => ctx.GetOpenTasksOfProject(projectId));
        }

        public IEnumerable<Task> GetTasks(Expression<Func<Task, bool>> taskCondition = null)
        {
            return DoContextOperations(ctx => GetTasks(taskCondition, ctx).ToList());
        }

        public IEnumerable<Stage> GetStages(int level)
        {
            return DoContextOperations(ctx => GetStages(s => s.Level == level, ctx).ToList());
        }

        public Task FindTask(int taskId)
        {
            return DoContextOperations(ctx => GetTasks(t => t.Id == taskId, ctx).First());
        }

        public Stage FindStage(int stageId)
        {
            return DoContextOperations(ctx => GetStages(s => s.Id == stageId, ctx).First());
        }
        public TaskType FindTaskType(int taskTypeId)
        {
            return DoContextOperations(ctx => GetTaskTypes(tt => tt.Id == taskTypeId, ctx).First());
        }

        public void Add(Task task)
        {
            DoContextOperations(ctx => Add(task, ctx));            
        }

        public void Add(Activity activity)
        {
            DoContextOperations(ctx => Add(activity, ctx));            
        }

        public void Add(Stage stage)
        {
            DoContextOperations(ctx => Add(stage, ctx));
        }

        public void Update(Task task)
        {
            DoContextOperations(ctx => Update(task, ctx));            
        }

        public void Update(Activity activity)
        {
            DoContextOperations(ctx => Update(activity, ctx));            
        }

        public void Update(Stage stage)
        {
            DoContextOperations(ctx => Update(stage, ctx));
        }

        public void SetTaskStatus(int taskId, Status newStatus)
        {
            DoContextOperations(ctx => ctx.SetTaskStatus(taskId, (int)newStatus));
        }

        public void AddTaskToStage(int taskId, int stageId)
        {
            DoContextOperations(ctx => UpdateTaskStageReference(taskId, stageId, true, ctx));
        }

        public void RemoveTaskFromStage(int taskId, int stageId)
        {
            DoContextOperations(ctx => UpdateTaskStageReference(taskId, stageId, false, ctx));
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

        public IQueryable<TaskType> GetTaskTypes(Expression<Func<TaskType, bool>> taskTypeCondition, TaskTrackerModelContainer ctx)
        {
            var tts = ctx.TaskTypeSet;
            return taskTypeCondition != null ? tts.Where(taskTypeCondition) : tts;
        }

        private IQueryable<Task> GetTasks(Expression<Func<Task, bool>> taskCondition, TaskTrackerModelContainer ctx)
        {
            var tasks = ctx.TaskSet;
            return (taskCondition != null ? tasks.Where(taskCondition) : tasks).Include(t => t.Stage.Select(s => s.Task));
        }

        private IQueryable<Stage> GetStages(Expression<Func<Stage, bool>> stageCondition, TaskTrackerModelContainer ctx)
        {
            var stages = ctx.StageSet;            
            var q = stageCondition != null ? stages.Where(stageCondition) : stages;
            return q.Include(s => s.Task.Select(t => t.Stage));
        }

        private void Add(Task task, TaskTrackerModelContainer ctx)
        {
            ctx.ProjectSet.Attach(task.Project);
            ctx.UserSet.Attach(task.Assignee);
            ctx.UserSet.Attach(task.Creator);

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

        private void Add(Stage stage, TaskTrackerModelContainer ctx)
        {
            stage.ForAll(
                s => 
                {
                    foreach (var task in s.Task)
                    {
                        ctx.TaskSet.Attach(task);
                    }

                    ctx.StageSet.Add(s);
                });

            ctx.SaveChanges();
        }

        private void Update<TEntity>(TEntity entity, TaskTrackerModelContainer ctx) 
            where TEntity : class
        {
            ctx.Set<TEntity>().Attach(entity);
            var taskEntry = ctx.Entry(entity);
            taskEntry.State = EntityState.Modified;

            ctx.SaveChanges();
        }

        private void UpdateTaskStageReference(int taskId, int stageId, bool connect, TaskTrackerModelContainer ctx)
        {
            var task = ctx.TaskSet.Find(taskId);
            var stage = ctx.StageSet.Find(stageId);

            if (connect)
            {
                stage.Task.Add(task);
            }
            else
            {
                stage.Task.Remove(task);
            }
            ctx.SaveChanges();
        }

        /*private void UpdateStage(Stage stage, TaskTrackerModelContainer ctx)
        {
            ctx.StageSet.Attach(stage);

            var newTasks = new List<Task>(stage.Task);

            //((IObjectContextAdapter)ctx).ObjectContext.ObjectStateManager.ChangeRelationshipState(lodging, lodging.Destination, l => l.Destination, EntityState.Added);
            //var mgr = ((IObjectContextAdapter)ctx).ObjectContext.ObjectStateManager.GetRelationshipManager(stage);
            //var ends = mgr.GetAllRelatedEnds();            

            //mgr = ((IObjectContextAdapter)ctx).ObjectContext.ObjectStateManager.GetRelationshipManager(stage);
            //ends = mgr.GetAllRelatedEnds();

            var stateMgr = ((IObjectContextAdapter)ctx).ObjectContext.ObjectStateManager;
            foreach (var t in newTasks)
            {
                stateMgr.ChangeRelationshipState(stage, t, s => s.Task, EntityState.Deleted);
            }
            //stage.Task.Clear();

            ((IObjectContextAdapter)ctx).ObjectContext.Refresh(RefreshMode.StoreWins, stage.Task);

            var dbTasks = stage.Task.ToList();

            var addedTasks = newTasks.Except(dbTasks, new TaskComparer()).ToList();
            var deletedTasks = dbTasks.Except(newTasks, new TaskComparer()).ToList();

            deletedTasks.ForEach(t =>
            {
                stateMgr.ChangeRelationshipState(stage, t, s => s.Task, EntityState.Deleted);
                //stage.Task.Remove(t);
            });

            addedTasks.ForEach(t =>
            {
                stateMgr.ChangeRelationshipState(stage, t, s => s.Task, EntityState.Added);
                //stage.Task.Add(t);
            });

            ctx.SaveChanges();



            //ctx.Entry(dbStage).CurrentValues.SetValues(stage);

            //addedTasks.ForEach(t => dbStage.Task.Add(t));
            //deletedTasks.ForEach(t => dbStage.Task.Remove(t));

            //            ctx.SaveChanges();

            //stage.Task.Add(task);
            //ctx.TaskSet.Attach(task);
            //ctx.StageSet.Attach(stage);
            //ctx.SaveChanges();
        }*/

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
                TaskTypeId = ctx.TaskTypeSet.FirstOrDefault(ttype => ttype.Name == tt).Id,
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
                var projects = new List<Project>()
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
        }

        private static void EnsureUsersGenerated(TaskTrackerModelContainer ctx)
        {
            if (ctx.UserSet.Count() == 0)
            {
                defaultUser = new User { Name = "Admin" };
                var users = new List<User>()
                {
                    defaultUser,
                    new User() { Name = "Serge" }
                };
                ctx.UserSet.AddRange(users);
                ctx.SaveChanges();
            }
            defaultUser = ctx.UserSet.Where(u => u.Name == "Admin").FirstOrDefault(); 
        }

        private static void EnsureTaskTypesGenerated(TaskTrackerModelContainer ctx)
        {
            if (ctx.TaskTypeSet.Count() == 0)
            {
                var taskTypes = new List<TaskType>()
                {
                    new TaskType { Name = "Accomplishable" },
                    new TaskType { Name = "Continuous" }
                };
                ctx.TaskTypeSet.AddRange(taskTypes);
                ctx.SaveChanges();
            }           
        }

        private static void EnsureStagesGenerated(TaskTrackerModelContainer ctx)
        {
            if (ctx.StageSet.Count() != 0)
                return;
            
            var calendar = CultureInfo.CurrentCulture.Calendar;

            var stage2 = Stage.CreateTopLevelStage("Stage #2");
            stage2.StartTime = new DateTime(2017, 6, 5);
            stage2.EndTime = new DateTime(2017, 8, 31);
            
            int weeks = (int)Math.Ceiling((stage2.EndTime.Value - stage2.StartTime.Value).TotalDays / 7f);
            for (int i = 0; i < weeks; i++)
            {
                var weekStage = stage2.AddSubStage($"Week #{i}");
                weekStage.StartTime = stage2.StartTime + TimeSpan.FromDays(i * 7);

                var weekEndTime = weekStage.StartTime + TimeSpan.FromDays(7);
                weekStage.EndTime = weekEndTime > stage2.EndTime ? stage2.EndTime : weekEndTime;

                int days = (int)Math.Ceiling((weekStage.EndTime.Value - weekStage.StartTime.Value).TotalDays);
                for (int j = 0; j < days; j++)
                {
                    var startTime = weekStage.StartTime.Value + TimeSpan.FromDays(j);
                    var dayStage = weekStage.AddSubStage(calendar.GetDayOfWeek(startTime).ToString());
                    dayStage.StartTime = startTime;
                    dayStage.EndTime = startTime + TimeSpan.FromDays(1);
                }
            }            
            stage2.ForAll(s => ctx.StageSet.Add(s));
            ctx.SaveChanges();            
        }

        private static void Init(string connectionString)
        {
            if (isInitialized)
                return;

            using (var taskTrackerContainer = CreateContext(connectionString))
            {
                using (var transaction = taskTrackerContainer.Database.BeginTransaction())
                {
                    try
                    {
                        EnsureProjectsGenerated(taskTrackerContainer);
                        EnsureUsersGenerated(taskTrackerContainer);
                        EnsureTaskTypesGenerated(taskTrackerContainer);
                        EnsureTasksGenerated(taskTrackerContainer);
                        EnsureStagesGenerated(taskTrackerContainer);

                        transaction.Commit();

                        isInitialized = true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
                }
            }            
        }
    }

    class TaskComparer : IEqualityComparer<Task>
    {
        public bool Equals(Task x, Task y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Task obj)
        {
            return obj.GetHashCode();
        }
    }

}
