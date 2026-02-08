using System;

namespace EveMiningFleet.Logic.Tiers.fuzzwork
{
    public class Fuzzwork
    {
        /// <summary>
        /// Purge des fleet inactives
        /// </summary>
        public static int GetId(string item)
        {
            try
            {

                var url = "https://www.fuzzwork.co.uk/api/typeid2.php?typename=" + item;
                dynamic resultqueryjson;
                using (System.Net.WebClient client = new System.Net.WebClient())
                {
                    resultqueryjson = Newtonsoft.Json.JsonConvert.DeserializeObject(client.DownloadString(url));
                }
                return resultqueryjson[0].typeID;
            }
            catch (Exception)
            {
            }
            return 0;
        }

    }
}
