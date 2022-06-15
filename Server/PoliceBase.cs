using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    static class PoliceBase
    {
        public static List<Cop> CopsList = new List<Cop>();

        static Department LSPD = new Department("Los Santos Police Department", "LSPD", Departments.LSPD);
        static Department LSCS = new Department("Los Santos Country Sheriff", "LSCS", Departments.LSPD);
        static Department TPALS = new Department("The Port Authority of Los Santos", "TPALS", Departments.LSPD);

        public static List<Department> AllDepartments;

        public static void Initialize()
        {
            AllDepartments = new List<Department>() { LSPD, LSCS, TPALS };
        }

        public enum Departments
        {
            LSPD = 0,
            LSCS = 1,
            TPALS = 2
        }

        #region Department Actions

        public static void JoinDepartment(Departments dep_toJoin, Cop cop)
        {
            switch (dep_toJoin)
            {
                case Departments.LSPD:
                    if (LSCS.employees.Contains(cop) || TPALS.employees.Contains(cop))
                    {
                        Debug.WriteLine($"===\nCop {cop.Name} tries to join LSPD, but he is already employed somewhere\n===");
                        break;
                    }
                    LSPD.Join(cop);
                    break;
                case Departments.LSCS:
                    if (LSPD.employees.Contains(cop) || TPALS.employees.Contains(cop))
                    {
                        Debug.WriteLine($"===\nCop {cop.Name} tries to join LSCS, but he is already employed somewhere\n===");
                        break;
                    }
                    LSCS.Join(cop);
                    break;
                case Departments.TPALS:
                    if (LSCS.employees.Contains(cop) || LSPD.employees.Contains(cop))
                    {
                        Debug.WriteLine($"===\nCop {cop.Name} tries to join TPALS, but he is already employed somewhere\n===");
                        break;
                    }
                    TPALS.Join(cop);
                    break;
            }
        }
        
        public static Departments? FindCurrentCopDepartment(Cop cop)
        {
            if (LSPD.employees.Contains(cop)) return Departments.LSPD;
            if (LSCS.employees.Contains(cop)) return Departments.LSCS;
            if (TPALS.employees.Contains(cop)) return Departments.TPALS;
            return null;
        }

        public static Department FindDepByShortName(string short_name)
        {
            switch (short_name)
            {
                case "LSPD":
                    return LSPD;
                case "LSCS":
                    return LSCS;
                case "TPALS":
                    return TPALS;

            }
            return null;
        }

        #endregion

        #region Base

        /// <summary>
        /// Checks if cop exists
        /// </summary>
        /// <param name="player_index">Player index on server</param>
        /// <returns>True if there is a cop; False if not exists</returns>
        public static bool CopExists(string steamid)
        {
            foreach (Cop cop in CopsList)
            {
                if (cop.steamid == steamid) return true;
            }
            return false;
        }

        public static bool isCopOnDuty(string name)
        {
            foreach (Department dep in AllDepartments)
            {
                if (dep.isCopAnEmployee(name)) return true;
            }
            return false;
        }

        public static string GetCopNameFromSteamId(string steam_id)
        {
            foreach (Cop cop in CopsList)
            {
                if (cop.steamid == steam_id)
                {
                    return cop.Name;
                }
            }
            return "";
        }

        public static string[] GetAllOnlineCopsSteamid()
        {
            List<string> result = new List<string>();
            foreach (Cop cop in CopsList)
            {
                result.Add(cop.steamid);
            }
            return result.ToArray();
        }

        public static Cop GetCopFromPlayer(Player player)
        {
            for (int i = 0; i < CopsList.Count; i++)
            {
                if (CopsList[i].steamid == player.Identifiers["steam"])
                {
                    CopsList[i].player = player;
                    return CopsList[i];
                }
            }
            return null;
        }

        #endregion
    }
}
