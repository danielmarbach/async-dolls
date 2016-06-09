using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using NUnit.Framework;
using Owin;

namespace AsyncDolls
{
    [TestFixture]
    public class OwinMiddleware
    {
        [Test]
        public void AppBuilder()
        {
            IAppBuilder appBuilder = null;

            appBuilder.Use(async (ctx, next) =>
            {
                await next();
            });
        }
    }

    class FilterOutInvalidOperationException : IActionFilter
    {
        public bool AllowMultiple { get; }

        public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            try
            {
                var response = await continuation();
                return response;
            }
            catch (InvalidOperationException)
            {
            }

            return new HttpResponseMessage();
        }
    }
}