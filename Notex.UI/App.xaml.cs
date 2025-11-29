using System.Windows;
using Notex.Core.Services;
using Notex.UI.ViewModels;

namespace Notex.UI;

public partial class App : Application
{
    private readonly ISettingsService _settingsService = new SettingsService();
    private readonly IAirtableService _airtableService = new AirtableService();
    private IBookService? _bookService;
    private INoteService? _noteService;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var settings = await _settingsService.LoadAsync();
        if (!string.IsNullOrWhiteSpace(settings.AirtableApiKey))
        {
            await _airtableService.ConfigureAsync(settings.AirtableApiKey, settings.AirtableBaseId);
            await _airtableService.EnsureBaseAndTablesAsync();
            _bookService = new BookService(_airtableService);
            _noteService = new NoteService(_airtableService);
        }

        var mainWindow = new MainWindow
        {
            DataContext = new MainViewModel(_settingsService, _airtableService, _bookService, _noteService)
        };

        mainWindow.Show();
    }
}
