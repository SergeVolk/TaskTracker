using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

using TaskTracker.Model;
using TaskTracker.Repository;

namespace TaskTracker
{
    public class TaskTrackerManager //: IDisposable
    {
        private IRepository repository;

        /*void IDisposable.Dispose()
        {
            ctx.Dispose();
            ctx = null;
        }*/

        public TaskTrackerManager(string connectionString) 
        {
            repository = new RepositoryFactory().CreateRepository(connectionString);
        }

        public void StartTaskProgress(int taskId)
        {
            repository.GroupOperations(repo =>
            {
                var task = repo.FindTask(taskId);
                if ((task == null) || (task.Status != Status.Open))
                    throw new InvalidOperationException();

                var activity = new Activity
                {
                    Task = task,
                    User = task.Assignee,
                    StartTime = DateTime.Now
                };
                repo.Add(activity);

                task.Status = Status.InProgress;
                repo.Update(task);                
            });
        }

        public void StopTaskProgress(int taskId, IDescriptionProvider progressDescriptionProvider)
        {
            repository.GroupOperations(repo =>
            {
                var task = repo.FindTask(taskId);
                if ((task == null) || (task.Status != Status.InProgress))
                    throw new InvalidOperationException();

                var activity = task.Activity.LastOrDefault(a => !a.EndTime.HasValue);

                DateTime now = DateTime.Now;

                var activityDescription = (progressDescriptionProvider != null) ? progressDescriptionProvider.GetDescription(task) : "";

                activity.Description = activityDescription;
                activity.EndTime = now;
                repo.Update(activity);

                task.Status = Status.Open;
                repo.Update(task);
            });
        }

        public void CloseTask(int taskId, IDescriptionProvider taskCloseInfoProvider)
        {
            repository.GroupOperations(repo =>
            {
                var task = repo.FindTask(taskId);
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

                    task.Status = Status.Closed;
                    repo.Update(task);
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

                    task.Status = Status.Closed;
                    repo.Update(task);
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

                task.Status = Status.Open;
                repo.Update(task);
            });
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
