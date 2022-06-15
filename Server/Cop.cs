using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Cop
    {
        public string Name;
        public string steamid;
        public Player player;
        public Department currentDepartment;
        public int serverId = -1;
        public bool isBusy = false;
        public bool onDuty = false;
        public bool inTouch = true;

        public void LeaveDepartment()
        {
            if (currentDepartment == null)
            {
                Debug.WriteLine($"=======\nPlayer {this.Name} tries to leave dep, but he is not found at any dep\n========");
                return;
            }
            else
            {
                currentDepartment.Leave(this);
                currentDepartment = null;
            }
            player.TriggerEvent("PoliceBasics:leftDepartment");
        }
    }
}
