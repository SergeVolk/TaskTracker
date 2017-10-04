using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

using TaskTracker.Model;
using TaskTracker.Model.Utils;
using TaskTracker.Repository;
using TaskTracker.Repository.Sql;
using TaskTracker.SyntaxUtils;

namespace TaskTracker.DBManager
{
    internal class DBInitializer
    {
        private static readonly string DefaultReporter = "Admin";
        private static readonly string DefaultAssignee = "User1";
        private static readonly Status DefaultStatus = Status.Open;
        private static readonly string notInitializedErrorTemplate = @"{0} are not initialized in the database.";

        private User defaultUser;
        private IEnumerable<User> users;
        private IEnumerable<Project> projects;
        private IEnumerable<TaskType> taskTypes;
        private IEnumerable<Task> tasks;
        private IEnumerable<Stage> stages;

        private IRepositoryQueries repoQueries;
        private IRepositoryTransaction repoCommands;

        private User DefaultUser
        {
            get
            {
                if (defaultUser == null)
                    defaultUser = Users.Where(u => u.Name.Equals(DefaultReporter)).First();

                return defaultUser;
            }
        }

        private IEnumerable<User> Users
        {
            get
            {
                if (users == null)
                    users = repoQueries.GetUsers();

                if (!users.Any())
                {
                    users = GenerateUsers();
                    repoCommands.Add(users);
                    users = repoQueries.GetUsers();
                }

                return users;
            }
        }

        private IEnumerable<Project> Projects
        {
            get
            {
                if (projects == null)
                    projects = repoQueries.GetProjects();

                if (!projects.Any())
                {
                    projects = GenerateProjects();
                    repoCommands.Add(projects);
                    projects = repoQueries.GetProjects();
                }

                return projects;
            }
        }

        private IEnumerable<TaskType> TaskTypes
        {
            get
            {
                if (taskTypes == null)
                    taskTypes = repoQueries.GetTaskTypes();

                if (!taskTypes.Any())
                {
                    taskTypes = GenerateTaskTypes();
                    repoCommands.Add(taskTypes);
                    taskTypes = repoQueries.GetTaskTypes();
                }

                return taskTypes;
            }
        }

        private IEnumerable<Task> Tasks
        {
            get
            {
                if (tasks == null)
                    tasks = repoQueries.GetTasks();

                if (!tasks.Any())
                {
                    tasks = GenerateTasks();
                    foreach (var t in tasks)
                    {
                        repoCommands.Add(t);
                    }
                    tasks = repoQueries.GetTasks();
                }

                return tasks;
            }
        }

        private IEnumerable<Stage> Stages
        {
            get
            {
                if (stages == null)
                    stages = repoQueries.GetStages(0);

                if (!stages.Any())
                {
                    stages = GenerateStages();
                    foreach (var s in stages)
                    {
                        repoCommands.Add(s);
                    }
                    stages = repoQueries.GetStages(0);
                }

                return stages;
            }
        }

        public static void CreateDB(string connectionString)
        {
            new DBInitializer().InternalCreateDB(connectionString);
        }

        public static void InitPreset(string connectionString)
        {
            new DBInitializer().InternalInitPreset(connectionString);
        }

        private void InternalCreateDB(string connectionString)
        {
            var repoFactory = new SqlRepositoryFactory(true);
            var repoMgr = repoFactory.CreateManager(connectionString);
            repoMgr.CreateIfNotExists();
        }

        private void InternalInitPreset(string connectionString)
        {
            Debug.Assert(!String.IsNullOrEmpty(connectionString));

            var repoFactory = new SqlRepositoryFactory(true);
            repoQueries = repoFactory.CreateRepositoryQueries(connectionString);
            repoCommands = repoFactory.CreateRepositoryCommands(connectionString).BeginTransaction();
            try
            {
                if (!Projects.Any())
                    throw new InvalidOperationException(String.Format(notInitializedErrorTemplate, "Projects"));

                if (!Users.Any())
                    throw new InvalidOperationException(String.Format(notInitializedErrorTemplate, "Users"));

                if (!TaskTypes.Any())
                    throw new InvalidOperationException(String.Format(notInitializedErrorTemplate, "Task Types"));

                if (!Tasks.Any())
                    throw new InvalidOperationException(String.Format(notInitializedErrorTemplate, "Tasks"));

                if (!Stages.Any())
                    throw new InvalidOperationException(String.Format(notInitializedErrorTemplate, "Stages"));

                repoCommands.CommitTransaction();

            }
            catch (Exception)
            {
                repoCommands.RollbackTransaction();
                throw;
            }
            finally
            {
                repoCommands = null;
                repoQueries = null;

                defaultUser = null;
                users = null;
                projects = null;
                taskTypes = null;
                tasks = null;
                stages = null;
            }
        }

        private void AddTask(string summary, string desc, string prio, string assignee, string reporter, string tt, string project,
            double? estimation, int[] stageIds, ICollection<Task> tasks)
        {
            Debug.Assert(!String.IsNullOrEmpty(reporter));
            Debug.Assert(!String.IsNullOrEmpty(tt));
            Debug.Assert(!String.IsNullOrEmpty(project));

            var task = new Task()
            {
                Summary = summary,
                Description = desc,
                Priority = (Priority)Enum.Parse(typeof(Priority), prio),
                Creator = Users.First(u => u.Name.Equals(reporter)),
                Assignee = !String.IsNullOrEmpty(assignee) ? Users.First(u => u.Name.Equals(assignee)) : null,
                TaskTypeId = TaskTypes.First(ttype => ttype.Name.Equals(tt)).Id,
                Project = Projects.First(p => p.Name.Equals(project)),
                Estimation = estimation,
                Status = DefaultStatus                
            };

            if (stageIds != null)
            {
                Stages.ForEach(s => 
                {
                    s.VisitAll(ss =>
                    {
                        if (stageIds.Contains(ss.Id))
                            task.Stage.Add(ss);
                    });
                });
            }

            tasks.Add(task);
        }

        private IEnumerable<Task> GenerateTasks()
        {
            var result = new List<Task>();

            AddTask("DO 1", "Do 1.1 ... do 1.2", "Normal", DefaultAssignee, DefaultReporter, "Accomplishable", "Project 1", 12,   new int[] { 5 }, result);
            AddTask("DO 2", "Do 2.1 ... do 2.2", "High",   DefaultReporter, DefaultReporter, "Continuous",     "Project 2", 1,    null,            result);
            AddTask("DO 3", "Do 3.1 ... do 3.2", "Low",    DefaultAssignee, DefaultReporter, "Accomplishable", "Project 3", 120,  null,            result);
            AddTask("DO 4", "Do 4.1 ... do 4.2", "High",   DefaultAssignee, DefaultReporter, "Accomplishable", "Project 1", null, new int[] { 4 }, result);
            AddTask("DO 5", "Do 5.1 ... do 5.2", "High",   DefaultAssignee, DefaultReporter, "Accomplishable", "Project 1", 1,    new int[] { 5 }, result);
            AddTask("DO 6", "Do 6.1 ... do 6.2", "Normal", DefaultAssignee, DefaultReporter, "Accomplishable", "Project 2", 1,    null,            result);

            return result;
        }

        private IEnumerable<Project> GenerateProjects()
        {
            return new List<Project>()
            {
                new Project()
                {
                    Name = "Project 1",
                    ShortName = "PRJ1"
                },
                new Project()
                {
                    Name = "Project 2",
                    ShortName = "PRJ2"
                },
                new Project()
                {
                    Name = "Project 3",
                    ShortName = "PRJ3"
                }
            };
        }

        private IEnumerable<User> GenerateUsers()
        {
            return new List<User>()
            {
                new User { Name = DefaultReporter },
                new User() { Name = DefaultAssignee }
            };
        }

        private IEnumerable<TaskType> GenerateTaskTypes()
        {
            return new List<TaskType>()
            {
                new TaskType { Name = "Accomplishable" },
                new TaskType { Name = "Continuous" }
            };
        }

        private IEnumerable<Stage> GenerateStages()
        {
            var calendar = CultureInfo.CurrentCulture.Calendar;

            var rootStartTime = new DateTime(2017, 6, 5);
            var rootEndTime = new DateTime(2017, 8, 31);

            var stage2 = StageUtils.CreateTopLevelStage("Stage #2");
            stage2.StartTime = rootStartTime;
            stage2.EndTime = rootEndTime;

            int weeks = (int)Math.Ceiling((stage2.EndTime.Value - stage2.StartTime.Value).TotalDays / 7f);
            for (int i = 0; i < weeks; i++)
            {
                var weekStage = stage2.AddSubStage($"Week #{i}");
                weekStage.StartTime = stage2.StartTime + TimeSpan.FromDays(i * 7);

                var weekEndTime = weekStage.StartTime + TimeSpan.FromDays(7);
                weekStage.EndTime = weekEndTime > stage2.EndTime ? stage2.EndTime : weekEndTime;

                int days = (int)Math.Ceiling((weekStage.EndTime.Value - weekStage.StartTime.Value).TotalDays);
                for (int j = 0; j < days; j++)
                {
                    var startTime = weekStage.StartTime.Value + TimeSpan.FromDays(j);
                    var dayStage = weekStage.AddSubStage(calendar.GetDayOfWeek(startTime).ToString());
                    dayStage.StartTime = startTime;
                    dayStage.EndTime = startTime + TimeSpan.FromDays(1);
                }
            }
                        
            return new List<Stage>() { stage2 };
        }
    }
}
