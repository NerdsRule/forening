using Organization.Shared.DatabaseObjects;

namespace Organization.Test;

public class TResetPasswordTests
{
    [Fact]
    public void RegisterResetRequest_FirstRequest_SetsTokenAndDoesNotBlock()
    {
        // First request should track metadata but remain eligible for another request.
        var entity = new TResetPassword { AppUserId = "u1" };
        var when = new DateTimeOffset(2026, 5, 30, 8, 0, 0, TimeSpan.Zero);

        entity.RegisterResetRequest(when, "token-1");

        entity.ResetRequestCount.Should().Be(1);
        entity.ResetToken.Should().Be("token-1");
        entity.LastResetRequestAt.Should().Be(when);
        entity.ResetTokenCreatedAt.Should().Be(when);
        entity.IsResetMailBlocked.Should().BeFalse();
        entity.CanSendResetMail.Should().BeTrue();
        entity.ResetMailBlockedAt.Should().BeNull();
    }

    [Fact]
    public void RegisterResetRequest_SecondRequest_BlocksFurtherResetMails()
    {
        // The second request reaches the configured threshold and blocks the reset mail flow.
        var entity = new TResetPassword { AppUserId = "u1" };
        var first = new DateTimeOffset(2026, 5, 30, 8, 0, 0, TimeSpan.Zero);
        var second = first.AddMinutes(2);

        entity.RegisterResetRequest(first, "token-1");
        entity.RegisterResetRequest(second, "token-2");

        entity.ResetRequestCount.Should().Be(TResetPassword.MaxResetRequestsBeforeBlock);
        entity.ResetToken.Should().Be("token-2");
        entity.LastResetRequestAt.Should().Be(second);
        entity.IsResetMailBlocked.Should().BeTrue();
        entity.CanSendResetMail.Should().BeFalse();
        entity.ResetMailBlockedAt.Should().Be(second);
    }

    [Fact]
    public void IsMatchingResetToken_UsesOrdinalExactMatching()
    {
        // Matching is strict: null/empty/wrong-case tokens must be rejected.
        var entity = new TResetPassword { AppUserId = "u1", ResetToken = "AbC123" };

        entity.IsMatchingResetToken("AbC123").Should().BeTrue();
        entity.IsMatchingResetToken("abc123").Should().BeFalse();
        entity.IsMatchingResetToken(" ").Should().BeFalse();
        entity.IsMatchingResetToken(null).Should().BeFalse();
    }
}