using Shared.Constants;
using Xunit;

namespace Shared.Tests;

public class ProjectInfoTests
{
    [Fact]
    public void Name_Is_Set()
        => Assert.False(string.IsNullOrWhiteSpace(ProjectInfo.Name));
}