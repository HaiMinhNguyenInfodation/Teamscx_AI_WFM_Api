-- Create Queue Table
CREATE TABLE Queues (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MicrosoftQueueId NVARCHAR(100) NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

-- Create Team Table
CREATE TABLE Teams (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MicrosoftTeamId NVARCHAR(100) NOT NULL,
    DisplayName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

-- Create Agent Table
CREATE TABLE Agents (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MicrosoftUserId NVARCHAR(100) NOT NULL,
    DisplayName NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    IsReported BIT DEFAULT 0
);

-- Create SchedulingGroup Table
CREATE TABLE SchedulingGroups (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MicrosoftGroupId NVARCHAR(100) NOT NULL,
    DisplayName NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

-- Create ScheduleShift Table
CREATE TABLE ScheduleShifts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MicrosoftShiftId NVARCHAR(100) NOT NULL,
    AgentId INT NOT NULL,
    SchedulingGroupId INT NOT NULL,
    StartDateTime DATETIME2 NOT NULL,
    EndDateTime DATETIME2 NOT NULL,
    Theme NVARCHAR(50),
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (AgentId) REFERENCES Agents(Id),
    FOREIGN KEY (SchedulingGroupId) REFERENCES SchedulingGroups(Id)
);

-- Create Junction Tables for Many-to-Many Relationships
CREATE TABLE QueueTeams (
    QueueId INT NOT NULL,
    TeamId INT NOT NULL,
    PRIMARY KEY (QueueId, TeamId),
    FOREIGN KEY (QueueId) REFERENCES Queues(Id),
    FOREIGN KEY (TeamId) REFERENCES Teams(Id)
);

CREATE TABLE TeamAgents (
    TeamId INT NOT NULL,
    AgentId INT NOT NULL,
    PRIMARY KEY (TeamId, AgentId),
    FOREIGN KEY (TeamId) REFERENCES Teams(Id),
    FOREIGN KEY (AgentId) REFERENCES Agents(Id)
);

CREATE TABLE TeamSchedulingGroups (
    TeamId INT NOT NULL,
    SchedulingGroupId INT NOT NULL,
    PRIMARY KEY (TeamId, SchedulingGroupId),
    FOREIGN KEY (TeamId) REFERENCES Teams(Id),
    FOREIGN KEY (SchedulingGroupId) REFERENCES SchedulingGroups(Id)
);

CREATE TABLE AgentActiveHistories (
    Id INT PRIMARY KEY,
    CreatedAt DATETIME NOT NULL,
    AgentId INT,
    IsActived BIT NOT NULL,
    QueueId INT,
    FOREIGN KEY (AgentId) REFERENCES Agents(Id),
    FOREIGN KEY (QueueId) REFERENCES Queues(Id)
);

CREATE TABLE AgentStatusHistories (
    Id INT PRIMARY KEY,
    CreatedAt DATETIME NOT NULL,
    AgentId INT,
    Status INT NOT NULL,
    FOREIGN KEY (AgentId) REFERENCES Agents(Id)
);

-- Create table for tracking reported agents in queues
CREATE TABLE QueueReportedAgents (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    QueueId INT NOT NULL,
    AgentId INT NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (QueueId) REFERENCES Queues(Id),
    FOREIGN KEY (AgentId) REFERENCES Agents(Id)
);

-- Create Indexes for Better Performance
CREATE INDEX IX_Queues_MicrosoftQueueId ON Queues(MicrosoftQueueId);
CREATE INDEX IX_Teams_MicrosoftTeamId ON Teams(MicrosoftTeamId);
CREATE INDEX IX_Agents_MicrosoftUserId ON Agents(MicrosoftUserId);
CREATE INDEX IX_SchedulingGroups_MicrosoftGroupId ON SchedulingGroups(MicrosoftGroupId);
CREATE INDEX IX_ScheduleShifts_MicrosoftShiftId ON ScheduleShifts(MicrosoftShiftId);
CREATE INDEX IX_ScheduleShifts_AgentId ON ScheduleShifts(AgentId);
CREATE INDEX IX_ScheduleShifts_SchedulingGroupId ON ScheduleShifts(SchedulingGroupId);
CREATE INDEX IX_QueueReportedAgents_QueueId ON QueueReportedAgents(QueueId);
CREATE INDEX IX_QueueReportedAgents_AgentId ON QueueReportedAgents(AgentId); 