using System.Threading.Tasks;
using NUnit.Framework;

namespace PuppeteerSharp.Tests
{
    public class PuppeteerPageBaseTest : PuppeteerBrowserContextBaseTest
    {
        protected IPage Page { get; set; }

        [SetUp]
        public async Task CreatePageAsync()
        {
            Page = await Context.NewPageAsync();
            Page.DefaultTimeout = System.Diagnostics.Debugger.IsAttached ? TestConstants.DebuggerAttachedTestTimeout : TestConstants.DefaultPuppeteerTimeout;
        }

        [TearDown]
        public Task ClosePageAsync() => Page.CloseAsync();

        protected Task WaitForError()
        {
            var wrapper = new TaskCompletionSource<bool>();

            void errorEvent(object sender, ErrorEventArgs e)
            {
                wrapper.SetResult(true);
                Page.Error -= errorEvent;
            }

            Page.Error += errorEvent;

            return wrapper.Task;
        }
    }
}
