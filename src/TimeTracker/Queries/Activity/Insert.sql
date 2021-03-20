INSERT INTO [activity] ([name])
VALUES (@name);
SELECT last_insert_rowid();