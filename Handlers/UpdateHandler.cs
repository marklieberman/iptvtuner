using System;
using System.Threading.Tasks;
using uhttpsharp;

namespace IPTVTuner.Handlers
{
    /**
     * Administrative route to trigger a provider lineup and EPG update.
     */
    class UpdateHandler : MyHandler, IHttpRequestHandler
    {
        private readonly Updater updater;

        public UpdateHandler(Updater updater)
        {
            this.updater = updater;
        }

        public Task Handle(IHttpContext context, Func<Task> next)
        {
            var discard = updater.Update();

            // 200 OK means the update has started.
            context.Response = new HttpResponse(HttpResponseCode.Ok, string.Empty, true);
            
            return Task.Factory.GetCompleted();
        }
    }
}
