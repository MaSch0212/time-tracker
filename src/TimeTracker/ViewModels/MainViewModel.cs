using TimeTracker.Dialogs;
using TimeTracker.Models;
using TimeTracker.Services;
using MaSch.Core;
using MaSch.Core.Attributes;
using MaSch.Core.Extensions;
using MaSch.Core.Observable;
using MaSch.Data.Extensions;
using MaSch.Presentation.Wpf.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MessageBox = MaSch.Presentation.Wpf.MessageBox;
using MaSch.Presentation.Wpf.MaterialDesign;

namespace TimeTracker.ViewModels
{
    [ObservablePropertyDefinition]
    internal interface IMainViewModel_Props
    {
        [ObservablePropertyAccessModifier(SetModifier = AccessModifier.Private)]
        IList<ActivityTimeSlot> Activitys { get; }

        string CurrentActivityName { get; set; }
    }

    public partial class MainViewModel : ObservableObject, IMainViewModel_Props
    {
        private readonly IDatabaseService _databaseService;

        public ICommand CreateActivityCommand { get; }
        public ICommand RemoveActivityCommand { get; }
        public ICommand ToggleTimeSlotCommand { get; }

        public MainViewModel()
        {
            if (!DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                ServiceContext.GetService(out _databaseService);

                LoadAllActivitys();
            }

            CreateActivityCommand = new AsyncDelegateCommand(OnExecuteCreateActivity);
            RemoveActivityCommand = new AsyncDelegateCommand<long>(OnExecuteRemoveActivity);
            ToggleTimeSlotCommand = new AsyncDelegateCommand<ActivityTimeSlot>(x => x != null, OnExecuteToggleTimeSlot);
        }

        private async Task OnExecuteCreateActivity()
        {
            var inputMsgBox = new InputMessageBox
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Message = "Please type in the name of the activity",
                Title = "Add Activity",
                TitleIcon = new MaterialDesignIcon(MaterialDesignIconCode.Plus)
            };
            if (inputMsgBox.ShowDialog() != true)
                return;
            var name = inputMsgBox.Text;

            using (var cmd = await _databaseService.CreateCommand(SqlQueries.Activity.Insert))
            {
                cmd.AddParameterWithValue("@name", name);

                var id = Convert.ToInt64(await cmd.ExecuteScalarAsync());
                Activitys.Insert(0, new ActivityTimeSlot(id, name, 0, 0D));
            }

            CurrentActivityName = null;
        }

        private async Task OnExecuteRemoveActivity(long activityId)
        {
            var activity = Activitys.First(x => x.ActivityId == activityId);
            if (MessageBox.Show(Application.Current.MainWindow, $"Do you really want to delete the activity \"{activity.ActivityName}\" and all recorded time slots?", "", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            using (var cmd = await _databaseService.CreateCommand(SqlQueries.Activity.Remove))
            {
                cmd.AddParameterWithValue("@id", activityId);

                await cmd.ExecuteNonQueryAsync();
                Activitys.RemoveWhere(x => x.ActivityId == activityId);
            }
        }

        private async Task OnExecuteToggleTimeSlot(ActivityTimeSlot timeSlot)
        {
            if (timeSlot.IsRunning)
                await timeSlot.StopAsync();
            else
                await timeSlot.StartAsync();
        }

        private async void LoadAllActivitys()
        {
            using (var cmd = await _databaseService.CreateCommand(SqlQueries.Activity.GetAll))
            using (var items = (await cmd.ExecuteReaderAsync()).ToEnumerable<long, string, int, double>())
            {
                Activitys = new ObservableCollection<ActivityTimeSlot>(items.Select(x => new ActivityTimeSlot(x.Item1, x.Item2, x.Item3, x.Item4)));
            }
        }
    }
}
