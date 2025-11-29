namespace Notex.Core.Services;

public interface ISettingsService
{
    Task<AppSettings> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(AppSettings settings, CancellationToken cancellationToken = default);
}

public class AppSettings
{
    public string? AirtableApiKey { get; set; }
    public string? AirtableBaseId { get; set; }
}
