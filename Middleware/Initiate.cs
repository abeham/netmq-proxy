using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Middleware
{
    public class Initiate : BackgroundService
    {
        private readonly ILogger<Initiate> _logger;

        public Initiate(ILogger<Initiate> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var publisher = new PublisherSocket($">tcp://127.0.0.1:5555");
            await Task.Delay(1000);
            _logger.LogInformation("Introducing the ball");
            publisher.SendMoreFrame("ping");
            publisher.SendMoreFrameEmpty();
            publisher.SendFrame(Encoding.ASCII.GetBytes("The ball"));
        }
    }
}
