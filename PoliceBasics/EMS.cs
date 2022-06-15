using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class EMS
    {
        private static List<EMS> WorkingEMSList = new List<EMS>();


        private const PedHash DriverHash = PedHash.Paramedic01SMM;
        private const PedHash Medic1Hash = PedHash.Paramedic01SMM;
        private const PedHash Medic2Hash = PedHash.Paramedic01SMM;
        private const VehicleHash vehicleHash = VehicleHash.Ambulance;
        private const bool MarkOnMap = true;
        private const int EMSDistance = 100;
        private const float SecToDelete = 360f;
        private const float SecToUnStuck = 60f;

        private const int DrivingStyle = 525100;

        private static List<Vector3> SpawnPoints = new List<Vector3>()
        {
            new Vector3(305.34f, -1436.77f, 29.9f),
            new Vector3(364.38f, -591.07f, 28.8f),
            new Vector3(1827.63f, 3694.06f, 34.3f),
            new Vector3(-259.45f, 6339.18f, 32.47f)
        };

        private static Vector3 GetClosestSpawnPoint(Vector3 position)
        {
            float distance = 999999999f;
            int posId = -1;
            for (int i = 0; i < SpawnPoints.Count; i++)
            {
                float point_distance = API.GetDistanceBetweenCoords(position.X, position.Y, position.Z, SpawnPoints[i].X, SpawnPoints[i].Y, SpawnPoints[i].Z, false);
                if (point_distance < distance)
                {
                    distance = point_distance;
                    posId = i;
                }
            }
            return SpawnPoints[posId];
        }
        public static void HandleAllEms()
        {
            foreach (EMS ems in WorkingEMSList)
            {
                ems.HandleActions();
            }
        }
        public static void SpawnEMS(Player target_player, Player caller_player)
        {
            new EMS(target_player, caller_player);
        }
        public static void SpawnEMS(Ped target, Player caller_player)
        {
            new EMS(target, caller_player);
        }

        private Ped target;
        private Player target_player;
        private Player caller_player;

        public EMS(Player target_player, Player caller_player)
        {
            this.target_player = target_player;
            this.target = target_player.Character;
            this.caller_player = caller_player;
            target_location = target_player.Character.Position;
            
            Spawn();

            WorkingEMSList.Add(this);
        }
        public EMS(Ped target, Player caller_player)
        {
            this.target = target;
            this.target_player = null;
            this.caller_player = caller_player;
            target_location = target.Position;

            Spawn();

            WorkingEMSList.Add(this);
        }


        private Vector3 target_location;
        private Vehicle vehicle;
        private int vehicleBlip;
        private Ped driver;
        private Ped medic1;
        private Ped medic2;
        private bool spawned = false;
        private bool enRoute = false;

        private async void Spawn()
        {
            //Spawn the Van
            Vector3 vehicleSpawnLocation = new Vector3();
            float spawnHeading = 0f;
            int unusedvar1 = 0;
            API.GetNthClosestVehicleNodeWithHeading(target.Position.X, target.Position.Y, target.Position.Z, EMSDistance, ref vehicleSpawnLocation, ref spawnHeading, ref unusedvar1, 9, 3.0f, 2.5f);

            await LoadModel((uint)vehicleHash);
            vehicle = await World.CreateVehicle(vehicleHash, vehicleSpawnLocation, spawnHeading-90);
            vehicle.Mods.LicensePlate = $"EMS {Game.Player.ServerId}";
            vehicle.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlue;


            //Van blip
            if (MarkOnMap)
            {
                vehicleBlip = API.AddBlipForEntity(vehicle.Handle);
                API.SetBlipColour(vehicleBlip, (int)BlipColor.Red);
                API.BeginTextCommandSetBlipName("STRING");
                API.AddTextComponentString("EMS");
                API.EndTextCommandSetBlipName(vehicleBlip);
            }

            //Spawn driver 
            await LoadModel((uint)DriverHash);
            driver = await World.CreatePed(DriverHash, vehicleSpawnLocation);
            driver.SetIntoVehicle(vehicle, VehicleSeat.Driver);
            driver.CanBeTargetted = false;

            //Spawn mdeic 1
            await LoadModel((uint)Medic1Hash);
            medic1 = await World.CreatePed(Medic1Hash, vehicleSpawnLocation);
            medic1.SetIntoVehicle(vehicle, VehicleSeat.Passenger);
            medic1.CanBeTargetted = false;

            //Spawn mdeic 2
            await LoadModel((uint)Medic2Hash);
            medic2 = await World.CreatePed(Medic2Hash, vehicleSpawnLocation);
            medic2.SetIntoVehicle(vehicle, VehicleSeat.Any);
            medic2.CanBeTargetted = false;

            // Configuration
            float targetHeading = 0F;
            API.GetClosestVehicleNodeWithHeading(target_location.X, target_location.Y, target_location.Z, ref target_location, ref targetHeading, 1, 3.0f, 0);
            TaskSequence ts = new TaskSequence();
            ts.AddTask.DriveTo(vehicle, target_location, 10f, 60f, DrivingStyle);
            driver.Task.PerformSequence(ts);
            vehicle.IsSirenActive = true;
            spawned = true;
            enRoute = true;

            
        }

        private void Despawn()
        {
            vehicle.Delete();
            driver.Delete();
            medic1.Delete();
            medic2.Delete();
        }

        private const int UnstuckDistance = 20;
        private void Unstuck()
        {
            Vector3 unstucklocation = new Vector3();
            float spawnHeading = 0f;
            int unusedvar1 = 0;
            API.GetNthClosestVehicleNodeWithHeading(target_player.Character.Position.X, target_player.Character.Position.Y, target_player.Character.Position.Z, UnstuckDistance, ref unstucklocation, ref spawnHeading, ref unusedvar1, 9, 3.0f, 2.5f);

            vehicle.Position = unstucklocation;
            vehicle.Heading = spawnHeading;
        }

        private bool loadPlayer = false;
        private float loadingProgress = 0;
        private bool goToPlayer = false;
        private bool goingBack = false;
        private Vector3 closestHosp;

        private void HandleActions()
        {
            if (spawned)
            {
                vehicle.IsSirenActive = true;
                if (enRoute && driver.TaskSequenceProgress == -1)
                {
                    enRoute = false;
                    goToPlayer = true;

                    //med1
                    TaskSequence med1Seq1 = new TaskSequence();
                    med1Seq1.AddTask.LeaveVehicle();
                    med1Seq1.AddTask.GoTo(target, new Vector3(0.1f, 0.1f, 0));
                    medic1.Task.PerformSequence(med1Seq1);

                    //med2
                    TaskSequence med2Seq1 = new TaskSequence();
                    med2Seq1.AddTask.LeaveVehicle();
                    med2Seq1.AddTask.GoTo(target, new Vector3(-0.1f, -0.1f, 0));
                    medic2.Task.PerformSequence(med2Seq1);

                }
                if (goToPlayer)
                {
                    float med1ToTargetDist = API.GetDistanceBetweenCoords(medic1.Position.X, medic1.Position.Y, medic1.Position.Z, target.Position.X, target.Position.Y, target.Position.Z, false);
                    float med2ToTargetDist = API.GetDistanceBetweenCoords(medic2.Position.X, medic2.Position.Y, medic2.Position.Z, target.Position.X, target.Position.Y, target.Position.Z, false);
                    
                    if (med1ToTargetDist < 1.5f && med2ToTargetDist < 1.5f)
                    {
                        goToPlayer = false;
                        loadPlayer = true;
                        //medic1.Task.PlayAnimation("amb@medic@standing@kneel@idle_a", "idle_a");
                        //medic2.Task.PlayAnimation("amb@medic@standing@tendtodead@idle_a", "idle_a");
                        medic1.Task.StartScenario("CODE_HUMAN_MEDIC_KNEEL", target.Position);
                        medic2.Task.StartScenario("CODE_HUMAN_MEDIC_TEND_TO_DEAD", target.Position);
                    }
                }
                if (loadPlayer)
                {
                    loadingProgress += 0.1f;
                    if (loadingProgress >= 5)
                    {
                        loadPlayer = false;
                        target.Position = vehicle.Position;
                        target.AttachTo(vehicle);
                        medic1.Task.WarpIntoVehicle(vehicle, VehicleSeat.Passenger);
                        medic2.Task.WarpIntoVehicle(vehicle, VehicleSeat.RightRear);
                        closestHosp = GetClosestSpawnPoint(driver.Position);
                        driver.Task.DriveTo(vehicle, closestHosp, 10, 60, DrivingStyle);
                        goingBack = true;
                    }
                }
                if (goingBack && API.GetDistanceBetweenCoords(vehicle.Position.X, vehicle.Position.Y, vehicle.Position.Z, closestHosp.X, closestHosp.Y, closestHosp.Z, false) < 14)
                {
                    target.Detach();
                    vehicle.Delete();
                    driver.Delete();
                    medic1.Delete();
                    medic2.Delete();
                    WorkingEMSList.Remove(this);
                    if (target_player == null)
                    {
                        target.Delete();
                        Screen.ShowNotification(Localization.Notification_Body_DeliveredToHospital);
                    }
                    else
                    {
                        API.NetworkResurrectLocalPlayer(Game.Player.Character.Position.X, Game.Player.Character.Position.Y + 2, Game.Player.Character.Position.Z, Game.Player.Character.Heading, true, false);
                        API.SetPlayerInvincible(Game.Player.ServerId, false);
                        Game.Player.Character.ClearBloodDamage();
                    }
                }

                CheckForStuck();
                CheckForDelete();
            }
        }
        /*
        private void HandleActions()
        {
            return;
            if (spawned)
            {
                vehicle.IsSirenActive = true;
                if (enRoute && API.GetDistanceBetweenCoords(vehicle.Position.X, vehicle.Position.Y, vehicle.Position.Z, 
                                                            target.Position.X, target.Position.Y, target.Position.Z, false) < 20 && vehicle.IsStopped)
                {
                    enRoute = false;
                    goToPlayer = true;
                    medic1.Task.LeaveVehicle();
                    medic2.Task.LeaveVehicle();
                    API.Wait(300);
                    medic1.Task.GoTo(target, new Vector3(0.1f, 0.1f, 0));
                    medic2.Task.GoTo(target, new Vector3(-0.1f, -0.1f, 0));
                }
                if (goToPlayer)
                {
                    float med1ToTargetDist = API.GetDistanceBetweenCoords(medic1.Position.X, medic1.Position.Y, medic1.Position.Z, target.Position.X, target.Position.Y, target.Position.Z, false);
                    float med2ToTargetDist = API.GetDistanceBetweenCoords(medic2.Position.X, medic2.Position.Y, medic2.Position.Z, target.Position.X, target.Position.Y, target.Position.Z, false);

                    if (med1ToTargetDist < 1.5f && med2ToTargetDist < 1.5f)
                    {
                        goToPlayer = false;
                        loadPlayer = true;
                        medic1.Task.PlayAnimation("amb@medic@standing@kneel@idle_a", "idle_a");
                        medic2.Task.PlayAnimation("amb@medic@standing@tendtodead@idle_a", "idle_a");
                    }
                }
                if (loadPlayer)
                {
                    loadingProgress += 0.1f;
                    if (loadingProgress >= 5)
                    {
                        loadPlayer = false;
                        target.Position = vehicle.Position;
                        target.AttachTo(vehicle);
                        medic1.Task.WarpIntoVehicle(vehicle, VehicleSeat.Passenger);
                        medic2.Task.WarpIntoVehicle(vehicle, VehicleSeat.RightRear);
                        closestHosp = GetClosestSpawnPoint(driver.Position);
                        driver.Task.DriveTo(vehicle, closestHosp, 10, 60, DrivingStyle);
                        goingBack = true;
                    }
                }
                if (goingBack && API.GetDistanceBetweenCoords(vehicle.Position.X, vehicle.Position.Y, vehicle.Position.Z, closestHosp.X, closestHosp.Y, closestHosp.Z, false) < 14)
                {
                    target.Detach();
                    vehicle.Delete();
                    driver.Delete();
                    medic1.Delete();
                    medic2.Delete();
                    WorkingEMSList.Remove(this);
                    if (target_player == null)
                    {
                        target.Delete();
                        Debug.WriteLine("===================" + target.ToString() + "=================");
                        Screen.ShowNotification(Localization.Notification_Body_DeliveredToHospital);
                    }
                    else
                    {
                        API.NetworkResurrectLocalPlayer(Game.Player.Character.Position.X, Game.Player.Character.Position.Y + 2, Game.Player.Character.Position.Z, Game.Player.Character.Heading, true, false);
                        API.SetPlayerInvincible(Game.Player.ServerId, false);
                        Game.Player.Character.ClearBloodDamage();
                    }
                }

                CheckForStuck();
                CheckForDelete();
            }
        }
        */

        private float DeleteCounter = 0;

        private void CheckForDelete()
        {
            DeleteCounter += 0.1f;
            if (DeleteCounter >= SecToDelete)
            {
                if (medic1.IsDead && medic2.IsDead)
                {
                    Despawn();
                }
            }
        }

        private float StuckCounter = 0;

        private void CheckForStuck()
        {
            StuckCounter += 0.1f;
            if (enRoute && StuckCounter >= SecToUnStuck)
            {
                StuckCounter = 0f;
                Unstuck();
            }
        }

        private async Task<bool> LoadModel(uint model)
        {
            if (!API.IsModelInCdimage(model))
            {
                Debug.WriteLine($"Invalid model {model} was supplied to LoadModel.");
                return false;
            }
            API.RequestModel(model);
            while (!API.HasModelLoaded(model))
            {
                Debug.WriteLine($"Waiting for model {model} to load");
                await BaseScript.Delay(100);
            }
            return true;
        }
    }

}
