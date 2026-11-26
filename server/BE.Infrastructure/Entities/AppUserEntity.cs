using LinqToDB.Mapping;

namespace BE.Infrastructure.Entities;

[Table("app_user")]
public class AppUserEntity
{
    [Column("id"), PrimaryKey] public Guid Id { get; set; }
    [Column("email"), NotNull] public string Email { get; set; } = "";
    [Column("password_hash"), NotNull] public string PasswordHash { get; set; } = "";
    [Column("role"), NotNull] public string Role { get; set; } = "";
    [Column("employee_id")] public Guid? EmployeeId { get; set; }
    [Column("created_at"), NotNull] public DateTimeOffset CreatedAt { get; set; }
}
