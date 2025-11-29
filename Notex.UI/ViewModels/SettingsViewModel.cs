using System.Windows.Input;
using Notex.Core.Services;

namespace Notex.UI.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly IAirtableService _airtableService;
    private string? _apiKey;
    private string? _baseId;
    private string _statusMessage = "";

    public SettingsViewModel(ISettingsService settingsService, IAirtableService airtableService)
    {
        _settingsService = settingsService;
        _airtableService = airtableService;
        SaveCommand = new RelayCommand(async _ => await SaveAsync());

        _ = LoadAsync();
    }

    public string? ApiKey
    {
        get => _apiKey;
        set => SetProperty(ref _apiKey, value);
    }

    public string? BaseId
    {
        get => _baseId;
        set => SetProperty(ref _baseId, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public ICommand SaveCommand { get; }

    public async Task LoadAsync()
    {
        var settings = await _settingsService.LoadAsync();
        ApiKey = settings.AirtableApiKey;
        BaseId = settings.AirtableBaseId;
    }

    public async Task<bool> SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(ApiKey))
        {
            StatusMessage = "API token is required.";
            return false;
        }

        StatusMessage = "Connecting...";
        var configured = await _airtableService.ConfigureAsync(ApiKey, BaseId);
        if (!configured)
        {
            StatusMessage = "Unable to configure Airtable service.";
            return false;
        }

        await _airtableService.EnsureBaseAndTablesAsync();

        var settings = new AppSettings
        {
            AirtableApiKey = ApiKey,
            AirtableBaseId = _airtableService.BaseId
        };

        await _settingsService.SaveAsync(settings);
        StatusMessage = "Saved.";
        return true;
    }
}
