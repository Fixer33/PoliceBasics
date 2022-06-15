using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class DriveByShooting : REvent
    {
        public Player target;

        private const int SpawnDistance = 60;

        public List<Ped> peds = new List<Ped>();
        public Vehicle vehicle;

        public DriveByShooting()
        {
            isCopOnly = false;

            SelectTarget();
            
        }

        private void SelectTarget()
        {
            int player_count = 0;
            foreach (Player player in ClientMain.players)
            {
                player_count++;
            }

            int player_to_target_id = new Random().Next(0, player_count);
            Player _target = null;

            int counter = 0;
            foreach (Player player in ClientMain.players)
            {
                if (counter++ >= player_to_target_id)
                {
                    _target = player;
                }
            }
            if (_target != null)
            {

            }
            else
            {
                Debug.WriteLine("~r~PoliceBasics:~w~ Could not find target for ~g~DriveByShooting~w~");
                return;
            }
        }


        public override void Handle()
        {
            
        }

        public override async void Summon()
        {
            //Ped playerPed = Game.PlayerPed;

            ////Get position and heading
            //Vector3 spawn_pos = new Vector3();
            //float heading = 0f;
            //int unused1 = 0;
            //API.GetNthClosestVehicleNodeWithHeading(playerPed.Position.X, playerPed.Position.Y, playerPed.Position.Z, SpawnDistance, ref spawn_pos, ref heading, ref unused1, 0, 0, 0);

            ////Select vehicle
            //List<VehicleHash> allowedVehicles = new List<VehicleHash>()
            //{
            //    VehicleHash.Cavalcade, VehicleHash.Granger, VehicleHash.Premier, VehicleHash.Washington, VehicleHash.GBurrito
            //};
            //VehicleHash vehicleModel = allowedVehicles[random.Next(0, allowedVehicles.Count)];

            ////Select weapons
            //List<WeaponHash> allowedWeapons = new List<WeaponHash>()
            //{
            //    WeaponHash.SMG, WeaponHash.MicroSMG, WeaponHash.AssaultSMG, WeaponHash.CombatPistol
            //};
            //List<WeaponHash> weapons = new List<WeaponHash>();
            //for (int i = 0; i < occupantsCount; i++)
            //{
            //    weapons.Add(allowedWeapons[random.Next(0, allowedWeapons.Count)]);
            //}

            ////Spawn
            ////vehicle
            //Vehicle vehicle = await World.CreateVehicle(vehicleModel, spawn_pos, heading);
            //vehicle.Mods.WindowTint = VehicleWindowTint.LightSmoke;

            ////peds
            //List<Ped> occupants = new List<Ped>();
            //for (int i = 0; i < occupantsCount; i++)
            //{
            //    Ped ped = await World.CreatePed(PedHash.Hao, spawn_pos, heading);
            //    ped.Weapons.Give(weapons[i], 9999, true, true);
            //    occupants.Add(ped);

            //    if (i == 0)
            //    {
            //        //driver
            //        ped.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            //    }
            //    else
            //    {
            //        //others
            //        ped.SetIntoVehicle(vehicle, VehicleSeat.Any);
            //    }
            //}

        }
    }
}
