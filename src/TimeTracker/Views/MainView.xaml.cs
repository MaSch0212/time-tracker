using TimeTracker.Models;
using TimeTracker.Services;
using TimeTracker.ViewModels;
using MaSch.Core;
using MaSch.Core.Extensions;
using MaSch.Presentation.Wpf.Common;
using MaSch.Presentation.Wpf.Extensions;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TimeTracker.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        private ActivityTimeSlot _currentTimeSlot;

        public MainView()
        {
            InitializeComponent();

            var settingsService = ServiceContext.GetService<ISettingsService>();
            Loaded += (s, e) =>
            {
                var settings = settingsService.LoadGuiSettings();
                WindowPosition.ApplyToWindow(settings.WindowPositions, this);
                Activate();
            };
            Closing += (s, e) =>
            {
                var settings = settingsService.LoadGuiSettings();
                WindowPosition.AddWindowToList(settings.WindowPositions, this);
                settingsService.SaveGuiSettings(settings);
            };
        }

        private void RemoveActivityItemClicked(object sender, RoutedEventArgs e)
        {
            if (_currentTimeSlot != null &&
                DataContext is MainViewModel viewModel &&
                viewModel.RemoveActivityCommand.CanExecute(_currentTimeSlot.ActivityId))
            {
                viewModel.RemoveActivityCommand.Execute(_currentTimeSlot.ActivityId);
            }
        }

        private async void AddTimeMenuItemClicked(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement menuItem &&
                _currentTimeSlot != null)
            {
                int value;
                if (menuItem.Tag?.ToString() == "CustomAddAmountTextBox")
                {
                    value = ((menuItem.Parent as StackPanel).Children[0] as MaSch.Presentation.Wpf.Controls.TextBox).NumericValue.ConvertTo<int>();
                }
                else
                    value = menuItem.Tag.ConvertTo<int>();

                var end = DateTime.Now;
                var start = end.AddMinutes(-value);
                await _currentTimeSlot.AddTimeSlot(start, end);
            }
        }

        private void TimeSlotContextMenuOpened(object sender, RoutedEventArgs e)
        {
            _currentTimeSlot = ((sender as ContextMenu)?.PlacementTarget as FrameworkElement)?.Tag as ActivityTimeSlot;
        }

        private async void AddCurrenTimeButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is ActivityTimeSlot timeSlot)
            {
                var value = element.Tag.ConvertTo<int>();
                var newStart = timeSlot.Start.Value.AddMinutes(-value);
                await timeSlot.ChangeCurrentStartTime(newStart);
            }
        }
    }
}
