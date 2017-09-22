using System;
using System.Data.Entity;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;

using TaskTracker.Model;
using TaskTracker.ExceptionUtils;
using TaskTracker.Repository.Sql.Migrations;

namespace TaskTracker.Repository.Sql
{
    internal partial class TaskTrackerDBContext : DbContext
    {
        public TaskTrackerDBContext() : this("name=TaskTrackerDB")
        { }

        public TaskTrackerDBContext(string connectionString) : base(connectionString)
        {
            ArgumentValidation.ThrowIfNullOrEmpty(connectionString, nameof(connectionString));
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<TaskTrackerDBContext, Configuration>(true));
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Project
            modelBuilder.Entity<Project>()
                .HasMany(e => e.Task)
                .WithRequired(e => e.Project)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Project>().Property(p => p.Name).IsRequired();
            modelBuilder.Entity<Project>().Property(p => p.ShortName).IsRequired();

            modelBuilder.Entity<Project>().ToTable("ProjectSet");

            // Stage
            modelBuilder.Entity<Stage>()
                .HasMany(e => e.SubStages)
                .WithOptional(e => e.ParentStage);

            modelBuilder.Entity<Stage>()
                .HasMany(e => e.Task)
                .WithMany(e => e.Stage)
                .Map(m => m.ToTable("StageTask").MapLeftKey("Stage_Id").MapRightKey("Task_Id"));

            modelBuilder.Entity<Stage>().Property(s => s.Name).IsRequired();

            modelBuilder.Entity<Stage>().ToTable("StageSet");

            // Task
            modelBuilder.Entity<Task>()
                .HasMany(e => e.Activity)
                .WithRequired(e => e.Task)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Task>().Property(t => t.Summary).IsRequired();
            modelBuilder.Entity<Task>().Property(t => t.Description).IsRequired();

            modelBuilder.Entity<Task>().ToTable("TaskSet");           

            // TaskType
            modelBuilder.Entity<TaskType>().Property(tt => tt.Name).IsRequired();
            modelBuilder.Entity<TaskType>().ToTable("TaskTypeSet");

            // User
            modelBuilder.Entity<User>()
                .HasMany(e => e.Activity)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.CreatedTask)
                .WithRequired(e => e.Creator)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Task)
                .WithRequired(e => e.Assignee)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>().Property(u => u.Name).IsRequired();

            modelBuilder.Entity<User>().ToTable("UserSet");

            // Activity
            modelBuilder.Entity<Activity>().ToTable("ActivitySet");

            // Complex types
            modelBuilder.ComplexType<TaskCountOfStage>();
            modelBuilder.ComplexType<ActivityCountOfStage>();
        }

        public virtual DbSet<Task> TaskSet { get; set; }

        public virtual DbSet<Activity> ActivitySet { get; set; }

        public virtual DbSet<Project> ProjectSet { get; set; }

        public virtual DbSet<User> UserSet { get; set; }

        public virtual DbSet<TaskType> TaskTypeSet { get; set; }

        public virtual DbSet<Stage> StageSet { get; set; }
    
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
    }
}
