using System;

namespace TaskTracker.Repository
{
    public interface ITransactionalRepositoryCommands : IRepositoryCommands
    {
        IRepositoryTransaction BeginTransaction();
    }

    public interface IRepositoryTransaction : IRepositoryCommands, IDisposable
    {
        void CommitTransaction();

        void RollbackTransaction();
    }
}
