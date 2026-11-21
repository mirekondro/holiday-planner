namespace BE.Domain.Holiday;

public record LedgerEntry(
    Guid EmployeeId,
    HolidayAccount Account,
    int EarningYear,
    DateOnly Date,
    LedgerEntryType Type,
    decimal Hours);
