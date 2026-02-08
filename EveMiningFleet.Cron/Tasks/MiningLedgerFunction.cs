using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using ESI.NET;
using EveMiningFleet.Entities;
using EveMiningFleet.Entities.Tables;
using EveMiningFleet.Logic.EsiEve;
using EveMiningFleet.Logic.Tools;
using Microsoft.EntityFrameworkCore;

namespace Cron.Tasks
{
    public class MiningLedgerFunction
    {
        //MINING LEDGER
        public static void Refresh()
        {
            List<Fleet> tmplist;
            List<Fleetcharacter> allFleetCharacterActiv = new List<Fleetcharacter>();
            int seuilFleetCharacterScan = 0;
            int fleetCharacterScannerCount = 0;



            using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
            {
                tmplist = DatabaseContext.Fleets.Where(x => x.End == null).ToList();

                foreach (Fleet FleetOpen in tmplist)
                {
                    allFleetCharacterActiv.AddRange(DatabaseContext.Fleetcharacters.Include("Character.Lastmininglogs")
                                                    .Where(fleetchar => fleetchar.FleetId == FleetOpen.Id && fleetchar.Quit == null).ToList());
                }

                seuilFleetCharacterScan = Convert.ToInt32(Math.Ceiling(allFleetCharacterActiv.Count() * 0.13));

                var newCharacter = allFleetCharacterActiv.Where(x => !x.LastRefresh.HasValue).OrderBy(x => x.Join);

                var activCharacterToRead = allFleetCharacterActiv.Where(x => x.LastRefresh.HasValue &&
                                                                    x.LastRefresh.Value.AddMinutes(11) <= DateTime.Now.ToUniversalTime())
                                                                .OrderBy(x => x.LastRefresh.Value).Take(seuilFleetCharacterScan);

                foreach (Fleetcharacter fleetcharacteritem in newCharacter)
                {
                    if (SetRealMiningLog(fleetcharacteritem, false)) fleetCharacterScannerCount += 1;
                }
                foreach (Fleetcharacter fleetcharacteritem in activCharacterToRead)
                {
                    if (SetRealMiningLog(fleetcharacteritem, true)) fleetCharacterScannerCount += 1;
                }
            }

            if (fleetCharacterScannerCount > 0)
            {
                if (fleetCharacterScannerCount >= seuilFleetCharacterScan)
                {
                    ClassLog.writeLog("RefreshMiningLedger => " + tmplist.Count() + " fleet, " + allFleetCharacterActiv.Count() + " Character et " + fleetCharacterScannerCount + " scanner. Lissage des analyses.");
                }
                else
                {
                    ClassLog.writeLog("RefreshMiningLedger => " + tmplist.Count() + " fleet, " + allFleetCharacterActiv.Count() + " Character et " + fleetCharacterScannerCount + " scanner.");
                }
            }

            tmplist = null;
        }


        static readonly object insertOreLock = new object();
        public static bool SetRealMiningLog(Fleetcharacter TargetFleetCharacter, bool CompareWithLastLog = false)
        {
            if (ScopeFunction.checkOneScope(TargetFleetCharacter.CharacterId) == false)
            {
                using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
                {
                    if (CompareWithLastLog)
                        ClassLog.writeLog(TargetFleetCharacter.Character.Name + " lose his token.");
                    else
                        ClassLog.writeLog(TargetFleetCharacter.Character.Name + " lose his token, first check.");

                    var fleet = DatabaseContext.Fleets.First(x => x.Id == TargetFleetCharacter.FleetId);

                    if (fleet.CharacterId != TargetFleetCharacter.CharacterId)
                    {
                        DatabaseContext.Fleetcharacters.First(x => x.Id == TargetFleetCharacter.Id).Quit = DateTime.Now.ToUniversalTime();
                        DatabaseContext.SaveChanges();
                    }
                    else
                    {
                        fleet.End = DateTime.Now.ToUniversalTime();
                        fleet.ReasonClose = "lose token au fleetCommander";
                        ClassLog.writeLog(TargetFleetCharacter.Character.Name + " lose his token, SO close fleet because his fleetcommander");
                        DatabaseContext.SaveChanges();
                    }


                }
                return false;
            }

            try
            {
                bool updateFleetDate = false;
                List<Lastmininglog> LastLogFromCCP = new List<Lastmininglog>();

                //Recuperation des derniere 24h de log de CCP
                Lastmininglog tmptarget;
                List<ESI.NET.Models.Industry.Entry> ListItems = GetLogFromCCP(TargetFleetCharacter.Character);


                Fleet fleettarget;
                using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
                {
                    fleettarget = DatabaseContext.Fleets.First(x => x.Id == TargetFleetCharacter.FleetId);
                }



                if (ListItems != null && ListItems.Count(x => x.Date >= fleettarget.Begin.AddDays(-1).Date) > 0)
                {

                    foreach (ESI.NET.Models.Industry.Entry item in ListItems.Where(x => x.Date >= fleettarget.Begin.Date.AddDays(-1)))
                    {
                        tmptarget = LastLogFromCCP.FirstOrDefault(x => x.CharacterId == TargetFleetCharacter.CharacterId && x.OreId == item.TypeId && x.Date == item.Date);
                        if (tmptarget == null)
                            LastLogFromCCP.Add(new Lastmininglog() { CharacterId = TargetFleetCharacter.CharacterId, Date = item.Date, OreId = item.TypeId, Quantity = item.Quantity });
                        else
                            tmptarget.Quantity += item.Quantity;
                    }

                    // Insert Unknow ORE
                    //
                    List<int> OreIdList = new List<int>();
                    foreach (Lastmininglog item in LastLogFromCCP)
                    {
                        if (!OreIdList.Contains(item.OreId))
                            OreIdList.Add(item.OreId);
                    }
                    lock (insertOreLock)
                    {
                        List<int> NewOreInsert;
                        using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
                        {
                            NewOreInsert = OreIdList.Distinct().Where(us => !DatabaseContext.Ores.Any(u => u.Id == us)).ToList();
                            if (NewOreInsert.Count > 0)
                            {
                                foreach (int OreItemId in NewOreInsert)
                                {
                                    DatabaseContext.Ores.Add(new Ore() { Id = OreItemId, Name = _Constante.OreNewItemName, Volume = 1, PriceCompressedBuy = 0, PriceCompressedSell = 0, PriceRefinedBuy = 0, PriceRefinedSell = 0 });
                                }
                                DatabaseContext.SaveChanges();
                            }
                        }
                        foreach (int OreItemId in NewOreInsert)
                        {
                            OreFunction.RefreshOreDb(OreItemId);
                        }
                        NewOreInsert = null;
                    }
                    OreIdList = null;

                    //calculation des Log reel de minage par rapport a la derniere update
                    // mise a jour des mininglog du player
                    if (CompareWithLastLog)
                    {
                        //calculation des Log reel de minage par rapport a la derniere update
                        Dictionary<int, long> dicoOfOreQuantity = new Dictionary<int, long>();
                        foreach (Lastmininglog LogFromCCP in LastLogFromCCP)
                        {
                            if (!dicoOfOreQuantity.ContainsKey(LogFromCCP.OreId))
                                dicoOfOreQuantity.Add(LogFromCCP.OreId, 0);

                            Lastmininglog LastLogInDb = TargetFleetCharacter.Character.Lastmininglogs.FirstOrDefault(x => x.Date == LogFromCCP.Date && x.OreId == LogFromCCP.OreId);
                            if (LastLogInDb == null)
                            {
                                dicoOfOreQuantity[LogFromCCP.OreId] += LogFromCCP.Quantity;
                            }
                            else
                            {
                                dicoOfOreQuantity[LogFromCCP.OreId] += LogFromCCP.Quantity - LastLogInDb.Quantity;
                            }
                        }

                        // mise a jour des mininglog du player
                        Mininglog mininglog;
                        using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
                        {
                            foreach (KeyValuePair<int, long> LastRealLog in dicoOfOreQuantity.Where(x => x.Value > 0))
                            {
                                mininglog = DatabaseContext.Mininglogs.FirstOrDefault(x => x.FleetCharacterId == TargetFleetCharacter.Id && x.OreId == LastRealLog.Key);
                                if (mininglog == null)
                                {
                                    mininglog = new Mininglog();
                                    mininglog.FleetCharacterId = TargetFleetCharacter.Id;
                                    mininglog.OreId = LastRealLog.Key;
                                    DatabaseContext.Mininglogs.Add(mininglog);
                                }
                                mininglog.Quantity += (int)LastRealLog.Value;
                                updateFleetDate = true;
                            }
                            DatabaseContext.SaveChanges();
                        }
                        mininglog = null;
                        dicoOfOreQuantity = null;
                    }

                    // update des LastLog du player
                    using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
                    {
                        // on vire les ancien
                        DatabaseContext.Lastmininglogs.RemoveRange(DatabaseContext.Lastmininglogs.Where(x => x.CharacterId == TargetFleetCharacter.CharacterId));
                        DatabaseContext.SaveChanges();
                        // on met les derniers
                        DatabaseContext.Lastmininglogs.AddRange(LastLogFromCCP);
                        DatabaseContext.SaveChanges();
                    }

                }
                ListItems = null;
                tmptarget = null;
                LastLogFromCCP = null;

                using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
                {
                    DatabaseContext.Fleetcharacters.First(x => x.Id == TargetFleetCharacter.Id).LastRefresh = DateTime.Now.ToUniversalTime();

                    if (updateFleetDate)
                        DatabaseContext.Fleets.First(x => x.Id == TargetFleetCharacter.FleetId).LastFullRefresh = DateTime.Now.ToUniversalTime();

                    DatabaseContext.SaveChanges();

                }

            }
            catch (HttpRequestException ex)
            {
                if (ex.Message == "Name or service not known")
                {
                    ClassLog.writeLog("/!\\ CCP_API is inaccessible, so I should be down? /!\\ ");
                    throw ex;
                }
                else
                    ClassLog.writeException(ex);
            }
            catch (Exception ex)
            {
                ClassLog.writeException(ex);
            }
            return true;
        }

        public static List<ESI.NET.Models.Industry.Entry> GetLogFromCCP(Character TargetJoueur)
        {
            List<ESI.NET.Models.Industry.Entry> Response = null;
            try
            {
                if (TargetJoueur.Id > 0)
                {
                    Response = Retry.Do(() =>
                    {
                        EveEsiConnexion tmpEsiConnection = new EveEsiConnexion();
                        tmpEsiConnection.RefreshConnection(TargetJoueur.RefreshToken).Wait();

                        if (tmpEsiConnection.authorizedCharacterData != null)
                        {
                            EsiResponse<List<ESI.NET.Models.Industry.Entry>> tmp = tmpEsiConnection.EsiClient.Industry.MiningLedger(1).Result;
                            tmpEsiConnection = null;
                            return tmp.Data;
                        }
                        else
                        {
                            tmpEsiConnection = null;
                            return null;
                        }
                    }, TimeSpan.FromMilliseconds(0), 10);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Response;
        }

    }
}
