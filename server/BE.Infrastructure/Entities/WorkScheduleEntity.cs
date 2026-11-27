using LinqToDB.Mapping;

namespace BE.Infrastructure.Entities;

[Table("work_schedule")]
public class WorkScheduleEntity
{
    [Column("id"), PrimaryKey] public Guid Id { get; set; }
    [Column("employee_id"), NotNull] public Guid EmployeeId { get; set; }
    [Column("valid_from"), NotNull] public DateOnly ValidFrom { get; set; }
    [Column("valid_to")] public DateOnly? ValidTo { get; set; }
}

[Table("work_schedule_day")]
public class WorkScheduleDayEntity
{
    [Column("schedule_id"), PrimaryKey(1)] public Guid ScheduleId { get; set; }

    /// <summary>1 = Monday … 5 = Friday.</summary>
    [Column("weekday"), PrimaryKey(2)] public int Weekday { get; set; }

    [Column("start_time")] public TimeOnly? StartTime { get; set; }
    [Column("end_time")] public TimeOnly? EndTime { get; set; }
}
