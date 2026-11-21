using LinqToDB.Mapping;

namespace BE.Infrastructure.Entities;

[Table("employee")]
public class EmployeeEntity
{
    [Column("id"), PrimaryKey] public Guid Id { get; set; }
    [Column("first_name"), NotNull] public string FirstName { get; set; } = "";
    [Column("last_name"), NotNull] public string LastName { get; set; } = "";
    [Column("email"), NotNull] public string Email { get; set; } = "";
    [Column("employment_start"), NotNull] public DateOnly EmploymentStart { get; set; }
    [Column("employment_end")] public DateOnly? EmploymentEnd { get; set; }
}
