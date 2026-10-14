using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace MojePwa.Server.Test;

public abstract class ControllerTestsBase
{
    protected abstract string ControllerUrl { get; }

    /// <summary>
    /// Společný základ pro všechny testy API: všude je potřeba klient
    /// </summary>
    protected static async Task DoAsync(Func<HttpClient, Task> action)
    {
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();
        await action(client);
    }

    protected static void AssertCreated(HttpResponseMessage r) => Assert.Equal(HttpStatusCode.Created, r.StatusCode);
    protected static void AssertConflict(HttpResponseMessage r) => Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
    protected static void AssertBadReq(HttpResponseMessage r) => Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
    protected static void AssertNoContent(HttpResponseMessage r) => Assert.Equal(HttpStatusCode.NoContent, r.StatusCode);
    protected static void AssertNotFound(HttpResponseMessage r) => Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    protected static void AssertOk(HttpResponseMessage r) => Assert.Equal(HttpStatusCode.OK, r.StatusCode);
}