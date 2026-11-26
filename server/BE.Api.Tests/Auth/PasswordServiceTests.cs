using BE.Api.Auth;

namespace BE.Api.Tests.Auth;

public class PasswordServiceTests
{
    private readonly PasswordService _passwords = new();

    [Fact]
    public void Hash_DoesNotContainThePassword()
    {
        var hash = _passwords.Hash("correct horse battery");

        Assert.DoesNotContain("correct horse battery", hash);
    }

    [Fact]
    public void Hash_IsSaltedSoSamePasswordGivesDifferentHashes() =>
        Assert.NotEqual(_passwords.Hash("correct horse battery"), _passwords.Hash("correct horse battery"));

    [Fact]
    public void Verify_CorrectPassword_Succeeds()
    {
        var hash = _passwords.Hash("correct horse battery");

        Assert.Equal(PasswordCheck.Success, _passwords.Verify(hash, "correct horse battery"));
    }

    [Fact]
    public void Verify_WrongPassword_Fails()
    {
        var hash = _passwords.Hash("correct horse battery");

        Assert.Equal(PasswordCheck.Failed, _passwords.Verify(hash, "Correct horse battery"));
    }

    [Fact]
    public void Verify_UnknownUser_Fails() =>
        Assert.Equal(PasswordCheck.Failed, _passwords.Verify(null, "not-a-real-password-just-for-timing"));
}
