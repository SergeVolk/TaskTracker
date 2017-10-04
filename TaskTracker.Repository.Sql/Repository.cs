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
using TaskTracker.Model.Utils;
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
        private Dictionary<string, SqlRepository> repositories;

        public SqlRepositoryFactory(bool proxyCreationEnabled)
        {
            this.proxyCreationEnabled = proxyCreationEnabled;
            this.repositories = new Dictionary<string, SqlRepository>();
        }

        public IITaskTrackerRepositoryManager CreateManager(string connectionString)
        {
            return GetRepo(connectionString) as IITaskTrackerRepositoryManager;
        }

        public override IRepositoryQueries CreateRepositoryQueries(string connectionString)
        {
            return GetRepo(connectionString) as IRepositoryQueries;
        }

        public override ITransactionalRepositoryCommands CreateRepositoryCommands(string connectionString)
        {
            return GetRepo(connectionString) as ITransactionalRepositoryCommands;
        }

        private SqlRepository GetRepo(string connectionString)
        {
            SqlRepository result;
            if (!repositories.TryGetValue(connectionString, out result))
            {
                result = new SqlRepository(connectionString, new TaskTrackerDBContextFactory(), proxyCreationEnabled);
                repositories.Add(connectionString, result);
            }
            return result;
        }
    }

    public interface IITaskTrackerRepositoryManager
    {
        void CreateIfNotExists();
    }

    internal interface ITaskTrackerDBContextFactory
    {
        TaskTrackerDBContext CreateDBContext(string connectionString);
    }

    internal class TaskTrackerDBContextFactory : ITaskTrackerDBContextFactory
    {
        public TaskTrackerDBContext CreateDBContext(string connectionString)
        {
            return new TaskTrackerDBContext(connectionString);
        }
    }

    internal class SqlRepository : IRepositoryQueries, IRepositoryCommands, ITransactionalRepositoryCommands, 
                                   IRepositoryTransaction, IITaskTrackerRepositoryManager
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
                return new InvalidOperationException($"Stage '{stageId}' not found in repository.");
            }

            public static Exception TaskNotFound(int taskId)
            {
                return new InvalidOperationException($"Task '{taskId}' not found in repository.");
            }

            public static Exception TransactionAlreadyStarted()
            {
                return new InvalidOperationException("Transaction is already started.");
            }

            public static Exception TransactionNotStarted()
            {
                return new InvalidOperationException("Transaction is not started yet.");
            }
        }

        private delegate void ContextOperation(TaskTrackerDBContext context);
        private delegate TResult ContextOperation<TResult>(TaskTrackerDBContext context);

        private string connectionString;
        private ITaskTrackerDBContextFactory modelContainerFactory;
        private TaskTrackerDBContext context;
        private DbContextTransaction transaction;
        private bool proxyCreationEnabled;

        private SqlRepository(TaskTrackerDBContext context)
        {
            Debug.Assert(context != null);
            this.context = context;            
        }

        public SqlRepository(string connectionString, ITaskTrackerDBContextFactory modelContainerFactory, bool proxyCreationEnabled)
        {
            ArgumentValidation.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));
            ArgumentValidation.ThrowIfNull(modelContainerFactory, nameof(modelContainerFactory));

            this.connectionString = connectionString;
            this.modelContainerFactory = modelContainerFactory;
            this.proxyCreationEnabled = proxyCreationEnabled;            
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

        public void Add(IEnumerable<Project> projects)
        {
            ArgumentValidation.ThrowIfNull(projects, nameof(projects));
            DoContextOperations(ctx => Add(projects, ctx));
        }

        public void Add(IEnumerable<User> users)
        {
            ArgumentValidation.ThrowIfNull(users, nameof(users));
            DoContextOperations(ctx => Add(users, ctx));
        }

        public void Add(IEnumerable<TaskType> taskTypes)
        {
            ArgumentValidation.ThrowIfNull(taskTypes, nameof(taskTypes));
            DoContextOperations(ctx => Add(taskTypes, ctx));
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
       
        public IRepositoryTransaction BeginTransaction()
        {
            if (transaction != null)
                throw RepositoryExceptions.TransactionAlreadyStarted();

            Debug.Assert(context == null);

            context = CreateContext(connectionString);
            transaction = context.Database.BeginTransaction();            
            return this as IRepositoryTransaction;
        }

        public void CommitTransaction()
        {
            if (transaction == null)
                throw RepositoryExceptions.TransactionNotStarted();            

            transaction.Commit();
            ReleaseTransaction();
        }

        public void RollbackTransaction()
        {
            if (transaction == null)
                throw RepositoryExceptions.TransactionNotStarted();

            transaction.Rollback();
            ReleaseTransaction();
        }

        // IDisposable
        public void Dispose()
        {
            if (transaction != null)
                RollbackTransaction();            
        }

        // IITaskTrackerRepositoryManager
        public void CreateIfNotExists()
        {
            using (var ctx = CreateContext(connectionString))
            { }
        }

        private void ReleaseTransaction()
        {
            Debug.Assert(transaction != null);
            Debug.Assert(context != null);

            transaction.Dispose();
            transaction = null;

            context.Dispose();
            context = null;
        }

        private TaskTrackerDBContext CreateContext(string connectionString)
        {
            return modelContainerFactory.CreateDBContext(connectionString);
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

        private IQueryable<TaskType> GetTaskTypes(Expression<Func<TaskType, bool>> taskTypeCondition, TaskTrackerDBContext ctx)
        {
            var taskTypes = ctx.TaskTypeSet;
            Debug.Assert(taskTypes != null);

            return taskTypeCondition != null ? taskTypes.Where(taskTypeCondition) : taskTypes;
        }

        private IQueryable<Task> GetTasks(Expression<Func<Task, bool>> taskCondition, 
            PropertySelector<Task> propertiesToInclude, TaskTrackerDBContext ctx)
        {
            Debug.Assert(ctx != null);

            IQueryable<Task> tasks = ctx.TaskSet;
            Debug.Assert(tasks != null);

            if (taskCondition != null)
                tasks = tasks.Where(taskCondition);  
				                                      
            return GetTasks(tasks, propertiesToInclude, ctx);
        }

        private IQueryable<Task> GetTasks(IQueryable<Task> tasks, PropertySelector<Task> propertiesToInclude, TaskTrackerDBContext ctx)
        {
            Debug.Assert(tasks != null);
            Debug.Assert(ctx != null);

            ctx.Configuration.ProxyCreationEnabled = proxyCreationEnabled;
            
            if (propertiesToInclude != null)
                tasks = PropertySelectionQueryBuilder<Task>.Select(tasks, propertiesToInclude);           

            return tasks;
        }
        
        private IQueryable<Stage> GetStages(Expression<Func<Stage, bool>> stageCondition,
            PropertySelector<Stage> propertiesToInclude, bool applySelectionToEntireGraph, TaskTrackerDBContext ctx)
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

        private void Add(Task task, TaskTrackerDBContext ctx)
        {
            Debug.Assert(task != null);
            Debug.Assert(task.Project != null);
            Debug.Assert(task.Assignee != null);
            Debug.Assert(task.Creator != null);

            ctx.ProjectSet.Attach(task.Project);
            ctx.UserSet.Attach(task.Assignee);
            ctx.UserSet.Attach(task.Creator);
            
            task.Stage.ForEach(s =>
            {
                s.VisitAll(ss => ctx.StageSet.Attach(ss));
            });

            ctx.TaskSet.Add(task);
            ctx.SaveChanges();            
        }

        private void Add(IEnumerable<Project> projects, TaskTrackerDBContext ctx)
        {
            Debug.Assert(projects != null);

            ctx.ProjectSet.AddRange(projects);
            ctx.SaveChanges();
        }

        private void Add(IEnumerable<User> users, TaskTrackerDBContext ctx)
        {
            Debug.Assert(users != null);

            ctx.UserSet.AddRange(users);
            ctx.SaveChanges();
        }

        private void Add(IEnumerable<TaskType> taskTypes, TaskTrackerDBContext ctx)
        {
            Debug.Assert(taskTypes != null);

            ctx.TaskTypeSet.AddRange(taskTypes);
            ctx.SaveChanges();
        }

        private void Add(Activity activity, TaskTrackerDBContext ctx)
        {
            Debug.Assert(activity != null);
            Debug.Assert(activity.User != null);
            Debug.Assert(activity.Task != null);
            
            ctx.UserSet.Attach(activity.User);
            ctx.TaskSet.Attach(activity.Task);

            ctx.ActivitySet.Add(activity);
            ctx.SaveChanges();            
        }

        private void Add(Stage stage, TaskTrackerDBContext ctx)
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

        private void Update<TEntity>(TEntity entity, TaskTrackerDBContext ctx) 
            where TEntity : class
        {
            Debug.Assert(entity != null);

            ctx.Set<TEntity>().Attach(entity);
            var taskEntry = ctx.Entry(entity);
            taskEntry.State = EntityState.Modified;

            ctx.SaveChanges();
        }

        private void UpdateTaskStageReference(int taskId, int stageId, bool connect, TaskTrackerDBContext ctx)
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
    }
}
