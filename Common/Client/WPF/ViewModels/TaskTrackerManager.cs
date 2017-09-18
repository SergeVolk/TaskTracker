using System;
using System.Diagnostics;
using System.Linq;

using TaskTracker.Common;
using TaskTracker.Model;
using TaskTracker.Repository;

namespace TaskTracker.Client.WPF.ViewModels
{
    public class TaskTrackerManager
    {
        private IRepository repository;

        public TaskTrackerManager(IRepository repository)
        {
            ArgumentValidation.ThrowIfNull(repository, nameof(repository));
            this.repository = repository;
        }

        public void StartTaskProgress(int taskId)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));

            repository.GroupOperations(repo =>
            {
                var task = repo.FindTask(taskId, new PropertySelector<Task>().Select(t => t.Assignee));                
                if (task == null)
                    throw TaskTrackerExceptions.TaskNotFoundInRepo(taskId);
                if (task.Status != Status.Open)
                    throw TaskTrackerExceptions.InvalidTaskStatus(task.Status, Status.Open, taskId);

                var activity = new Activity
                {
                    Task = task,
                    User = task.Assignee,
                    StartTime = DateTime.Now
                };
                repo.Add(activity);
                repo.SetTaskStatus(taskId, Status.InProgress);
            });
        }

        public void StopTaskProgress(int taskId, IDescriptionProvider progressDescriptionProvider)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            ArgumentValidation.ThrowIfNull(progressDescriptionProvider, nameof(progressDescriptionProvider));

            repository.GroupOperations(repo =>
            {
                var task = repo.FindTask(taskId, new PropertySelector<Task>().Select(t => t.Activity));
                if (task == null)
                    throw TaskTrackerExceptions.TaskNotFoundInRepo(taskId);
                if (task.Status != Status.InProgress)
                    throw TaskTrackerExceptions.InvalidTaskStatus(task.Status, Status.InProgress, taskId);
                
                var activity = task.Activity.LastOrDefault(a => !a.EndTime.HasValue);

                DateTime now = DateTime.Now;

                var activityDescription = (progressDescriptionProvider != null) ? progressDescriptionProvider.GetDescription(task) : "";

                activity.Description = activityDescription;
                activity.EndTime = now;
                repo.Update(activity);

                repo.SetTaskStatus(taskId, Status.Open);
            });
        }

        public void CloseTask(int taskId, IDescriptionProvider taskCloseInfoProvider)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            ArgumentValidation.ThrowIfNull(taskCloseInfoProvider, nameof(taskCloseInfoProvider));

            repository.GroupOperations(repo =>
            {
                var task = repo.FindTask(taskId, new PropertySelector<Task>().Select(t => t.Activity).Select(t => t.Assignee));
                if (task == null)
                    throw TaskTrackerExceptions.TaskNotFoundInRepo(taskId);

                switch (task.Status)
                {
                    case Status.Open:
                    {
                        IActivityProvider activityProvider = taskCloseInfoProvider as IActivityProvider;
                        if (activityProvider != null)
                        {
                            var activity = activityProvider.GetActivity(task);
                            if (activity != null)
                            {
                                if (!activity.EndTime.HasValue)
                                    activity.EndTime = DateTime.Now;

                                repo.Add(activity);
                            }
                        }
                        repo.SetTaskStatus(taskId, Status.Closed);
                        break;
                    }                        
                    case Status.InProgress:
                    {
                        DateTime now = DateTime.Now;

                        var activity = task.Activity.Last(a => !a.EndTime.HasValue);
                        var activityDescription = (taskCloseInfoProvider != null) ? taskCloseInfoProvider.GetDescription(task) : "";

                        activity.Description = activityDescription;
                        activity.EndTime = now;
                        repo.Update(activity);

                        repo.SetTaskStatus(taskId, Status.Closed);
                        break;
                    }
                    case Status.Closed:
                        throw TaskTrackerExceptions.InvalidTaskStatus(task.Status, Status.Closed, taskId);
                    default:
                        throw TaskTrackerExceptions.NotSupported(task.Status);                        
                }                
            });
        }

        public void ReopenTask(int taskId)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));

            repository.GroupOperations(repo =>
            {
                var task = repo.FindTask(taskId);
                if (task == null)
                    throw TaskTrackerExceptions.TaskNotFoundInRepo(taskId);
                if (task.Status != Status.Closed)
                    throw TaskTrackerExceptions.InvalidTaskStatus(task.Status, Status.Closed, taskId);

                repo.SetTaskStatus(taskId, Status.Open);
            });
        }

        public void AddTaskToStage(Task task, Stage stage)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));

            stage.Task.Add(task);
            repository.AddTaskToStage(task.Id, stage.Id);
        }

        public void RemoveTaskFromStage(Task task, Stage stage)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));

            if (!stage.Task.Remove(task))
                throw TaskTrackerExceptions.TaskNotRemoved(task.Id, stage.Id);

            repository.RemoveTaskFromStage(task.Id, stage.Id);
        }
    }

    public interface IDescriptionProvider
    {
        string GetDescription(Task task);
    }

    public interface IActivityProvider
    {
        Activity GetActivity(Task task);
    }
}
