using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NativeUI;
using CitizenFX.Core.UI;

namespace Client
{
    public class ClientMain : BaseScript
    {
        public static PlayerList players;
        public static void SetPlayers(PlayerList _players)
        {
            players = _players;
        }

        #region Cop vars

        public bool isCop = false;
        public bool is1197Active = false;

        #endregion


        public bool isEMS = false;
        public bool isCiv = false;
        #region Head labels
        private void DrawText3D(float x, float y, float z, string text)
        {
            float _x = 0, _y = 0;
            bool onScreen = API.GetScreenCoordFromWorldCoord(x, y, z, ref _x, ref _y);
            var dist = API.GetDistanceBetweenCoords(API.GetGameplayCamCoord().X, API.GetGameplayCamCoord().Y, API.GetGameplayCamCoord().Z, x, y, z, true);
            var ped_1 = API.PlayerPedId();

            float scale = (4.00001f / dist) * 0.3f;
            if (scale > 0.2)
            {
                scale = 0.2f;
            }
            else if (scale < 0.15)
            {
                scale = 0.15f;
            }
            float fov = (1f / API.GetGameplayCamFov()) * 100f;
            scale = scale * fov;
            if (onScreen)
            {
                API.SetTextFont((int)Font.Pricedown);
                API.SetTextScale(scale, scale);
                API.SetTextProportional(true);
                API.SetTextColour(210, 210, 210, 210);
                API.SetTextCentre(true);
                API.SetTextDropshadow(50, 210, 210, 210, 255);
                API.SetTextOutline();
                API.SetTextEntry("STRING"); ;
                API.AddTextComponentString(text);
                API.DrawText(_x, _y - 0.025f);
            }
        }
        private float disPlayerNames = 0;
        private void ManageHeadLabels()
        {
            //for (int i = 0; i < 255; i++)
            //{
            //    if (API.NetworkIsPlayerActive(i))
            //    {
            //        int iPed = API.GetPlayerPed(i);
            //        int lPed = API.PlayerPedId();
            //        int lPlayer = API.PlayerId();
            //        if (iPed != lPed)
            //        {
            //            if (API.DoesEntityExist(iPed))
            //            {
            //                var headLabelId = API.CreateMpGamerTag(iPed, " ", false, false, " ", 0);
            //                API.SetMpGamerTagName(headLabelId, " ");
            //                API.SetMpGamerTagVisibility(headLabelId, 0, false);
            //                API.RemoveMpGamerTag(headLabelId);
            //                float distance = API.GetDistanceBetweenCoords(API.GetEntityCoords(lPed, true).X, API.GetEntityCoords(lPed, true).Y, API.GetEntityCoords(lPed, true).Z, API.GetEntityCoords(iPed, true).X, API.GetEntityCoords(iPed, true).Y, API.GetEntityCoords(iPed, true).Z, true);
            //                if (distance < disPlayerNames)
            //                {
            //                    DrawText3D(API.GetEntityCoords(iPed, true).X, API.GetEntityCoords(iPed, true).Y, API.GetEntityCoords(iPed, true).Z, API.GetPlayerServerId(i).ToString() + " : " + API.GetPlayerName(i));
            //                }
            //            }
            //        }
            //    }
            //}
        }
        #endregion

        public ClientMain()
        {
            API.Wait(200);

            Tick += onTick;

            Localization.LoadLocalization();

            InitializeEvents();
            InitializeCommands();
            InitializeMenu();
            Backup.Initialize();
        }

        private async Task onTick()
        {
            //ManageHeadLabels();
            Coroner.CalculateCoronerLogic();
            EMS.HandleAllEms();
            HandleBlips();

            await Delay(100);
        }

        #region Blips

        private List<KeyValuePair<Blip, float>> blips_to_handle = new List<KeyValuePair<Blip, float>>();
        private List<float> blips_to_handle_time = new List<float>();

        private void HandleBlips()
        {
            for (int i = 0; i < blips_to_handle.Count; i++)
            {
                blips_to_handle_time[i] += 0.1f;
                if (blips_to_handle_time[i] >= blips_to_handle[i].Value)
                {
                    blips_to_handle[i].Key.Delete();
                    blips_to_handle.RemoveAt(i);
                    blips_to_handle_time.RemoveAt(i);
                }
            }
        }

        private void Blip_Add(Blip _blip, float time_to_live)
        {
            KeyValuePair<Blip, float> _new = new KeyValuePair<Blip, float>(_blip, time_to_live);
            blips_to_handle.Add(_new);
            blips_to_handle_time.Add(0);
        }

        #endregion

        #region Menu

        private MenuPool menuPool;
        private UIMenu menu_PoliceBasics;

        private UIMenu menu_Main;
        private UIMenu menu_Backup;
        private UIMenu menu_Dispatch;
        private UIMenu menu_BodyInteract;

        private void InitializeMenu()
        {
            menuPool = new MenuPool();
            menu_PoliceBasics = new UIMenu("Police Basics", "Best standalone cop plugin"); menu_PoliceBasics.ResetCursorOnOpen = false;
            menuPool.Add(menu_PoliceBasics);

            InitializeMainMenu();
            InitializeBackupMenu();
            InitializeDispatchMenu();
            InitializeBodyInteractionMenu();

            menuPool.RefreshIndex();

            Tick += async () =>
            {
                menuPool.ProcessMenus();
                if (Game.IsControlJustPressed(0, Control.CinematicSlowMo) && !menuPool.IsAnyMenuOpen()) // Our menu on/off switch
                    menu_PoliceBasics.Visible = !menu_PoliceBasics.Visible;
            };
        }

        private void InitializeBodyInteractionMenu()
        {
            menu_BodyInteract = menuPool.AddSubMenu(menu_PoliceBasics, Localization.Menues_Body_Menu_Head);

            UIMenuItem menu_callEMS = new UIMenuItem(Localization.Menues_Body_CallEMS);
            UIMenuItem menu_drag = new UIMenuItem(Localization.Menues_Body_DragBody);

            menu_BodyInteract.AddItem(menu_callEMS);
            menu_BodyInteract.AddItem(menu_drag);

            menu_BodyInteract.OnItemSelect += (sender, item, index) =>
            {
                if (item == menu_callEMS)
                {
                    EMS.SpawnEMS(BodyInteract.body, Game.Player);
                }
                else if (item == menu_drag)
                {
                    if (BodyInteract.isDragging)
                    {
                        BodyInteract.StopDragging();
                    }
                    else
                    {
                        BodyInteract.StartDragging();
                    }
                }
            };

            menu_BodyInteract.OnMenuOpen += (sender) =>
            {
                if (!BodyInteract.CheckForBody())
                {
                    menu_BodyInteract.Visible = false;
                }
            };
            
        }

        private void InitializeBackupMenu()
        {
            menu_Backup = menuPool.AddSubMenu(menu_PoliceBasics, Localization.Menues_Backup_title);

            UIMenuItem menu_panic = new UIMenuItem("PANIC", "Panic button", Colors.RedLight, Colors.Red);
            UIMenuItem menu_coroner = new UIMenuItem("Coroner");
            UIMenuItem menu_ems = new UIMenuItem("EMS");

            menu_Backup.AddItem(menu_panic);
            menu_Backup.AddItem(menu_coroner);
            menu_Backup.AddItem(menu_ems);

            menu_Backup.OnItemSelect += (sender, item, index) =>
            {
                if (item == menu_panic)
                {
                    Command_PanicButton();
                }
                else if (item == menu_coroner)
                {
                    CoronerCommand();
                }
                else if (item == menu_ems)
                {
                    Command_CallEMS();
                }
            };
        }

        private void InitializeDispatchMenu()
        {
            menu_Dispatch = menuPool.AddSubMenu(menu_PoliceBasics, Localization.Menues_Dispatch_Title);

            UIMenuItem code1 = new UIMenuItem(Localization.Menues_Dispatch_Code1);
            UIMenuItem code2 = new UIMenuItem(Localization.Menues_Dispatch_Code2);
            UIMenuItem code3 = new UIMenuItem(Localization.Menues_Dispatch_Code3);
            UIMenuItem code4 = new UIMenuItem(Localization.Menues_Dispatch_Code4);
            UIMenuItem code6 = new UIMenuItem(Localization.Menues_Dispatch_Code6);
            UIMenuItem code8 = new UIMenuItem(Localization.Menues_Dispatch_Code8);
            UIMenuItem code9 = new UIMenuItem(Localization.Menues_Dispatch_Code9);
            UIMenuItem code10 = new UIMenuItem(Localization.Menues_Dispatch_Code10);
            UIMenuItem code20 = new UIMenuItem(Localization.Menues_Dispatch_Code20);
            UIMenuItem code30 = new UIMenuItem(Localization.Menues_Dispatch_Code30);
            
            UIMenuItem code10_4 = new UIMenuItem(Localization.Menues_Dispatch_Code10_4);
            UIMenuItem code10_7 = new UIMenuItem(Localization.Menues_Dispatch_Code10_7);
            UIMenuItem code10_8 = new UIMenuItem(Localization.Menues_Dispatch_Code10_8);
            UIMenuItem code10_9 = new UIMenuItem(Localization.Menues_Dispatch_Code10_9);
            UIMenuItem code10_22 = new UIMenuItem(Localization.Menues_Dispatch_Code10_22);
            UIMenuItem code10_23 = new UIMenuItem(Localization.Menues_Dispatch_Code10_23);
            UIMenuItem code10_97 = new UIMenuItem(Localization.Menues_Dispatch_Code10_97);
            UIMenuItem code10_26 = new UIMenuItem(Localization.Menues_Dispatch_Code10_26);
            UIMenuListItem code10_20 = new UIMenuListItem(Localization.Menues_Dispatch_Code10_20, new List<dynamic>() { "No cops" }, 0);
            UIMenuListItem code10_39 = new UIMenuListItem(Localization.Menues_Dispatch_Code10_39, new List<dynamic>() { "No cops" }, 0);
            UIMenuItem code10_10 = new UIMenuItem(Localization.Menues_Dispatch_Code10_10);
            UIMenuItem code10_53 = new UIMenuItem(Localization.Menues_Dispatch_Code10_53);
            UIMenuItem code10_66 = new UIMenuItem(Localization.Menues_Dispatch_Code10_66);
            UIMenuItem code10_71 = new UIMenuItem(Localization.Menues_Dispatch_Code10_71);
            UIMenuItem code10_72 = new UIMenuItem(Localization.Menues_Dispatch_Code10_72);
            UIMenuItem code10_79 = new UIMenuItem(Localization.Menues_Dispatch_Code10_79);
            UIMenuItem code10_15 = new UIMenuItem(Localization.Menues_Dispatch_Code10_15);
            UIMenuItem code10_16 = new UIMenuItem(Localization.Menues_Dispatch_Code10_16);
            UIMenuItem code10_17 = new UIMenuItem(Localization.Menues_Dispatch_Code10_17);
            UIMenuItem code10_19 = new UIMenuItem(Localization.Menues_Dispatch_Code10_19);
            UIMenuItem code10_96 = new UIMenuItem(Localization.Menues_Dispatch_Code10_96);
            menu_Dispatch.OnMenuOpen += (sender) =>
            {
                List<dynamic> players_ids = new List<dynamic>();
                foreach (Player player in Players)
                {
                    players_ids.Add(player.ServerId);
                }
                code10_20.Items = players_ids;
                code10_39.Items = players_ids;
            };

            UIMenuItem code11_54 = new UIMenuItem(Localization.Menues_Dispatch_Code11_54);
            UIMenuItem code11_41 = new UIMenuItem(Localization.Menues_Dispatch_Code11_41);
            UIMenuItem code11_42 = new UIMenuItem(Localization.Menues_Dispatch_Code11_42);
            UIMenuItem code11_55 = new UIMenuItem(Localization.Menues_Dispatch_Code11_55);
            UIMenuItem code11_56 = new UIMenuItem(Localization.Menues_Dispatch_Code11_56);
            UIMenuItem code11_57 = new UIMenuItem(Localization.Menues_Dispatch_Code11_57);
            UIMenuItem code11_85 = new UIMenuItem(Localization.Menues_Dispatch_Code11_85);
            UIMenuItem code11_94 = new UIMenuItem(Localization.Menues_Dispatch_Code11_94);
            UIMenuItem code11_95 = new UIMenuItem(Localization.Menues_Dispatch_Code11_95);
            UIMenuItem code11_96 = new UIMenuItem(Localization.Menues_Dispatch_Code11_96);
            UIMenuItem code11_97 = new UIMenuItem(Localization.Menues_Dispatch_Code11_97);
            
            menu_Dispatch.AddItem(code1);
            menu_Dispatch.AddItem(code2);
            menu_Dispatch.AddItem(code3);
            menu_Dispatch.AddItem(code4);
            menu_Dispatch.AddItem(code6);
            menu_Dispatch.AddItem(code8);
            menu_Dispatch.AddItem(code9);
            menu_Dispatch.AddItem(code10);
            menu_Dispatch.AddItem(code20);
            menu_Dispatch.AddItem(code30);

            //submenu_codes10 = menuPool.AddSubMenu(menu_Dispatch, Localization.Menues_Dispatch_Codes10_title);
            menu_Dispatch.AddItem(code10_4);
            menu_Dispatch.AddItem(code10_8);
            menu_Dispatch.AddItem(code10_7);
            menu_Dispatch.AddItem(code10_9);
            menu_Dispatch.AddItem(code10_22);
            menu_Dispatch.AddItem(code10_23);
            menu_Dispatch.AddItem(code10_97);
            menu_Dispatch.AddItem(code10_26);
            menu_Dispatch.AddItem(code10_20);
            menu_Dispatch.AddItem(code10_39);
            menu_Dispatch.AddItem(code10_10);
            menu_Dispatch.AddItem(code10_53);
            menu_Dispatch.AddItem(code10_66);
            menu_Dispatch.AddItem(code10_71);
            menu_Dispatch.AddItem(code10_72);
            menu_Dispatch.AddItem(code10_79);
            menu_Dispatch.AddItem(code10_15);
            menu_Dispatch.AddItem(code10_16);
            menu_Dispatch.AddItem(code10_17);
            menu_Dispatch.AddItem(code10_19);
            menu_Dispatch.AddItem(code10_96);
            //submenu_codes10.OnItemSelect += (sender, item, index) =>
            //{
            //    TriggerServerEvent("PoliceBasics:dispatchToCops", Game.Player.ServerId, item.Text);
            //};
            menu_Dispatch.OnListSelect += (sender, listitem, index) =>
            {
                TriggerServerEvent("PoliceBasics:dispatchToCop", Game.Player.ServerId, listitem.Text, (int)listitem.Items[index]);
            };

            //submenu_codes11 = menuPool.AddSubMenu(menu_Dispatch, Localization.Menues_Dispatch_Codes11_title);
            menu_Dispatch.AddItem(code11_54);
            menu_Dispatch.AddItem(code11_41);
            menu_Dispatch.AddItem(code11_42);
            menu_Dispatch.AddItem(code11_55);
            menu_Dispatch.AddItem(code11_56);
            menu_Dispatch.AddItem(code11_57);
            menu_Dispatch.AddItem(code11_85);
            menu_Dispatch.AddItem(code11_94);
            menu_Dispatch.AddItem(code11_95);
            menu_Dispatch.AddItem(code11_96);
            menu_Dispatch.AddItem(code11_97);
            //submenu_codes11.OnItemSelect += (sender, item, index) =>
            //{
            //    TriggerServerEvent("PoliceBasics:dispatchToCops", Game.Player.ServerId, item.Text);
            //};

            menu_Dispatch.OnItemSelect += (sender, item, index) =>
            {
                //Answer to dispatch
                if (is1197Active && item == code10_26)
                {
                    TriggerServerEvent("PoliceBasics:dispatch_1197Answer", Game.Player.ServerId);
                    is1197Active = false;
                    return;
                }

                if (item == code10_8)
                {
                    TriggerServerEvent("PoliceBasics:CopDuty", Game.Player.ServerId, true);
                    TriggerServerEvent("PoliceBasics:CopBusy", Game.Player.ServerId, false);
                }
                else if (item == code10_10)
                {
                    TriggerServerEvent("PoliceBasics:CopDuty", Game.Player.ServerId, false);
                }
                else if (item == code4)
                {
                    TriggerServerEvent("PoliceBasics:CopBusy", Game.Player.ServerId, false);
                }

                TriggerServerEvent("PoliceBasics:dispatchToCops", Game.Player.ServerId, item.Text);
            };
        }

        #region Main menu

        private string dep_To_Join = "LSPD";
        private string dep_CODList = "All";

        private void InitializeMainMenu()
        {
            menu_Main = menuPool.AddSubMenu(menu_PoliceBasics, Localization.Menues_Main_title);

            UIMenuItem main_gocop = new UIMenuItem(Localization.Menues_Main_GoCop, Localization.Menues_Main_GoCop_Description);
            UIMenuItem main_copslist = new UIMenuItem(Localization.Menues_Main_CopList);
            UIMenuItem main_deplist = new UIMenuItem(Localization.Menues_Main_DepList);
            UIMenuListItem main_joinDep = new UIMenuListItem(Localization.Menues_Main_JoinDep,
                new List<dynamic>()
                {
                    "LSPD", "LSCS", "TPALS"
                }, 1);
            UIMenuItem main_leaveDep = new UIMenuItem(Localization.Menues_Main_LeaveDep);
            UIMenuListItem main_CODList = new UIMenuListItem(Localization.Menues_Main_CODList,
                new List<dynamic>()
                {
                    "All", "LSPD", "LSCS", "TPALS"
                }, 0);
            menu_Main.AddItem(main_gocop);
            menu_Main.AddItem(main_copslist);
            menu_Main.AddItem(main_deplist);
            menu_Main.AddItem(main_joinDep);
            menu_Main.AddItem(main_leaveDep);
            menu_Main.AddItem(main_CODList);
            menu_Main.OnListChange += (sender, item, index) => 
            { 
                if (item == main_joinDep)
                {
                    dep_To_Join = item.Items[index].ToString();
                }
                if (item == main_CODList)
                {
                    dep_CODList = item.Items[index].ToString();
                }
            };
            //menu_Main.OnItemSelect += menu_Main_OnItemSelect;
            menu_Main.OnItemSelect += (sender, item, index) =>
            {
                if (item == main_gocop)
                {
                    //GoCop
                    Command_GoCop();
                }
                else if (item == main_copslist)
                {
                    //Cop list
                    Command_CopList();
                }
                else if (item == main_deplist)
                {
                    //Dep list
                    Command_DepList();
                }
                else if (item == main_leaveDep)
                {
                    //Leave dep
                    Command_LeaveDepartment();
                }
            };
            menu_Main.OnListSelect += (sender, listItem, index) =>
            {
                if (listItem == main_joinDep)
                {
                    //Join dep
                    try
                    {
                        Command_JoinDepartment(Game.Player.ServerId, new List<object>() { listItem.Items[index] }, $"/joindep {listItem.Items[index]}");
                    }
                    catch
                    {
                        
                    }
                }
                else if (listItem == main_CODList)
                {
                    //COD list
                    try
                    {
                        if (dep_CODList == "All")
                        {
                            Command_CopsOnDutyList(Game.Player.ServerId, new List<object>() { }, "/codlist");
                        }
                        else
                        {
                            Command_CopsOnDutyList(Game.Player.ServerId, new List<object>() { listItem.Items[index] }, $"/codlist {listItem.Items[index]}");
                        }
                    }
                    catch
                    {
                        Debug.WriteLine("======\nyyyyyyyyyyyyyyyyyy\n=========");
                    }
                }
            };
        }

        #endregion

        #endregion



        #region Actions

        #region Backup

        private void Action_RequestBackup()
        {
            if (!isCop) return;

            
        }

        #endregion

        #endregion



        #region Events

        private void InitializeEvents()
        {
            EventHandlers["PoliceBasics:becomeCop"] += new Action(Event_BecomeCop);
            EventHandlers["PoliceBasics:getOnlineCopList"] += new Action<string>(Event_getOnlineCopList);
            EventHandlers["PoliceBasics:joinedDepartment"] += new Action<string>(Event_joinedDepartment);
            EventHandlers["PoliceBasics:leftDepartment"] += new Action(Event_leftDepartment);
            EventHandlers["PoliceBasics:showDepCODList"] += new Action<string, string>(Event_ShowDepartmentCODList);
            EventHandlers["PoliceBasics:getDispatchMessage"] += new Action<string, string>(Event_GetDispatchMessage);
            EventHandlers["PoliceBasics:panicButtonShow"] += new Action<string,  int, int, int>(Event_PanicButShow);
            EventHandlers["PoliceBasics:refreshCopList"] += new Action<List<int>, List<string>>(Event_RefreshCopList);

            //Dispatch
            EventHandlers["PoliceBasics:dispatch_1197"] += new Action(Event_Dispatch_1197);
            EventHandlers["PoliceBasics:dispatch_checkOfficer"] += new Action<int>(Event_Dispatch_CheckOfficer);
            EventHandlers["PoliceBasics:dispatch_officersSentToLKL"] += new Action(Event_Dispatch_Officers_Sent_To_LKL);

            EventHandlers["playerConnecting"] += new Action<Player, string, dynamic, dynamic>(Event_OnPlayerConnecting);
            EventHandlers["playerDropped"] += new Action<Player, string>(Event_OnPlayerDropped);

            RandomEvents.InitializeEvents(EventHandlers);
        }

        #region Dispatch events

        private void Event_Dispatch_Officers_Sent_To_LKL()
        {
            Screen.ShowSubtitle(Localization.Dispatch_Officers_Sent_To_LKL, 5000);
            is1197Active = false;
        }

        private void Event_Dispatch_CheckOfficer(int officer_id)
        {
            SendChatMessage(Localization.Menues_Dispatch_Title + $": ({Players[officer_id].Name})", Localization.Dispatch_Officer_Not_Answering, 255, 0, 0);
            Player officer_to_check = Players[officer_id];
            Blip officer_blip = World.CreateBlip(officer_to_check.Character.Position);
            officer_blip.Color = BlipColor.Blue;
            officer_blip.Name = $"{officer_to_check.Name} - {World.CurrentDayTime.Hours}:{World.CurrentDayTime.Minutes}";
            officer_blip.ShowRoute = false;
            officer_blip.IsFlashing = false;
            Blip_Add(officer_blip, 120);
        }

        private void Event_Dispatch_1197()
        {
            is1197Active = true;
            //SendChatMessage(Localization.Menues_Dispatch_Title + ": ", Localization.Menues_Dispatch_Code11_97, 0, 0, 255);
            Screen.ShowNotification(Localization.Menues_Dispatch_Title + ":" + Localization.Menues_Dispatch_Code11_97);
        }

        #endregion

        private void Event_RefreshCopList(List<int> playerIds, List<string> departmentIds)
        {
            CopsBase.playerCopIds = playerIds;
            CopsBase.copDepSName = departmentIds;
        }

        private void Event_PanicButShow(string from, int x, int y, int z)
        {
            SendChatMessage("Police: ", Localization.Dispatch_Panic_Message + from, 255, 0, 0);
            try
            {
                API.PlaySound(-1, "Missile_Incoming_Miss", "DOCKS_HEIST_FINALE_2B_SOUNDS", false, 0, true);
            }
            catch { }
            //Blip
            int blip = API.AddBlipForCoord(x, y, z);
            API.SetBlipColour(blip, (int)BlipColor.Red);
            API.BeginTextCommandSetBlipName("STRING");
            API.AddTextComponentString("Panic");
            API.EndTextCommandSetBlipName(blip);
        }

        private void Event_GetDispatchMessage(string from, string message)
        {
            SendChatMessage(from + ": ", message, 255, 0, 50);
        }

        private void Event_ShowDepartmentCODList(string dep_name, string employee_string)
        {
            SendChatMessage(dep_name, employee_string, 141, 65, 255);
        }

        private void Event_leftDepartment()
        {
            SendChatMessage("PoliceBase: ", Localization.PoliceBase_left_department, 0, 5, 255);
        }

        private void Event_joinedDepartment(string department_name)
        {
            SendChatMessage("PoliceBase: ", Localization.PoliceBase_joined_department + department_name, 0, 5, 255);
        }

        private void Event_getOnlineCopList(string namesLine)
        {
            if (namesLine == null || namesLine.Length == 0)
            {
                SendChatMessage("PolceBase: ", $"{Localization.PoliceBase_no_online_cops}", 0, 0, 255);
                return;
            }
            
            SendChatMessage("Online cops:", namesLine, 0, 1, 254);
        }

        private void Event_BecomeCop()
        {
            isCop = true;
            isCiv = false;
            isEMS = false;
            SendChatMessage("PoliceBasics: ", $"{Localization.Police_notification_now_cop}", 0, 0, 255);
            for (int i = 0; i < CopsBase.playerCopIds.Count; i++)
            {
                API.SetRelationshipBetweenGroups(0, (uint)API.GetPedGroupIndex(Game.Player.Handle), (uint)API.GetPedGroupIndex(Players[CopsBase.playerCopIds[i]].Handle));
            }
        }

        private void Event_OnPlayerConnecting([FromSource]Player player, string playerName, dynamic setKickReason, dynamic deferrals)
        {
            deferrals.defer();

            SetPlayers(Players);

            deferrals.update($"Hello {playerName}");

            deferrals.done();
        }

        private void Event_OnPlayerDropped([FromSource]Player player, string reason)
        {
            if (player == null) return;
            SetPlayers(Players);
        }

        #endregion

        private void SendChatMessage(string title, string message, int r, int g, int b)
        {

            TriggerEvent("chat:addMessage", new
            {
                color = new[] { r, g, b },
                multiline = true,
                args = new[] { title, message }
            });
        }

        #region Commands

        //Initialize all client commands
        public void InitializeCommands()
        {
            API.RegisterCommand("coroner", new Action(CoronerCommand), false);
            API.RegisterCommand("gocop", new Action(Command_GoCop), false);
            API.RegisterCommand("copslist", new Action(Command_CopList), false);
            API.RegisterCommand("deplist", new Action(Command_DepList), false);
            API.RegisterCommand("joindep", new Action<int, List<object>, string>(Command_JoinDepartment), false);
            API.RegisterCommand("leavedep", new Action(Command_LeaveDepartment), false);
            API.RegisterCommand("codlist", new Action<int, List<object>, string>(Command_CopsOnDutyList), false);
            API.RegisterCommand("ems", new Action(Command_CallEMS), false);
            API.RegisterCommand("panic", new Action(Command_PanicButton), false);

            API.RegisterCommand("rev", new Action(() => 
            { 
                API.NetworkResurrectLocalPlayer(Game.Player.Character.Position.X, Game.Player.Character.Position.Y + 2, Game.Player.Character.Position.Z, Game.Player.Character.Heading, true, false);
                API.SetPlayerInvincible(Game.Player.ServerId, false);
            }), false);

            API.RegisterCommand("pbmm", new Action(() => { menu_PoliceBasics.Visible = !menu_PoliceBasics.Visible; }), false);

            API.RegisterCommand("me", new Action<int, List<object>, string>(Command_Me), false);
            API.RegisterCommand("do", new Action<int, List<object>, string>(Command_Do), false);
            API.RegisterCommand("try", new Action<int, List<object>, string>(Command_Try), false);
            API.RegisterCommand("ooc", new Action<int, List<object>, string>(Command_OOC), false);

        }

        private void Command_Me(int source, List<object> args, string rawCommand)
        {
            if (args.Count == 0) return;
            string command = "";
            foreach (string arg in args)
            {
                command += arg + " ";
            }
            List<int> players_to_send = new List<int>();

            foreach (Player player in Players)
            {
                if (API.GetDistanceBetweenCoords(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, player.Character.Position.X, player.Character.Position.Y, player.Character.Position.Z, false) < 21)
                {
                    players_to_send.Add(player.ServerId);
                }
            }
            TriggerServerEvent("PoliceBasics:sendMessageToPlayers", $"{Game.Player.Name} {command}", "", 180, 0, 255, players_to_send);
        }

        private void Command_Do(int source, List<object> args, string rawCommand)
        {
            if (args.Count == 0) return;
            string command = "";
            foreach (string arg in args)
            {
                command += arg + " ";
            }
            List<int> players_to_send = new List<int>();

            foreach (Player player in Players)
            {
                if (API.GetDistanceBetweenCoords(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, player.Character.Position.X, player.Character.Position.Y, player.Character.Position.Z, false) < 21)
                {
                    players_to_send.Add(player.ServerId);
                }
            }
            TriggerServerEvent("PoliceBasics:sendMessageToPlayers", $"{Game.Player.Name} {command}", "", 180, 140, 255, players_to_send);
        }

        private void Command_Try(int source, List<object> args, string rawCommand)
        {
            if (args.Count == 0) return;
            string command = "";
            foreach (string arg in args)
            {
                command += arg + " ";
            }
            List<int> players_to_send = new List<int>();

            foreach (Player player in Players)
            {
                if (API.GetDistanceBetweenCoords(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, player.Character.Position.X, player.Character.Position.Y, player.Character.Position.Z, false) < 21)
                {
                    players_to_send.Add(player.ServerId);
                }
            }

            Random r = new Random();
            string l_ul = r.Next(0, 2) == 0 ? $"{Localization.String_Success}" : $"{Localization.String_Failure}";

            TriggerServerEvent("PoliceBasics:sendMessageToPlayers", $"{Game.Player.Name} {command}", $"( {l_ul} )", 180, 0, 255, players_to_send);
        }

        private void Command_OOC(int source, List<object> args, string rawCommand)
        {
            if (args.Count == 0) return;
            string command = "";
            foreach (string arg in args)
            {
                command += arg + " ";
            }
            List<int> players_to_send = new List<int>();

            foreach (Player player in Players)
            {
                players_to_send.Add(player.ServerId);
            }

            TriggerServerEvent("PoliceBasics:sendMessageToPlayers", $"[OOC] {Game.Player.Name}", $"{{  {command}  }}", 91, 215, 255, players_to_send);
        }

        private void Command_PanicButton()
        {
            if (isCop)
            {
                Backup.Panic();
                TriggerServerEvent("PoliceBasics:panicButton", Game.Player.ServerId);
            }
            else
            {
                SendChatMessage("Police: ", Localization.Error_not_cop, 255, 0, 0);
            }
        }

        private void Command_CallEMS()
        {
            EMS.SpawnEMS(Game.Player, Game.Player);
        }

        private void Command_CopsOnDutyList(int source, List<object> args, string rawCommand)
        {
            if (!isCop)
            {
                SendChatMessage("PoliceBasics: ", Localization.Error_not_cop, 100, 0, 254);
                return;
            }

            if (args.Count == 0)
            {
                TriggerServerEvent("PoliceBasics:getCODList", null, Game.Player.ServerId);
            }
            else if (args.Count == 1)
            {
                TriggerServerEvent("PoliceBasics:getCODList", args[0], Game.Player.ServerId);
            }
            else
            {
                SendChatMessage("Error: ", "Too many arguments", 255, 0, 0);
                return;
            }
        }

        private void Command_LeaveDepartment()
        {
            if (!isCop)
            {
                SendChatMessage("Error: ", Localization.Error_not_cop, 255, 2, 2);
                return;
            }

            TriggerServerEvent("PoliceBasics:leaveDepartment", Game.Player.ServerId);

        }

        private void Command_JoinDepartment(int source, List<object> args, string rawCommand)
        {
            //if true, this was executed from rcon or smthng else
            //if (source <= 0)
            //{
            //    Debug.Write("=========\nHz why?\n=========");
            //    return;
            //}
            if (args.Count != 1)
            {
                SendChatMessage("Command syntax error: ", "Enter only department", 255, 0, 0);
                return;
            }
            string department_id_s = (string)args[0];
            if (!isCop)
            {
                SendChatMessage("Error: ", Localization.Error_not_cop, 255, 2, 2);
                return;
            }
            if (department_id_s == "TPALS")
            {
                SendChatMessage("Join warning", "Not developed! Не разработано!", 255, 0, 0);
                TriggerServerEvent("PoliceBasics:joinDepartment", department_id_s, Game.Player.ServerId);
            }
            if (department_id_s == "LSPD" || department_id_s == "LSCS")
            {
                TriggerServerEvent("PoliceBasics:joinDepartment", department_id_s, Game.Player.ServerId);
            }
            else if (int.TryParse(department_id_s, out int department_id))
            {
                switch (department_id)
                {
                    case 1:
                        TriggerServerEvent("PoliceBasics:joinDepartment", "LSPD", Game.Player.ServerId);
                        break;
                    case 2:
                        TriggerServerEvent("PoliceBasics:joinDepartment", "LSCS", Game.Player.ServerId);
                        break;
                    case 3:
                        TriggerServerEvent("PoliceBasics:joinDepartment", "TPALS", Game.Player.ServerId);
                        break;
                    default:
                        SendChatMessage("Error: ", Localization.Error_No_dep, 255, 0, 0);
                        break;
                }
            }
            else
            {
                SendChatMessage("Error: ", Localization.Error_No_dep, 255, 0, 0);
            }
        }

        private void Command_DepList()
        {
            SendChatMessage("Police departments: ", "1 = LSPD, 2 = LSCS, 3 = TPALS", 0, 2, 253);
        }

        private void Command_CopList()
        {
            TriggerServerEvent("PoliceBasics:sendOnlineCopList", Game.Player.ServerId);
        }

        private void Command_GoCop()
        {
            if (isCop)
            {
                SendChatMessage("PoliceBasics: ", $"{Localization.Police_notification_already_cop}!", 0, 0, 255);
            }
            else
            {
                TriggerServerEvent("PoliceBasics:registerCop", Game.Player.ServerId);
            }
        }

        private void CoronerCommand()
        {
            Coroner.Summon();
        }

        #endregion
    }
}
