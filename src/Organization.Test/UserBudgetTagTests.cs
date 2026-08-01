using Organization.Shared.DatabaseObjects;

namespace Organization.Test;

public class UserBudgetTagTests
{
    [Fact]
    public void TUserBudget_Tags_Should_Store_Multiple_Tag_Values()
    {
        var budget = new TUserBudget
        {
            Tags = ["alpha", "beta"]
        };

        Assert.Equal(2, budget.Tags.Count);
        Assert.Contains("alpha", budget.Tags);
        Assert.Contains("beta", budget.Tags);
    }
}
