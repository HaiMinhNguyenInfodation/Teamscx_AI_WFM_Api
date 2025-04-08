-- Create Queue Table
CREATE TABLE Queues (
    id INT IDENTITY(1,1) PRIMARY KEY,
    microsoft_queue_id NVARCHAR(100) NOT NULL,
    name NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Create Team Table
CREATE TABLE Teams (
    id INT IDENTITY(1,1) PRIMARY KEY,
    microsoft_team_id NVARCHAR(100) NOT NULL,
    display_name NVARCHAR(255) NOT NULL,
    description NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Create Agent Table
CREATE TABLE Agents (
    id INT IDENTITY(1,1) PRIMARY KEY,
    microsoft_user_id NVARCHAR(100) NOT NULL,
    display_name NVARCHAR(255) NOT NULL,
    email NVARCHAR(255),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Create SchedulingGroup Table
CREATE TABLE SchedulingGroups (
    id INT IDENTITY(1,1) PRIMARY KEY,
    microsoft_group_id NVARCHAR(100) NOT NULL,
    display_name NVARCHAR(255),
    is_active BIT DEFAULT 1,
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE()
);

-- Create ScheduleShift Table
CREATE TABLE ScheduleShifts (
    id INT IDENTITY(1,1) PRIMARY KEY,
    microsoft_shift_id NVARCHAR(100) NOT NULL,
    agent_id INT NOT NULL,
    scheduling_group_id INT NOT NULL,
    start_date_time DATETIME2 NOT NULL,
    end_date_time DATETIME2 NOT NULL,
    theme NVARCHAR(50),
    notes NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 DEFAULT GETDATE(),
    FOREIGN KEY (agent_id) REFERENCES Agent(id),
    FOREIGN KEY (scheduling_group_id) REFERENCES SchedulingGroup(id)
);

-- Create Junction Tables for Many-to-Many Relationships
CREATE TABLE QueueTeams (
    queue_id INT NOT NULL,
    team_id INT NOT NULL,
    PRIMARY KEY (queue_id, team_id),
    FOREIGN KEY (queue_id) REFERENCES Queue(id),
    FOREIGN KEY (team_id) REFERENCES Team(id)
);

CREATE TABLE TeamAgents (
    team_id INT NOT NULL,
    agent_id INT NOT NULL,
    PRIMARY KEY (team_id, agent_id),
    FOREIGN KEY (team_id) REFERENCES Teams(id),
    FOREIGN KEY (agent_id) REFERENCES Agents(id)
);

CREATE TABLE TeamSchedulingGroups (
    team_id INT NOT NULL,
    scheduling_group_id INT NOT NULL,
    PRIMARY KEY (team_id, scheduling_group_id),
    FOREIGN KEY (team_id) REFERENCES Teams(id),
    FOREIGN KEY (scheduling_group_id) REFERENCES SchedulingGroups(id)
);

CREATE TABLE AgentActiveHistories (
    Id INT  PRIMARY KEY,
    CreatedAt DATETIME NOT NULL,
    AgentId INT,
    IsActived BIT NOT NULL,
    QueueId INT,
    FOREIGN KEY (AgentId) REFERENCES Agents (Id),
    FOREIGN KEY (QueueId) REFERENCES Queues (Id)
);

CREATE TABLE AgentStatusHistories (
    Id INT PRIMARY KEY,
    CreatedAt DATETIME NOT NULL,
    AgentId INT,
    Status NVARCHAR(255) NOT NULL,
    FOREIGN KEY (AgentId) REFERENCES Agents(Id)
);

-- Create Indexes for Better Performance
CREATE INDEX IX_Queues_MicrosoftQueueId ON Queues(microsoft_queue_id);
CREATE INDEX IX_Teams_MicrosoftTeamId ON Teams(microsoft_team_id);
CREATE INDEX IX_Agents_MicrosoftUserId ON Agents(microsoft_user_id);
CREATE INDEX IX_SchedulingGroups_MicrosoftGroupId ON SchedulingGroups(microsoft_group_id);
CREATE INDEX IX_ScheduleShifts_MicrosoftShiftId ON ScheduleShifts(microsoft_shift_id);
CREATE INDEX IX_ScheduleShifts_AgentId ON ScheduleShifts(agent_id);
CREATE INDEX IX_ScheduleShifts_SchedulingGroupId ON ScheduleShifts(scheduling_group_id); 