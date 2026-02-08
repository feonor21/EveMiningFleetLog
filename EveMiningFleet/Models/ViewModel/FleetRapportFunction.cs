using System;
using System.Collections.Generic;
using System.Linq;
using EveMiningFleet.Entities.Tables;

namespace EveMiningFleet.Models.ViewModel
{
    public static class FleetRapportFunction
    {
        public class ReadMiningLog
        {
            public int Ore_Id;
            public string Name;
            public int Quantity;
            public float VolumeTotal;
            public float VolumeOre;
            public double PriceTotalCompressedSell;
            public double PriceTotalCompressedBuy;
            public double PriceTotalRefinedSell;
            public double PriceTotalRefinedBuy;
            public double PriceOreCompressedSell;
            public double PriceOreCompressedBuy;
            public double PriceOreRefinedSell;
            public double PriceOreRefinedBuy;
        }
        public class ReadRapportStructure
        {
            public string NameParent;
            public string Namecharacter;
            public int idCharacter;
            public List<ReadMiningLog> loot;
        }

        public static List<ReadMiningLog> GetAllLootFromFleet(Fleet FleetItem, int IDCharacter = 0, Single Taxe = 0)
        {
            var listresponse = new List<ReadMiningLog>();
            ReadMiningLog check;
            IEnumerable<Fleetcharacter> fleetcharacters;
            if (IDCharacter == 0)
                fleetcharacters = FleetItem.Fleetcharacters;
            else
                fleetcharacters = FleetItem.Fleetcharacters.Where(x => x.CharacterId == IDCharacter);

            foreach (var FleetChar in fleetcharacters)
            {
                foreach (var miningLog in FleetChar.Mininglogs)
                {
                    check = listresponse.FirstOrDefault(x => x.Ore_Id == miningLog.OreId);
                    if (check == null)
                    {
                        check = new ReadMiningLog();
                        check.Ore_Id = miningLog.OreId;
                        check.Name = miningLog.Ore.Name;
                        check.Quantity = 0;
                        check.VolumeTotal = 0;
                        check.VolumeOre = 0;
                        check.PriceOreCompressedBuy = 0;
                        check.PriceOreCompressedSell = 0;
                        check.PriceOreRefinedBuy = 0;
                        check.PriceOreRefinedSell = 0;
                        if (miningLog.Ore.Volume.HasValue)
                            check.VolumeOre = miningLog.Ore.Volume.Value;
                        if (miningLog.Ore.PriceCompressedBuy.HasValue)
                            check.PriceOreCompressedBuy = miningLog.Ore.PriceCompressedBuy.Value;
                        if (miningLog.Ore.PriceCompressedSell.HasValue)
                            check.PriceOreCompressedSell = miningLog.Ore.PriceCompressedSell.Value;
                        if (miningLog.Ore.PriceRefinedBuy.HasValue)
                            check.PriceOreRefinedBuy = miningLog.Ore.PriceRefinedBuy.Value;
                        if (miningLog.Ore.PriceRefinedSell.HasValue)
                            check.PriceOreRefinedSell = miningLog.Ore.PriceRefinedSell.Value;
                        listresponse.Add(check);
                    }
                    check.Quantity += (int)Math.Floor(miningLog.Quantity * ((100 - Taxe) / 100));
                }
            }

            foreach (var item in listresponse)
            {
                item.VolumeTotal = (float)item.Quantity * (float)item.VolumeOre;
                item.PriceTotalCompressedBuy = (float)item.Quantity * (float)item.PriceOreCompressedBuy;
                item.PriceTotalCompressedSell = (float)item.Quantity * (float)item.PriceOreCompressedSell;
                item.PriceTotalRefinedSell = (float)item.Quantity * (float)item.PriceOreRefinedSell * (float)(FleetItem.Reprocess / 100);
                item.PriceTotalRefinedBuy = (float)item.Quantity * (float)item.PriceOreRefinedBuy * (float)(FleetItem.Reprocess / 100);
            }

            listresponse = listresponse.Where(x => x.Quantity > 0).ToList();

            return listresponse;
        }

        public static ICollection<Fleettaxes> GetAllTaxeFromFleet(Fleet FleetItem)
        {
            return FleetItem.Fleettaxes;
        }

        public static Single GetMontantTaxeFromFleet(Fleet FleetItem)
        {
            Single response = 0;
            foreach (var item in GetAllTaxeFromFleet(FleetItem))
            {
                response += item.Taxe;
            }
            return response;
        }

        public static List<Character> GetAllCharacterFromFleet(Fleet FleetItem)
        {
            var listresponse = new List<Character>();
            foreach (var item in FleetItem.Fleetcharacters.GroupBy(y => y.CharacterId))
            {
                if (item != null && item.First() != null && item.First().Character != null)
                    listresponse.Add(item.First().Character);
            }
            return listresponse;
        }

        public static List<Character> GetAllCharacterFromMyGroup(Fleet FleetItem, int characterID)
        {
            var listresponse = new List<Character>();
            Fleetgroup checktwo = FleetItem.Fleetgroups.FirstOrDefault(x => x.Fleetgroupcharacters.Any(y => y.CharacterId == characterID));
            if (checktwo != null)
            {
                foreach (var item in checktwo.Fleetgroupcharacters)
                {
                    listresponse.Add(item.Character);
                }
            }
            return listresponse;

        }

        public static List<ReadRapportStructure> GetLightRapportFromFleet(Fleet FleetItem, int? Distribution)
        {
            var listresponse = new List<ReadRapportStructure>();

            Single MontantTaxe = GetMontantTaxeFromFleet(FleetItem);
            ICollection<Fleettaxes> AllTaxe = GetAllTaxeFromFleet(FleetItem);
            List<ReadMiningLog> AllLoot = GetAllLootFromFleet(FleetItem);
            List<Character> Allcharacters = GetAllCharacterFromFleet(FleetItem);
            float FleetVolume = AllLoot.Sum(item => item.VolumeTotal);


            //chaque taxe non affecter
            foreach (var fleetTaxeWithNoCharacter in AllTaxe.Where(x => x.CharacterId == null))
            {
                ReadRapportStructure Insert = new ReadRapportStructure() { NameParent = fleetTaxeWithNoCharacter.Name, Namecharacter = fleetTaxeWithNoCharacter.Name, idCharacter = 0 };
                Insert.loot = new List<ReadMiningLog>();
                listresponse.Add(Insert);

                foreach (var miningLog in GetAllLootFromFleet(FleetItem, 0, 100 - fleetTaxeWithNoCharacter.Taxe))
                {
                    var check = Insert.loot.FirstOrDefault(x => x.Ore_Id == miningLog.Ore_Id);
                    if (check == null)
                    {
                        check = miningLog;
                        Insert.loot.Add(check);
                    }
                    else
                    {
                        check.Quantity += miningLog.Quantity;
                        check.VolumeTotal += miningLog.VolumeTotal;
                        check.PriceTotalCompressedBuy += miningLog.PriceTotalCompressedBuy;
                        check.PriceTotalCompressedSell += miningLog.PriceTotalCompressedSell;
                        check.PriceTotalRefinedBuy += miningLog.PriceTotalRefinedBuy;
                        check.PriceTotalRefinedSell += miningLog.PriceTotalRefinedSell;
                    }
                }
                Insert.NameParent += " (" + fleetTaxeWithNoCharacter.Taxe.ToString("0.#") + "%)";
            }

            //chaque character apres
            foreach (var fleetgroups in FleetItem.Fleetgroups)
            {
                string memoparent = "";
                ReadRapportStructure Insert = null;
                foreach (var fleetgroupcharacters in fleetgroups.Fleetgroupcharacters)
                {
                    if (memoparent == "")
                    {
                        memoparent = fleetgroupcharacters.Character.Name;
                        Insert = new ReadRapportStructure() { NameParent = memoparent, Namecharacter = memoparent, idCharacter = fleetgroupcharacters.Character.Id };
                        Insert.loot = new List<ReadMiningLog>();
                        listresponse.Add(Insert);
                    }

                    List<ReadMiningLog> LootPlayer = GetAllLootFromFleet(FleetItem, fleetgroupcharacters.Character.Id, 0);
                    float percentPlayer = ((float)100 - MontantTaxe) * (LootPlayer.Sum(item => item.VolumeTotal) / FleetVolume);

                    switch (Distribution)
                    {
                        case 2:
                            percentPlayer = ((float)100 - MontantTaxe) / (float)Allcharacters.Count;
                            LootPlayer = GetAllLootFromFleet(FleetItem, 0, 100 - percentPlayer);
                            break;
                        case 1:
                            percentPlayer = ((float)100 - MontantTaxe) * (LootPlayer.Sum(item => item.VolumeTotal) / FleetVolume);
                            LootPlayer = GetAllLootFromFleet(FleetItem, 0, 100 - percentPlayer);
                            break;
                        default:
                            percentPlayer = ((float)100 - MontantTaxe) * (LootPlayer.Sum(item => item.VolumeTotal) / FleetVolume);
                            LootPlayer = GetAllLootFromFleet(FleetItem, fleetgroupcharacters.Character.Id, MontantTaxe);
                            break;
                    }


                    foreach (var miningLog in LootPlayer)
                    {
                        var check = Insert.loot.FirstOrDefault(x => x.Ore_Id == miningLog.Ore_Id);
                        if (check == null)
                        {
                            check = miningLog;
                            Insert.loot.Add(check);
                        }
                        else
                        {
                            check.Quantity += miningLog.Quantity;
                            check.VolumeTotal += miningLog.VolumeTotal;
                            check.PriceTotalCompressedBuy += miningLog.PriceTotalCompressedBuy;
                            check.PriceTotalCompressedSell += miningLog.PriceTotalCompressedSell;
                            check.PriceTotalRefinedBuy += miningLog.PriceTotalRefinedBuy;
                            check.PriceTotalRefinedSell += miningLog.PriceTotalRefinedSell;
                        }
                    }

                    if (AllTaxe.Any(x => x.CharacterId == fleetgroupcharacters.Character.Id))
                    {
                        foreach (var item in AllTaxe.Where(x => x.CharacterId == fleetgroupcharacters.Character.Id))
                        {
                            foreach (var miningLog in GetAllLootFromFleet(FleetItem, 0, 100 - item.Taxe))
                            {
                                var check = Insert.loot.FirstOrDefault(x => x.Ore_Id == miningLog.Ore_Id);
                                if (check == null)
                                {
                                    check = miningLog;
                                    Insert.loot.Add(check);
                                }
                                else
                                {
                                    check.Quantity += miningLog.Quantity;
                                    check.VolumeTotal += miningLog.VolumeTotal;
                                    check.PriceTotalCompressedBuy += miningLog.PriceTotalCompressedBuy;
                                    check.PriceTotalCompressedSell += miningLog.PriceTotalCompressedSell;
                                    check.PriceTotalRefinedBuy += miningLog.PriceTotalRefinedBuy;
                                    check.PriceTotalRefinedSell += miningLog.PriceTotalRefinedSell;
                                }
                            }
                        }
                    }

                }

                float percentGroup = 100 * (Insert.loot.Sum(item => item.VolumeTotal) / FleetVolume);
                Insert.NameParent += " (" + percentGroup.ToString("0.#") + "%)";
            }




            AllTaxe = null;
            AllLoot = null;
            return listresponse;
        }

        public static List<ReadRapportStructure> GetRapportFromFleet(Fleet FleetItem, int? Distribution)
        {
            var listresponse = new List<ReadRapportStructure>();

            Single MontantTaxe = GetMontantTaxeFromFleet(FleetItem);
            ICollection<Fleettaxes> AllTaxe = GetAllTaxeFromFleet(FleetItem);
            List<ReadMiningLog> AllLoot = GetAllLootFromFleet(FleetItem);
            List<Character> Allcharacters = GetAllCharacterFromFleet(FleetItem);
            float FleetVolume = AllLoot.Sum(item => item.VolumeTotal);



            //chaque taxe non affecter
            foreach (var fleetTaxeWithNoCharacter in AllTaxe.Where(x => x.CharacterId == null))
            {
                ReadRapportStructure Insert = new ReadRapportStructure() { NameParent = fleetTaxeWithNoCharacter.Name, Namecharacter = fleetTaxeWithNoCharacter.Name, idCharacter = 0 };
                Insert.loot = new List<ReadMiningLog>();
                listresponse.Add(Insert);

                foreach (var miningLog in GetAllLootFromFleet(FleetItem, 0, 100 - fleetTaxeWithNoCharacter.Taxe))
                {
                    var check = Insert.loot.FirstOrDefault(x => x.Ore_Id == miningLog.Ore_Id);
                    if (check == null)
                    {
                        check = miningLog;
                        Insert.loot.Add(check);
                    }
                    else
                    {
                        check.Quantity += miningLog.Quantity;
                        check.VolumeTotal += miningLog.VolumeTotal;
                        check.PriceTotalCompressedBuy += miningLog.PriceTotalCompressedBuy;
                        check.PriceTotalCompressedSell += miningLog.PriceTotalCompressedSell;
                        check.PriceTotalRefinedBuy += miningLog.PriceTotalRefinedBuy;
                        check.PriceTotalRefinedSell += miningLog.PriceTotalRefinedSell;
                    }
                }
                Insert.NameParent += " (" + fleetTaxeWithNoCharacter.Taxe.ToString("0.#") + "%)";
            }


            foreach (var fleetgroups in FleetItem.Fleetgroups)
            {
                string memoparent = "";
                foreach (var fleetgroupcharacters in fleetgroups.Fleetgroupcharacters)
                {
                    if (memoparent == "")
                        memoparent = fleetgroupcharacters.Character.Name;

                    ReadRapportStructure Insert = new ReadRapportStructure() { NameParent = memoparent, Namecharacter = fleetgroupcharacters.Character.Name };
                    List<ReadMiningLog> LootPlayer = GetAllLootFromFleet(FleetItem, fleetgroupcharacters.Character.Id);
                    float percentPlayer;


                    switch (Distribution)
                    {
                        case 2:
                            percentPlayer = ((float)100 - MontantTaxe) / (float)Allcharacters.Count;
                            LootPlayer = GetAllLootFromFleet(FleetItem, 0, 100 - percentPlayer);
                            break;
                        case 1:
                            percentPlayer = ((float)100 - MontantTaxe) * (LootPlayer.Sum(item => item.VolumeTotal) / FleetVolume);
                            LootPlayer = GetAllLootFromFleet(FleetItem, 0, 100 - percentPlayer);
                            break;
                        default:
                            percentPlayer = ((float)100 - MontantTaxe) * (LootPlayer.Sum(item => item.VolumeTotal) / FleetVolume);
                            LootPlayer = GetAllLootFromFleet(FleetItem, fleetgroupcharacters.Character.Id, MontantTaxe);
                            break;
                    }

                    Insert.Namecharacter += " (" + percentPlayer.ToString("0.#") + "%)";
                    Insert.loot = LootPlayer;
                    if (Insert.loot.Count > 0)
                        listresponse.Add(Insert);

                    if (AllTaxe.Any(x => x.CharacterId == fleetgroupcharacters.Character.Id))
                    {
                        foreach (var item in AllTaxe.Where(x => x.CharacterId == fleetgroupcharacters.Character.Id))
                        {
                            Insert = new ReadRapportStructure() { NameParent = memoparent, Namecharacter = fleetgroupcharacters.Character.Name + " => Taxes : " + item.Name + " (" + item.Taxe.ToString("0.#") + "%)" };
                            Insert.loot = GetAllLootFromFleet(FleetItem, 0, 100 - item.Taxe);
                            if (Insert.loot.Count > 0)
                                listresponse.Add(Insert);
                        }
                    }
                }
            }
            AllTaxe = null;
            AllLoot = null;
            return listresponse;
        }



    }
}