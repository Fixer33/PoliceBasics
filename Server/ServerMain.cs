using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace Server
{
    public class ServerMain : BaseScript
    {
        public static PlayerList players;
        public static void SetPlayers(PlayerList _players)
        {
            players = _players;
        }
        public ServerMain()
        {
            SetPlayers(Players);
            DataManager.Players = Players;
            DataManager.LoadData();
            PoliceBase.Initialize();
            InitializeServerEvents();
            InitializeServerCommands();
            Tick += onTick;
        }

        private async Task onTick()
        {
            Dispatch.Handle();
            await Delay(100);
        }

        #region Commands

        private void InitializeServerCommands()
        {
            
        }

        #endregion

        #region Events

        private void InitializeServerEvents()
        {
            EventHandlers["PoliceBasics:registerCop"] += new Action<int>(Event_RegisterCop);
            EventHandlers["PoliceBasics:sendOnlineCopList"] += new Action<int>(Event_SendOnlineCopList);
            EventHandlers["PoliceBasics:joinDepartment"] += new Action<string, int>(Event_JoinDepartment);
            EventHandlers["PoliceBasics:leaveDepartment"] += new Action<int>(Event_LeaveDepartment);
            EventHandlers["PoliceBasics:getCODList"] += new Action<string, int>(Event_GetCopOnDutyList);
            EventHandlers["PoliceBasics:dispatchToCops"] += new Action<int, string>(Event_DispatchToCops);
            EventHandlers["PoliceBasics:dispatchToCop"] += new Action<int, string, int>(Event_DispatchToCop);
            EventHandlers["PoliceBasics:panicButton"] += new Action<int>(Event_PanicButton);
            EventHandlers["PoliceBasics:sendMessageToPlayers"] += new Action<string, string, int, int, int, List<object>>(Event_SendMessageToPlayers);

            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(Event_OnPlayerConnecting);
            EventHandlers["playerDropped"] += new Action<Player, string>(Event_OnPlayerDropped);

            //Dispatch
            EventHandlers["PoliceBasics:dispatch_1197Answer"] += new Action<int>(Event_Dispatch_1197Answ);

            //Cop
            EventHandlers["PoliceBasics:CopDuty"] += new Action<int, bool>(Event_CopDuty);
            EventHandlers["PoliceBasics:CopBusy"] += new Action<int, bool>(Event_CopBusy);
        }

        private void Event_SendMessageToPlayers(string title, string message, int r, int g, int b, List<object> players_ids)
        {
            foreach (int id in players_ids)
            {
                SendChatMessageToPlayer(Players[id], title, message, r, g, b);
            }
        }

        #region Dispatch events

        private void Event_Dispatch_1197Answ(int player_id)
        {
            Player player = Players[player_id];
            Cop cop = PoliceBase.GetCopFromPlayer(player);
            Dispatch.Get_1197_CallbackFromOfficer(cop);
        }

        #endregion

        #region Cop

        private void Event_CopDuty(int player_id, bool isOn)
        {
            Cop cop = PoliceBase.GetCopFromPlayer(Players[player_id]);
            Debug.WriteLine(isOn.ToString() + "      " + cop.onDuty.ToString() + "     " + cop.Name);
            cop.onDuty = isOn;
        }
        private void Event_CopBusy(int player_id, bool isOn)
        {
            Cop cop = PoliceBase.GetCopFromPlayer(Players[player_id]);
            cop.isBusy = isOn;
        }

        #endregion

        private void Event_PanicButton(int player_id)
        {
            Player caller = Players[player_id];
            foreach (Cop cop in PoliceBase.CopsList)
            {
                if (!cop.player.Equals(caller))
                {
                    cop.player.TriggerEvent("PoliceBasics:panicButtonShow", caller.Name, caller.Character.Position.X, caller.Character.Position.Y, caller.Character.Position.Z);
                }
            }
        }

        private void Event_DispatchToCop(int player_id, string message, int target_player_id)
        {
            try
            {
                Player target = Players[target_player_id];
                Player from = Players[player_id];
                target.TriggerEvent("PoliceBasics:getDispatchMessage", $"{from.Name}({player_id})", message);
            }
            catch
            {
                Players[player_id].TriggerEvent("PoliceBasics:getDispatchMessage", "Dispatch", "No such cop found or avaible");
            }
        }

        private void Event_DispatchToCops(int player_id, string message)
        {
            Player player = Players[player_id];
            foreach (Player _player in Players)
            {
                if (PoliceBase.isCopOnDuty(_player.Name))
                {
                    _player.TriggerEvent("PoliceBasics:getDispatchMessage", $"{player.Name}({player_id})", message);
                }
            }
        }

        private void Event_GetCopOnDutyList(string dep_name, int player_servId)
        {
            Player player = Players[player_servId];
            if (string.IsNullOrEmpty(dep_name))
            {
                //Show all deps
                foreach (Department dep in PoliceBase.AllDepartments)
                {
                    string depart_name = dep.fullName + ": ";
                    string employee_string = dep.GetEmployeeString();
                    if (!string.IsNullOrEmpty(employee_string))
                    {
                        player.TriggerEvent("PoliceBasics:showDepCODList", depart_name, employee_string);
                    }
                }
            }
            else
            {
                //Show only current dep
                Department dep = PoliceBase.FindDepByShortName(dep_name);
                if (dep == null) return;
                else
                {
                    string depart_name = dep.fullName + ": ";
                    string employee_string = dep.GetEmployeeString();
                    player.TriggerEvent("PoliceBasics:showDepCODList", depart_name, employee_string);
                }
            }
        }

        private void Event_LeaveDepartment(int player_serverId)
        {
            Player player = Players[player_serverId];
            Cop player_cop = PoliceBase.GetCopFromPlayer(player);
            if (player_cop == null)
            {
                Debug.WriteLine($"=============\nPlayer {player.Name} tried to leave current department, but he was not recognised as a cop\n==========");
                return;
            }

            player_cop.LeaveDepartment();
            RefreshCopListOnAllClients();
        }

        private void Event_JoinDepartment(string department_name, int player_serverId)
        {
            Player player = Players[player_serverId];
            Cop player_cop = PoliceBase.GetCopFromPlayer(player);
            if (player_cop == null)
            {
                Debug.WriteLine($"=============\nPlayer {player.Name} tried to become {department_name}, but he was not recognised as a cop\n==========");
                return;
            }
            switch (department_name)
            {
                case "LSPD":
                    PoliceBase.JoinDepartment(PoliceBase.Departments.LSPD, player_cop);
                    break;
                case "LSCS":
                    PoliceBase.JoinDepartment(PoliceBase.Departments.LSCS, player_cop);
                    break;
                case "TPALS":
                    PoliceBase.JoinDepartment(PoliceBase.Departments.TPALS, player_cop);
                    break;
            }
            RefreshCopListOnAllClients();
        }

        private void Event_SendOnlineCopList(int playerId_toSend)
        {
            Player player = Players[playerId_toSend];

            List<string> onlineCopsNames = new List<string>();

            string[] cops = PoliceBase.GetAllOnlineCopsSteamid();
            for (int i = 0; i < cops.Length; i++)
            {
                foreach (Player _player in Players)
                {
                    if (cops[i] == _player.Identifiers["steam"])
                    {
                        onlineCopsNames.Add(PoliceBase.GetCopNameFromSteamId(cops[i]));
                    }
                }
            }
            string[] names = onlineCopsNames.ToArray();
            string namesLine = "";
            if (names.Length == 1)
            {
                namesLine = names[0];
            }
            else
            {
                for (int i = 0; i < names.Length - 1; i++)
                {
                    namesLine += names[i] + ", ";
                }
                namesLine += names[names.Length - 1];
            }

            player.TriggerEvent("PoliceBasics:getOnlineCopList", namesLine);
        }

        private void Event_OnPlayerConnecting([FromSource]Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            deferrals.defer();

            DataManager.Players = Players;

            Debug.WriteLine(new string('=', 10));
            foreach (var item in player.Identifiers)
            {
                Debug.WriteLine(item);
            }
            Debug.WriteLine(new string('=', 10));

            RefreshCopListOnAllClients();
            SetPlayers(Players);

            deferrals.update($"Hello {playerName}");

            deferrals.done();
        }

        private void Event_OnPlayerDropped([FromSource]Player player, string reason)
        {
            if (player == null) return;
            Cop player_cop = PoliceBase.GetCopFromPlayer(player);
            if (player_cop != null) player_cop.LeaveDepartment();
            RefreshCopListOnAllClients();
            SetPlayers(Players);
        }

        private void Event_RegisterCop(int player_server_id)
        {
            Player player = Players[player_server_id];
            if (!PoliceBase.CopExists(player.Identifiers["steam"]))
            {
                Cop newCop = new Cop();
                newCop.Name = player.Name;
                newCop.steamid = player.Identifiers["steam"];
                newCop.serverId = player_server_id;
                PoliceBase.CopsList.Add(newCop);
                DataManager.SaveCopBase();
                Debug.WriteLine("Cop created!");
            }
            TriggerClientEvent(player, "PoliceBasics:becomeCop");

            //Refresh cop list on all clients
            RefreshCopListOnAllClients();
        }

        #endregion

        private void RefreshCopListOnAllClients()
        {
            List<int> playerIds = new List<int>();
            List<string> depts = new List<string>();
            foreach (Cop cop in PoliceBase.CopsList)
            {
                int temp1 = GetPlayerServerIdFromName(cop.Name);
                playerIds.Add(temp1);
                if (cop.currentDepartment == null)
                {
                    depts.Add("");
                }
                else
                {
                    depts.Add(cop.currentDepartment.shortName);
                }
            }
            foreach (Player player1 in Players)
            {
                player1.TriggerEvent("PoliceBasics:refreshCopList", playerIds, depts);
            }
        }

        private int GetPlayerServerIdFromName(string name)
        {
            int ind = -1;
            foreach (Player player in Players)
            {
                ind++;
                if (player.Name == name) return ind+1;
            }
            return -1;
        }

        private void Debugn(int num)
        {
            Debug.WriteLine("");
            Debug.WriteLine(new string('=', 10));
            Debug.WriteLine($"Debug {num}");
            Debug.WriteLine(new string('=', 10));
            Debug.WriteLine("");
        }

        private void SendChatMessageToPlayer(Player player, string title, string message, int r, int g, int b)
        {
            player.TriggerEvent("chat:addMessage", new
            {
                color = new[] { r, g, b },
                multiline = true,
                args = new[] { title, message }
            });
        }
    }
}
