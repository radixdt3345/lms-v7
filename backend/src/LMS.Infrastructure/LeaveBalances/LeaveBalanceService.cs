using LMS.Infrastructure.Data;
using LMS.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.LeaveBalances;

public sealed class LeaveBalanceService(LmsDbContext db) : ILeaveBalanceService
{
    public async Task<IReadOnlyList<LeaveBalanceDto>> GetMyBalancesAsync(Guid userId, int year)
        => await db.LeaveBalances
            .Where(b => b.EmployeeId == userId && b.Year == year)
            .Include(b => b.Employee)
            .Include(b => b.LeaveType)
            .Select(b => ToDto(b))
            .ToListAsync();

    public async Task<IReadOnlyList<LeaveBalanceDto>> GetEmployeeBalancesAsync(Guid employeeId, int year)
        => await db.LeaveBalances
            .Where(b => b.EmployeeId == employeeId && b.Year == year)
            .Include(b => b.Employee)
            .Include(b => b.LeaveType)
            .Select(b => ToDto(b))
            .ToListAsync();

    public async Task<IReadOnlyList<LeaveBalanceDto>> GetAllBalancesAsync(int year)
        => await db.LeaveBalances
            .Where(b => b.Year == year)
            .Include(b => b.Employee)
            .Include(b => b.LeaveType)
            .Select(b => ToDto(b))
            .ToListAsync();

    public async Task CreditAnnualBalancesAsync(int year)
    {
        var employees = await db.Users.Where(u => u.Status == "Active").ToListAsync();
        var leaveTypes = await db.LeaveTypes.Where(lt => lt.IsActive).ToListAsync();

        foreach (var emp in employees)
        {
            foreach (var lt in leaveTypes)
            {
                var exists = await db.LeaveBalances.AnyAsync(b =>
                    b.EmployeeId == emp.Id && b.LeaveTypeId == lt.Id && b.Year == year);
                if (!exists)
                {
                    db.LeaveBalances.Add(new LeaveBalance
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = emp.Id,
                        LeaveTypeId = lt.Id,
                        Year = year,
                        TotalDays = lt.MaxDaysPerYear,
                        UsedDays = 0,
                        PendingDays = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
            }
        }
        await db.SaveChangesAsync();
    }

    public async Task AdjustBalanceAsync(AdjustBalanceRequest request)
    {
        var balance = await db.LeaveBalances.FirstOrDefaultAsync(b =>
            b.EmployeeId == request.EmployeeId &&
            b.LeaveTypeId == request.LeaveTypeId &&
            b.Year == request.Year);

        if (balance is null)
        {
            db.LeaveBalances.Add(new LeaveBalance
            {
                Id = Guid.NewGuid(),
                EmployeeId = request.EmployeeId,
                LeaveTypeId = request.LeaveTypeId,
                Year = request.Year,
                TotalDays = request.AdjustmentDays,
                UsedDays = 0,
                PendingDays = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            balance.TotalDays += request.AdjustmentDays;
            balance.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
    }

    private static LeaveBalanceDto ToDto(LeaveBalance b) => new(
        b.Id, b.EmployeeId, b.Employee.Name, b.LeaveTypeId, b.LeaveType.Name,
        b.Year, b.TotalDays, b.UsedDays, b.PendingDays,
        b.TotalDays - b.UsedDays - b.PendingDays);
}
