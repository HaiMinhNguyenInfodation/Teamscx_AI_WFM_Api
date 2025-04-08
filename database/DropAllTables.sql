-- First drop all foreign key constraints
DECLARE @sql NVARCHAR(MAX) = '';

-- Generate drop constraint statements for all foreign keys
SELECT @sql = @sql + 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
    ' DROP CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
FROM sys.foreign_keys;

-- Execute the generated SQL
EXEC sp_executesql @sql;

-- Then drop all tables
DROP TABLE IF EXISTS AgentStatusHistories;
DROP TABLE IF EXISTS AgentActiveHistories;
DROP TABLE IF EXISTS TeamSchedulingGroups;
DROP TABLE IF EXISTS TeamAgents;
DROP TABLE IF EXISTS QueueTeams;
DROP TABLE IF EXISTS ScheduleShifts;
DROP TABLE IF EXISTS SchedulingGroups;
DROP TABLE IF EXISTS Agents;
DROP TABLE IF EXISTS Teams;
DROP TABLE IF EXISTS Queues; 