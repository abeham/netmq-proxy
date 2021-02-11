using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Middleware
{
    public class PongService : BackgroundService
    {
        private readonly ILogger<PongService> _logger;

        public PongService(ILogger<PongService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tcs = new TaskCompletionSource();

            var workerThread = new Thread(() => {
                using var runtime = new NetMQRuntime();
                runtime.Run(stoppingToken, RunAsync(stoppingToken));
                tcs.SetResult();
            });

            workerThread.Start();

            await tcs.Task;
        }

        private async Task RunAsync(CancellationToken stoppingToken)
        {
            using var subscriber = new SubscriberSocket($">tcp://127.0.0.1:4444");
            subscriber.Subscribe("pong");
            using var publisher = new PublisherSocket($">tcp://127.0.0.1:5555");
            _logger.LogInformation("Pong service ready");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var msg = await subscriber.ReceiveMultipartMessageAsync(cancellationToken: stoppingToken);
                    if (stoppingToken.IsCancellationRequested) break;
                    _logger.LogInformation("Pong received");
                    await Task.Delay(100);

                    publisher.SendMoreFrame("ping");
                    publisher.SendMoreFrameEmpty();
                    publisher.SendFrame(msg[2].Buffer);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "ERROR in PongService");
                    break;
                }
            }
        }
    }
}
