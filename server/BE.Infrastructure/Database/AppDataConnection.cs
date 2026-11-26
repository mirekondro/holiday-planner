using BE.Infrastructure.Entities;
using LinqToDB;
using LinqToDB.Data;

namespace BE.Infrastructure.Database;

public class AppDataConnection(DataOptions<AppDataConnection> options) : DataConnection(options.Options)
{
    public ITable<EmployeeEntity> Employees => this.GetTable<EmployeeEntity>();
    public ITable<AppUserEntity> Users => this.GetTable<AppUserEntity>();
}
