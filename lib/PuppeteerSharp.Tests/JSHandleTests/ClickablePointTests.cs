using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Threading.Tasks;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Nunit;
using NUnit.Framework;

namespace PuppeteerSharp.Tests.JSHandleTests
{
    public class ClickablePointTests : PuppeteerPageBaseTest
    {
        public ClickablePointTests(): base()
        {
        }

        [PuppeteerTest("jshandle.spec.ts", "JSHandle.clickablePoint", "should work")]
        [PuppeteerTimeout]
        public async Task ShouldWork()
        {
            await Page.EvaluateExpressionAsync(@"document.body.style.padding = '0';
                document.body.style.margin = '0';
                document.body.innerHTML = '<div style=""cursor: pointer; width: 120px; height: 60px; margin: 30px; padding: 15px;""></div>';
                ");

            await Page.EvaluateExpressionAsync("new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));");

            var divHandle = await Page.QuerySelectorAsync("div");

            var clickablePoint = await divHandle.ClickablePointAsync();

            // margin + middle point offset
            Assert.AreEqual(45 + 60, clickablePoint.X);
            Assert.AreEqual(45 + 30, clickablePoint.Y);

            clickablePoint = await divHandle.ClickablePointAsync(new Offset { X = 10, Y = 15 });

            // margin + offset
            Assert.AreEqual(30 + 10, clickablePoint.X);
            Assert.AreEqual(30 + 15, clickablePoint.Y);
        }

        [PuppeteerTest("jshandle.spec.ts", "JSHandle.clickablePoint", "should work for iframes")]
        [PuppeteerTimeout]
        public async Task ShouldWorkForIFrames()
        {
            await Page.EvaluateExpressionAsync(@"document.body.style.padding = '10px';
                document.body.style.margin = '10px';
                document.body.innerHTML = `<iframe style=""border: none; margin: 0; padding: 0;"" seamless sandbox srcdoc=""<style>* { margin: 0; padding: 0;}</style><div style='cursor: pointer; width: 120px; height: 60px; margin: 30px; padding: 15px;' />""></iframe>`
                ");

            await Page.EvaluateExpressionAsync("new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve)));");

            var frame = Page.FirstChildFrame();

            var divHandle = await frame.QuerySelectorAsync("div");

            var clickablePoint = await divHandle.ClickablePointAsync();

            // iframe pos + margin + middle point offset
            Assert.AreEqual(20 + 45 + 60, clickablePoint.X);
            Assert.AreEqual(20 + 45 + 30, clickablePoint.Y);

            clickablePoint = await divHandle.ClickablePointAsync(new Offset { X = 10, Y = 15 });

            // iframe pos + margin + offset
            Assert.AreEqual(20 + 30 + 10, clickablePoint.X);
            Assert.AreEqual(20 + 30 + 15, clickablePoint.Y);
        }
    }
}
