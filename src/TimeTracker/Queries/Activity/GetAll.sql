SELECT [p].[id]
     , [p].[name]
	 , COUNT([t].[activityid]) [count]
	 , COALESCE(SUM(julianday([t].[end]) - julianday([t].[start])), 0) [timeindays]
FROM [activity] [p]
LEFT JOIN [timeslot] [t] ON [t].[activityid] = [p].[id]
GROUP BY [p].[id], [p].[name]
ORDER BY [count] DESC