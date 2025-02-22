using System.Threading.Tasks;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Nunit;
using NUnit.Framework;

namespace PuppeteerSharp.Tests.NetworkTests
{
    public class RequestHeadersTests : PuppeteerPageBaseTest
    {
        public RequestHeadersTests(): base()
        {
        }

        [PuppeteerTest("network.spec.ts", "Request.Headers", "should work")]
        [PuppeteerTimeout]
        public async Task ShouldWork()
        {
            var response = await Page.GoToAsync(TestConstants.EmptyPage);

            if (TestConstants.IsChrome)
            {
                StringAssert.Contains("Chrome", response.Request.Headers["User-Agent"]);
            }
            else
            {
                StringAssert.Contains("Firefox", response.Request.Headers["User-Agent"]);
            }
        }
    }
}
