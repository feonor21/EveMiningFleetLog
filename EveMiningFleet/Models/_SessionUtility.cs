using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using EveMiningFleet.Entities;
using EveMiningFleet.Entities.Tables;

namespace EveMiningFleet
{
    [Serializable()]
    public class _SessionUtility : IDisposable
    {
        public static _SessionUtility DecryptSession(Byte[] arrBytes)
        {
            _SessionUtility obj;

            if (arrBytes != null)
            {
                MemoryStream memStream = new MemoryStream();
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                obj = (_SessionUtility)binForm.Deserialize(memStream);
            }
            else
                obj = new _SessionUtility();

            return obj;
        }

        public byte[] ToByteArray()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, this);
            return ms.ToArray();
        }


        public bool darkmode = false;
        public int MainCharacterID = 0;
        public int MainCoorpID = 0;
        public int MainAllianceID = 0;

        public int TimeZone = 0;

        public Dictionary<int, string> AllCharacter = new Dictionary<int, string>();

        public Dictionary<int, string> AllCoorp = new Dictionary<int, string>();

        public Dictionary<int, string> AllAlliance = new Dictionary<int, string>();

        public void ConstructList(IQueryable<Character> listCharacter)
        {
            this.AllCharacter.Clear();
            this.AllCoorp.Clear();
            this.AllAlliance.Clear();

            if (listCharacter != null)
            {
                foreach (var cursorcharacter in listCharacter)
                {
                    if (!this.AllCharacter.ContainsKey(cursorcharacter.Id))
                        this.AllCharacter.Add(cursorcharacter.Id, cursorcharacter.Name);

                    if (!this.AllCoorp.ContainsKey(cursorcharacter.Corporation.Id))
                        this.AllCoorp.Add(cursorcharacter.Corporation.Id, cursorcharacter.Corporation.Name);

                    if (!this.AllAlliance.ContainsKey(cursorcharacter.Alliance.Id))
                        this.AllAlliance.Add(cursorcharacter.Alliance.Id, cursorcharacter.Alliance.Name);
                }
            }
        }

        public bool checkToken()
        {
            if (MainCharacterID == 0)
                return false;

            using (EveMiningFleetContext DbContext = new EveMiningFleetContext())
            {
                try
                {
                    Character mainCharater = DbContext.Characters.First(x => x.Id == MainCharacterID);
                    if (!String.IsNullOrEmpty(mainCharater.Token))
                        return false;
                    else
                        return true;
                }
                catch (Exception)
                {
                    return true;
                }
            }
        }

        public void Dispose()
        {
            MainCharacterID = 0;
            MainCoorpID = 0;
            MainAllianceID = 0;
            darkmode = false;
            AllCharacter.Clear();
            AllCoorp.Clear();
            AllAlliance.Clear();
        }
    }
}
