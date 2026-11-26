using BE.Domain.Users;

namespace BE.Domain.Tests.Users;

public class UserRulesTests
{
    [Theory]
    [InlineData("anna@businessesbjerg.com")]
    [InlineData("  Anna.Hansen@BusinessEsbjerg.com ")]
    public void IsOrganizationEmail_AcceptsCompanyAddresses(string email) =>
        Assert.True(UserRules.IsOrganizationEmail(email));

    [Theory]
    [InlineData("anna@gmail.com")]
    [InlineData("@businessesbjerg.com")]
    [InlineData("anna@businessesbjerg.com.evil.dk")]
    [InlineData("anna@evil@businessesbjerg.com")]
    [InlineData("")]
    public void IsOrganizationEmail_RejectsEverythingElse(string email) =>
        Assert.False(UserRules.IsOrganizationEmail(email));

    [Fact]
    public void NormalizeEmail_TrimsAndLowercases() =>
        Assert.Equal("anna@businessesbjerg.com", UserRules.NormalizeEmail("  Anna@BusinessEsbjerg.COM "));

    [Theory]
    [InlineData("short", false)]
    [InlineData("exactly12chr", true)]
    [InlineData("            ", false)]
    public void IsAcceptablePassword_ChecksLength(string password, bool expected) =>
        Assert.Equal(expected, UserRules.IsAcceptablePassword(password));

    [Fact]
    public void IsAcceptablePassword_RejectsVeryLongInput() =>
        Assert.False(UserRules.IsAcceptablePassword(new string('a', UserRules.MaxPasswordLength + 1)));

    [Fact]
    public void HasValidEmployeeLink_EmployeeNeedsEmployee() =>
        Assert.False(UserRules.HasValidEmployeeLink(UserRole.Employee, null));

    [Fact]
    public void HasValidEmployeeLink_AccountantMustNotBeLinked()
    {
        Assert.False(UserRules.HasValidEmployeeLink(UserRole.Accountant, Guid.NewGuid()));
        Assert.True(UserRules.HasValidEmployeeLink(UserRole.Accountant, null));
    }

    [Theory]
    [InlineData(UserRole.Employee, false)]
    [InlineData(UserRole.Accountant, true)]
    [InlineData(UserRole.SystemOwner, true)]
    public void IsSingleAccountRole(UserRole role, bool expected) =>
        Assert.Equal(expected, UserRules.IsSingleAccountRole(role));
}
