using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskTracker.Model;

namespace TaskTracker.Client.WPF.ViewModels
{
    public static class TaskTrackerExceptions
    {
        public static Exception TaskNotFoundInRepo(int taskId)
        {
            return new InvalidOperationException(String.Format("Task '{0}' not found in repository.", taskId));
        }

        public static Exception InvalidTaskStatus(Status actual, Status expected, int taskId)
        {
            return new InvalidOperationException(String.Format("Invalid task status: '{0}'. Task '{1}' must be '{2}' to perform this operation.",
                actual.ToString(), taskId, expected.ToString()));
        }

        public static Exception TaskNotRemoved(int taskId, int stageId)
        {
            return new InvalidOperationException(String.Format("Task '{0}' cannot be removed from stage '{1}'.", taskId, stageId));
        }

        public static Exception NotSupported(Status status)
        {
            return new NotSupportedException(String.Format("Status '{0}' not supported.", status.ToString()));
        }
    }
}
