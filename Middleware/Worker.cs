using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Middleware
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("XPub/XSub starting...");
                using var _publisher = new XPublisherSocket($"@tcp://127.0.0.1:4444");
                _logger.LogInformation($"XPub at 127.0.0.1:4444.");
                using var _subscriber = new XSubscriberSocket($"@tcp://127.0.0.1:5555");
                _logger.LogInformation($"XSub at 127.0.0.1:5555.");
                var proxy = new Proxy(_subscriber, _publisher);
                var t = Task.Factory.StartNew(_ => {
                    try
                    {
                        proxy.Start();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "ERROR in Proxy Task");
                    }
                }, TaskCreationOptions.LongRunning);
                await Task.Delay(-1, stoppingToken).ContinueWith(_ => { });
                proxy.Stop();
                await t; // to avoid proxy accessing disposed objects
            }
            catch (Exception e)
            {
                _logger.LogError(e, "ERROR in Worker");
            }
            finally
            {
                _logger.LogInformation("XPub/XSub terminating");
            }
        }
    }
}
