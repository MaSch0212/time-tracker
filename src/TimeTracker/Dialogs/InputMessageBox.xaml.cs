using MaSch.Core.Attributes;
using MaSch.Presentation.Wpf;
using System.Windows;
using System.Windows.Input;
using MessageBox = MaSch.Presentation.Wpf.MessageBox;

namespace TimeTracker.Dialogs
{
    [ObservablePropertyDefinition]
    internal interface IInputMessageBox_Props
    {
        string Message { get; set; }
        string Text { get; set; }
        IIcon TitleIcon { get; set; }
    }

    [GenerateObservableObject]
    public partial class InputMessageBox : IInputMessageBox_Props
    {
        private bool _enterDown = false;

        public InputMessageBox()
        {
            InitializeComponent();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Text))
            {
                MessageBox.Show(this, "Please type in a name.", "Time Tracker", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void NameTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !e.IsRepeat)
                _enterDown = true;
        }

        private void NameTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _enterDown)
                OnOkClicked(sender, e);
        }
    }
}
