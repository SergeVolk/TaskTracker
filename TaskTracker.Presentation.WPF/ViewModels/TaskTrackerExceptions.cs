using System;

using TaskTracker.Model;

namespace TaskTracker.Presentation.WPF.ViewModels
{
    internal static class ExceptionFactory
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

        public static Exception NotSupported(Priority priority)
        {
            return new NotSupportedException(String.Format("Priority '{0}' not supported.", priority.ToString()));
        }
    }
}
