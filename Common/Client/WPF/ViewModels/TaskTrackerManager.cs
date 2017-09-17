using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this.repository = repository;
        }

        public void StartTaskProgress(int taskId)
        {
            repository.GroupOperations(repo =>
            {
                var task = repo.FindTask(taskId, new SelectedProperties<Task>().Select(t => t.Assignee));
                if ((task == null) || (task.Status != Status.Open))
                    throw new InvalidOperationException();

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
            repository.GroupOperations(repo =>
            {
                var task = repo.FindTask(taskId, new SelectedProperties<Task>().Select(t => t.Activity));
                if ((task == null) || (task.Status != Status.InProgress))
                    throw new InvalidOperationException();

                var activity = task.Activity.LastOrDefault(a => !a.EndTime.HasValue);

                DateTime now = DateTime.Now;

                var activityDescription = (progressDescriptionProvider != null) ?
                    progressDescriptionProvider.GetDescription(task) : "";

                activity.Description = activityDescription;
                activity.EndTime = now;
                repo.Update(activity);

                repo.SetTaskStatus(taskId, Status.Open);
            });
        }

        public void CloseTask(int taskId, IDescriptionProvider taskCloseInfoProvider)
        {
            repository.GroupOperations(repo =>
            {
                var task = repo.FindTask(taskId, new SelectedProperties<Task>().Select(t => t.Activity).Select(t => t.Assignee));
                if ((task == null) || (task.Status == Status.Closed))
                    throw new InvalidOperationException();

                if (task.Status == Status.InProgress)
                {
                    DateTime now = DateTime.Now;

                    var activity = task.Activity.Last(a => !a.EndTime.HasValue);
                    var activityDescription = (taskCloseInfoProvider != null) ? taskCloseInfoProvider.GetDescription(task) : "";

                    activity.Description = activityDescription;
                    activity.EndTime = now;
                    repo.Update(activity);

                    repo.SetTaskStatus(taskId, Status.Closed);
                }
                else if (task.Status == Status.Open)
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
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });
        }

        public void ReopenTask(int taskId)
        {
            repository.GroupOperations(repo =>
            {
                var task = repo.FindTask(taskId); 
                if ((task == null) || (task.Status != Status.Closed))
                    throw new InvalidOperationException();

                repo.SetTaskStatus(taskId, Status.Open);
            });
        }

        public void AddTaskToStage(Task task, Stage stage)
        {
            stage.Task.Add(task);
            repository.AddTaskToStage(task.Id, stage.Id);
        }

        public void RemoveTaskFromStage(Task task, Stage stage)
        {
            stage.Task.Remove(task);
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
