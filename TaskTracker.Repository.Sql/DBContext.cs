using System;
using System.Data.Entity;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Infrastructure;
using System.IO;

using TaskTracker.Model;
using TaskTracker.ExceptionUtils;
using TaskTracker.Repository.Sql.Migrations;

namespace TaskTracker.Repository.Sql
{
    internal partial class TaskTrackerDBContext : DbContext
    {
        #region Entity Configs
        private class ProjectConfig : EntityTypeConfiguration<Project>
        {
            public ProjectConfig()
            {
                HasMany(e => e.Task).WithRequired(e => e.Project).WillCascadeOnDelete(false);
                Property(p => p.Name).IsRequired();
                Property(p => p.ShortName).IsRequired();
                ToTable("ProjectSet");
            }
        }

        private class StageConfig : EntityTypeConfiguration<Stage>
        {
            public StageConfig()
            {
                HasMany(e => e.SubStages).WithOptional(e => e.ParentStage);
                HasMany(e => e.Task).WithMany(e => e.Stage)
                    .Map(m => m.ToTable("StageTask").MapLeftKey("Stage_Id").MapRightKey("Task_Id"));

                Property(s => s.Name).IsRequired();
                ToTable("StageSet");
            }
        }

        private class TaskConfig : EntityTypeConfiguration<Task>
        {
            public TaskConfig()
            {
                HasMany(e => e.Activity).WithRequired(e => e.Task).WillCascadeOnDelete(false);
                Property(t => t.Summary).IsRequired();
                Property(t => t.Description).IsRequired();
                ToTable("TaskSet");
            }
        }

        private class TaskTypeConfig : EntityTypeConfiguration<TaskType>
        {
            public TaskTypeConfig()
            {
                Property(tt => tt.Name).IsRequired();
                ToTable("TaskTypeSet");
            }
        }

        private class UserConfig : EntityTypeConfiguration<User>
        {
            public UserConfig()
            {
                HasMany(e => e.Activity).WithRequired(e => e.User).WillCascadeOnDelete(false);
                HasMany(e => e.CreatedTask).WithRequired(e => e.Creator).WillCascadeOnDelete(false);
                HasMany(e => e.Task).WithRequired(e => e.Assignee).WillCascadeOnDelete(false);
                Property(u => u.Name).IsRequired();
                ToTable("UserSet");
            }
        }

        private class ActivityConfig : EntityTypeConfiguration<Activity>
        {
            public ActivityConfig()
            {
                ToTable("ActivitySet");
            }
        }

        private class TaskCountOfStageConfig : ComplexTypeConfiguration<TaskCountOfStage>
        {
            public TaskCountOfStageConfig()
            { }
        }

        private class ActivityCountOfStageConfig : ComplexTypeConfiguration<ActivityCountOfStage>
        {
            public ActivityCountOfStageConfig()
            { }
        }
        #endregion

        private void ExecuteLocalScript(string scriptFile)
        {
            var scriptPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDi‌​rectory, 
                $@"..\..\..\TaskTracker.Repository.Sql\Scripts\{scriptFile}"));
            var script = File.ReadAllText(scriptPath);
            Database.ExecuteSqlCommand(script);
        }

        private void EnsureDBInitialized(string connectionString)
        {
            if (!Database.Exists(connectionString))
            {
                Database.Create();
                Database.Initialize(true);

                var adapter = (IObjectContextAdapter)this;
                var script = adapter.ObjectContext.CreateDatabaseScript();
                Database.ExecuteSqlCommand(script);

                ExecuteLocalScript("dbo.GetOpenTasksOfProject.sql");
                ExecuteLocalScript("dbo.GetOpenTasksOfUser.sql");
                ExecuteLocalScript("dbo.GetStagesWithMaxActivities.sql");
                ExecuteLocalScript("dbo.GetStagesWithMaxTasks.sql");
                ExecuteLocalScript("dbo.GetTotalActivitiesTimeOfStage.sql");
                ExecuteLocalScript("dbo.SetTaskStatus.sql");
            }
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ProjectConfig());
            modelBuilder.Configurations.Add(new StageConfig());
            modelBuilder.Configurations.Add(new TaskConfig());
            modelBuilder.Configurations.Add(new TaskTypeConfig());
            modelBuilder.Configurations.Add(new UserConfig());
            modelBuilder.Configurations.Add(new ActivityConfig());
            modelBuilder.Configurations.Add(new TaskCountOfStageConfig());
            modelBuilder.Configurations.Add(new ActivityCountOfStageConfig());

            base.OnModelCreating(modelBuilder);
        }

        public TaskTrackerDBContext() : this("name=TaskTrackerDB")
        { }

        public TaskTrackerDBContext(string connectionString) : base(connectionString)
        {
            ArgumentValidation.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));

            EnsureDBInitialized(connectionString);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TaskTrackerDBContext, Configuration>(true));
        }        

        #region Entity sets
        public virtual DbSet<Task> TaskSet { get; set; }

        public virtual DbSet<Activity> ActivitySet { get; set; }

        public virtual DbSet<Project> ProjectSet { get; set; }

        public virtual DbSet<User> UserSet { get; set; }

        public virtual DbSet<TaskType> TaskTypeSet { get; set; }

        public virtual DbSet<Stage> StageSet { get; set; } 
        #endregion

        #region Stored Procedures
        public virtual IEnumerable<Task> GetOpenTasksOfUser(int? userId)
        {
            SqlParameter userIdParam = new SqlParameter("@userId", userId);
            return Database.SqlQuery<Task>("GetOpenTasksOfProject @userId", userIdParam).ToList();
        }

        public virtual IEnumerable<Task> GetOpenTasksOfProject(int? projectId)
        {
            SqlParameter projectIdParam = new SqlParameter("@projectId", projectId);
            return Database.SqlQuery<Task>("GetOpenTasksOfProject @projectId", projectIdParam).ToList();
        }

        public virtual int SetTaskStatus(int? taskId, int? newStatus)
        {
            SqlParameter taskIdParam = new SqlParameter("@taskId", taskId);
            SqlParameter newStatusParam = new SqlParameter("@newStatus", newStatus);
            return Database.ExecuteSqlCommand("SetTaskStatus @taskId, @newStatus", taskIdParam, newStatusParam);
        }

        public virtual IEnumerable<TaskCountOfStage> GetStagesWithMaxTasks(int? stageLimit)
        {
            SqlParameter stageLimitParam = new SqlParameter("@stageLimit", stageLimit);
            return Database.SqlQuery<TaskCountOfStage>("GetStagesWithMaxTasks @stageLimit", stageLimitParam).ToList();
        }

        public virtual IEnumerable<decimal?> GetTotalActivitiesTimeOfStage(int? stageId)
        {
            SqlParameter stageIdParam = new SqlParameter("@stageId", stageId);
            return Database.SqlQuery<decimal?>("GetTotalActivitiesTimeOfStage @stageId", stageIdParam).ToList();
        }

        public virtual IEnumerable<ActivityCountOfStage> GetStagesWithMaxActivities(int? stageLimit)
        {
            SqlParameter stageLimitParam = new SqlParameter("@stageLimit", stageLimit);
            return Database.SqlQuery<ActivityCountOfStage>("GetStagesWithMaxActivities @stageLimit", stageLimitParam).ToList();
        } 
        #endregion
    }
}
