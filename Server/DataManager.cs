using CitizenFX.Core.Native;
using CitizenFX.Core;
using System;

namespace Server
{
    static class DataManager
    {
        public static PlayerList Players;
        private const string CopsBasePath = "Base/Police/Cops.ini";

        public static void LoadData()
        {
            LoadCopBase();
        }

        private static void LoadCopBase()
        {
            String data = Function.Call<string>(Hash.LOAD_RESOURCE_FILE, "PoliceBasics", CopsBasePath);
            var dataParse = data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (dataParse == null || dataParse.Length <= 0) return;

            for (int i = 0; i < dataParse.Length; i++)
            {
                Player player = GetPlayerFromSteamid(dataParse[i]);
                if (player != null)
                {
                    Cop newCop = new Cop()
                    {
                        steamid = dataParse[i],
                        Name = player.Name,
                        player = player,
                        isBusy = false,
                        onDuty = false
                    };
                   
                    PoliceBase.CopsList.Add(newCop);

                }
                
            }
        }

        public static Player GetPlayerFromSteamid(string steamid)
        {
            if (Players == null) return null;
            foreach (Player player in Players)
            {
                if (player.Identifiers["steam"] == steamid)
                {
                    return player;
                }
            }
            return null;
        }

        public static void SaveCopBase()
        {
            String data = "";
            foreach (Cop cop in PoliceBase.CopsList)
            {
                data += cop.steamid + "\r\n";
            }
            Function.Call(Hash.SAVE_RESOURCE_FILE, "PoliceBasics", CopsBasePath, data);
        }
    }
}
