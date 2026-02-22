using System;
using System.Net.Http;
using System.Threading.Tasks;
using EveMiningFleet.Logic.Tools;

namespace EveMiningFleet.Logic.EsiEve
{
	public static class EsiCorporation
	{
		/// <summary>
		/// recupere le nom de la corp
		/// </summary>
		/// <param name="ID"></param>
		/// <returns></returns>
		public static string GetName(int ID)
		{
			return Retry.Do(() =>
			{
				try
				{
					var eveEsiConnexion = new EveEsiConnexion();

					if (eveEsiConnexion == null)
					{
						Console.Error.WriteLine($"[EsiEve.EsiCorporation][GetName] EveEsiConnexion NULL. CorpId={ID}");
						return null;
					}

					if (eveEsiConnexion.EsiClient == null)
					{
						Console.Error.WriteLine($"[EsiEve.EsiCorporation][GetName] EsiClient NULL. CorpId={ID}");
						return null;
					}

					var response = eveEsiConnexion
						.EsiClient
						.Corporation
						.Information(ID)
						.Result;

					if (response == null)
					{
						Console.Error.WriteLine($"[EsiEve.EsiCorporation][GetName] Response NULL. CorpId={ID}");
						return null;
					}

					if (response.Data == null)
					{
						Console.Error.WriteLine($"[EsiEve.EsiCorporation][GetName] Response.Data NULL. CorpId={ID}");
						return null;
					}

					if (string.IsNullOrWhiteSpace(response.Data.Name))
					{
						Console.Error.WriteLine($"[EsiEve.EsiCorporation][GetName] Name EMPTY. CorpId={ID}");
						return null;
					}

					return response.Data.Name;
				}
				catch (HttpRequestException ex)
				{
					// erreur transitoire → retry OK
					Console.Error.WriteLine($"[EsiEve.EsiCorporation][GetName] HttpRequestException. CorpId={ID} : {ex.Message}");
					throw;
				}
				catch (TaskCanceledException ex)
				{
					// timeout → retry OK
					Console.Error.WriteLine($"[EsiEve.EsiCorporation][GetName] Timeout. CorpId={ID} : {ex.Message}");
					throw;
				}
				catch (Exception ex)
				{
					// bug logique → on log et on STOP le retry
					Console.Error.WriteLine($"[EsiEve.EsiCorporation][GetName] FATAL. CorpId={ID} : {ex}");
					return null;
				}

			}, TimeSpan.FromMilliseconds(10), 3);
		}
	}
}
