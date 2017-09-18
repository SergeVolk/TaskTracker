using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

using TaskTracker.Common;
using TaskTracker.Model;

// Workaround: otherwise "EntityFramework.SqlServer.dll" is not copyied to client-app's "bin" along with "Repository.dll"
using SqlProviderServices = System.Data.Entity.SqlServer.SqlProviderServices;

namespace TaskTracker.Repository.Sql
{
    public class SqlRepositoryFactory : RepositoryFactory
    {
        private bool proxyCreationEnabled;

        public SqlRepositoryFactory(bool proxyCreationEnabled)
        {
            this.proxyCreationEnabled = proxyCreationEnabled;
        }

        public override IRepository CreateRepository(string connectionString)
        {
            return new SqlRepository(connectionString, proxyCreationEnabled);
        }
    }

    internal partial class TaskTrackerModelContainer : DbContext
    {
        public TaskTrackerModelContainer(string connectionString) : base(connectionString)
        { }
    }

    internal static class ConnectionStringManager
    {
        public static string GetEmdAwareConnectionString(string srcConnectionString)
        {
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

            var entityFrameworkDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDi‌​rectory, @"..\..\..\TaskTracker.EF"));

            entityBuilder.Provider = "System.Data.SqlClient"; 
            entityBuilder.ProviderConnectionString = srcConnectionString;
            entityBuilder.Metadata = $@"{entityFrameworkDir}\TaskTrackerModel.csdl|" + 
                                     $@"{entityFrameworkDir}\TaskTrackerModel.ssdl|" +
                                     $@"{entityFrameworkDir}\TaskTrackerModel.msl";
            return entityBuilder.ToString();            
        }
    }    

    internal class SqlRepository : IRepository
    {
        private class PropertySelectionQueryBuilder
        {
            public static PropertySelectionQueryBuilder<T> Create<T>(IQueryable<T> items)
            {
                return new PropertySelectionQueryBuilder<T>(items);
            }

            public static IQueryable<T> Select<T>(IQueryable<T> originalQuery, PropertySelector<T> propertiesToInclude)
            {
                return PropertySelectionQueryBuilder<T>.Select(originalQuery, propertiesToInclude);                
            }
        }

        private class PropertySelectionQueryBuilder<T> : PropertySelectionQueryBuilder 
        {
            public PropertySelectionQueryBuilder(IQueryable<T> originalQuery)
            {
                if (originalQuery == null)
                    throw new ArgumentNullException(nameof(originalQuery));

                this.CurrentQuery = originalQuery;
            }            

            public void Select(string path)
            {
                CurrentQuery = CurrentQuery.Include(path);                
            }

            public IQueryable<T> CurrentQuery { get; private set; }

            public static IQueryable<T> Select(IQueryable<T> originalQuery, PropertySelector<T> propertiesToInclude)
            {
                var propertySelector = new PropertySelectionQueryBuilder<T>(originalQuery);
                foreach (var item in propertiesToInclude.GetProperties())
                {
                    propertySelector.Select(item);
                };
                return propertySelector.CurrentQuery;
            }
        }

        private static readonly string DefaultReporter = "Admin";
        private static readonly string DefaultAssignee = "Serge";

        private delegate void ContextOperation(TaskTrackerModelContainer context);
        private delegate TResult ContextOperation<TResult>(TaskTrackerModelContainer context);

        private static bool isInitialized = false;
        private static User defaultUser;
        private static Status defaultStatus = Status.Open;

        private string connectionString;
        private TaskTrackerModelContainer context;
        private bool proxyCreationEnabled;

        private SqlRepository(TaskTrackerModelContainer context)
        {
            this.context = context;            
        }

        public SqlRepository(string connectionString, bool proxyCreationEnabled)
        {
            this.connectionString = connectionString;
            this.proxyCreationEnabled = proxyCreationEnabled;
            Init(connectionString);
        }

        public IEnumerable<Project> GetProjects(PropertySelector<Project> propertiesToInclude = null)
        {
            return DoContextOperations(ctx =>
            {
                ctx.Configuration.ProxyCreationEnabled = proxyCreationEnabled;

                IQueryable<Project> projects = ctx.ProjectSet;

                if (propertiesToInclude != null)
                    projects = PropertySelectionQueryBuilder.Select(projects, propertiesToInclude);
                
                return projects.ToList();
            });
        }

        public IEnumerable<User> GetUsers(PropertySelector<User> propertiesToInclude = null)
        {
            return DoContextOperations(ctx =>
            {
                ctx.Configuration.ProxyCreationEnabled = proxyCreationEnabled;

                IQueryable<User> users = ctx.UserSet;

                if (propertiesToInclude != null)
                    users = PropertySelectionQueryBuilder.Select(users, propertiesToInclude);
                
                return users.ToList();
            });
        }

        public IEnumerable<TaskType> GetTaskTypes()
        {
            return DoContextOperations(ctx => ctx.TaskTypeSet.ToList());
        }

        public IEnumerable<Task> GetOpenTasksOfUser(int userId, PropertySelector<Task> propertiesToInclude = null)
        {
            return DoContextOperations(ctx => ctx.GetOpenTasksOfUser(userId));            
        }

        public IEnumerable<Task> GetOpenTasksOfProject(int projectId, PropertySelector<Task> propertiesToInclude = null)
        {
            return DoContextOperations(ctx => ctx.GetOpenTasksOfProject(projectId));
        }
        
        public IEnumerable<Task> GetTasks(TaskFilter filter = null, PropertySelector<Task> sel = null)
        {
            return DoContextOperations(ctx =>
            {
                IQueryable<Task> exp = ctx.TaskSet;
                if (filter.Statuses != null)
                {
                    exp = from t in exp
                          where filter.Statuses.Contains(t.Status.ToString())
                          select t;
                }

                if (filter.Projects != null)
                {
                    exp = from t in exp
                          where filter.Projects.Contains(t.Project.Name)
                          select t;
                }

                if (filter.Priorities != null)
                {
                    exp = from t in exp
                          where filter.Priorities.Contains(t.Priority.ToString())
                          select t;
                }
                
                return GetTasks(exp, sel, ctx).ToList();
            });
        }

        public IEnumerable<Stage> GetStages(int level, PropertySelector<Stage> propertiesToInclude = null, bool applySelectionToEntireGraph = false)
        {
            return DoContextOperations(ctx =>
            {
                return GetStages(s => s.Level == level, propertiesToInclude, applySelectionToEntireGraph, ctx).ToList();
            });
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxActivities(int stageLimit, PropertySelector<Stage> propertiesToInclude = null)
        {
            return DoContextOperations(ctx =>
            {
                var maxActStages = ctx.GetStagesWithMaxActivities(stageLimit);

                IEnumerable<Tuple<Stage, int>> result = maxActStages.Select(e =>
                {
                    var stage = FindStage(e.StageId.GetValueOrDefault(), propertiesToInclude);
                    var actCount = e.ActivityCount.GetValueOrDefault();
                    return new Tuple<Stage, int>(stage, actCount);
                });
                return result.ToList();               
            });
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxTasks(int stageLimit, PropertySelector<Stage> propertiesToInclude = null)
        {
            return DoContextOperations(ctx =>
            {
                var taskCountByStageEntries = ctx.GetStagesWithMaxTasks(stageLimit);

                IEnumerable<Tuple<Stage, int>> result = taskCountByStageEntries.Select(e =>
                {
                    var stage = FindStage(e.StageId, propertiesToInclude);
                    var taskCount = e.TaskCount.GetValueOrDefault();
                    return new Tuple<Stage, int>(stage, taskCount);
                });
                return result.ToList();
            });
        }

        public double GetTotalActivityTimeOfStage(int stageId)
        {
            return DoContextOperations(ctx => 
            {
                var result = ctx.GetTotalActivitiesTimeOfStage(stageId).Single().GetValueOrDefault();
                return (double)result;
            });
        }

        public Task FindTask(int taskId, PropertySelector<Task> propertiesToInclude = null)
        {
            return DoContextOperations(ctx => GetTasks(t => t.Id == taskId, propertiesToInclude, ctx).First());
        }

        public Stage FindStage(int stageId, PropertySelector<Stage> propertiesToInclude = null)
        {
            return DoContextOperations(ctx => GetStages(s => s.Id == stageId, propertiesToInclude, true, ctx).First());
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
                operations(new SqlRepository(taskTrackerContainer));                
            }
        }

        private static TaskTrackerModelContainer CreateContext(string connectionString)
        {
            return new TaskTrackerModelContainer(ConnectionStringManager.GetEmdAwareConnectionString(connectionString));
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

        private IQueryable<Task> GetTasks(Expression<Func<Task, bool>> taskCondition, 
            PropertySelector<Task> propertiesToInclude, TaskTrackerModelContainer ctx)
        {            
            ctx.Configuration.ProxyCreationEnabled = proxyCreationEnabled;

            IQueryable<Task> taskQuery = ctx.TaskSet;
            if (taskCondition != null)
                taskQuery = taskQuery.Where(taskCondition);

            if (propertiesToInclude != null)
                taskQuery = PropertySelectionQueryBuilder.Select(taskQuery, propertiesToInclude);                                        

            var r = taskQuery.ToList();
 
            return taskQuery;
        }

        private IQueryable<Task> GetTasks(IQueryable<Task> tasks, PropertySelector<Task> propertiesToInclude, TaskTrackerModelContainer ctx)
        {
            ctx.Configuration.ProxyCreationEnabled = proxyCreationEnabled;
            
            if (propertiesToInclude != null)
                tasks = PropertySelectionQueryBuilder<Task>.Select(tasks, propertiesToInclude);
            
            var r = tasks.ToList();

            return tasks;
        }
        
        private IQueryable<Stage> GetStages(Expression<Func<Stage, bool>> stageCondition,
            PropertySelector<Stage> propertiesToInclude, bool applySelectionToEntireGraph, TaskTrackerModelContainer ctx)
        {
            ctx.Configuration.ProxyCreationEnabled = proxyCreationEnabled;

            IQueryable<Stage> stages = ctx.StageSet;             
            
            if (propertiesToInclude != null)
            {
                stages = PropertySelectionQueryBuilder.Select(stages, propertiesToInclude);

                if (applySelectionToEntireGraph)
                {
                    var requiredProps = new PropertySelector<Stage>().
                        Select(s => s.SubStages).
                        Select(s => s.ParentStage);
                    stages = PropertySelectionQueryBuilder.Select(stages, requiredProps);                    
                    stages.Load();
                }
            }

            if (stageCondition != null)
                stages = stages.Where(stageCondition);            

            return stages;
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
            stage.VisitAll(s => 
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

        private static void AddTask(string summary, string desc, string prio, string assignee, string reporter, string tt, string project, 
            double? estimation, TaskTrackerModelContainer ctx)
        {
            var task = new Task()
            {
                Summary = summary,
                Description = desc,
                Priority = (Priority)Enum.Parse(typeof(Priority), prio),
                Creator = ctx.UserSet.FirstOrDefault(u => u.Name == reporter), 
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

            AddTask("DO 1", "Do 1.1 ... do 1.2", "Normal", DefaultAssignee, DefaultReporter, "Accomplishable", "Project 1", 12,   ctx);
            AddTask("DO 2", "Do 2.1 ... do 2.2", "High",   DefaultReporter, DefaultReporter, "Continuous",     "Project 2", 1,    ctx);
            AddTask("DO 3", "Do 3.1 ... do 3.2", "Low",    DefaultAssignee, DefaultReporter, "Accomplishable", "Project 3", 120,  ctx);
            AddTask("DO 4", "Do 4.1 ... do 4.2", "High",   DefaultAssignee, DefaultReporter, "Accomplishable", "Project 1", null, ctx);
            AddTask("DO 5", "Do 5.1 ... do 5.2", "High",   DefaultAssignee, DefaultReporter, "Accomplishable", "Project 1", 1,    ctx);
            AddTask("DO 6", "Do 6.1 ... do 6.2", "Normal", DefaultAssignee, DefaultReporter, "Accomplishable", "Project 2", 1,    ctx);

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
                defaultUser = new User { Name = DefaultReporter };
                var users = new List<User>()
                {
                    defaultUser,
                    new User() { Name = DefaultAssignee }
                };
                ctx.UserSet.AddRange(users);
                ctx.SaveChanges();
            }
            defaultUser = ctx.UserSet.Where(u => u.Name == DefaultReporter).FirstOrDefault(); 
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
            stage2.VisitAll(s => ctx.StageSet.Add(s));
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
}
