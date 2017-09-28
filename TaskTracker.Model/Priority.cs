using System;

namespace TaskTracker.Model
{    
    /// <summary>
    /// This enumeration defines available priorities of a task
    /// </summary>
    public enum Priority : byte
    {
        Normal = 0,
        Low = 1,        
        High = 2        
    }
}
