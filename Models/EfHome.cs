using System.Text.Json.Serialization;

namespace EndfieldBot.Models;

public record EfHomeModelCode(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("startDate")] DateTime? StartDate,
    [property: JsonPropertyName("expirationDate")] DateTime? ExpirationDate,
    [property: JsonPropertyName("active")] bool Active
);

public record EfHomeModelEvent(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("startDate")] DateTime? StartDate,
    [property: JsonPropertyName("endDate")] DateTime? EndDate,
    [property: JsonPropertyName("coverImage")] string CoverImage,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("active")] bool Active,
    [property: JsonPropertyName("notConfirmed")] bool NotConfirmed,
    [property: JsonPropertyName("tintColor")] string TintColor
);

public record EfHomeModel(
    [property: JsonPropertyName("codes")] IReadOnlyList<EfHomeModelCode> Codes,
    [property: JsonPropertyName("events")] IReadOnlyList<EfHomeModelEvent> Events,
    [property: JsonPropertyName("lastUpdated")] DateTime? LastUpdated
);
