using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    static class Backup
    {
        public static void Initialize()
        {
            AllAttachedUnits = new List<BackupUnit>();
        }

        public static void RequestBackup()
        {

        }

        public static void Panic()
        {
            Debug.WriteLine("=============\n1\n========");
            Random r = new Random();
            int count = r.Next(1, 3);
            for (int i = 0; i < count; i++)
            {
                Debug.WriteLine("=============\n2"+i+"\n========");
                AllAttachedUnits.Add(new SwatUnit(6, Game.Player));
            }
            
        }

        public static void HandleBackup()
        {
            for (int i = 0; i < AllAttachedUnits.Count; i++)
            {
                AllAttachedUnits[i].Handle();
            }
        }

        public enum BackupClass
        {
            LSPD,
            LSCS,
            TPALS,
            FBI,
            SWAT
        }

        public enum BackupCode
        {
            code1,
            code2,
            code3,
            pursuit,
            trafficstop
        }
        
        private static List<BackupUnit> AllAttachedUnits;


        abstract class BackupUnit
        {
            public Backup.BackupClass UnitClass;
            public List<Ped> UnitPeds;
            public Vehicle UnitVehicle;
            public int groupId;

            public static int drivingStyle;

            #region logic vars
            public bool isSpawned = false;
            public bool enRoute = false;
            public bool fight = false;
            #endregion

            public List<WeaponHash> AllowedPrimary = new List<WeaponHash>()
            {
                WeaponHash.AssaultRifle,
                WeaponHash.BullpupRifle,
                WeaponHash.CarbineRifle,
                WeaponHash.AssaultRifleMk2
            };
            public List<WeaponHash> AllowedSecondary = new List<WeaponHash>()
            {
                WeaponHash.Pistol,
                WeaponHash.Pistol50,
                WeaponHash.CombatPistol
            };
            public List<WeaponHash> AllowedExtra = new List<WeaponHash>()
            {
                WeaponHash.SmokeGrenade,
                WeaponHash.BZGas,
                WeaponHash.ProximityMine
            };

            public Player backup_target;

            public abstract void Handle();
        }
        class SwatUnit : BackupUnit
        {
            const int spawnDistance = 300;

            public SwatUnit(int unitCount, Player to_backup)
            {
                if (unitCount > 6 || unitCount < 1) return;

                InitializeDefaultValues();

                backup_target = to_backup;
                Vector3 spawnPos = new Vector3();
                float heading = 0;
                int unusedvar1 = 0;
                API.GetNthClosestVehicleNodeWithHeading
                    (backup_target.Character.Position.X, backup_target.Character.Position.Y, backup_target.Character.Position.Z, spawnDistance, ref spawnPos, ref heading, ref unusedvar1, 9, 3.0f, 2.5f);
                SpawnUnit(spawnPos, heading - 90f, unitCount);
                AllAttachedUnits.Add(this);
            }

            private void InitializeDefaultValues()
            {
                UnitPeds = new List<Ped>();
                drivingStyle = 525100;
            }

            private async void SpawnUnit(Vector3 pos, float heading, int count)
            {
                groupId = API.CreateGroup(0);
                UnitVehicle = await World.CreateVehicle(VehicleHash.Riot, pos, heading);
                UnitVehicle.IsSirenActive = true;

                Random r = new Random();
                for (int i = 0; i < count; i++)
                {
                    Ped ped = await World.CreatePed(PedHash.Swat01SMY, pos, heading);
                    API.SetPedAsGroupMember(ped.Handle, groupId);
                    API.SetRelationshipBetweenGroups(0, (uint)groupId, (uint)API.GetPedGroupIndex(Game.Player.Handle));
                    //Debug.WriteLine("============\n" + (uint)groupId + "  -  " + (uint)API.GetPedGroupIndex(Game.Player.Handle) + "\n===========");
                    ped.Weapons.Give(AllowedPrimary[r.Next(0, AllowedPrimary.Count)], 300, true, true);
                    UnitPeds.Add(ped);
                    if (i != 0)
                    {
                        ped.SetIntoVehicle(UnitVehicle, VehicleSeat.Any);
                    }
                    else
                    {
                        ped.SetIntoVehicle(UnitVehicle, VehicleSeat.Driver);
                        ped.Task.DriveTo(UnitVehicle, backup_target.Character.Position, 20f, 50f, drivingStyle);
                        enRoute = true;
                    }
                }
                isSpawned = true;
                enRoute = true;
            }

            public override void Handle()
            {
                if (isSpawned)
                {
                    if (enRoute)
                    {
                        Vector3 unPos = UnitVehicle.Position;
                        Vector3 tarPos = backup_target.Character.Position;
                        if (API.GetDistanceBetweenCoords(unPos.X, unPos.Y, unPos.Z, tarPos.X, tarPos.Y, tarPos.Z, true) < 36)
                        {
                            enRoute = false;
                            for (int i = 0; i < UnitPeds.Count; i++)
                            {
                                UnitPeds[i].Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                            }
                            fight = true;
                            API.Wait(200);
                        }
                    }
                    else
                    {
                        if (fight)
                        {
                            for (int i = 0; i < UnitPeds.Count; i++)
                            {
                                UnitPeds[i].Task.FightAgainstHatedTargets(60);
                            }
                            Ped[] peds = World.GetAllPeds();
                            for (int i = 0; i < peds.Length; i++)
                            {
                                //if (peds[i].IsInCombat && !peds[i].IsPlayer && API.GetDistanceBetweenCoords())

                            }
                        }
                    }


                    
                }
            }
        }
    }
}
