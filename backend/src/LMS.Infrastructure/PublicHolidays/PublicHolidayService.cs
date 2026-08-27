using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.PublicHolidays;

public sealed class PublicHolidayService(LmsDbContext db) : IPublicHolidayService
{
    public async Task<List<PublicHolidayDto>> ListAsync(int year, CancellationToken ct = default)
    {
        return await db.PublicHolidays
            .Where(h => h.Year == year)
            .OrderBy(h => h.Date)
            .Select(h => ToDto(h))
            .ToListAsync(ct);
    }

    public async Task<PublicHolidayDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var h = await db.PublicHolidays.FindAsync([id], ct);
        return h is null ? null : ToDto(h);
    }

    public async Task<PublicHolidayDto> CreateAsync(CreatePublicHolidayRequest req, CancellationToken ct = default)
    {
        var h = new PublicHoliday
        {
            Id = Guid.NewGuid(),
            Date = req.Date,
            Name = req.Name,
            Year = req.Date.Year,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.PublicHolidays.Add(h);
        await db.SaveChangesAsync(ct);
        return ToDto(h);
    }

    public async Task<PublicHolidayDto?> UpdateAsync(Guid id, UpdatePublicHolidayRequest req, CancellationToken ct = default)
    {
        var h = await db.PublicHolidays.FindAsync([id], ct);
        if (h is null) return null;
        if (req.Date is { } d) { h.Date = d; h.Year = d.Year; }
        if (req.Name is { } n) h.Name = n;
        h.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return ToDto(h);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var h = await db.PublicHolidays.FindAsync([id], ct);
        if (h is null) return false;
        db.PublicHolidays.Remove(h);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<BulkImportPreview> BulkImportAsync(BulkImportRequest req, CancellationToken ct = default)
    {
        var existing = await db.PublicHolidays
            .Where(h => h.Year == req.Year)
            .Select(h => h.Date)
            .ToListAsync(ct);

        var toCreate = req.Holidays.Where(i => !existing.Contains(i.Date)).ToList();
        var toSkip = req.Holidays.Where(i => existing.Contains(i.Date)).ToList();

        if (req.Confirm)
        {
            foreach (var item in toCreate)
            {
                db.PublicHolidays.Add(new PublicHoliday
                {
                    Id = Guid.NewGuid(),
                    Date = item.Date,
                    Name = item.Name,
                    Year = req.Year,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            await db.SaveChangesAsync(ct);
        }

        return new BulkImportPreview(toCreate, toSkip, req.Holidays.Count);
    }

    private static PublicHolidayDto ToDto(PublicHoliday h) =>
        new(h.Id, h.Date, h.Name, h.Year, h.CreatedAt, h.UpdatedAt);
}
