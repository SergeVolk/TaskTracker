using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using TaskTracker.ExceptionUtils;
using TaskTracker.Filters;
using TaskTracker.Model;
using TaskTracker.Presentation.WPF.Utils;
using TaskTracker.Presentation.WPF.ViewModels;
using TaskTracker.Repository;
using TaskTracker.Repository.Sql;
using TaskTracker.SyntaxUtils;

namespace TaskTracker.Presentation.WPF.Tests
{
    [TestFixture]
    public class ViewModel_WorkflowTests
    {
        public class TestData
        {
            public readonly string DefaultReporterName = "User1";
            public readonly string DefaultAssigneeName = "Admin";
            public readonly Status DefaultStatus = Status.Open;

            public readonly int[] TaskIDs = Enumerable.Range(1, 6).ToArray();
            public readonly string[] ProjectNames =
            {
                "Project 1",
                "Project 2",
                "Project 3"
            };
            public readonly string[] ProjectShortNames =
            {
                "PRJ1",
                "PRJ2",
                "PRJ3"
            };
            public readonly string[] TaskTypeNames =
            {
                "Accomplishable",
                "Continuous"
            };
            public readonly string RootStage               = "Stage #2";
            public readonly string StageWithManyTasks_Path = "Stage #2.Week #0.Wednesday";
            public readonly string StageWithOneTasks_Path  = "Stage #2.Week #0.Tuesday";
            public readonly string StageWithNoTasks_L2     = "Stage #2.Week #0";
            public readonly string StageWithNoTasks_L3     = "Stage #2.Week #0.Monday";

            public const int StageWithManyTasks_Id = 5;            
            public const int StageWithOneTasks_Id  = 4;

            public readonly double StageWithManyTasks_ActivityTime = 5.0;
            public readonly double StageWithOneTasks_ActivityTime  = 0.3;

            public readonly int StageWithManyTasks_TaskCount = 2;
            public readonly int StageWithOneTasks_TaskCount = 1;

            public readonly string[] TaskSummaries;
            public readonly string[] TaskDescripions;

            private int idBase = 1;

            public TestData()
            {
                TaskDescripions = new string[TaskIDs.Length];
                TaskSummaries = new string[TaskIDs.Length];
                for (int i = 0; i < TaskIDs.Length; i++)
                {
                    var friendlyCounter = i + 1;
                    TaskDescripions[i] = $"Do {friendlyCounter}.1 ... do {friendlyCounter}.2";
                    TaskSummaries[i] = $"DO {friendlyCounter}";
                }               

                Projects = GenerateProjects();
                Users = GenerateUsers();
                TaskTypes = GenerateTaskTypes();
                Stages = GenerateStages();
                Tasks = GenerateTasks();
            }            

            public List<Task> Tasks { get; private set; }

            public List<Project> Projects { get; private set; }

            public List<User> Users { get; private set; }

            public List<TaskType> TaskTypes { get; private set; }

            public List<Stage> Stages { get; private set; }

            public List<Activity> Activities { get; private set; }

            private int GetNextId()
            {
                return idBase++;
            }

            private void ResetId()
            {
                idBase = 1;
            }

            private Task AddTaskAndCrossLinks(int id, string summary, string desc, string prio, string assignee, string reporter, string tt, 
                string project, double? estimation, IEnumerable<Stage> stages)
            {
                ArgumentValidation.ThrowIfNullOrEmpty(reporter, nameof(reporter));
                ArgumentValidation.ThrowIfNullOrEmpty(tt, nameof(tt));
                ArgumentValidation.ThrowIfNullOrEmpty(project, nameof(project));                

                var result = new Task();
                result.Id = id;
                result.Summary = summary;
                result.Description = desc;
                result.Priority = (Priority)Enum.Parse(typeof(Priority), prio);

                result.Creator = Users.First(u => u.Name.Equals(reporter));
                result.Creator.CreatedTask.Add(result);

                result.Assignee = !String.IsNullOrEmpty(assignee) ? Users.First(u => u.Name.Equals(assignee)) : null;
                result.Assignee?.Task.Add(result);

                result.Project = Projects.First(p => p.Name.Equals(project));
                result.Project.Task.Add(result);

                result.TaskTypeId = TaskTypes.First(ttype => ttype.Name.Equals(tt)).Id;
                result.Estimation = estimation;
                result.Status = DefaultStatus;

                if (stages != null)
                {
                    foreach (var s in stages)
                    {
                        result.Stage.Add(s);
                    }                    
                    stages.ForEach(s => s.Task.Add(result));
                }

                return result;
            }

            private List<Task> GenerateTasks()
            {
                string[] assignees = 
                {
                    DefaultAssigneeName,
                    DefaultReporterName,
                    DefaultAssigneeName,
                    DefaultAssigneeName,
                    DefaultAssigneeName,
                    DefaultAssigneeName
                };

                string[] taskTypes =
                {
                    TaskTypeNames[0],
                    TaskTypeNames[1],
                    TaskTypeNames[0],
                    TaskTypeNames[0],
                    TaskTypeNames[0],
                    TaskTypeNames[0],
                };

                string[] projects =
                {
                    ProjectNames[0],
                    ProjectNames[1],
                    ProjectNames[2],
                    ProjectNames[0],
                    ProjectNames[0],
                    ProjectNames[1]
                };

                double?[] estimations = { 12, 1, 120, null, 1, 1 };

                string[] priorities =
                {
                    Priority.Normal.ToString(),
                    Priority.High.ToString(),
                    Priority.Low.ToString(),
                    Priority.High.ToString(),
                    Priority.High.ToString(),
                    Priority.Normal.ToString()
                };

                var stageWithManyTasks = Utils.FindStageById(Stages, StageWithManyTasks_Id);
                var stageWithOneTask = Utils.FindStageById(Stages, StageWithOneTasks_Id);

                IEnumerable<Stage>[] taskStages =
                {
                    new[] { stageWithManyTasks },
                    null,
                    null,
                    new[] { stageWithOneTask },
                    new[] { stageWithManyTasks },
                    null
                };

                var result = new List<Task>();
                for (int i = 0; i < TaskIDs.Length; i++)
                {                    
                    var task = AddTaskAndCrossLinks(TaskIDs[i], TaskSummaries[i], TaskDescripions[i], priorities[i], assignees[i], 
                        DefaultReporterName, taskTypes[i], projects[i], estimations[i], taskStages[i]);
                    result.Add(task);
                }      
                return result;
            }

            private List<TaskType> GenerateTaskTypes()
            {
                ResetId();
                var result = new List<TaskType>();
                for (int i = 0; i < TaskTypeNames.Length; i++)
                {
                    var tt = new TaskType
                    {
                        Id = GetNextId(),
                        Name = TaskTypeNames[i]
                    };
                    result.Add(tt);
                }
                return result;
            }

            private List<User> GenerateUsers()
            {
                ResetId();
                return new List<User>
                {
                    new User
                    {
                        Id = GetNextId(),
                        Name = DefaultReporterName
                    },
                    new User
                    {
                        Id = GetNextId(),
                        Name = DefaultAssigneeName
                    }
                };
            }

            private List<Project> GenerateProjects()
            {
                ResetId();
                var result = new List<Project>();
                for (int i = 0; i < ProjectNames.Length; i++)
                {
                    var project = new Project()
                    {
                        Id = GetNextId(),
                        Name = ProjectNames[i],
                        ShortName = ProjectShortNames[i]
                    };
                    result.Add(project);
                }
                return result;
            }

            private List<Stage> GenerateStages()
            {                
                var calendar = CultureInfo.CurrentCulture.Calendar;

                var rootStartTime = new DateTime(2017, 6, 5);
                var rootEndTime = new DateTime(2017, 8, 31);

                ResetId();

                Stage result = StageUtils.CreateTopLevelStage(RootStage);
                result.Id = GetNextId();
                result.StartTime = rootStartTime;
                result.EndTime = rootEndTime;
                                
                int weeks = (int)Math.Ceiling((result.EndTime.Value - result.StartTime.Value).TotalDays / 7f);
                for (int i = 0; i < weeks; i++)
                {
                    var weekStage = result.AddSubStage($"Week #{i}");
                    weekStage.Id = GetNextId();
                    weekStage.StartTime = result.StartTime + TimeSpan.FromDays(i * 7);

                    var weekEndTime = weekStage.StartTime + TimeSpan.FromDays(7);
                    weekStage.EndTime = weekEndTime > result.EndTime ? result.EndTime : weekEndTime;

                    int days = (int)Math.Ceiling((weekStage.EndTime.Value - weekStage.StartTime.Value).TotalDays);
                    for (int j = 0; j < days; j++)
                    {
                        var startTime = weekStage.StartTime.Value + TimeSpan.FromDays(j);
                        var dayStage = weekStage.AddSubStage(calendar.GetDayOfWeek(startTime).ToString());
                        dayStage.Id = GetNextId();
                        dayStage.StartTime = startTime;
                        dayStage.EndTime = startTime + TimeSpan.FromDays(1);
                    }
                }

                return new List<Stage> { result };
            }
        }      

        public abstract class Context
        {
            public Context()
            {
                ExpectedData = new TestData();
            }

            public abstract IRepository CreateRepository();            

            public virtual void Verify<TResult>(Expression<Func<IRepository, TResult>> expression, Times times)
            {
                // empty
            }

            public virtual void Verify(Expression<Action<IRepository>> expression, Times times)
            {
                // empty
            }

            public TestData ExpectedData { get; private set; }
        }

        public class MockedRepoContext : Context
        {
            private Mock<IRepository> mockedRepo;
            
            public MockedRepoContext()
            { }

            public override IRepository CreateRepository()
            {
                return GetMockedRepo().Object;
            }

            public override void Verify<TResult>(Expression<Func<IRepository, TResult>> expression, Times times)
            {
                GetMockedRepo().Verify(expression, times);
            }

            public override void Verify(Expression<Action<IRepository>> expression, Times times)
            {
                GetMockedRepo().Verify(expression, times);
            }

            private static void Setup_GetProjects(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.GetProjects(It.IsAny<PropertySelector<Project>>())).Returns<PropertySelector<Project>>((ps) =>
                {
                    var result = testData.Projects;

                    if (ps != null)
                    {
                        var props = ps.GetProperties();
                        foreach (var item in result)
                        {
                            var t = item.GetType();
                            t.GetProperties().Where(p => !props.Contains(p.Name)).ForEach(p => p.SetValue(item, null));
                        }
                    }

                    return result;
                });
            }

            private static void Setup_GetUsers(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.GetUsers(It.IsAny<PropertySelector<User>>())).Returns<PropertySelector<Project>>((ps) =>
                {
                    return testData.Users;
                });
            }

            private static void Setup_GetTaskTypes(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.GetTaskTypes()).Returns(() =>
                {
                    return testData.TaskTypes;
                });
            }

            private static void Setup_GetStages(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.GetStages(It.IsAny<int>(), It.IsAny<PropertySelector<Stage>>(), It.IsAny<bool>())).
                    Returns<int, PropertySelector<Stage>, bool>((level, ps, selectionScope) =>
                {
                    return testData.Stages;
                });
            }

            private static void Setup_GetTasks(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.GetTasks(It.IsAny<TaskFilter>(), It.IsAny<PropertySelector<Task>>())).
                    Returns<TaskFilter, PropertySelector<Task>>((filter, ps) =>
                {
                    IEnumerable<Task> result = testData.Tasks;

                    if (filter != null)
                    {
                        if (filter.Priorities != null)
                            result = result.Where(t => filter.Priorities.Contains(t.Priority.ToString()));

                        if (filter.Statuses != null)
                            result = result.Where(t => filter.Statuses.Contains(t.Status.ToString()));

                        if (filter.Projects != null)
                            result = result.Where(t => filter.Projects.Contains(t.Project.Name));
                    }

                    return result;
                });
            }

            private static void Setup_FindStage(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.FindStage(It.IsAny<int>(), It.IsAny<PropertySelector<Stage>>())).
                    Returns<int, PropertySelector<Stage>>((id, ps) =>
                {
                    return Utils.FindStageById(testData.Stages, id);
                });
            }

            private static void Setup_FindTask(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.FindTask(It.IsAny<int>(), It.IsAny<PropertySelector<Task>>())).
                    Returns<int, PropertySelector<Task>>((id, ps) =>
                {
                    return testData.Tasks.Find(t => t.Id == id);
                });
            }

            private static void Setup_FindTaskType(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.FindTaskType(It.IsAny<int>())).Returns<int>((id) =>
                {
                    return testData.TaskTypes.Find(tt => tt.Id == id);
                });
            }

            private static void Setup_GroupOperations(Mock<IRepository> repo)
            {
                repo.Setup(r => r.GroupOperations(It.IsAny<RepositoryOperations>())).Callback<RepositoryOperations>((op) =>
                {
                    op(repo.Object);
                });
            }

            private static void Setup_GetStagesWithMaxActivities(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.GetStagesWithMaxActivities(It.IsAny<int>(), It.IsAny<PropertySelector<Stage>>())).
                    Returns<int, PropertySelector<Stage>>((stageLimit, ps) =>
                {
                    var result = new List<Tuple<Stage, int>>();

                    var stage = Utils.FindStageById(testData.Stages, TestData.StageWithManyTasks_Id);
                    result.Add(new Tuple<Stage, int>(stage, 123 /* not used in tests */));

                    stage = Utils.FindStageById(testData.Stages, TestData.StageWithOneTasks_Id);
                    result.Add(new Tuple<Stage, int>(stage, 122 /* not used in tests */));

                    return result;
                });
            }

            private static void Setup_GetTotalActivityTimeOfStage(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.GetTotalActivityTimeOfStage(It.IsAny<int>())).Returns<int>((stageId) =>
                {
                    switch (stageId)
                    {
                        case TestData.StageWithManyTasks_Id: return testData.StageWithManyTasks_ActivityTime;
                        case TestData.StageWithOneTasks_Id:  return testData.StageWithOneTasks_ActivityTime;
                        default:
                            return 0;
                    }
                });
            }

            private static void Setup_GetStagesWithMaxTasks(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.GetStagesWithMaxTasks(It.IsAny<int>(), It.IsAny<PropertySelector<Stage>>())).
                    Returns<int, PropertySelector<Stage>>((stageLimit, ps) =>
                {
                    var result = new List<Tuple<Stage, int>>();

                    var stage = Utils.FindStageById(testData.Stages, TestData.StageWithManyTasks_Id);
                    result.Add(new Tuple<Stage, int>(stage, stage.Task.Count));

                    stage = Utils.FindStageById(testData.Stages, TestData.StageWithOneTasks_Id);
                    result.Add(new Tuple<Stage, int>(stage, stage.Task.Count));

                    return result;
                });
            }

            private static void Setup_UpdateTask(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.Update(It.IsAny<Task>())).Callback<Task>(task =>
                {
                    ArgumentValidation.ThrowIfLess(task.Id, 0, nameof(task.Id));
                    
                    var existingTask = testData.Tasks.Find(t => t.Id == task.Id);
                    if (existingTask == null)
                        throw ExceptionFactory.TaskNotFoundInRepo(task.Id);

                    existingTask.Description = task.Description;
                    existingTask.Estimation = task.Estimation;
                    existingTask.Priority = task.Priority;
                    existingTask.Status = task.Status;
                    existingTask.Summary = task.Summary;
                    existingTask.TaskTypeId = task.TaskTypeId;

                    if (existingTask.Project.Id != task.Project.Id)
                    {
                        existingTask.Project.Task.Remove(existingTask);
                        existingTask.Project = task.Project;
                        existingTask.Project.Task.Add(existingTask);
                    }

                    if (existingTask.Assignee.Id != task.Assignee.Id)
                    {
                        existingTask.Assignee.Task.Remove(existingTask);
                        existingTask.Assignee = task.Assignee;
                        existingTask.Assignee.Task.Add(existingTask);
                    }

                    if (existingTask.Creator.Id != task.Creator.Id)
                    {
                        existingTask.Creator.Task.Remove(existingTask);
                        existingTask.Creator = task.Creator;
                        existingTask.Creator.Task.Add(existingTask);
                    }

                    if (existingTask.Stage.SequenceEqual(task.Stage))
                    {
                        existingTask.Stage.Clear();
                        foreach (var s in task.Stage)
                        {
                            existingTask.Stage.Add(s);
                        }                        
                    }

                    if (existingTask.Activity.SequenceEqual(task.Activity))
                    {
                        existingTask.Activity.Clear();
                        foreach (var a in task.Activity)
                        {
                            existingTask.Activity.Add(a);
                        }                        
                    }
                });
            }

            private static void Setup_AddTaskToStage(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.AddTaskToStage(It.IsAny<int>(), It.IsAny<int>())).Callback<int, int>((taskId, stageId) =>
                {
                    var task = testData.Tasks.Find(t => t.Id == taskId);
                    if (task == null)
                        throw ExceptionFactory.TaskNotFoundInRepo(taskId);

                    var stage = Utils.FindStageById(testData.Stages, stageId);
                    if (stage == null)
                        throw Utils.StageNotFoundException(stageId);                    

                    task.Stage.Add(stage);
                    stage.Task.Add(task);
                });
            }

            private static void Setup_RemoveTaskFromStage(Mock<IRepository> repo, TestData testData)
            {
                repo.Setup(r => r.RemoveTaskFromStage(It.IsAny<int>(), It.IsAny<int>())).Callback<int, int>((taskId, stageId) =>
                {
                    var task = testData.Tasks.Find(t => t.Id == taskId);
                    if (task == null)
                        throw ExceptionFactory.TaskNotFoundInRepo(taskId);

                    var stage = Utils.FindStageById(testData.Stages, stageId);
                    if (stage == null)
                        throw Utils.StageNotFoundException(stageId);

                    task.Stage.Remove(stage);
                    stage.Task.Remove(task);
                });
            }

            private static void Setup_AddTask(Mock<IRepository> repo)
            {
                repo.Setup(r => r.Add(It.IsAny<Task>())).Callback<Task>(t =>
                {
                    throw new NotImplementedException("Add(Task) is not mocked.");
                });
            }

            private static void Setup_AddActivity(Mock<IRepository> repo)
            {
                repo.Setup(r => r.Add(It.IsAny<Activity>())).Callback<Activity>(a =>
                {
                    throw new NotImplementedException("Add(Activity) is not mocked.");
                });
            }

            private static void Setup_AddStage(Mock<IRepository> repo)
            {
                repo.Setup(r => r.Add(It.IsAny<Stage>())).Callback<Stage>(s =>
                {
                    throw new NotImplementedException("Add(Stage) is not mocked.");
                });
            }

            private static void Setup_UpdateActivity(Mock<IRepository> repo)
            {
                repo.Setup(r => r.Update(It.IsAny<Activity>())).Callback<Activity>(a =>
                {
                    throw new NotImplementedException("Update(Activity) is not mocked.");
                });
            }

            private static void Setup_UpdateStage(Mock<IRepository> repo)
            {
                repo.Setup(r => r.Update(It.IsAny<Stage>())).Callback<Stage>(a =>
                {
                    throw new NotImplementedException("Update(Stage) is not mocked.");
                });
            }

            private static void Setup_SetTaskStatus(Mock<IRepository> repo)
            {
                repo.Setup(r => r.SetTaskStatus(It.IsAny<int>(), It.IsAny<Status>())).Callback<int, Status>((taskId, newStatus) =>
                {
                    throw new NotImplementedException("SetTaskStatus is not mocked.");
                });
            }

            private Mock<IRepository> GetMockedRepo()
            {                
                if (mockedRepo != null)
                    return mockedRepo;

                mockedRepo = new Mock<IRepository>();

                Setup_GetProjects(mockedRepo, ExpectedData);
                Setup_GetUsers(mockedRepo, ExpectedData);
                Setup_GetTaskTypes(mockedRepo, ExpectedData);
                Setup_GetStages(mockedRepo, ExpectedData);
                Setup_GetTasks(mockedRepo, ExpectedData);
                Setup_FindStage(mockedRepo, ExpectedData);
                Setup_FindTask(mockedRepo, ExpectedData);
                Setup_FindTaskType(mockedRepo, ExpectedData);
                Setup_GroupOperations(mockedRepo);
                Setup_GetStagesWithMaxActivities(mockedRepo, ExpectedData);
                Setup_GetStagesWithMaxTasks(mockedRepo, ExpectedData);
                Setup_GetTotalActivityTimeOfStage(mockedRepo, ExpectedData);
                Setup_UpdateTask(mockedRepo, ExpectedData);
                Setup_AddTaskToStage(mockedRepo, ExpectedData);
                Setup_RemoveTaskFromStage(mockedRepo, ExpectedData);

                // Not Implemented
                Setup_AddTask(mockedRepo);
                Setup_AddActivity(mockedRepo);
                Setup_AddStage(mockedRepo);
                Setup_UpdateActivity(mockedRepo);
                Setup_UpdateStage(mockedRepo);
                Setup_SetTaskStatus(mockedRepo);

                return mockedRepo;
            }
        }

        public class SqlRepoContext : Context
        {
            public override IRepository CreateRepository()
            {
                var dbConnectionString = ConfigurationManager.ConnectionStrings["TaskTrackerDB"].ConnectionString;
                return new SqlRepositoryFactory(true).CreateRepository(dbConnectionString);
            }
        }

        private class Utils
        {
            public static Exception StageNotFoundException(int stageId)
            {
                return new InvalidOperationException($"Stage '{stageId.ToString()}' not found in repository.");
            }

            public static Exception StageNotFoundException(string stagePath)
            {
                return new InvalidOperationException($"Stage '{stagePath}' not found in repository.");
            }

            public static Stage FindStageById(IEnumerable<Stage> stages, int stageId)
            {
                var result = FindFirst(stages, s => s.Id == stageId);
                if (result == null)
                    throw StageNotFoundException(stageId);
                return result;
            }

            private static Stage FindFirst(IEnumerable<Stage> stages, Predicate<Stage> predicate)
            {
                Stage result = null;
                stages.ForEach(s =>
                {
                    result = FindFirst(s, stageToCheck => predicate(stageToCheck));
                    if (result != null)
                        return;
                });
                return result;
            }

            private static Stage FindFirst(Stage root, Predicate<Stage> predicate)
            {
                bool found = predicate(root);
                if (found)
                    return root;

                foreach (var item in root.SubStages)
                {
                    var result = FindFirst(item, predicate);
                    if (result != null)
                        return result;
                }

                return null;
            }
        }

        private class UIService : IUIService
        {
            public delegate bool ShowWindowDelegate(object dataContext);

            public bool ShowInputDialog(string message, out string input)
            {
                throw new NotImplementedException();
            }

            public bool ShowTaskCreationWindow(object dataContext)
            {
                throw new NotImplementedException();
            }

            public bool ShowTaskEditorWindow(object dataContext)
            {
                var handler = ShowTaskEditorWindowHandler;
                return handler != null ? handler(dataContext) : false;
            }

            public ShowWindowDelegate ShowTaskEditorWindowHandler { get; set; }
        }
        
        private Type lastRepositoryFactoryType;
        private Context lastFactory;

        public ViewModel_WorkflowTests()
        { }

        [TestCase(typeof(MockedRepoContext))]
        [TestCase(typeof(SqlRepoContext))]
        public void TasksFiltering_ChangeFilters_CheckDisplayedAndSelectedTasks(Type ctxType)
        {
            var ctx = GetContext(ctxType);

            var mainWindowVM = new MainWindowViewModel(new UIService(), ctx.CreateRepository());

            ctx.Verify(r => r.GetTasks(It.IsAny<TaskFilter>(), It.IsAny<PropertySelector<Task>>()), Times.AtLeastOnce());
            ctx.Verify(r => r.GetProjects(It.IsAny<PropertySelector<Project>>()), Times.AtLeastOnce());
            ctx.Verify(r => r.GetStages(It.IsAny<int>(), It.IsAny<PropertySelector<Stage>>(), It.IsAny<bool>()), Times.AtLeastOnce());
            ctx.Verify(r => r.GetUsers(It.IsAny<PropertySelector<User>>()), Times.AtLeastOnce());            

            mainWindowVM.PriorityFilterVM.SetSelection(true);
            mainWindowVM.ProjectFilterVM.SetSelection(true);
            mainWindowVM.StatusFilterVM.SetSelection(true);

            ctx.Verify(r => r.GetTasks(It.IsAny<TaskFilter>(), It.IsAny<PropertySelector<Task>>()), Times.AtLeastOnce());

            var actualTaskSummaries = mainWindowVM.TaskViewerViewModels.Select(taskVM => taskVM.Summary).ToList();
            CollectionAssert.AreEquivalent(ctx.ExpectedData.TaskSummaries, actualTaskSummaries);

            var actualTaskDescriptions = mainWindowVM.TaskViewerViewModels.Select(taskVM => taskVM.Description).ToList();
            CollectionAssert.AreEquivalent(ctx.ExpectedData.TaskDescripions, actualTaskDescriptions);

            Assert.AreEqual(mainWindowVM.SelectedTask.TaskId, ctx.ExpectedData.TaskIDs[4]);

            mainWindowVM.PriorityFilterVM.SetSelection(false, new[] { Priority.High.ToString() });
            
            ctx.Verify(r => r.GetTasks(It.IsAny<TaskFilter>(), It.IsAny<PropertySelector<Task>>()), Times.AtLeastOnce());

            var actualTaskIds = mainWindowVM.TaskViewerViewModels.Select(taskVM => taskVM.TaskId).ToList();
            CollectionAssert.AreEquivalent(
                new[] 
                {
                    ctx.ExpectedData.TaskIDs[5],
                    ctx.ExpectedData.TaskIDs[2],
                    ctx.ExpectedData.TaskIDs[0]
                }, 
                actualTaskIds);

            Assert.AreEqual(mainWindowVM.SelectedTask.TaskId, ctx.ExpectedData.TaskIDs[5]);

            mainWindowVM.ProjectFilterVM.SetSelection(false, new[] 
            {
                ctx.ExpectedData.ProjectNames[1],
                ctx.ExpectedData.ProjectNames[2]
            });

            ctx.Verify(r => r.GetTasks(It.IsAny<TaskFilter>(), It.IsAny<PropertySelector<Task>>()), Times.AtLeastOnce());

            actualTaskIds = mainWindowVM.TaskViewerViewModels.Select(taskVM => taskVM.TaskId).ToList();
            CollectionAssert.AreEqual(new[] { ctx.ExpectedData.TaskIDs[0] }, actualTaskIds);

            Assert.AreEqual(mainWindowVM.SelectedTask.TaskId, ctx.ExpectedData.TaskIDs[0]);        
        }

        [TestCase(typeof(MockedRepoContext))]
        [TestCase(typeof(SqlRepoContext))]
        public void TasksScheduling_CheckTasksAssignedToStages_AssignRemoveAdditionalTasks(Type ctxType)
        {
            var ctx = GetContext(ctxType);

            var mainWindowVM = new MainWindowViewModel(new UIService(), ctx.CreateRepository());
            var taskStageEditor = mainWindowVM.TaskStageEditorVM;

            var actualTasks = taskStageEditor.AllTasks.Select(t => t.Task.Id);
            CollectionAssert.AreEqual(ctx.ExpectedData.TaskIDs, actualTasks);

            Assert.True(
                AreEquivalent(taskStageEditor.TopLevelStagesVM, ctx.ExpectedData.Stages), 
                "View Model of Stages do not correspond to Stages.");
            Assert.IsNull(taskStageEditor.SelectedStageVM);

            var s = FindStageByPath(taskStageEditor.TopLevelStagesVM, ctx.ExpectedData.StageWithOneTasks_Path);
            CollectionAssert.AreEqual(new[] { ctx.ExpectedData.TaskIDs[3] }, s.StageTasks.Select(t => t.Task.Id));
            s.IsSelected = true;
            CollectionAssert.AreEqual(
                new[] { ctx.ExpectedData.TaskIDs[3] }, 
                taskStageEditor.SelectedStageVM.StageTasks.Select(t => t.Task.Id));    

            s = FindStageByPath(taskStageEditor.TopLevelStagesVM, ctx.ExpectedData.StageWithManyTasks_Path);
            CollectionAssert.AreEqual(
                new[] { ctx.ExpectedData.TaskIDs[0], ctx.ExpectedData.TaskIDs[4] }, 
                s.StageTasks.Select(t => t.Task.Id));
            s.IsSelected = true;
            CollectionAssert.AreEqual(
                new[] { ctx.ExpectedData.TaskIDs[0], ctx.ExpectedData.TaskIDs[4] }, 
                taskStageEditor.SelectedStageVM.StageTasks.Select(t => t.Task.Id));

            SelectStageAndCheckOldDeselect(ctx.ExpectedData.StageWithNoTasks_L3, taskStageEditor);
            SelectStageAndCheckOldDeselect(ctx.ExpectedData.StageWithNoTasks_L2, taskStageEditor);
            SelectStageAndCheckOldDeselect(ctx.ExpectedData.RootStage, taskStageEditor);

            //
            s = FindStageByPath(taskStageEditor.TopLevelStagesVM, ctx.ExpectedData.StageWithNoTasks_L3);            
            s.IsSelected = true;

            var taskVM = taskStageEditor.AllTasks.Where(t => t.Task.Id == ctx.ExpectedData.TaskIDs[5]).Single();
            taskStageEditor.SelectedTask = taskVM;

            taskStageEditor.AddTaskCommand.Execute(null);

            ctx.Verify(r => r.AddTaskToStage(It.IsAny<int>(), It.IsAny<int>()), Times.Once());

            CollectionAssert.AreEqual(
                new[] { ctx.ExpectedData.TaskIDs[5] }, 
                taskStageEditor.SelectedStageVM.StageTasks.Select(t => t.Task.Id));
            CollectionAssert.AreEqual(
                new[] { ctx.ExpectedData.TaskIDs[5] }, 
                s.StageTasks.Select(t => t.Task.Id));

            s.SelectedStageTask = taskVM;

            taskStageEditor.RemoveTaskCommand.Execute(null);

            ctx.Verify(r => r.RemoveTaskFromStage(It.IsAny<int>(), It.IsAny<int>()), Times.Once());

            Assert.IsNull(s.SelectedStageTask);
            CollectionAssert.IsEmpty(taskStageEditor.SelectedStageVM.StageTasks);            
        }

        [TestCase(typeof(MockedRepoContext))]
        [TestCase(typeof(SqlRepoContext))]
        public void Reports_RequestReportsAndCheckData(Type ctxType)
        {
            var ctx = GetContext(ctxType);

            var mainWindowVM = new MainWindowViewModel(new UIService(), ctx.CreateRepository());
            var reports = mainWindowVM.ReportsVM;

            //
            var allStages = mainWindowVM.TaskStageEditorVM.TopLevelStagesVM;
            var maxActivitiesStage = FindStageByPath(allStages, ctx.ExpectedData.StageWithManyTasks_Path);
            Assert.AreEqual(maxActivitiesStage.StageId, reports.MaxActivitiesStageReportVM.Stage.Id);
            
            ctx.Verify(r => r.GetStagesWithMaxActivities(It.IsAny<int>(), It.IsAny<PropertySelector<Stage>>()), Times.Once());

            var maxTasksStages = reports.MaxTasksStagesReportVM.Stages;
            var stages = maxTasksStages.Select(s => s.Stage.Id);
            CollectionAssert.AreEqual(
                new[]
                {
                    FindStageByPath(allStages, ctx.ExpectedData.StageWithManyTasks_Path).StageId,
                    FindStageByPath(allStages, ctx.ExpectedData.StageWithOneTasks_Path).StageId
                },
                stages);
            var tasks = maxTasksStages.Select(s => s.TaskCount);
            CollectionAssert.AreEqual(
                new[] { ctx.ExpectedData.StageWithManyTasks_TaskCount, ctx.ExpectedData.StageWithOneTasks_TaskCount }, 
                tasks);
            
            ctx.Verify(r => r.GetStagesWithMaxTasks(It.IsAny<int>(), It.IsAny<PropertySelector<Stage>>()), Times.Once());

            //
            CollectionAssert.IsNotEmpty(reports.TotalActivitiesTimeOfStageReportVM.TopLevelStages);

            var sId = FindStageByPath(allStages, ctx.ExpectedData.StageWithNoTasks_L3).StageId;
            reports.TotalActivitiesTimeOfStageReportVM.StageSelectedCommand.Execute(sId);
            Assert.AreEqual(0, reports.TotalActivitiesTimeOfStageReportVM.TotalStageTime);
            
            sId = FindStageByPath(allStages, ctx.ExpectedData.StageWithManyTasks_Path).StageId;
            reports.TotalActivitiesTimeOfStageReportVM.StageSelectedCommand.Execute(sId);
            Assert.AreEqual(
                (int)ctx.ExpectedData.StageWithManyTasks_ActivityTime, 
                (int)reports.TotalActivitiesTimeOfStageReportVM.TotalStageTime);
            
            sId = FindStageByPath(allStages, ctx.ExpectedData.StageWithOneTasks_Path).StageId;
            reports.TotalActivitiesTimeOfStageReportVM.StageSelectedCommand.Execute(sId);
            Assert.AreEqual(
                ctx.ExpectedData.StageWithOneTasks_ActivityTime, 
                reports.TotalActivitiesTimeOfStageReportVM.TotalStageTime);

            ctx.Verify(r => r.GetTotalActivityTimeOfStage(It.IsAny<int>()), Times.Exactly(3));
        }

        [TestCase(typeof(MockedRepoContext))]
        [TestCase(typeof(SqlRepoContext))]
        public void Tasks_EditTask(Type ctxType)
        {
            var ctx = GetContext(ctxType);

            var uiService = new UIService();
            var mainWindowVM = new MainWindowViewModel(uiService, ctx.CreateRepository());
            mainWindowVM.ShowAllTasksCommand.Execute(null);

            ctx.Verify(r => r.GetTasks(It.IsAny<TaskFilter>(), It.IsAny<PropertySelector<Task>>()), Times.AtLeastOnce());

            var actualTaskSummaries = mainWindowVM.TaskViewerViewModels.Select(taskVM => taskVM.Summary).ToList();
            CollectionAssert.AreEquivalent(ctx.ExpectedData.TaskSummaries, actualTaskSummaries);

            mainWindowVM.SelectedTask = mainWindowVM.TaskViewerViewModels.First();

            var orgDesc = mainWindowVM.SelectedTask.Description;
            var newDesc = orgDesc + "...";

            var orgSummary = mainWindowVM.SelectedTask.Summary;
            var newSummary = orgSummary + "...";

            var orgEstimation = mainWindowVM.SelectedTask.Estimation;
            var newEstimation = "123";

            var orgPriority = mainWindowVM.SelectedTask.Priority;
            var newPriority = Enum.GetNames(typeof(Priority)).Where(p => p != orgPriority.ToString()).First();

            //
            uiService.ShowTaskEditorWindowHandler = (dataContext) =>
            {
                var taskEditor = dataContext as TaskEditorViewModel;
                if (taskEditor == null)
                    return false;

                taskEditor.Description = newDesc;
                taskEditor.Summary = newSummary;
                taskEditor.Estimation = newEstimation;
                taskEditor.SelectedPriority = newPriority;
                return true;
            };

            mainWindowVM.SelectedTask.EditTaskCommand.Execute(null);
            
            Assert.AreEqual(newDesc, mainWindowVM.SelectedTask.Description);
            Assert.AreEqual(newSummary, mainWindowVM.SelectedTask.Summary);
            Assert.AreEqual(newEstimation, mainWindowVM.SelectedTask.Estimation);
            Assert.AreEqual(newPriority, mainWindowVM.SelectedTask.Priority.ToString());

            ctx.Verify(r => r.Update(It.IsAny<Task>()), Times.Once());

            //
            uiService.ShowTaskEditorWindowHandler = (dataContext) =>
            {
                var taskEditor = dataContext as TaskEditorViewModel;
                if (taskEditor == null)
                    return false;

                taskEditor.Description = orgDesc;
                taskEditor.Summary = orgSummary;
                taskEditor.Estimation = orgEstimation;
                taskEditor.SelectedPriority = orgPriority.ToString();
                return true;
            };

            mainWindowVM.SelectedTask.EditTaskCommand.Execute(null);

            Assert.AreEqual(orgDesc, mainWindowVM.SelectedTask.Description);
            Assert.AreEqual(orgSummary, mainWindowVM.SelectedTask.Summary);
            Assert.AreEqual(orgEstimation, mainWindowVM.SelectedTask.Estimation);
            Assert.AreEqual(orgPriority, mainWindowVM.SelectedTask.Priority);

            ctx.Verify(r => r.Update(It.IsAny<Task>()), Times.Exactly(2));
        }

        private Context GetContext(Type repositoryFactoryType)
        {
            if (lastRepositoryFactoryType != repositoryFactoryType)
            {
                lastFactory = Activator.CreateInstance(repositoryFactoryType) as Context;
                if (lastFactory == null)
                    throw new InvalidOperationException(
                        $"Test expects a parameter of type '{typeof(Context).Name}' instead of '{repositoryFactoryType.Name}'.");
                lastRepositoryFactoryType = repositoryFactoryType;
            }
            return lastFactory;
        }

        private StageViewModel FindStageByPath(IEnumerable<StageViewModel> stages, string stagePath)
        {
            StageViewModel result = null;
            IEnumerable<StageViewModel> currentStages = stages;
            foreach (var name in stagePath.Split('.'))
            {
                result = FindStage(currentStages, name);
                if (result == null)
                    break;
                currentStages = result.SubStagesVM;
            }
            if (result == null)
                throw Utils.StageNotFoundException(stagePath);
            return result;
        }

        private StageViewModel FindStage(IEnumerable<StageViewModel> stages, string name)
        {
            if (stages == null)
                return null;
            
            StageViewModel result = null;            
            foreach (var s in stages)
            {
                if (s.Name.Equals(name))
                {
                    result = s;
                    break;
                }
            }
            return result;
        }

        private bool AreEquivalent(IEnumerable<StageViewModel> stageVM, IEnumerable<Stage> stage)
        {            
            var stageList = stage.ToArray();
            var stageVMList = stageVM.ToArray();
            bool result = stageList.Length == stageVMList.Length;
            if (result)
            {
                for (int i = 0; i < stageVMList.Length; i++)
                {
                    var vmItem = stageVMList[i];
                    var sItem = stageList[i];
                    result = vmItem.Name.Equals(sItem.Name) && AreEquivalent(vmItem.SubStagesVM, sItem.SubStages);
                    if (!result)
                        break;
                }
            }
            return result;
        }

        private void SelectStageAndCheckOldDeselect(string stagePath, StageTasksEditorViewModel editor)
        {
            var s = FindStageByPath(editor.TopLevelStagesVM, stagePath);
            CollectionAssert.IsEmpty(s.StageTasks);
            s.IsSelected = true;
            CollectionAssert.IsEmpty(editor.SelectedStageVM.StageTasks);
        }
    }
}
