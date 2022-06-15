using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    static class RandomEvents
    {
        private static Random random;

        public static void InitializeEvents(EventHandlerDictionary eventHandlers)
        {
            random = new Random();

            eventHandlers["PoliceBasics:randomEvent_spawnDriveBy"] += new Action<int, int, int>(Event_SpawnDriveByShooting);
        }

        private static async void Event_SpawnDriveByShooting(int rEvent_id, int spawnDistance, int occupantsCount)
        {
            


        }
    }
}
