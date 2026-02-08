using System;
using System.Threading;
using System.Threading.Tasks;
using EveMiningFleet.Entities;
using EveMiningFleet.Logic.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cron
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
            {
                DatabaseContext.Database.Migrate();
            }


            FluentScheduler.JobManager.Initialize(new BackgroundTask());
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                });
    }


    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1000, stoppingToken);
            ClassLog.writeLog("Worker => is launched");
            await Task.Delay(1000, stoppingToken);

            Cron.Tasks.MarketFunction.RefreshMarketPrice();

            var period = TimeSpan.FromSeconds(60);
            var next = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                next = next.Add(period);

                Cron.Tasks.MiningLedgerFunction.Refresh();

                var delay = next - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                    await Task.Delay(delay, stoppingToken);
            }

            ClassLog.writeLog("Worker => is stopped");
        }
    }
}
