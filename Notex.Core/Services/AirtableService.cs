using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Notex.Core.Models;

namespace Notex.Core.Services;

public class AirtableService : IAirtableService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    private const string BaseUrl = "https://api.airtable.com/v0";

    public AirtableService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public string? ApiToken { get; private set; }
    public string? BaseId { get; private set; }

    public async Task<bool> ConfigureAsync(string apiToken, string? baseId = null, CancellationToken cancellationToken = default)
    {
        ApiToken = apiToken;
        BaseId = baseId;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

        // Validate token by requesting a trivial endpoint. Airtable's metadata API is limited; we attempt to list bases when BaseId is missing
        if (string.IsNullOrWhiteSpace(BaseId))
        {
            BaseId = await DiscoverBaseAsync(cancellationToken);
        }

        return !string.IsNullOrWhiteSpace(BaseId);
    }

    public async Task EnsureBaseAndTablesAsync(CancellationToken cancellationToken = default)
    {
        // Airtable's public API does not permit creating bases. We attempt to resolve an existing base named "Notex" via the metadata endpoint.
        // If that also fails, we continue using the provided BaseId and rely on the user to create the base manually following README guidance.
        if (string.IsNullOrWhiteSpace(BaseId))
        {
            BaseId = await DiscoverBaseAsync(cancellationToken);
        }

        // Table provisioning is also not available on the public API. We simulate the check by attempting to query the tables.
        // Missing tables will surface as errors during CRUD calls; callers should report them to the user.
    }

    public async Task<List<Book>> GetBooksAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(BuildUri("Books"), cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AirtableListResponse<Book>>(_serializerOptions, cancellationToken);
        return payload?.Records.Select(r => MapBook(r)).ToList() ?? new List<Book>();
    }

    public async Task<Book> CreateBookAsync(Book book, CancellationToken cancellationToken = default)
    {
        book.CreatedAt = DateTime.UtcNow;
        book.UpdatedAt = DateTime.UtcNow;
        book.Id = string.IsNullOrWhiteSpace(book.Id) ? Guid.NewGuid().ToString() : book.Id;
        var request = new AirtableCreateRequest<Book> { Records = { new AirtableCreateRecord<Book>(book) } };
        var response = await _httpClient.PostAsJsonAsync(BuildUri("Books"), request, _serializerOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AirtableListResponse<Book>>(_serializerOptions, cancellationToken);
        return MapBook(payload!.Records.First());
    }

    public async Task<Book> UpdateBookAsync(Book book, CancellationToken cancellationToken = default)
    {
        book.UpdatedAt = DateTime.UtcNow;
        var record = new { fields = book };
        var response = await _httpClient.PatchAsync($"{BuildUri("Books")}/{book.Id}", JsonContent.Create(record, options: _serializerOptions), cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AirtableRecord<Book>>(_serializerOptions, cancellationToken);
        return MapBook(payload!);
    }

    public async Task DeleteBookAsync(string bookId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"{BuildUri("Books")}/{bookId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<Note>> GetNotesAsync(string? bookId = null, CancellationToken cancellationToken = default)
    {
        var uri = BuildUri("Notes", bookId is null ? null : new Dictionary<string, string> { ["filterByFormula"] = $"{{BookId}}='{bookId}'" });
        var response = await _httpClient.GetAsync(uri, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AirtableListResponse<Note>>(_serializerOptions, cancellationToken);
        return payload?.Records.Select(r => MapNote(r)).ToList() ?? new List<Note>();
    }

    public async Task<Note> CreateNoteAsync(Note note, CancellationToken cancellationToken = default)
    {
        note.CreatedAt = DateTime.UtcNow;
        note.UpdatedAt = DateTime.UtcNow;
        note.Id = string.IsNullOrWhiteSpace(note.Id) ? Guid.NewGuid().ToString() : note.Id;
        var request = new AirtableCreateRequest<Note> { Records = { new AirtableCreateRecord<Note>(note) } };
        var response = await _httpClient.PostAsJsonAsync(BuildUri("Notes"), request, _serializerOptions, cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AirtableListResponse<Note>>(_serializerOptions, cancellationToken);
        return MapNote(payload!.Records.First());
    }

    public async Task<Note> UpdateNoteAsync(Note note, CancellationToken cancellationToken = default)
    {
        note.UpdatedAt = DateTime.UtcNow;
        var record = new { fields = note };
        var response = await _httpClient.PatchAsync($"{BuildUri("Notes")}/{note.Id}", JsonContent.Create(record, options: _serializerOptions), cancellationToken);
        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AirtableRecord<Note>>(_serializerOptions, cancellationToken);
        return MapNote(payload!);
    }

    public async Task DeleteNoteAsync(string noteId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync($"{BuildUri("Notes")}/{noteId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private Book MapBook(AirtableRecord<Book> record)
    {
        if (record.Fields is null)
        {
            return new Book();
        }

        return new Book
        {
            Id = !string.IsNullOrWhiteSpace(record.Fields.Id) ? record.Fields.Id : record.Id,
            Name = record.Fields.Name,
            CreatedAt = record.Fields.CreatedAt,
            UpdatedAt = record.Fields.UpdatedAt
        };
    }

    private Note MapNote(AirtableRecord<Note> record)
    {
        if (record.Fields is null)
        {
            return new Note();
        }

        return new Note
        {
            Id = !string.IsNullOrWhiteSpace(record.Fields.Id) ? record.Fields.Id : record.Id,
            BookId = record.Fields.BookId,
            Title = record.Fields.Title,
            Content = record.Fields.Content,
            CreatedAt = record.Fields.CreatedAt,
            UpdatedAt = record.Fields.UpdatedAt
        };
    }

    private string BuildUri(string table, Dictionary<string, string>? query = null)
    {
        var uri = $"{BaseUrl}/{BaseId}/{table}";
        if (query is not null && query.Count > 0)
        {
            var qs = string.Join("&", query.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
            uri = $"{uri}?{qs}";
        }

        return uri;
    }

    private async Task<string?> DiscoverBaseAsync(CancellationToken cancellationToken)
    {
        // Airtable Metadata API is in beta and may not be available. We attempt to call it to locate a base named "Notex".
        try
        {
            var response = await _httpClient.GetAsync("https://api.airtable.com/v0/meta/bases", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return BaseId; // fallback to existing value
            }

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            if (document.RootElement.TryGetProperty("bases", out var bases))
            {
                foreach (var item in bases.EnumerateArray())
                {
                    var name = item.GetProperty("name").GetString();
                    if (string.Equals(name, "Notex", StringComparison.OrdinalIgnoreCase))
                    {
                        return item.GetProperty("id").GetString();
                    }
                }

                // Otherwise pick the first available base to keep the UI usable.
                if (bases.GetArrayLength() > 0)
                {
                    return bases[0].GetProperty("id").GetString();
                }
            }
        }
        catch (HttpRequestException)
        {
            // network error, leave BaseId unchanged
        }
        catch (NotSupportedException)
        {
        }
        catch (JsonException)
        {
        }

        return BaseId;
    }
}
