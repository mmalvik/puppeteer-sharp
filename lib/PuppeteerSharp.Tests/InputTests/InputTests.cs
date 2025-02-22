using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PuppeteerSharp.Tests.Attributes;
using PuppeteerSharp.Nunit;
using NUnit.Framework;

namespace PuppeteerSharp.Tests.InputTests
{
    public class InputTests : PuppeteerPageBaseTest
    {
        public InputTests(): base()
        {
        }

        [PuppeteerTest("input.spec.ts", "Input", "should upload the file")]
        [Skip(SkipAttribute.Targets.Firefox)]
        public async Task ShouldUploadTheFile()
        {
            await Page.GoToAsync(TestConstants.ServerUrl + "/input/fileupload.html");
            var filePath = TestConstants.FileToUpload;
            var input = await Page.QuerySelectorAsync("input");
            await input.UploadFileAsync(filePath);
            Assert.AreEqual("file-to-upload.txt", await Page.EvaluateFunctionAsync<string>("e => e.files[0].name", input));
            Assert.AreEqual("contents of the file", await Page.EvaluateFunctionAsync<string>(@"e => {
                const reader = new FileReader();
                const promise = new Promise(fulfill => reader.onload = fulfill);
                reader.readAsText(e.files[0]);
                return promise.then(() => reader.result);
            }", input));
        }
    }
}
