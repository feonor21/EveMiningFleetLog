using System;
using System.Linq;
using EveMiningFleet.Entities.Tables;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EveMiningFleet.Controllers
{
    public class ToolsController : BaseController
    {
        private readonly ILogger<ToolsController> _logger;
        private readonly IWebHostEnvironment _env;

        public ToolsController(ILogger<ToolsController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public IActionResult Reprocess(decimal? BuyBack, decimal? ReprocessingEfficiency, string rapport)
        {
            ViewBag.Title = "Calcul reprocess";
            ViewBag.Index = true;
            ViewBag.Description = "allows you to calculate the quantity and value of minerals after reprocessing";
            EveMiningFleet.Models.ViewModel.ReprocessModel model = new Models.ViewModel.ReprocessModel();


            if (ReprocessingEfficiency != null && rapport != null && rapport != "")
            {
                if (ReprocessingEfficiency.HasValue)
                    model.ReprocessingEfficiency = ReprocessingEfficiency.Value;

                if (BuyBack.HasValue)
                    model.Buyback = BuyBack.Value;

                model.ConversionLancer = true;
                try
                {

                    Ore oreparent;
                    Ore oreitem;
                    string[] AllLine = rapport.Split("\r\n");
                    foreach (string item in AllLine)
                    {
                        string[] AllColumn;

                        if (item.Contains("\t"))
                            AllColumn = item.Split("\t");
                        else
                            AllColumn = item.Split("    ");

                        if (item != "" && AllColumn.Length >= 2)
                        {
                            string oreName = AllColumn[0].ToLower().Replace(" ", "").Replace("compressed", "");
                            int Quantity = 0;

                            try
                            {
                                //remove all space FUCKING UTF8 character 160 or 16#A0
                                Quantity = int.Parse(AllColumn[1].Replace(" ", "").Replace(((char)160).ToString(), ""));
                            }
                            catch (Exception)
                            {
                                throw new Exception("Erreur de formatage. un champs numeric est textuelle");
                            }

                            //on check le si on a le ore
                            if (!model.ReprocessOreModels.ContainsKey(oreName))
                            {

                                oreitem = DbContext.Ores.FirstOrDefault(x => x.Name.Replace(" ", "").ToLower() == oreName);
                                if (oreitem != null)
                                {
                                    var tmp = new EveMiningFleet.Models.ViewModel.ReprocessOreModel();
                                    tmp.OreItem = oreitem;
                                    tmp.idreprocess = oreitem.Id;

                                    oreparent = DbContext.Ores.FirstOrDefault(x => x.IdCompressed == oreitem.Id);
                                    if (oreparent != null)
                                        tmp.idreprocess = oreparent.Id;

                                    model.ReprocessOreModels.Add(oreName, tmp);
                                }
                            }

                            //on check si le ore exist si il exist toujours pas c'est que c'est pas un minerai
                            if (model.ReprocessOreModels.ContainsKey(oreName))
                                model.ReprocessOreModels[oreName].quantity += Quantity;

                        }
                    }

                    model.ConversionOK = true;

                }
                catch (Exception ex)
                {

                }
            }

            if (model.ConversionOK)
            {
                foreach (var oreitem in model.ReprocessOreModels)
                {
                    if (oreitem.Value.quantity >= oreitem.Value.OreItem.QuantityForReprocess)
                    {
                        foreach (var InvTypeItem in DbContext.Invtypematerials.Where(x => x.TypeId == oreitem.Value.idreprocess))
                        {
                            //on check le si on a le ore
                            if (!model.ReprocessOreMaterialModels.ContainsKey(InvTypeItem.MaterialTypeId))
                            {
                                var tmpdataprice = DbContext.Dataprices.FirstOrDefault(x => x.TypeId == InvTypeItem.MaterialTypeId);
                                if (tmpdataprice != null)
                                {
                                    var tmp = new EveMiningFleet.Models.ViewModel.ReprocessOreMaterialModel();
                                    tmp.dataprice = tmpdataprice;

                                    model.ReprocessOreMaterialModels.Add(InvTypeItem.MaterialTypeId, tmp);
                                }
                            }

                            //on check si le minerai exist si il exist toujours pas c'est qu'on le connais
                            if (model.ReprocessOreMaterialModels.ContainsKey(InvTypeItem.MaterialTypeId))
                            {
                                int coef = (int)Math.Floor((double)oreitem.Value.quantity / (double)oreitem.Value.OreItem.QuantityForReprocess.Value);
                                if (model.ReprocessingEfficiency > 0)
                                    model.ReprocessOreMaterialModels[InvTypeItem.MaterialTypeId].Quantity += (int)Math.Floor((decimal)InvTypeItem.Quantity * (decimal)coef * (decimal)(model.ReprocessingEfficiency / 100));
                                else
                                    model.ReprocessOreMaterialModels[InvTypeItem.MaterialTypeId].Quantity += InvTypeItem.Quantity * coef;

                            }
                        }
                    }

                }

                foreach (var ReprocessOre in model.ReprocessOreModels)
                {
                    if (ReprocessOre.Value.OreItem.PriceCompressedBuy.HasValue)
                        ReprocessOre.Value.PriceCompressedBuy = ReprocessOre.Value.quantity * ReprocessOre.Value.OreItem.PriceCompressedBuy.Value * (double)(model.Buyback / 100);
                    else
                        ReprocessOre.Value.PriceCompressedBuy = 0;

                    if (ReprocessOre.Value.OreItem.PriceCompressedSell.HasValue)
                        ReprocessOre.Value.PriceCompressedSell = ReprocessOre.Value.quantity * ReprocessOre.Value.OreItem.PriceCompressedSell.Value * (double)(model.Buyback / 100);
                    else
                        ReprocessOre.Value.PriceCompressedSell = 0;

                    if (ReprocessOre.Value.OreItem.PriceRefinedBuy.HasValue)
                        ReprocessOre.Value.PriceRefinedBuy = ReprocessOre.Value.quantity * ReprocessOre.Value.OreItem.PriceRefinedBuy.Value * (double)(model.Buyback / 100) * ((double)model.ReprocessingEfficiency / 100);
                    else
                        ReprocessOre.Value.PriceRefinedBuy = 0;

                    if (ReprocessOre.Value.OreItem.PriceRefinedSell.HasValue)
                        ReprocessOre.Value.PriceRefinedSell = ReprocessOre.Value.quantity * ReprocessOre.Value.OreItem.PriceRefinedSell.Value * (double)(model.Buyback / 100) * ((double)model.ReprocessingEfficiency / 100);
                    else
                        ReprocessOre.Value.PriceRefinedSell = 0;
                }

                foreach (var ReprocessOreMaterial in model.ReprocessOreMaterialModels)
                {
                    ReprocessOreMaterial.Value.PriceRefinedBuy = ReprocessOreMaterial.Value.Quantity * ReprocessOreMaterial.Value.dataprice.PriceBuy * (double)(model.Buyback / 100);
                    ReprocessOreMaterial.Value.PriceRefinedSell = ReprocessOreMaterial.Value.Quantity * ReprocessOreMaterial.Value.dataprice.PriceSell * (double)(model.Buyback / 100);
                }

            }

            return View("Reprocess", model);
        }

        public IActionResult WhatIShouldMine(decimal? ReprocessingEfficiency, string rapport)
        {
            ViewBag.Title = "What I Should Mine";
            ViewBag.Index = true;
            ViewBag.Description = "by a survey scanner or manually,specify all your asteroids and it will answer what is more profitable";
            EveMiningFleet.Models.ViewModel.WhatIShouldMineModel model = new Models.ViewModel.WhatIShouldMineModel();


            if (ReprocessingEfficiency != null && rapport != null && rapport != "")
            {
                if (ReprocessingEfficiency.HasValue)
                    model.ReprocessingEfficiency = ReprocessingEfficiency.Value;

                model.ConversionLancer = true;
                try
                {

                    Ore oreitem;
                    string item;

                    string[] AllLine = rapport.Split("\r\n");
                    foreach (string row in AllLine)
                    {
                        string[] AllColumn;

                        if (row.Contains("\t"))
                            AllColumn = row.Split("\t");
                        else
                            AllColumn = row.Split("    ");

                        foreach (string column in AllColumn)
                        {
                            item = column.Replace(" ", "").Replace(((char)160).ToString(), "");
                            if (item != "")
                            {
                                //on check le si on a le ore
                                if (!model.AllOreModels.ContainsKey(item))
                                {
                                    oreitem = DbContext.Ores.FirstOrDefault(x => x.Name.Replace(" ", "") == item);
                                    if (oreitem != null)
                                        model.AllOreModels.Add(item, oreitem);
                                }
                            }
                        }
                    }



                    foreach (var ore in model.AllOreModels)
                    {
                        if (ore.Value.PriceCompressedBuy.HasValue)
                            ore.Value.PriceCompressedBuy = ore.Value.PriceCompressedBuy / ((double)ore.Value.Volume);
                        else
                            ore.Value.PriceCompressedBuy = 0;

                        if (ore.Value.PriceCompressedSell.HasValue)
                            ore.Value.PriceCompressedSell = ore.Value.PriceCompressedSell / ((double)ore.Value.Volume);
                        else
                            ore.Value.PriceCompressedSell = 0;

                        if (ore.Value.PriceRefinedBuy.HasValue)
                            ore.Value.PriceRefinedBuy = ore.Value.PriceRefinedBuy / ((double)ore.Value.Volume);
                        else
                            ore.Value.PriceRefinedBuy = 0;

                        if (ore.Value.PriceRefinedSell.HasValue)
                            ore.Value.PriceRefinedSell = ore.Value.PriceRefinedSell / ((double)ore.Value.Volume);
                        else
                            ore.Value.PriceRefinedSell = 0;


                    }


                    model.ConversionOK = true;
                }
                catch (Exception ex)
                {

                }
            }

            if (model.ConversionOK)
            {
                foreach (var ore in model.AllOreModels)
                {
                    if (ore.Value.PriceRefinedBuy.HasValue)
                        ore.Value.PriceRefinedBuy = ore.Value.PriceRefinedBuy * ((double)model.ReprocessingEfficiency / 100);
                    else
                        ore.Value.PriceRefinedBuy = 0;

                    if (ore.Value.PriceRefinedSell.HasValue)
                        ore.Value.PriceRefinedSell = ore.Value.PriceRefinedSell * ((double)model.ReprocessingEfficiency / 100);
                    else
                        ore.Value.PriceRefinedSell = 0;

                }
            }

            model.Allorename.AddRange(DbContext.Ores.Select(x => x.Name));

            return View("WhatIShouldMine", model);
        }

        public IActionResult MoonReport(decimal? Reprocess, bool? sellorder, decimal? Taxe, string Rapport)
        {
            ViewBag.Title = "Report Moon ledger";
            ViewBag.Index = true;
            ViewBag.Description = "this tool allows you to calculate the amount of tax on a ratio of moon mining by character";

            EveMiningFleet.Models.ViewModel.ReportMoonModel model = new Models.ViewModel.ReportMoonModel();

            if (Taxe != null && Rapport != null && Rapport != "")
            {
                if (Taxe.HasValue)
                    model.Taxe = Taxe.Value;

                if (Reprocess.HasValue)
                    model.Reprocess = Reprocess.Value;

                if (sellorder.HasValue)
                    model.sellvalue = sellorder.Value;

                model.ConversionLancer = true;
                try
                {
                    string[] AllLine = Rapport.Split("\r\n");
                    foreach (string item in AllLine)
                    {
                        string[] AllColumn;

                        if (item.Contains("\t"))
                            AllColumn = item.Split("\t");
                        else
                            AllColumn = item.Split("    ");

                        if (AllColumn.Length == 9)
                        {
                            //On agit que si c'est pas une ligne d'entete
                            if (AllColumn[0].Contains("."))
                            {
                                string corp = "";
                                string member = "";
                                int OreID = 0;
                                int Quantity = 0;

                                corp = AllColumn[1];
                                member = AllColumn[2];

                                try
                                {
                                    OreID = int.Parse(AllColumn[7]);
                                }
                                catch (Exception)
                                {
                                    throw new Exception("Erreur de formatage. un champs numeric est textuelle");
                                }

                                try
                                {
                                    Quantity = int.Parse(AllColumn[4]);
                                }
                                catch (Exception)
                                {
                                    throw new Exception("Erreur de formatage. un champs numeric est textuelle");
                                }

                                //on check la corp
                                if (!model.Coorporations.ContainsKey(corp))
                                    model.Coorporations.Add(corp, new Models.ViewModel.RML_Coorporation());

                                //on viens recuperer le main grace au data de chez nous
                                Character charactermain = DbContext.Characters.Include("CharacterMain").FirstOrDefault(x => x.Name == member);
                                if (charactermain != null)
                                    member = charactermain.CharacterMain.Name;

                                //on check le membre
                                if (!model.Coorporations[corp].Characters.ContainsKey(member))
                                    model.Coorporations[corp].Characters.Add(member, new Models.ViewModel.RML_character());


                                //on regarde si le minerai de la ligne existes dans character
                                if (!model.Coorporations[corp].Characters[member].Ores.ContainsKey(OreID))
                                    model.Coorporations[corp].Characters[member].Ores.Add(OreID, new Models.ViewModel.RML_Ore());

                                model.Coorporations[corp].Characters[member].Ores[OreID].QuantityTotal += Quantity;

                                //on regarde si le minerai de la ligne existes dans coorp
                                if (!model.Coorporations[corp].Ores.ContainsKey(OreID))
                                    model.Coorporations[corp].Ores.Add(OreID, new Models.ViewModel.RML_Ore());

                                model.Coorporations[corp].Ores[OreID].QuantityTotal += Quantity;

                                //on regarde si le minerai de la ligne existes dans moon
                                if (!model.Ores.ContainsKey(OreID))
                                    model.Ores.Add(OreID, new Models.ViewModel.RML_Ore());

                                model.Ores[OreID].QuantityTotal += Quantity;

                            }
                        }
                        else
                            throw new Exception("Erreur de formatage. le nombre de colone est incorrect");
                    }
                    model.ConversionOK = true;
                }
                catch (Exception ex)
                {

                }

                if (model.ConversionOK)
                {
                    foreach (var Coorporation in model.Coorporations)
                    {
                        foreach (var Character in Coorporation.Value.Characters)
                        {
                            foreach (var Ore in Character.Value.Ores)
                            {
                                var oretmp = DbContext.Ores.FirstOrDefault(x => x.Id == Ore.Key);
                                if (oretmp != null)
                                {
                                    Ore.Value.Name = oretmp.Name;

                                    if (model.sellvalue)
                                    {
                                        Ore.Value.PriceOre = oretmp.PriceRefinedSell.Value;
                                        Ore.Value.PriceTotal = oretmp.PriceRefinedSell.Value * Ore.Value.QuantityTotal; ;
                                    }
                                    else
                                    {
                                        Ore.Value.PriceOre = oretmp.PriceRefinedBuy.Value;
                                        Ore.Value.PriceTotal = oretmp.PriceRefinedBuy.Value * Ore.Value.QuantityTotal;
                                    }

                                    Ore.Value.VolumeOre = oretmp.Volume.Value;
                                    Ore.Value.VolumeTotal = oretmp.Volume.Value * Ore.Value.QuantityTotal;
                                }
                            }
                        }

                        foreach (var Ore in Coorporation.Value.Ores)
                        {
                            var oretmp = DbContext.Ores.FirstOrDefault(x => x.Id == Ore.Key);
                            if (oretmp != null)
                            {
                                Ore.Value.Name = oretmp.Name;

                                if (model.sellvalue)
                                {
                                    Ore.Value.PriceOre = oretmp.PriceRefinedSell.Value;
                                    Ore.Value.PriceTotal = oretmp.PriceRefinedSell.Value * Ore.Value.QuantityTotal; ;
                                }
                                else
                                {
                                    Ore.Value.PriceOre = oretmp.PriceRefinedBuy.Value;
                                    Ore.Value.PriceTotal = oretmp.PriceRefinedBuy.Value * Ore.Value.QuantityTotal;
                                }

                                Ore.Value.VolumeOre = oretmp.Volume.Value;
                                Ore.Value.VolumeTotal = oretmp.Volume.Value * Ore.Value.QuantityTotal;
                            }
                        }
                    }
                    foreach (var Ore in model.Ores)
                    {
                        var oretmp = DbContext.Ores.FirstOrDefault(x => x.Id == Ore.Key);
                        if (oretmp != null)
                        {
                            Ore.Value.Name = oretmp.Name;

                            if (model.sellvalue)
                            {
                                Ore.Value.PriceOre = oretmp.PriceRefinedSell.Value;
                                Ore.Value.PriceTotal = oretmp.PriceRefinedSell.Value * Ore.Value.QuantityTotal; ;
                            }
                            else
                            {
                                Ore.Value.PriceOre = oretmp.PriceRefinedBuy.Value;
                                Ore.Value.PriceTotal = oretmp.PriceRefinedBuy.Value * Ore.Value.QuantityTotal;
                            }

                            Ore.Value.VolumeOre = oretmp.Volume.Value;
                            Ore.Value.VolumeTotal = oretmp.Volume.Value * Ore.Value.QuantityTotal;
                        }
                    }


                    foreach (var Coorporation in model.Coorporations)
                    {
                        Coorporation.Value.PriceTotal = 0;
                        Coorporation.Value.VolumeTotal = 0;

                        foreach (var Character in Coorporation.Value.Characters)
                        {
                            Character.Value.PriceTotal = 0;
                            Character.Value.VolumeTotal = 0;

                            foreach (var Ore in Character.Value.Ores)
                            {
                                Character.Value.PriceTotal += Ore.Value.PriceTotal * (double)(Reprocess / 100);
                                Character.Value.VolumeTotal += Ore.Value.VolumeTotal;

                                Ore.Value.QuantityTAXE = (int)Math.Round((double)Ore.Value.QuantityTotal * (double)(Taxe / 100));
                                Ore.Value.Quantitygain = Ore.Value.QuantityTotal - Ore.Value.QuantityTAXE;
                            }

                            Character.Value.PriceTAXE = Character.Value.PriceTotal * (double)(Taxe / 100);
                            Character.Value.Pricegain = Character.Value.PriceTotal - Character.Value.PriceTAXE;

                            Coorporation.Value.PriceTotal += Character.Value.PriceTotal;
                            Coorporation.Value.VolumeTotal += Character.Value.VolumeTotal;

                        }

                        foreach (var Ore in Coorporation.Value.Ores)
                        {
                            Ore.Value.QuantityTAXE = (int)Math.Round((double)Ore.Value.QuantityTotal * (double)(Taxe / 100));
                            Ore.Value.Quantitygain = Ore.Value.QuantityTotal - Ore.Value.QuantityTAXE;
                        }

                        Coorporation.Value.PriceTAXE = Coorporation.Value.PriceTotal * (double)(Taxe / 100);
                        Coorporation.Value.Pricegain = Coorporation.Value.PriceTotal - Coorporation.Value.PriceTAXE;

                        model.PriceTotal += Coorporation.Value.PriceTotal;
                        model.VolumeTotal += Coorporation.Value.VolumeTotal;
                    }
                    model.PriceTAXE = model.PriceTotal * (double)(Taxe / 100);
                    model.Pricegain = model.PriceTotal - model.PriceTAXE;


                    foreach (var Ore in model.Ores)
                    {
                        Ore.Value.QuantityTAXE = (int)Math.Round((double)Ore.Value.QuantityTotal * (double)(Taxe / 100));
                        Ore.Value.Quantitygain = Ore.Value.QuantityTotal - Ore.Value.QuantityTAXE;
                    }
                }

            }

            return View("MoonReport", model);
        }
    }
}
