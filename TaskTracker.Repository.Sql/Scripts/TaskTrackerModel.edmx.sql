SET QUOTED_IDENTIFIER OFF;
GO
USE [TaskTrackerDB1];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_TaskProjects]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TaskSet] DROP CONSTRAINT [FK_TaskProjects];
GO
IF OBJECT_ID(N'[dbo].[FK_ActivityTask]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ActivitySet] DROP CONSTRAINT [FK_ActivityTask];
GO
IF OBJECT_ID(N'[dbo].[FK_TaskUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TaskSet] DROP CONSTRAINT [FK_TaskUser];
GO
IF OBJECT_ID(N'[dbo].[FK_UserTask]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TaskSet] DROP CONSTRAINT [FK_UserTask];
GO
IF OBJECT_ID(N'[dbo].[FK_ActivityUser]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ActivitySet] DROP CONSTRAINT [FK_ActivityUser];
GO
IF OBJECT_ID(N'[dbo].[FK_StageTask_Stage]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StageTask] DROP CONSTRAINT [FK_StageTask_Stage];
GO
IF OBJECT_ID(N'[dbo].[FK_StageTask_Task]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StageTask] DROP CONSTRAINT [FK_StageTask_Task];
GO
IF OBJECT_ID(N'[dbo].[FK_StageStage]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[StageSet] DROP CONSTRAINT [FK_StageStage];
GO
IF OBJECT_ID(N'[dbo].[FK_TaskTaskType]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TaskSet] DROP CONSTRAINT [FK_TaskTaskType];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[TaskSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TaskSet];
GO
IF OBJECT_ID(N'[dbo].[ActivitySet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ActivitySet];
GO
IF OBJECT_ID(N'[dbo].[ProjectSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ProjectSet];
GO
IF OBJECT_ID(N'[dbo].[UserSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserSet];
GO
IF OBJECT_ID(N'[dbo].[TaskTypeSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TaskTypeSet];
GO
IF OBJECT_ID(N'[dbo].[StageSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StageSet];
GO
IF OBJECT_ID(N'[dbo].[StageTask]', 'U') IS NOT NULL
    DROP TABLE [dbo].[StageTask];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'TaskSet'
CREATE TABLE [dbo].[TaskSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Summary] nvarchar(max)  NOT NULL,
    [Description] nvarchar(max)  NOT NULL,
    [Priority] tinyint  NOT NULL,
    [Estimation] float  NULL,
    [Status] tinyint  NOT NULL,
    [TaskTypeId] int  NOT NULL,
    [Project_Id] int  NOT NULL,
    [Creator_Id] int  NOT NULL,
    [Assignee_Id] int  NOT NULL
);
GO

-- Creating table 'ActivitySet'
CREATE TABLE [dbo].[ActivitySet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [StartTime] datetime  NOT NULL,
    [EndTime] datetime  NULL,
    [Description] nvarchar(max)  NULL,
    [Task_Id] int  NOT NULL,
    [User_Id] int  NOT NULL
);
GO

-- Creating table 'ProjectSet'
CREATE TABLE [dbo].[ProjectSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [ShortName] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'UserSet'
CREATE TABLE [dbo].[UserSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'TaskTypeSet'
CREATE TABLE [dbo].[TaskTypeSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'StageSet'
CREATE TABLE [dbo].[StageSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [StartTime] datetime  NULL,
    [EndTime] datetime  NULL,
    [Level] int  NOT NULL,
    [Description] nvarchar(max)  NULL,
    [Name] nvarchar(max)  NOT NULL,
    [ParentStage_Id] int  NULL
);
GO

-- Creating table 'StageTask'
CREATE TABLE [dbo].[StageTask] (
    [Stage_Id] int  NOT NULL,
    [Task_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'TaskSet'
ALTER TABLE [dbo].[TaskSet]
ADD CONSTRAINT [PK_TaskSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ActivitySet'
ALTER TABLE [dbo].[ActivitySet]
ADD CONSTRAINT [PK_ActivitySet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ProjectSet'
ALTER TABLE [dbo].[ProjectSet]
ADD CONSTRAINT [PK_ProjectSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserSet'
ALTER TABLE [dbo].[UserSet]
ADD CONSTRAINT [PK_UserSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TaskTypeSet'
ALTER TABLE [dbo].[TaskTypeSet]
ADD CONSTRAINT [PK_TaskTypeSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'StageSet'
ALTER TABLE [dbo].[StageSet]
ADD CONSTRAINT [PK_StageSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Stage_Id], [Task_Id] in table 'StageTask'
ALTER TABLE [dbo].[StageTask]
ADD CONSTRAINT [PK_StageTask]
    PRIMARY KEY CLUSTERED ([Stage_Id], [Task_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [Project_Id] in table 'TaskSet'
ALTER TABLE [dbo].[TaskSet]
ADD CONSTRAINT [FK_TaskProjects]
    FOREIGN KEY ([Project_Id])
    REFERENCES [dbo].[ProjectSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TaskProjects'
CREATE INDEX [IX_FK_TaskProjects]
ON [dbo].[TaskSet]
    ([Project_Id]);
GO

-- Creating foreign key on [Task_Id] in table 'ActivitySet'
ALTER TABLE [dbo].[ActivitySet]
ADD CONSTRAINT [FK_ActivityTask]
    FOREIGN KEY ([Task_Id])
    REFERENCES [dbo].[TaskSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ActivityTask'
CREATE INDEX [IX_FK_ActivityTask]
ON [dbo].[ActivitySet]
    ([Task_Id]);
GO

-- Creating foreign key on [Creator_Id] in table 'TaskSet'
ALTER TABLE [dbo].[TaskSet]
ADD CONSTRAINT [FK_TaskUser]
    FOREIGN KEY ([Creator_Id])
    REFERENCES [dbo].[UserSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TaskUser'
CREATE INDEX [IX_FK_TaskUser]
ON [dbo].[TaskSet]
    ([Creator_Id]);
GO

-- Creating foreign key on [Assignee_Id] in table 'TaskSet'
ALTER TABLE [dbo].[TaskSet]
ADD CONSTRAINT [FK_UserTask]
    FOREIGN KEY ([Assignee_Id])
    REFERENCES [dbo].[UserSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_UserTask'
CREATE INDEX [IX_FK_UserTask]
ON [dbo].[TaskSet]
    ([Assignee_Id]);
GO

-- Creating foreign key on [User_Id] in table 'ActivitySet'
ALTER TABLE [dbo].[ActivitySet]
ADD CONSTRAINT [FK_ActivityUser]
    FOREIGN KEY ([User_Id])
    REFERENCES [dbo].[UserSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ActivityUser'
CREATE INDEX [IX_FK_ActivityUser]
ON [dbo].[ActivitySet]
    ([User_Id]);
GO

-- Creating foreign key on [Stage_Id] in table 'StageTask'
ALTER TABLE [dbo].[StageTask]
ADD CONSTRAINT [FK_StageTask_Stage]
    FOREIGN KEY ([Stage_Id])
    REFERENCES [dbo].[StageSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Task_Id] in table 'StageTask'
ALTER TABLE [dbo].[StageTask]
ADD CONSTRAINT [FK_StageTask_Task]
    FOREIGN KEY ([Task_Id])
    REFERENCES [dbo].[TaskSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_StageTask_Task'
CREATE INDEX [IX_FK_StageTask_Task]
ON [dbo].[StageTask]
    ([Task_Id]);
GO

-- Creating foreign key on [ParentStage_Id] in table 'StageSet'
ALTER TABLE [dbo].[StageSet]
ADD CONSTRAINT [FK_StageStage]
    FOREIGN KEY ([ParentStage_Id])
    REFERENCES [dbo].[StageSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_StageStage'
CREATE INDEX [IX_FK_StageStage]
ON [dbo].[StageSet]
    ([ParentStage_Id]);
GO

-- Creating foreign key on [TaskTypeId] in table 'TaskSet'
ALTER TABLE [dbo].[TaskSet]
ADD CONSTRAINT [FK_TaskTaskType]
    FOREIGN KEY ([TaskTypeId])
    REFERENCES [dbo].[TaskTypeSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_TaskTaskType'
CREATE INDEX [IX_FK_TaskTaskType]
ON [dbo].[TaskSet]
    ([TaskTypeId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------