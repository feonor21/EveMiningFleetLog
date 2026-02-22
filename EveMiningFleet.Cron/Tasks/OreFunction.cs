using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using CsvHelper;
using ESI.NET;
using ESI.NET.Models.Market;
using EveMiningFleet.Entities;
using EveMiningFleet.Entities.Tables;
using EveMiningFleet.Logic.EsiEve;
using EveMiningFleet.Logic.Tools;

namespace Cron.Tasks
{
	public class OreFunction
	{


		//REPROCESS
		public static void RefreshAllOreDataWithEsiDump()
		{
			if (System.Environment.GetEnvironmentVariable("ORESCAN") == "1" || !System.Environment.GetEnvironmentVariable("ENVIRONMENT").Contains("Development"))
			{
				RefreshOreDb();
			}

			if (System.Environment.GetEnvironmentVariable("PRICESCAN") == "1" || !System.Environment.GetEnvironmentVariable("ENVIRONMENT").Contains("Development"))
			{
				if (NewEsiDump())
					InsertEsiSDE();

				PopulateDataPrice();
			}
		}


		/// <summary>
		/// REFRESH ORE DETAILS
		/// </summary>
		/// <param name="OreId"></param>
		public static void RefreshOreDb(int OreId = 0)
		{
			ClassLog.writeLog("RefreshOreDb (" + OreId + ")=> refresh des datas des ore");

			List<int> allIDtoanalyse = new List<int>();
			List<ESI.NET.Models.Market.Group> AllGroup = new List<ESI.NET.Models.Market.Group>();

			Dictionary<string, int> resultAllcompress = new Dictionary<string, int>();

			if (OreId > 0)
				allIDtoanalyse.Add(OreId);
			else
			{
				EveEsiConnexion tmpEsiConnection = new EveEsiConnexion();
				EsiResponse<int[]> resultqueryMarketGroups = null;
				int lenght = 0;
				int cursor = 0;

				Retry.Do(() =>
				{
					resultqueryMarketGroups = tmpEsiConnection.EsiClient.Market.Groups().Result;
					if (resultqueryMarketGroups.StatusCode != HttpStatusCode.OK)
						throw new Exception();
				}, TimeSpan.FromMilliseconds(50), 5);

				if (resultqueryMarketGroups.StatusCode == HttpStatusCode.OK)
				{
					lenght = resultqueryMarketGroups.Data.Count();
					foreach (int MarketGroupID in resultqueryMarketGroups.Data)
					{
						cursor++;
						ClassLog.writeLog("analyse groupe " + MarketGroupID + ", " + cursor + "/" + lenght);

						ESI.NET.Models.Market.Group result = null;
						Retry.Do(() =>
						{
							result = tmpEsiConnection.EsiClient.Market.Group(MarketGroupID).Result.Data;
							ClassLog.writeLog("   " + result.Name);
							if (result == null)
								throw new Exception();
							AllGroup.Add(result);
						}, TimeSpan.FromMilliseconds(100), 5);
					}
					foreach (var item in AllGroup.Where(x => x.Types.Length > 0))
					{
						if (isMaterialsRawGroup(AllGroup, item))
							allIDtoanalyse.AddRange(item.Types);
					}
				}

			}

			ClassLog.writeLog("RefreshOreDb ore launch");
			resultAllcompress = RefreshOreDbtask(allIDtoanalyse.ToArray());

			ClassLog.writeLog("RefreshOreDb compress launch");
			RefreshOreCompressedDbtask(resultAllcompress);

			ClassLog.writeLog("RefreshOreDb (" + OreId + ")=> finished...");

		}

		/// <summary>
		/// function permettant de savoir si le group target est bien un enfant( ou lui meme ) des group cibler dans les constante
		/// </summary>
		/// <param name="AllGroup">ALL GROUP of ccp</param>
		/// <param name="target">le group actuellement viser</param>
		/// <returns></returns>
		public static bool isMaterialsRawGroup(List<Group> AllGroup, Group target)
		{
			if (target.ParentGroupId == 0 && !_Constante.MarketIdGroupAnalysed.Contains(target.MarketGroupId))
				return false;
			else if (_Constante.MarketIdGroupAnalysed.Contains(target.ParentGroupId) || _Constante.MarketIdGroupAnalysed.Contains(target.MarketGroupId))
				return true;
			else
			{
				Group tmp = AllGroup.Find(x => x.MarketGroupId == target.ParentGroupId);
				return isMaterialsRawGroup(AllGroup, tmp);
			}
		}

		/// <summary>
		/// met a jour que les ore basic, dans la table ORE
		/// </summary>
		/// <param name="allIDtoanalyse"></param>
		/// <returns></returns>
		public static Dictionary<string, int> RefreshOreDbtask(int[] allIDtoanalyse)
		{
			Dictionary<string, int> result = new Dictionary<string, int>();
			int lenght = allIDtoanalyse.Length;
			int cursor = 0;
			if (allIDtoanalyse == null)
				return result;


			EveEsiConnexion tmpEsiConnection = new EveEsiConnexion();

			using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
			{
				bool newore = false;

				foreach (var item in allIDtoanalyse)
				{
					cursor++;
					ClassLog.writeLog("analyse detailler de " + item + ", " + cursor + "/" + lenght);
					EsiResponse<ESI.NET.Models.Universe.Type> resultquery = null;
					Retry.Do(() =>
					{
						resultquery = tmpEsiConnection.EsiClient.Universe.Type(item).Result;
						if (resultquery.StatusCode != HttpStatusCode.OK)
							throw new Exception();
					}, TimeSpan.FromMilliseconds(30 * 1000), 5);

					if (resultquery.StatusCode == HttpStatusCode.OK)
					{
						newore = false;

						//oreBasicType
						if (resultquery.Data.Name.Contains("compressed") || resultquery.Data.Name.Contains("Compressed"))
						{
							result.Add(resultquery.Data.Name, resultquery.Data.TypeId);
						}
						else
						{
							var ore = DatabaseContext.Ores.FirstOrDefault(x => x.Id == resultquery.Data.TypeId);
							if (ore == null)
							{
								newore = true;
								ore = new Ore() { Id = resultquery.Data.TypeId };
							}
							ore.Publish = null;

							if (resultquery.Data != null && resultquery.Data.Published && resultquery.Data.Published)
								ore.Publish = true;

							ore.Name = resultquery.Data.Name;

							ore.QuantityForReprocess = resultquery.Data.PortionSize;
							ore.Volume = resultquery.Data.Volume;

							if (resultquery.Data.DogmaAttributes.Any(x => x.AttributeId == 790))
								ore.IdSkillOreReprocessing = (int)resultquery.Data.DogmaAttributes.First(x => x.AttributeId == 790).Value;
							if (newore)
								DatabaseContext.Ores.Add(ore);
						}

					}

				}

				DatabaseContext.SaveChanges();
			}

			return result;
		}

		/// <summary>
		/// function permettant de recuperer les ID des compressed de nos ORE
		/// </summary>
		/// <param name="allCompressed"></param>
		public static void RefreshOreCompressedDbtask(Dictionary<string, int> allCompressed)
		{
			if (allCompressed == null)
				return;

			int lenght = allCompressed.Count;
			int cursor = 0;

			using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
			{
				foreach (var item in allCompressed)
				{
					cursor++;
					ClassLog.writeLog("analyse du compresse de " + item + ", " + cursor + "/" + lenght);

					var ore = DatabaseContext.Ores.FirstOrDefault(x => x.Name.Replace(" ", "") == item.Key.Replace("Compressed", "").Replace("compressed", "").Replace(" ", ""));
					if (ore != null)
					{
						ore.IdCompressed = item.Value;
						ore.CanBeCompressed = true;

					}
					else
					{
						ClassLog.writeLog("nest pas un bon minerai.....");
					}
				}
				DatabaseContext.SaveChanges();
			}
		}


		public static bool NewEsiDump()
		{
			ClassLog.writeLog("NewEsiDump => Check if dump ESI is different");
			Uri adressInvType = new Uri("https://www.fuzzwork.co.uk/dump/latest/invTypeMaterials.csv");


			byte[] oldchecksum = { };
			byte[] newchecksum = { };


			//on enregistre le checksum de l'ancien
			if (File.Exists(_Constante.filepathsESIcsv))
			{
				using (var md5 = MD5.Create())
				{
					using (var stream = File.OpenRead(_Constante.filepathsESIcsv))
					{
						oldchecksum = md5.ComputeHash(stream);
					}
				}

				//on recupere le checksum du nouveau
				using (WebClient webclient = new WebClient())
				{
					webclient.Headers.Add("User-Agent: Other");
					using (var md5 = MD5.Create())
					{
						using (var stream = webclient.OpenRead(adressInvType))
						{
							newchecksum = md5.ComputeHash(stream);
						}
					}
				}
			}



			if (File.Exists(_Constante.filepathsESIcsv) && newchecksum.SequenceEqual(oldchecksum))
			{
				return false;
			}
			else
			{

				Directory.CreateDirectory(Path.GetDirectoryName(_Constante.filepathsESIcsv));

				if (File.Exists(_Constante.filepathsESIcsv))
					File.Delete(_Constante.filepathsESIcsv);

				using (WebClient webclient = new WebClient())
				{
					webclient.Headers.Add("User-Agent: Other");
					webclient.DownloadFile(adressInvType, _Constante.filepathsESIcsv);
				}
				return true;
			}


		}
		public static void InsertEsiSDE()
		{
			ClassLog.writeLog("InsertEsiSDE => Reinsertion des information ESI dans le invtypeMaterial");
			List<InvTypeMaterialCsv> records;

			using (var reader = new StreamReader(_Constante.filepathsESIcsv))
			using (var csv = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
			{
				records = csv.GetRecords<InvTypeMaterialCsv>().ToList();
			}

			using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
			{
				DatabaseContext.Invtypematerials.RemoveRange(DatabaseContext.Invtypematerials.ToList());
				foreach (InvTypeMaterialCsv item in records)
				{
					DatabaseContext.Invtypematerials.Add(new Invtypematerial() { TypeId = item.typeID, MaterialTypeId = item.materialTypeID, Quantity = item.quantity });
				}
				DatabaseContext.SaveChanges();
			}

			records = null;
		}
		public class InvTypeMaterialCsv
		{
			public int typeID { get; set; }
			public int materialTypeID { get; set; }
			public int quantity { get; set; }
		}

		public static void PopulateDataPrice()
		{
			ClassLog.writeLog("PopulateDataPrice => lancement des ajout au data analyser");
			using (EveMiningFleetContext DatabaseContext = new EveMiningFleetContext())
			{
				foreach (var OreItem in DatabaseContext.Ores.Where(x => x.Publish == true))
				{
					if (!DatabaseContext.Dataprices.Any(x => x.TypeId == OreItem.Id))
						DatabaseContext.Dataprices.Add(new DataPrice() { TypeId = OreItem.Id, PriceBuy = 0.0, PriceSell = 0.0 });

					if (OreItem.IdCompressed.HasValue && !DatabaseContext.Dataprices.Any(x => x.TypeId == OreItem.IdCompressed))
						DatabaseContext.Dataprices.Add(new DataPrice() { TypeId = OreItem.IdCompressed.Value, PriceBuy = 0.0, PriceSell = 0.0 });

					DatabaseContext.SaveChanges();

					foreach (var ReprocessItem in DatabaseContext.Invtypematerials.Where(x => x.TypeId == OreItem.Id))
					{
						if (!DatabaseContext.Dataprices.Any(x => x.TypeId == ReprocessItem.MaterialTypeId))
						{
							DatabaseContext.Dataprices.Add(new DataPrice() { TypeId = ReprocessItem.MaterialTypeId, PriceBuy = 0.0, PriceSell = 0.0 });
							DatabaseContext.SaveChanges();
						}
					}
				}
				DatabaseContext.SaveChanges();
			}

			MarketFunction.RefreshOrePrice();
		}


	}
}
