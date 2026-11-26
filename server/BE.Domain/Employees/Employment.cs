namespace BE.Domain.Employees;

// Údaje o zaměstnanci, které potřebuje výpočet dovolené: nástup, odchod a historie rozvrhů.
public record Employment(DateOnly Start, DateOnly? End, IReadOnlyList<WorkSchedule> Schedules)
{
    // Byl v ten den zaměstnaný? End = null znamená „pořád pracuje".
    public bool IsEmployedOn(DateOnly date) => date >= Start && (End is null || date <= End);

    // Který rozvrh platil v daný den (null = žádný, např. mezera v historii).
    public WorkSchedule? ScheduleOn(DateOnly date) => Schedules.FirstOrDefault(s => s.AppliesOn(date));
}
