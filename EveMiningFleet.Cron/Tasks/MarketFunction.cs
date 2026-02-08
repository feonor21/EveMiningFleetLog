using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using ESI.NET;
using EveMiningFleet.Entities;
using EveMiningFleet.Entities.Tables;
using EveMiningFleet.Logic.EsiEve;
using EveMiningFleet.Logic.Tools;

namespace Cron.Tasks
{
    public class MarketFunction
    {
        /// <summary>
        /// Refresh la table dataprice
        /// </summary>
        public static void RefreshMarketPrice()
        {
            try
            {
                ClassLog.writeLog("RefreshMarketPrice => lancement des analyses de market");
                EveEsiConnexion tmpEsiConnection = new EveEsiConnexion();
                EsiResponse<ESI.NET.Models.Universe.Type> QueryType = null;
                using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
                {
                    int lenght = DatabaseContext.Dataprices.Count();
                    int cursor = 0;
                    double percentilePrice = double.TryParse(System.Environment.GetEnvironmentVariable("PERCENTILEPRICE"), out var value) ? value : 98.9;

                    ClassLog.writeLog("RefreshMarketPrice => percentilePrice :" + percentilePrice.ToString());

                    foreach (var DataPriceItem in DatabaseContext.Dataprices)
                    {
                        cursor++;
                        ClassLog.writeLog("analyse dataprice de " + DataPriceItem.TypeId + ", " + cursor + "/" + lenght);

                        try
                        {
                            List<orderPercentileProcessing> listofallorder = GetAllorder(10000002, ESI.NET.Enumerations.MarketOrderType.All, DataPriceItem.TypeId);

                            if (listofallorder.Any(x => !x.isbuyorder))
                                DataPriceItem.PriceSell = CalculatePercentile(percentilePrice, listofallorder.OrderByDescending(x => x.value).Where(x => !x.isbuyorder).ToList());
                            if (listofallorder.Any(x => x.isbuyorder))
                                DataPriceItem.PriceBuy = CalculatePercentile(percentilePrice, listofallorder.OrderBy(x => x.value).Where(x => x.isbuyorder).ToList());


                            //get name
                            if (DataPriceItem.Name == null || DataPriceItem.Name == "")
                            {
                                Retry.Do(() =>
                                {
                                    QueryType = tmpEsiConnection.EsiClient.Universe.Type(DataPriceItem.TypeId).Result;
                                    if (QueryType.StatusCode != HttpStatusCode.OK && QueryType.StatusCode != HttpStatusCode.NotFound)
                                        throw new Exception();
                                }, TimeSpan.FromMilliseconds(50), 5);

                                if (QueryType.StatusCode == HttpStatusCode.OK)
                                {
                                    DataPriceItem.Name = QueryType.Data.Name;
                                }
                                if (QueryType.StatusCode == HttpStatusCode.NotFound)
                                {
                                    DataPriceItem.Name = "CCP not found";
                                }


                            }

                            var fr = CultureInfo.GetCultureInfo("fr-FR");

                            ClassLog.writeLog(" " +
                                DataPriceItem.Name + " " +
                                DataPriceItem.PriceSell.ToString("N2", fr) +
                                " | " +
                                DataPriceItem.PriceBuy.ToString("N2", fr)
                            );

                        }
                        catch (Exception ex)
                        {
                            ClassLog.writeLog("erreur on :" + DataPriceItem.TypeId);
                            ClassLog.writeException(ex);
                        }
                    }

                    DatabaseContext.SaveChanges();
                }
                ClassLog.writeLog("RefreshMarketPrice => terminer");
            }
            catch (Exception ex)
            {
                ClassLog.writeLog("erreur on RefreshMarketPrice global :/");
                ClassLog.writeException(ex);
            }

        }

        /// <summary>
        /// refresh les Ore prices via les dataprices
        /// </summary>
        public static void RefreshOrePrice()
        {
            try
            {
                ClassLog.writeLog("RefreshOrePrice => lancement du calcul des prix des ORE");

                double? PriceRefinedBuy = 0;
                double? PriceRefinedSell = 0;

                using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
                {
                    foreach (var OreItem in DatabaseContext.Ores)
                    {
                        //refresh dataprice for compressed value
                        DataPrice temp;
                        if (OreItem.IdCompressed.HasValue)
                            temp = DatabaseContext.Dataprices.FirstOrDefault(x => x.TypeId == OreItem.IdCompressed.Value);
                        else
                            temp = DatabaseContext.Dataprices.FirstOrDefault(x => x.TypeId == OreItem.Id);

                        if (temp != null)
                        {
                            OreItem.PriceCompressedBuy = temp.PriceBuy;
                            OreItem.PriceCompressedSell = temp.PriceSell;
                        }

                        //refresh dataprice for refined value
                        PriceRefinedBuy = 0;
                        PriceRefinedSell = 0;
                        foreach (var InvTypeItem in DatabaseContext.Invtypematerials.Where(x => x.TypeId == OreItem.Id))
                        {
                            temp = DatabaseContext.Dataprices.FirstOrDefault(x => x.TypeId == InvTypeItem.MaterialTypeId);
                            if (temp != null)
                            {
                                PriceRefinedBuy += (temp.PriceBuy * InvTypeItem.Quantity) / OreItem.QuantityForReprocess;
                                PriceRefinedSell += (temp.PriceSell * InvTypeItem.Quantity) / OreItem.QuantityForReprocess;
                            }
                        }

                        OreItem.PriceRefinedBuy = PriceRefinedBuy;
                        OreItem.PriceRefinedSell = PriceRefinedSell;
                    }

                    DatabaseContext.SaveChanges();
                }
                ClassLog.writeLog("RefreshMarketPrice => terminer");
            }
            catch (Exception ex)
            {
                ClassLog.writeLog("erreur on RefreshMarketPrice global :/");
                ClassLog.writeException(ex);
            }

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="RegionID"></param>
        /// <param name="sellorbuy"></param>
        /// <param name="typeID"></param>
        /// <returns></returns>
        public static List<orderPercentileProcessing> GetAllorder(int regionId, ESI.NET.Enumerations.MarketOrderType orderType, int typeId, int? systemIdFilter = null, bool npcStationsOnlyIfSystemFilter = true)
        {
            var outList = new List<orderPercentileProcessing>();
            var tmpEsiConnection = new EveEsiConnexion();

            // Cache stationId -> systemId (évite de rappeler 10k fois)
            var stationSystemCache = new Dictionary<long, int>();

            int page = 1;
            int totalPages = 1;

            while (page <= totalPages)
            {
                EsiResponse<List<ESI.NET.Models.Market.Order>> resp = null;

                Retry.Do(() =>
                {
                    resp = tmpEsiConnection.EsiClient.Market.RegionOrders(regionId, orderType, page, typeId).Result;

                    // 200 OK -> OK
                    // 404 -> page hors range (mais normalement évité via X-Pages)
                    if (resp.StatusCode != HttpStatusCode.OK && resp.StatusCode != HttpStatusCode.NotFound)
                        throw new Exception("ESI call failed");
                }, TimeSpan.FromMilliseconds(100), 5);

                if (resp.StatusCode == HttpStatusCode.NotFound)
                    break;

                if (page == 1)
                {
                    totalPages = resp.Pages ?? 1;
                    if (totalPages > 1)
                        ClassLog.writeLog("HOP HOP HOP HOP HOP HOP HOP HOP HOP HOP HOP");

                }

                foreach (var order in resp.Data)
                {
                    // Filtre system (client-side)
                    if (systemIdFilter.HasValue && systemIdFilter.Value != order.LocationId)
                    {
                        continue;
                    }

                    outList.Add(new orderPercentileProcessing
                    {
                        value = order.Price,
                        volume = order.VolumeRemain,
                        isbuyorder = order.IsBuyOrder
                    });
                }

                page++;
            }

            return outList;
        }

        /// <summary>
        /// pernet le calcul du percentil
        /// </summary>
        /// <param name="percentileSetpoint"></param>
        /// <param name="arrayOrder"></param>
        /// <returns></returns>
        public static double CalculatePercentile(double percentileSetpoint, List<orderPercentileProcessing> arrayOrder)
        {
            if (arrayOrder == null || arrayOrder.Count == 0)
                return 0.0;

            List<orderPercentileProcessing> ordered =
                arrayOrder.First().isbuyorder
                    ? arrayOrder.OrderByDescending(x => x.value).ToList()
                    : arrayOrder.OrderBy(x => x.value).ToList();

            if (percentileSetpoint < 0) percentileSetpoint = 0;
            if (percentileSetpoint > 100) percentileSetpoint = 100;

            long total = 0;
            foreach (var x in ordered)
                total += x.volume;

            long VolumeTarget = (long)(total * ((100.0 - percentileSetpoint) / 100.0));

            long VolumeCumul = 0;
            double PriceCumul = 0.0;
            double medianPrice = 0.0;

            foreach (var item in ordered)
            {
                if ((VolumeCumul + item.volume) >= VolumeTarget)
                {
                    PriceCumul += (double)item.value * (VolumeTarget - VolumeCumul);
                    VolumeCumul = VolumeTarget;

                    break;
                }
                else
                {
                    VolumeCumul += item.volume;
                    PriceCumul += (double)item.value * (double)item.volume;
                }

            }
            ordered.Clear();
            ordered = null;

            if (PriceCumul == 0.0 || VolumeCumul == 0)
                return 0.0;

            medianPrice = PriceCumul / VolumeCumul;
            return medianPrice;


        }

        /// <summary>
        /// 
        /// </summary>
        public class orderPercentileProcessing
        {
            public decimal value { get; set; }
            public long volume { get; set; }
            public bool isbuyorder { get; set; }
        }

    }
}
