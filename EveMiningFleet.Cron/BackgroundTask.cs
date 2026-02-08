using Cron.Tasks;
using FluentScheduler;


public class BackgroundTask : Registry
{
    public BackgroundTask()
    {
        Schedule(() => ScopeFunction.checkAllScope()).ToRunEvery(1).Days().At(3, 30);
        Schedule(() => ScopeFunction.checkAllScope()).ToRunEvery(1).Days().At(15, 30);

        Schedule(() => Cron.Tasks.FleetFunction.purgeFleet()).ToRunEvery(1).Days().At(2, 30);
        Schedule(() => Cron.Tasks.FleetFunction.purgeFleet()).ToRunEvery(1).Days().At(14, 30);

        Schedule(() => Cron.Tasks.MarketFunction.RefreshMarketPrice()).ToRunEvery(1).Days().At(0, 0);
        Schedule(() => Cron.Tasks.MarketFunction.RefreshMarketPrice()).ToRunEvery(1).Days().At(12, 0);

        Schedule(() => Cron.Tasks.OreFunction.RefreshAllOreDataWithEsiDump()).ToRunEvery(1).Days().At(13, 15);

        //Schedule(() => Cron.Tasks.HistoryFunction.AddHistoryFleet()).ToRunNow().AndEvery(1).Hours();
    }

}
