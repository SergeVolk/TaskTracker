using System;
using System.Linq;

using TaskTracker.Model;
using TaskTracker.Repository;
using TaskTracker.ExceptionUtils;
using TaskTracker.SyntaxUtils;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal class TaskTrackerController
    {
        private IRepositoryQueries repositoryQueries;
        private ITransactionalRepositoryCommands repositoryCommands;

        public TaskTrackerController(IRepositoryQueries repositoryQueries, ITransactionalRepositoryCommands repositoryCommands)
        {
            ArgumentValidation.ThrowIfNull(repositoryQueries, nameof(repositoryQueries));
            ArgumentValidation.ThrowIfNull(repositoryCommands, nameof(repositoryCommands));

            this.repositoryQueries = repositoryQueries;
            this.repositoryCommands = repositoryCommands;
        }

        public void StartTaskProgress(int taskId)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));

            var task = repositoryQueries.FindTask(taskId, new PropertySelector<Task>().Select(t => t.Assignee));
            if (task == null)
                throw ExceptionFactory.TaskNotFoundInRepo(taskId);
            if (task.Status != Status.Open)
                throw ExceptionFactory.InvalidTaskStatus(task.Status, Status.Open, taskId);

            var activity = new Activity
            {
                Task = task,
                User = task.Assignee,
                StartTime = DateTime.Now
            };

            using (var txn = repositoryCommands.BeginTransaction())
            {
                txn.Add(activity);
                txn.SetTaskStatus(taskId, Status.InProgress);

                txn.CommitTransaction();
            }
        }

        public void StopTaskProgress(int taskId, IDescriptionProvider progressDescriptionProvider)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            ArgumentValidation.ThrowIfNull(progressDescriptionProvider, nameof(progressDescriptionProvider));

            var task = repositoryQueries.FindTask(taskId, new PropertySelector<Task>().Select(t => t.Activity));
            if (task == null)
                throw ExceptionFactory.TaskNotFoundInRepo(taskId);
            if (task.Status != Status.InProgress)
                throw ExceptionFactory.InvalidTaskStatus(task.Status, Status.InProgress, taskId);

            var activity = task.Activity.LastOrDefault(a => !a.EndTime.HasValue);

            DateTime now = DateTime.Now;

            var activityDescription = (progressDescriptionProvider != null) ? progressDescriptionProvider.GetDescription(task) : "";

            activity.Description = activityDescription;
            activity.EndTime = now;

            using (var txn = repositoryCommands.BeginTransaction())                
            {
                txn.Update(activity);
                txn.SetTaskStatus(taskId, Status.Open);

                txn.CommitTransaction();
            };
        }

        public void CloseTask(int taskId, IDescriptionProvider taskCloseInfoProvider)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            ArgumentValidation.ThrowIfNull(taskCloseInfoProvider, nameof(taskCloseInfoProvider));

            var task = repositoryQueries.FindTask(taskId, new PropertySelector<Task>().Select(t => t.Activity).Select(t => t.Assignee));
            if (task == null)
                throw ExceptionFactory.TaskNotFoundInRepo(taskId);

            Activity activity = null;

            switch (task.Status)
            {
                case Status.Open:
                {
                    IActivityProvider activityProvider = taskCloseInfoProvider as IActivityProvider;
                    if (activityProvider != null)
                    {
                        activity = activityProvider.GetActivity(task);
                        if (activity != null && !activity.EndTime.HasValue)
                            activity.EndTime = DateTime.Now;                                                            
                    }
                    break;
                }
                case Status.InProgress:
                {
                    DateTime now = DateTime.Now;

                    activity = task.Activity.Last(a => !a.EndTime.HasValue);
                    var activityDescription = (taskCloseInfoProvider != null) ? taskCloseInfoProvider.GetDescription(task) : "";

                    activity.Description = activityDescription;
                    activity.EndTime = now;

                    break;
                }
                case Status.Closed:
                    throw ExceptionFactory.InvalidTaskStatus(task.Status, Status.Closed, taskId);
                default:
                    throw ExceptionFactory.NotSupported(task.Status);
            }

            if (activity == null)
            {
                repositoryCommands.SetTaskStatus(taskId, Status.Closed);
            }
            else
            {
                using (var txn = repositoryCommands.BeginTransaction())                    
                {
                    txn.Update(activity);
                    txn.SetTaskStatus(taskId, Status.Closed);
                };
            }
        }

        public void ReopenTask(int taskId)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));

            var task = repositoryQueries.FindTask(taskId);
            if (task == null)
                throw ExceptionFactory.TaskNotFoundInRepo(taskId);
            if (task.Status != Status.Closed)
                throw ExceptionFactory.InvalidTaskStatus(task.Status, Status.Closed, taskId);

            repositoryCommands.SetTaskStatus(taskId, Status.Open);            
        }

        public void AddTaskToStage(Task task, Stage stage)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));

            stage.Task.Add(task);
            repositoryCommands.AddTaskToStage(task.Id, stage.Id);
        }

        public void RemoveTaskFromStage(Task task, Stage stage)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));

            if (!stage.Task.Remove(task))
                throw ExceptionFactory.TaskNotRemoved(task.Id, stage.Id);

            repositoryCommands.RemoveTaskFromStage(task.Id, stage.Id);
        }
    }

    internal interface IDescriptionProvider
    {
        string GetDescription(Task task);
    }

    internal interface IActivityProvider
    {
        Activity GetActivity(Task task);
    }
}
