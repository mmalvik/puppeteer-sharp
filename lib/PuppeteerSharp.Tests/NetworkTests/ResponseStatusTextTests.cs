using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Nunit;
using NUnit.Framework;

namespace PuppeteerSharp.Tests.NetworkTests
{
    public class ResponseStatusTextTests : PuppeteerPageBaseTest
    {
        public ResponseStatusTextTests(): base()
        {
        }

        [PuppeteerTest("network.spec.ts", "Response.statusText", "should work")]
        [PuppeteerTimeout]
        public async Task ShouldWork()
        {
            Server.SetRoute("/cool", (context) =>
            {
                context.Response.StatusCode = 200;
                //There are some debates about this on these issues
                //https://github.com/aspnet/HttpAbstractions/issues/395
                //https://github.com/aspnet/HttpAbstractions/issues/486
                //https://github.com/aspnet/HttpAbstractions/issues/794
                context.Features.Get<IHttpResponseFeature>().ReasonPhrase = "cool!";
                return Task.CompletedTask;
            });
            var response = await Page.GoToAsync(TestConstants.ServerUrl + "/cool");
            Assert.AreEqual("cool!", response.StatusText);
        }
    }
}
