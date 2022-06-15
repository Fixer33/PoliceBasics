using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Threading.Tasks;

namespace Client
{
    static class Coroner
    {

        private const int coronerDistance = 30;

        //https://wiki.gtanet.work/index.php?title=Vehicle_Models#Vans
        private const VehicleHash model_to_spawn = VehicleHash.Speedo;

        //https://vespura.com/drivingstyle/
        private const int drivingStyle = 786603;

        //Let player trace of coroner movement
        private const bool MarkOnMap = true;

        private static Vehicle cVan;
        private static Ped cVanPed1;
        private static Ped cVanPed2;
        private static int cVanBlip;
        private static Vector3 targetLocation;
        private static Vector3 callLocation;

        private static bool eventSpawned = false;
        private static bool enRoute = false;
        private static bool working = false;

        public async static void Summon()
        {
            if (eventSpawned) return;

            Ped player = Game.Player.Character;
            callLocation = player.Position;
            Screen.ShowNotification(Localization.Coroner_answer_request + Game.Player.Name);
            //Spawn the Van
            Vector3 cVanSpawnLocation = new Vector3();
            float spawnHeading = 0f;
            int unusedvar1 = 0;
            //API.GetNthClosestVehicleNodeWithHeading(player.Position.X, player.Position.Y, player.Position.Z, coronerDistance, ref cVanSpawnLocation, ref spawnHeading, ref unusedvar1, 9, 3.0f, 2.5f);
            API.GetNthClosestVehicleNodeWithHeading(player.Position.X, player.Position.Y, player.Position.Z, coronerDistance, ref cVanSpawnLocation, ref spawnHeading, ref unusedvar1, 0, 0, 0);
            await LoadModel((uint)model_to_spawn);
            cVan = await World.CreateVehicle(model_to_spawn, cVanSpawnLocation, spawnHeading);
            cVan.Mods.PrimaryColor = VehicleColor.MetallicSteelGray;
            cVan.Mods.LicensePlate = $"SA C {Game.Player.ServerId}";
            cVan.Mods.LicensePlateStyle = LicensePlateStyle.YellowOnBlue;

            //Van blip
            if (MarkOnMap)
            {
                cVanBlip = API.AddBlipForEntity(cVan.Handle);
                API.SetBlipColour(cVanBlip, 40);
                API.BeginTextCommandSetBlipName("STRING");
                API.AddTextComponentString("Coroner");
                API.EndTextCommandSetBlipName(cVanBlip);
            }

            //Spawn driver 
            await LoadModel((uint)PedHash.Doctor01SMM);
            cVanPed1 = await World.CreatePed(PedHash.Doctor01SMM, cVanSpawnLocation);
            cVanPed1.SetIntoVehicle(cVan, VehicleSeat.Driver);
            cVanPed1.CanBeTargetted = false;

            //Spawn passenger
            await LoadModel((uint)PedHash.Scientist01SMM);
            cVanPed2 = await World.CreatePed(PedHash.Scientist01SMM, cVanSpawnLocation);
            cVanPed2.SetIntoVehicle(cVan, VehicleSeat.Passenger);
            cVanPed2.CanBeTargetted = false;
            // Configuration
            targetLocation = new Vector3();
            float targetHeading = 0F;
            API.GetClosestVehicleNodeWithHeading(player.Position.X, player.Position.Y, player.Position.Z, ref targetLocation, ref targetHeading, 9, 3.0f, 0);
            TaskSequence ts = new TaskSequence();
            ts.AddTask.DriveTo(cVan, targetLocation, 1f, 35f, drivingStyle);
            cVanPed1.Task.PerformSequence(ts);
            eventSpawned = true;
            enRoute = true;
        }

        private static bool startWorking = false;

        private static bool ped1Clean = false;
        private static bool ped2Clean = false;
        private static int ped1CleanTimes = 0;
        private static int ped2CleanTimes = 0;
        private static Ped current_ped1Target;
        private static Ped current_ped2Target;

        private static bool VanVanishing = false;
        private static int vanishingLevel = 255;

        public static void CalculateCoronerLogic()
        {
            if (eventSpawned)
            {
                if (startWorking)
                {
                    startWorking = false;
                    cVanPed1.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                    cVanPed2.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                    API.Wait(100);
                    //cVanPed1.Task.Wait(100);
                    //cVanPed2.Task.Wait(100);

                    working = true;
                }

                if (enRoute)
                { 
                    if (cVanPed1.TaskSequenceProgress == -1)
                    {
                        enRoute = false;
                        startWorking = true;

                    }
                }

                if (working)
                {

                    Ped[] allPeds = World.GetAllPeds();
                    if (!ped1Clean || !ped2Clean)
                    {

                        for (int i = 0; i < allPeds.Length; i++)
                        {
                            if (!ped1Clean)
                            {
                                if (allPeds[i].IsDead && API.GetDistanceBetweenCoords(cVanPed1.Position.X, cVanPed1.Position.Y, cVanPed1.Position.Z,
                                    allPeds[i].Position.X, allPeds[i].Position.Y, allPeds[i].Position.Z, false) < 40
                                    && (current_ped2Target == null || !current_ped2Target.Equals(allPeds[i])))
                                {
                                    current_ped1Target = allPeds[i];
                                    //cVanPed1.Task.GoTo(current_ped1Target);
                                    cVanPed1.Task.GoTo(current_ped1Target.Position);
                                    //API.Wait(1000);
                                    BaseScript.Delay(1000);
                                    cVanPed1.Task.PlayAnimation("missmechanic", "work2_base");
                                    ped1Clean = true;
                                }

                            }
                            if (!ped2Clean)
                            {
                                if (allPeds[i].IsDead && API.GetDistanceBetweenCoords(cVanPed1.Position.X, cVanPed1.Position.Y, cVanPed1.Position.Z,
                                    allPeds[i].Position.X, allPeds[i].Position.Y, allPeds[i].Position.Z, false) < 40
                                    && (current_ped1Target == null || !current_ped1Target.Equals(allPeds[i])))
                                {
                                    current_ped2Target = allPeds[i];
                                    //cVanPed2.Task.GoTo(current_ped2Target);
                                    cVanPed2.Task.GoTo(current_ped2Target.Position);
                                    //API.Wait(1000);
                                    BaseScript.Delay(1000);
                                    cVanPed2.Task.PlayAnimation("missmechanic", "work2_base");
                                    ped2Clean = true;
                                }
                            }
                        }
                        if (!ped1Clean && !ped2Clean)
                        {
                            cVanPed1.Task.WarpIntoVehicle(cVan, VehicleSeat.Driver);
                            cVanPed2.Task.WarpIntoVehicle(cVan, VehicleSeat.Passenger);
                            cVanPed1.Task.DriveTo(cVan, new Vector3(500f, -1000f, 31f), 50f, 35f, drivingStyle);
                            VanVanishing = true;
                        }
                    }
                }

                if (ped1Clean)
                {
                    if (ped1CleanTimes++ >= 200)
                    {
                        cVanPed1.Task.StandStill(100);
                        current_ped1Target.Delete();
                        ped1Clean = false;
                    }
                }
                if (ped2Clean)
                {
                    if (ped2CleanTimes++ >= 200)
                    {
                        cVanPed2.Task.StandStill(100);
                        current_ped2Target.Delete();
                        ped2Clean = false;
                    }
                }
                if (VanVanishing)
                {

                    cVan.Opacity = vanishingLevel--;
                    if (vanishingLevel <= 0)
                    {
                        cVan.Delete();
                        cVanPed1.Delete();
                        cVanPed2.Delete();
                        eventSpawned = false;
                        Reset();
                    }
                }
            }
        }

        private static void Reset()
        {
            cVan.Delete();
            cVanPed1.Delete();
            cVanPed2.Delete();
            eventSpawned = false;
            enRoute = false;
            working = false;
            VanVanishing = false;
            startWorking = false;
            ped1Clean = false;
            ped2Clean = false;
        }

        private static void Debugn(int i)
        {
            Debug.WriteLine("=========================");
            Debug.WriteLine(i.ToString());
            Debug.WriteLine("=========================");
        }

        private static async Task<bool> LoadModel(uint model)
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
