using EndfieldBot.Models;

namespace EndfieldBot.DB;

public class RedeemCodes
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public string? Code { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsActive { get; set; }
}

public class CurrentEvents
{
    public string Id { get; set; } = Ulid.NewUlid().ToString();
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? CoverImage { get; set; }
    public string? Url { get; set; }
    public bool IsActive { get; set; }
    public bool NotConfirmed { get; set; }
    public string? TintColor { get; set; }
}

public static partial class Mapper
{
    public static RedeemCodes ToEntity(this EfHomeModelCode efHomeModelCode)
    {
        return new()
        {
            Code = efHomeModelCode.Code,
            Description = efHomeModelCode.Description,
            StartDate = efHomeModelCode.StartDate,
            ExpirationDate = efHomeModelCode.ExpirationDate,
            IsActive = efHomeModelCode.Active
        };
    }

    public static CurrentEvents ToEntity(this EfHomeModelEvent efHomeModelEvent)
    {
        return new()
        {
            Name = efHomeModelEvent.Name,
            Description = efHomeModelEvent.Description,
            StartDate = efHomeModelEvent.StartDate,
            EndDate = efHomeModelEvent.EndDate,
            CoverImage = efHomeModelEvent.CoverImage,
            Url = efHomeModelEvent.Url,
            IsActive = efHomeModelEvent.Active,
            NotConfirmed = efHomeModelEvent.NotConfirmed,
            TintColor = efHomeModelEvent.TintColor
        };
    }
}
