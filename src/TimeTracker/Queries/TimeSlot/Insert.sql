INSERT INTO [timeslot] ([activityid], [start], [end])
VALUES (@activityid, @start, @end);
SELECT last_insert_rowid();