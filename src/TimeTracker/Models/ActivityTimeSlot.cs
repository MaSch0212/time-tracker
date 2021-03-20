using TimeTracker.Services;
using MaSch.Core;
using MaSch.Core.Attributes;
using MaSch.Core.Helper;
using MaSch.Core.Observable;
using MaSch.Data.Extensions;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.CompilerServices;

namespace TimeTracker.Models
{
    [ObservablePropertyDefinition]
    internal interface IActivityTimeSlot_Props
    {
        [ObservablePropertyAccessModifier(SetModifier = AccessModifier.Private)]
        DateTime? Start { get; }

        [ObservablePropertyAccessModifier(SetModifier = AccessModifier.Private)]
        DateTime? End { get; }

        [ObservablePropertyAccessModifier(SetModifier = AccessModifier.Private)]
        TimeSpan TotalTime { get; }

        [ObservablePropertyAccessModifier(SetModifier = AccessModifier.Private)]
        int SlotCount { get; }
    }

    public partial class ActivityTimeSlot : ObservableObject, IActivityTimeSlot_Props
    {
        private readonly IDatabaseService _databaseService;
        private readonly Timer _currentTimeTimer;

        public long ActivityId { get; }
        public string ActivityName { get; }
        public long? CurrentTimeSlotId { get; private set; }

        [DependsOn(nameof(Start), nameof(End))]
        public TimeSpan CurrentTime => Start.HasValue ? (End ?? DateTime.Now) - Start.Value : TimeSpan.Zero;

        [DependsOn(nameof(Start), nameof(End))]
        public bool IsRunning => Start.HasValue && !End.HasValue;

        public string ShortName => new string(ActivityName?.Split(' ').Take(3).Select(x => x[0]).ToArray()).ToUpperInvariant();

        public ActivityTimeSlot(long activityId, string activityName, int slotCount, double timeInDays)
        {
            ServiceContext.GetService(out _databaseService);

            _currentTimeTimer = new Timer(1000);
            _currentTimeTimer.Elapsed += OnCurrentTimeTimerElapsed;

            ActivityId = activityId;
            ActivityName = activityName;
            SlotCount = slotCount;
            TotalTime = TimeSpan.FromDays(timeInDays);
        }

        partial void OnStartChanged(DateTime? previous, DateTime? value)
        {
            if (previous.HasValue != value.HasValue)
            {
                if (value.HasValue)
                    _currentTimeTimer.Start();
                else
                    _currentTimeTimer.Stop();
            }
        }

        partial void OnEndChanged(DateTime? previous, DateTime? value)
        {
            if (previous.HasValue != value.HasValue)
            {
                if (value.HasValue)
                    _currentTimeTimer.Stop();
                else
                    _currentTimeTimer.Start();
            }
        }

        public async Task StartAsync()
        {
            var start = DateTime.Now;
            Start = start;

            using (var cmd = await _databaseService.CreateCommand(SqlQueries.TimeSlot.Insert))
            {
                cmd.AddParameterWithValue("@activityid", ActivityId);
                cmd.AddParameterWithValue("@start", start.ToUniversalTime());
                cmd.AddParameterWithValue("@end", DBNull.Value);

                CurrentTimeSlotId = Convert.ToInt64(await cmd.ExecuteScalarAsync());
            }
        }

        public async Task StopAsync()
        {
            if (!Start.HasValue || End.HasValue || !CurrentTimeSlotId.HasValue)
                return;

            var end = DateTime.Now;
            End = end;

            using (var cmd = await _databaseService.CreateCommand(SqlQueries.TimeSlot.UpdateEnd))
            {
                cmd.AddParameterWithValue("@id", CurrentTimeSlotId.Value);
                cmd.AddParameterWithValue("@end", end.ToUniversalTime());

                await cmd.ExecuteNonQueryAsync();
            }

            TotalTime += CurrentTime;
            SlotCount++;
            Start = End = null;
            CurrentTimeSlotId = null;
        }

        public async Task AddTimeSlot(DateTime start, DateTime end)
        {
            if (end == start)
                return;
            if (end < start)
                CommonHelper.Swap(ref start, ref end);

            using (var cmd = await _databaseService.CreateCommand(SqlQueries.TimeSlot.Insert))
            {
                cmd.AddParameterWithValue("@activityid", ActivityId);
                cmd.AddParameterWithValue("@start", start.ToUniversalTime());
                cmd.AddParameterWithValue("@end", end.ToUniversalTime());

                await cmd.ExecuteNonQueryAsync();
            }

            TotalTime += end - start;
            SlotCount++;
        }

        public async Task ChangeCurrentStartTime(DateTime start)
        {
            if (start > DateTime.Now)
                start = DateTime.Now;

            using (var cmd = await _databaseService.CreateCommand(SqlQueries.TimeSlot.UpdateStart))
            {
                cmd.AddParameterWithValue("@id", CurrentTimeSlotId);
                cmd.AddParameterWithValue("@start", start.ToUniversalTime());

                await cmd.ExecuteNonQueryAsync();
            }

            Start = start;
        }

        private void OnCurrentTimeTimerElapsed(object sender, ElapsedEventArgs e)
        {
            NotifyPropertyChanged(nameof(CurrentTime));
        }
    }
}
