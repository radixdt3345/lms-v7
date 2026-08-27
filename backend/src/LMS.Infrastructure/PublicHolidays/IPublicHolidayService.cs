namespace LMS.Infrastructure.PublicHolidays;

public interface IPublicHolidayService
{
    Task<List<PublicHolidayDto>> ListAsync(int year, CancellationToken ct = default);
    Task<PublicHolidayDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PublicHolidayDto> CreateAsync(CreatePublicHolidayRequest req, CancellationToken ct = default);
    Task<PublicHolidayDto?> UpdateAsync(Guid id, UpdatePublicHolidayRequest req, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<BulkImportPreview> BulkImportAsync(BulkImportRequest req, CancellationToken ct = default);
}
