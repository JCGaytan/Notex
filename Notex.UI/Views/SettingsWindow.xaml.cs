using System.Windows;
using Notex.UI.ViewModels;

namespace Notex.UI.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm && sender is System.Windows.Controls.PasswordBox box)
        {
            vm.ApiKey = box.Password;
        }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel vm)
        {
            DialogResult = false;
            return;
        }

        var saved = await vm.SaveAsync();
        if (saved)
        {
            DialogResult = true;
            Close();
        }
    }
}
