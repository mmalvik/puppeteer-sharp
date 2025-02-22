using System.Linq;
using System.Net;
using System.Threading.Tasks;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Nunit;
using NUnit.Framework;

namespace PuppeteerSharp.Tests.NavigationTests
{
    public class FrameWaitForNavigationTests : PuppeteerPageBaseTest
    {
        public FrameWaitForNavigationTests(): base()
        {
        }

        [PuppeteerTest("navigation.spec.ts", "Frame.waitForNavigation", "should work")]
        [Skip(SkipAttribute.Targets.Firefox)]
        public async Task ShouldWork()
        {
            await Page.GoToAsync(TestConstants.ServerUrl + "/frames/one-frame.html");
            var frame = Page.FirstChildFrame();
            var waitForNavigationResult = frame.WaitForNavigationAsync();

            await Task.WhenAll(
                waitForNavigationResult,
                frame.EvaluateFunctionAsync("url => window.location.href = url", TestConstants.ServerUrl + "/grid.html")
            );
            var response = await waitForNavigationResult;
            Assert.AreEqual(HttpStatusCode.OK, response.Status);
            StringAssert.Contains("grid.html", response.Url);
            Assert.AreSame(frame, response.Frame);
            StringAssert.Contains("/frames/one-frame.html", Page.Url);
        }

        [PuppeteerTest("navigation.spec.ts", "Frame.waitForNavigation", "should fail when frame detaches")]
        [PuppeteerTimeout]
        public async Task ShouldFailWhenFrameDetaches()
        {
            await Page.GoToAsync(TestConstants.ServerUrl + "/frames/one-frame.html");
            var frame = Page.FirstChildFrame();
            Server.SetRoute("/empty.html", _ => Task.Delay(10000));
            var waitForNavigationResult = frame.WaitForNavigationAsync();
            await Task.WhenAll(
                Server.WaitForRequest("/empty.html"),
                frame.EvaluateFunctionAsync($"() => window.location = '{TestConstants.EmptyPage}'"));

            await Page.QuerySelectorAsync("iframe").EvaluateFunctionAsync("frame => frame.remove()");
            var exception = Assert.ThrowsAsync<PuppeteerException>(() => waitForNavigationResult);
            Assert.AreEqual("Navigating frame was detached", exception.Message);
        }
    }
}
