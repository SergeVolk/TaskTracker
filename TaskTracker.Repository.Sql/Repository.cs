using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;

using TaskTracker.Model;
using TaskTracker.ExceptionUtils;
using TaskTracker.SyntaxUtils;
using TaskTracker.Filters;

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
            return new SqlRepository(connectionString, new ModelContainerFactory(), proxyCreationEnabled);
        }
    }

    internal partial class TaskTrackerModelContainer : DbContext
    {
        public TaskTrackerModelContainer(string connectionString) : base(connectionString)
        {
            ArgumentValidation.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));
        }
    }
        
    internal interface IModelContainerFactory
    {
        TaskTrackerModelContainer CreateModelContainer(string connectionString);
    }

    internal class ModelContainerFactory : IModelContainerFactory
    {
        public TaskTrackerModelContainer CreateModelContainer(string connectionString)
        {
            return new TaskTrackerModelContainer(GetEmdAwareConnectionString(connectionString));
        }

        private static string GetEmdAwareConnectionString(string srcConnectionString)
        {
            Debug.Assert(!String.IsNullOrEmpty(srcConnectionString));

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

    internal static class DBUtils
    {        
        private static readonly string DefaultReporter = "Admin";
        private static readonly string DefaultAssignee = "User1";

        private static User defaultUser;
        private static Status defaultStatus = Status.Open;

        public static void Init(string connectionString)
        {
            Debug.Assert(!String.IsNullOrEmpty(connectionString));

            using (var ctx = new ModelContainerFactory().CreateModelContainer(connectionString))
            {
                using (var transaction = ctx.Database.BeginTransaction())
                {
                    try
                    {
                        EnsureProjectsGenerated(ctx);
                        EnsureUsersGenerated(ctx);
                        EnsureTaskTypesGenerated(ctx);
                        EnsureTasksGenerated(ctx);
                        EnsureStagesGenerated(ctx);

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }          
        }

        private static void AddTask(string summary, string desc, string prio, string assignee, string reporter, string tt, string project,
         double? estimation, TaskTrackerModelContainer ctx)
        {
            Debug.Assert(!String.IsNullOrEmpty(reporter));
            Debug.Assert(!String.IsNullOrEmpty(tt));
            Debug.Assert(!String.IsNullOrEmpty(project));

            var task = new Task()
            {
                Summary = summary,
                Description = desc,
                Priority = (Priority)Enum.Parse(typeof(Priority), prio),
                Creator = ctx.UserSet.First(u => u.Name.Equals(reporter)),
                Assignee = !String.IsNullOrEmpty(assignee) ? ctx.UserSet.First(u => u.Name.Equals(assignee)) : null,
                TaskTypeId = ctx.TaskTypeSet.First(ttype => ttype.Name.Equals(tt)).Id,
                Project = ctx.ProjectSet.First(p => p.Name.Equals(project)),
                Estimation = estimation,
                Status = defaultStatus
            };

            ctx.TaskSet.Add(task);
        }

        private static void EnsureTasksGenerated(TaskTrackerModelContainer ctx)
        {
            if (ctx.TaskSet.Any())
                return;

            AddTask("DO 1", "Do 1.1 ... do 1.2", "Normal", DefaultAssignee, DefaultReporter, "Accomplishable", "Project 1", 12, ctx);
            AddTask("DO 2", "Do 2.1 ... do 2.2", "High", DefaultReporter, DefaultReporter, "Continuous", "Project 2", 1, ctx);
            AddTask("DO 3", "Do 3.1 ... do 3.2", "Low", DefaultAssignee, DefaultReporter, "Accomplishable", "Project 3", 120, ctx);
            AddTask("DO 4", "Do 4.1 ... do 4.2", "High", DefaultAssignee, DefaultReporter, "Accomplishable", "Project 1", null, ctx);
            AddTask("DO 5", "Do 5.1 ... do 5.2", "High", DefaultAssignee, DefaultReporter, "Accomplishable", "Project 1", 1, ctx);
            AddTask("DO 6", "Do 6.1 ... do 6.2", "Normal", DefaultAssignee, DefaultReporter, "Accomplishable", "Project 2", 1, ctx);

            ctx.SaveChanges();
        }

        private static void EnsureProjectsGenerated(TaskTrackerModelContainer ctx)
        {
            if (ctx.ProjectSet.Any())
                return;

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

        private static void EnsureUsersGenerated(TaskTrackerModelContainer ctx)
        {
            if (!ctx.UserSet.Any())
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
            defaultUser = ctx.UserSet.Where(u => u.Name.Equals(DefaultReporter)).First();
        }

        private static void EnsureTaskTypesGenerated(TaskTrackerModelContainer ctx)
        {
            if (ctx.TaskTypeSet.Any())
                return;

            var taskTypes = new List<TaskType>()
            {
                new TaskType { Name = "Accomplishable" },
                new TaskType { Name = "Continuous" }
            };
            ctx.TaskTypeSet.AddRange(taskTypes);
            ctx.SaveChanges();
        }

        private static void EnsureStagesGenerated(TaskTrackerModelContainer ctx)
        {
            if (ctx.StageSet.Any())
                return;

            var calendar = CultureInfo.CurrentCulture.Calendar;

            var rootStartTime = new DateTime(2017, 6, 5);
            var rootEndTime = new DateTime(2017, 8, 31);

            var stage2 = StageUtils.CreateTopLevelStage("Stage #2");
            stage2.StartTime = rootStartTime;
            stage2.EndTime = rootEndTime;

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
                ArgumentValidation.ThrowIfNull(originalQuery, nameof(originalQuery));
                this.CurrentQuery = originalQuery;
            }            

            public void Select(string path)
            {
                CurrentQuery = CurrentQuery.Include(path);                
            }

            public IQueryable<T> CurrentQuery { get; private set; }

            public static IQueryable<T> Select(IQueryable<T> originalQuery, PropertySelector<T> propertiesToInclude)
            {
                ArgumentValidation.ThrowIfNull(originalQuery, nameof(originalQuery));
                ArgumentValidation.ThrowIfNull(propertiesToInclude, nameof(propertiesToInclude));

                var propertySelector = new PropertySelectionQueryBuilder<T>(originalQuery);
                foreach (var item in propertiesToInclude.GetProperties())
                {
                    propertySelector.Select(item);
                };
                return propertySelector.CurrentQuery;
            }
        }

        private static class RepositoryExceptions
        {
            public static Exception StageNotFound(int stageId)
            {
                return new InvalidOperationException(String.Format("Stage '{0}' not found in repository.", stageId));
            }

            public static Exception TaskNotFound(int taskId)
            {
                return new InvalidOperationException(String.Format("Task '{0}' not found in repository.", taskId));
            }
        }

        private delegate void ContextOperation(TaskTrackerModelContainer context);
        private delegate TResult ContextOperation<TResult>(TaskTrackerModelContainer context);

        private bool isInitialized = false;
        private string connectionString;
        private IModelContainerFactory modelContainerFactory;
        private TaskTrackerModelContainer context;
        private bool proxyCreationEnabled;

        private SqlRepository(TaskTrackerModelContainer context)
        {
            Debug.Assert(context != null);
            this.context = context;            
        }

        public SqlRepository(string connectionString, IModelContainerFactory modelContainerFactory, bool proxyCreationEnabled)
        {
            ArgumentValidation.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));
            ArgumentValidation.ThrowIfNull(modelContainerFactory, nameof(modelContainerFactory));

            this.connectionString = connectionString;
            this.modelContainerFactory = modelContainerFactory;
            this.proxyCreationEnabled = proxyCreationEnabled;
            Init(connectionString);
        }

        public IEnumerable<Project> GetProjects(PropertySelector<Project> propertiesToInclude = null)
        {
            return DoContextOperations(ctx =>
            {
                ctx.Configuration.ProxyCreationEnabled = proxyCreationEnabled;

                IQueryable<Project> projects = ctx.ProjectSet;
                Debug.Assert(projects != null);

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
                Debug.Assert(users != null);

                if (propertiesToInclude != null)
                    users = PropertySelectionQueryBuilder.Select(users, propertiesToInclude);
                
                return users.ToList();
            });
        }

        public IEnumerable<TaskType> GetTaskTypes()
        {
            return DoContextOperations(ctx =>
            {
                Debug.Assert(ctx.TaskTypeSet != null);
                return ctx.TaskTypeSet.ToList();
            });
        }

        public IEnumerable<Task> GetOpenTasksOfUser(int userId, PropertySelector<Task> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(userId, 0, nameof(userId));
            return DoContextOperations(ctx => ctx.GetOpenTasksOfUser(userId));            
        }

        public IEnumerable<Task> GetOpenTasksOfProject(int projectId, PropertySelector<Task> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(projectId, 0, nameof(projectId));
            return DoContextOperations(ctx => ctx.GetOpenTasksOfProject(projectId));
        }
        
        public IEnumerable<Task> GetTasks(TaskFilter filter = null, PropertySelector<Task> sel = null)
        {
            return DoContextOperations(ctx =>
            {
                IQueryable<Task> tasks = ctx.TaskSet;
                Debug.Assert(tasks != null);

                if (filter != null)
                {
                    if (filter.Statuses != null)
                    {
                        tasks = from t in tasks
                                where filter.Statuses.Contains(t.Status.ToString())
                                select t;
                    }

                    if (filter.Projects != null)
                    {
                        tasks = from t in tasks
                                where filter.Projects.Contains(t.Project.Name)
                                select t;
                    }

                    if (filter.Priorities != null)
                    {
                        tasks = from t in tasks
                                where filter.Priorities.Contains(t.Priority.ToString())
                                select t;
                    }
                }                

                return GetTasks(tasks, sel, ctx).ToList();
            });
        }

        public IEnumerable<Stage> GetStages(int level, PropertySelector<Stage> propertiesToInclude = null, bool applySelectionToEntireGraph = false)
        {
            ArgumentValidation.ThrowIfLess(level, 0, nameof(level));
            return DoContextOperations(ctx =>
            {
                return GetStages(s => s.Level == level, propertiesToInclude, applySelectionToEntireGraph, ctx).ToList();
            });
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxActivities(int stageLimit, PropertySelector<Stage> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(stageLimit, 0, nameof(stageLimit));
            return DoContextOperations(ctx =>
            {
                var maxActStages = ctx.GetStagesWithMaxActivities(stageLimit);

                IEnumerable<Tuple<Stage, int>> result = maxActStages.Select(e =>
                {
                    Debug.Assert(e.StageId.HasValue);
                    Debug.Assert(e.ActivityCount.HasValue);

                    var stageId = e.StageId.GetValueOrDefault();
                    var stage = FindStage(stageId, propertiesToInclude);
                    if (stage == null)
                        throw RepositoryExceptions.StageNotFound(stageId);

                    var actCount = e.ActivityCount.GetValueOrDefault();
                    return new Tuple<Stage, int>(stage, actCount);
                });
                return result.ToList();               
            });
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxTasks(int stageLimit, PropertySelector<Stage> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(stageLimit, 0, nameof(stageLimit));
            return DoContextOperations(ctx =>
            {
                var taskCountByStageEntries = ctx.GetStagesWithMaxTasks(stageLimit);

                IEnumerable<Tuple<Stage, int>> result = taskCountByStageEntries.Select(e =>
                {
                    Debug.Assert(e.TaskCount.HasValue);

                    var stage = FindStage(e.StageId, propertiesToInclude);
                    if (stage == null)
                        throw RepositoryExceptions.StageNotFound(e.StageId);

                    var taskCount = e.TaskCount.GetValueOrDefault();
                    return new Tuple<Stage, int>(stage, taskCount);
                });
                return result.ToList();
            });
        }

        public double GetTotalActivityTimeOfStage(int stageId)
        {
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            return DoContextOperations(ctx => 
            {
                var result = ctx.GetTotalActivitiesTimeOfStage(stageId).Single();                                
                return (double)result.GetValueOrDefault();
            });
        }

        public Task FindTask(int taskId, PropertySelector<Task> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            return DoContextOperations(ctx => GetTasks(t => t.Id == taskId, propertiesToInclude, ctx).First());
        }

        public Stage FindStage(int stageId, PropertySelector<Stage> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            return DoContextOperations(ctx => GetStages(s => s.Id == stageId, propertiesToInclude, true, ctx).First());
        }

        public TaskType FindTaskType(int taskTypeId)
        {
            ArgumentValidation.ThrowIfLess(taskTypeId, 0, nameof(taskTypeId));
            return DoContextOperations(ctx => GetTaskTypes(tt => tt.Id == taskTypeId, ctx).First());
        }

        public void Add(Task task)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            DoContextOperations(ctx => Add(task, ctx));            
        }

        public void Add(Activity activity)
        {
            ArgumentValidation.ThrowIfNull(activity, nameof(activity));
            DoContextOperations(ctx => Add(activity, ctx));            
        }

        public void Add(Stage stage)
        {
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));
            DoContextOperations(ctx => Add(stage, ctx));
        }

        public void Update(Task task)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            DoContextOperations(ctx => Update(task, ctx));            
        }

        public void Update(Activity activity)
        {
            ArgumentValidation.ThrowIfNull(activity, nameof(activity));
            DoContextOperations(ctx => Update(activity, ctx));            
        }

        public void Update(Stage stage)
        {
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));
            DoContextOperations(ctx => Update(stage, ctx));
        }

        public void SetTaskStatus(int taskId, Status newStatus)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            DoContextOperations(ctx => ctx.SetTaskStatus(taskId, (int)newStatus));
        }

        public void AddTaskToStage(int taskId, int stageId)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            DoContextOperations(ctx => UpdateTaskStageReference(taskId, stageId, true, ctx));
        }

        public void RemoveTaskFromStage(int taskId, int stageId)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            DoContextOperations(ctx => UpdateTaskStageReference(taskId, stageId, false, ctx));
        }

        public void GroupOperations(RepositoryOperations operations)
        {
            ArgumentValidation.ThrowIfNull(operations, nameof(operations));

            using (var taskTrackerContainer = CreateContext(connectionString))
            {
                operations(new SqlRepository(taskTrackerContainer));                
            }
        }

        private TaskTrackerModelContainer CreateContext(string connectionString)
        {
            return modelContainerFactory.CreateModelContainer(connectionString);
        }

        private void DoContextOperations(ContextOperation operations)
        {
            Debug.Assert(operations != null);

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
            Debug.Assert(operations != null);

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

        private IQueryable<TaskType> GetTaskTypes(Expression<Func<TaskType, bool>> taskTypeCondition, TaskTrackerModelContainer ctx)
        {
            var taskTypes = ctx.TaskTypeSet;
            Debug.Assert(taskTypes != null);

            return taskTypeCondition != null ? taskTypes.Where(taskTypeCondition) : taskTypes;
        }

        private IQueryable<Task> GetTasks(Expression<Func<Task, bool>> taskCondition, 
            PropertySelector<Task> propertiesToInclude, TaskTrackerModelContainer ctx)
        {
            Debug.Assert(ctx != null);

            IQueryable<Task> tasks = ctx.TaskSet;
            Debug.Assert(tasks != null);

            if (taskCondition != null)
                tasks = tasks.Where(taskCondition);  
				                                      
            return GetTasks(tasks, propertiesToInclude, ctx);
        }

        private IQueryable<Task> GetTasks(IQueryable<Task> tasks, PropertySelector<Task> propertiesToInclude, TaskTrackerModelContainer ctx)
        {
            Debug.Assert(tasks != null);
            Debug.Assert(ctx != null);

            ctx.Configuration.ProxyCreationEnabled = proxyCreationEnabled;
            
            if (propertiesToInclude != null)
                tasks = PropertySelectionQueryBuilder<Task>.Select(tasks, propertiesToInclude);           

            return tasks;
        }
        
        private IQueryable<Stage> GetStages(Expression<Func<Stage, bool>> stageCondition,
            PropertySelector<Stage> propertiesToInclude, bool applySelectionToEntireGraph, TaskTrackerModelContainer ctx)
        {            
            ctx.Configuration.ProxyCreationEnabled = proxyCreationEnabled;

            IQueryable<Stage> stages = ctx.StageSet;
            Debug.Assert(stages != null);

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
            Debug.Assert(task != null);
            Debug.Assert(task.Project != null);
            Debug.Assert(task.Assignee != null);
            Debug.Assert(task.Creator != null);

            ctx.ProjectSet.Attach(task.Project);
            ctx.UserSet.Attach(task.Assignee);
            ctx.UserSet.Attach(task.Creator);

            ctx.TaskSet.Add(task);
            ctx.SaveChanges();            
        }

        private void Add(Activity activity, TaskTrackerModelContainer ctx)
        {
            Debug.Assert(activity != null);
            Debug.Assert(activity.User != null);
            Debug.Assert(activity.Task != null);
            
            ctx.UserSet.Attach(activity.User);
            ctx.TaskSet.Attach(activity.Task);

            ctx.ActivitySet.Add(activity);
            ctx.SaveChanges();            
        }

        private void Add(Stage stage, TaskTrackerModelContainer ctx)
        {
            Debug.Assert(stage != null);
            Debug.Assert(ctx.TaskSet != null);
            Debug.Assert(ctx.StageSet != null);

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
            Debug.Assert(entity != null);

            ctx.Set<TEntity>().Attach(entity);
            var taskEntry = ctx.Entry(entity);
            taskEntry.State = EntityState.Modified;

            ctx.SaveChanges();
        }

        private void UpdateTaskStageReference(int taskId, int stageId, bool connect, TaskTrackerModelContainer ctx)
        {
            Debug.Assert(taskId >= 0);
            Debug.Assert(stageId >= 0);

            var task = ctx.TaskSet.Find(taskId);
            if (task == null)
                throw RepositoryExceptions.TaskNotFound(taskId);

            var stage = ctx.StageSet.Find(stageId);
            if (stage == null)
                throw RepositoryExceptions.StageNotFound(stageId);

            var tasks = stage.Task;
            if (connect)
            {
                tasks.Add(task);
            }
            else
            {
                tasks.Remove(task);
            }
            ctx.SaveChanges();
        }
      
        private void Init(string connectionString)
        {
            Debug.Assert(!String.IsNullOrEmpty(connectionString));

            if (isInitialized)
                return;

            DBUtils.Init(connectionString);                                        
        }
    }
}
