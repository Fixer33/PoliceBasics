using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using NativeUI;

namespace Client
{
    static class BodyInteract
    {
        #region static and const Vars

        private const float bodyDistance = 1f;

        public static Ped body { get; private set; }

        #endregion

        public static bool CheckForBody()
        {
            isDragging = false;
            Player player = Game.Player;
            Vector3 player_pos = player.Character.Position;

            /*Vector3 cast_to = API.GetOffsetFromEntityGivenWorldCoords(player.Handle, 0.0f, bodyDistance, 0.0f);
            int ray_handle = API.CastRayPointToPoint(player_pos.X, player_pos.Y, player_pos.Z, cast_to.X, cast_to.Y, cast_to.Z, 10, API.GetPlayerPed(player.ServerId), 0);
            
            bool hit = false;
            Vector3 endCoords = new Vector3();
            Vector3 surface_normal = new Vector3();
            int entity_hit = 0;
            int ray_result = API.GetRaycastResult(ray_handle, ref hit, ref endCoords, ref surface_normal, ref entity_hit);
            Ped body;
            try
            {
                body = API.GetPed
            }*/

            Ped[] peds = World.GetAllPeds();
            for (int i = 0; i < peds.Length; i++)
            {
                if (peds[i].IsDead)
                {
                    Vector3 body_pos = peds[i].Position;
                    if (API.GetDistanceBetweenCoords(player_pos.X, player_pos.Y, player_pos.Z, body_pos.X, body_pos.Y, body_pos.Z, false) <= bodyDistance)
                    {
                        Debug.WriteLine("==========\nBodyFound\n=========");
                        body = peds[i];
                        return true;
                    }
                }
            }
            return false;
        }

        public static void PerformCPR()
        {
            TaskSequence reviveAnim = new TaskSequence();
            reviveAnim.AddTask.PlayAnimation("get_up@directional@transition@prone_to_knees@injured", "front");
            reviveAnim.AddTask.PlayAnimation("get_up@directional@movement@from_knees@injured", "getup_r_180");
            Game.PlayerPed.Task.ClearAllImmediately();
            Game.PlayerPed.Task.PerformSequence(reviveAnim);
        }

        #region Dragging

        public static bool isDragging { get; private set; }

        public static void StartDragging()
        {
            if (isDragging) return;
            body.AttachTo(Game.Player.Character, new Vector3(0, 0, 0));
            isDragging = true;
        }

        public static void StopDragging()
        {
            if (!isDragging) return;
            Debug.WriteLine("================\nDetach\n=========");
            body.Detach();
        }

        #endregion

    }
}
