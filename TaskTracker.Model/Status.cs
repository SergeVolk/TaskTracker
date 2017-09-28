using System;

namespace TaskTracker.Model
{    
    /// <summary>
    /// This enumeration defines available statuses of a task.
    /// </summary>
    public enum Status : byte
    {
        Open = 0,
        InProgress = 1,
        Closed = 2
    }
}
