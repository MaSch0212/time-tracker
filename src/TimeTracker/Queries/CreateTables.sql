CREATE TABLE [activity] (
    [id]        INTEGER NOT NULL PRIMARY KEY,
    [name]      NVARCHAR(255) NOT NULL
);

CREATE TABLE [timeslot] (
    [id]        INTEGER NOT NULL PRIMARY KEY,
    [activityid]  INTEGER NOT NULL,
    [start]     DATETIME NOT NULL,
    [end]       DATETIME NULL,
    FOREIGN KEY ([activityid]) REFERENCES [activity] ([id]) ON DELETE CASCADE
)