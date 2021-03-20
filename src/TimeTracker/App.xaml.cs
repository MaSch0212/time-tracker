using TimeTracker.Services;
using TimeTracker.Views;
using MaSch.Core;
using MaSch.Presentation.Wpf;
using System.Windows;
using System.Runtime.InteropServices;
using System.IO;
using System;
using System.Globalization;

namespace TimeTracker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

            var binDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            if (Directory.Exists(binDir))
                SetDllDirectory(binDir);

            InitializeServices();
            ServiceContext.GetService<IThemeManager>().LoadTheme(Theme.FromDefaultTheme(DefaultTheme.Dark));

            MainWindow = new MainView();
            MainWindow.Show();
        }

        private void InitializeServices()
        {
            ServiceContext.AddService(ThemeManager.DefaultThemeManager);
            ServiceContext.AddService<IDatabaseService>(new DatabaseService());
            ServiceContext.AddService<ISettingsService>(new SettingsService());
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectory(string lpPathName);
    }
}
