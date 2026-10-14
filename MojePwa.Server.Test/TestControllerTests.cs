using MojePwa.Server.Controllers;
using MojePwa.Server.Services.Exceptions;
using Xunit;

namespace MojePwa.Server.Test;

public sealed class TestControllerTests
{
    [Fact]
    public void GetRandomResult_ReturnsDocumentedResponsesOrExceptions()
    {
        var controller = new TestController();

        for (var index = 0; index < 100; index++)
        {
            string? result = null;
            var exception = Record.Exception(() => result = controller.GetRandomResult());

            if (exception is null)
            {
                Assert.Equal("API text response", result);
                continue;
            }

            if (exception is UserFriendlyException userFriendlyException)
            {
                Assert.Contains(userFriendlyException.HttpStatus, new[] { 401, 403, 404 });
                continue;
            }

            var argumentException = Assert.IsType<ArgumentException>(exception);
            Assert.Equal("TestParameterName", argumentException.ParamName);
            Assert.StartsWith("TEST argument message", argumentException.Message);
        }
    }
}
