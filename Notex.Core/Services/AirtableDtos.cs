using System.Text.Json.Serialization;

namespace Notex.Core.Services;

public class AirtableRecord<T>
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("fields")]
    public T? Fields { get; set; }
}

public class AirtableListResponse<T>
{
    [JsonPropertyName("records")]
    public List<AirtableRecord<T>> Records { get; set; } = new();
}

public class AirtableCreateRequest<T>
{
    [JsonPropertyName("records")]
    public List<AirtableCreateRecord<T>> Records { get; set; } = new();
}

public class AirtableCreateRecord<T>
{
    [JsonPropertyName("fields")]
    public T Fields { get; set; }

    public AirtableCreateRecord(T fields)
    {
        Fields = fields;
    }
}
