using System;

namespace TaskTracker.Repository
{    
    public abstract class RepositoryFactory
    {
        public abstract IRepositoryQueries CreateRepositoryQueries(string connectionString);
        public abstract ITransactionalRepositoryCommands CreateRepositoryCommands(string connectionString);
    }    
}
