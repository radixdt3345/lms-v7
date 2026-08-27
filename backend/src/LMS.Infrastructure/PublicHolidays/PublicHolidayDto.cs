namespace LMS.Infrastructure.PublicHolidays;

public sealed record PublicHolidayDto(
    Guid Id,
    DateOnly Date,
    string Name,
    int Year,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record CreatePublicHolidayRequest(
    DateOnly Date,
    string Name
);

public sealed record UpdatePublicHolidayRequest(
    DateOnly? Date,
    string? Name
);

public sealed record BulkImportHolidayItem(
    DateOnly Date,
    string Name
);

public sealed record BulkImportRequest(
    int Year,
    List<BulkImportHolidayItem> Holidays,
    bool Confirm = false
);

public sealed record BulkImportPreview(
    List<BulkImportHolidayItem> ToCreate,
    List<BulkImportHolidayItem> ToSkip,
    int Total
);
