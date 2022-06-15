using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Dispatch
    {
        #region Constants

        private const float COP_CHECK_TIME = 300f;

        #endregion

        #region Handling

        public static void Handle()
        {
            Handle_11_97();
        }

        #region 11_97

        private static List<Cop> pending_answers_from = new List<Cop>();

        private static float CopCheckCounter = 0;
        private static bool pendingCops1 = false;
        private static bool pendingCops2 = false;
        private static bool pendingCops3 = false;
        private static float CopCheck1ndCounter = 0f;
        private static float CopCheck2ndCounter = 0f;
        private static float CopCheck3ndCounter = 0f;

        private static void Handle_11_97()
        {
            if (!pendingCops1 && !pendingCops2 && !pendingCops3)
            {
                CopCheckCounter += 0.1f;
                if (CopCheckCounter >= COP_CHECK_TIME)
                {
                    CopCheckCounter = 0f;
                    Send11_97_ToCops();

                    if (pending_answers_from.Count > 0)
                        pendingCops1 = true;
                }

            }

            if (pendingCops1)
            {
                CopCheck1ndCounter += 0.1f;
                if (CopCheck1ndCounter >= COP_CHECK_TIME / 2)
                {
                    CopCheck1ndCounter = 0f;
                    Send11_97_ToCops(pending_answers_from);

                    if (pending_answers_from.Count > 0)
                        pendingCops2 = true;
                }

            }

            if (pendingCops2)
            {
                CopCheck2ndCounter += 0.1f;
                if (CopCheck2ndCounter >= COP_CHECK_TIME / 4)
                {
                    CopCheck2ndCounter = 0f;
                    Send11_97_ToCops(pending_answers_from);

                    if (pending_answers_from.Count > 0)
                        pendingCops3 = true;
                }

            }

            if (pendingCops3)
            {
                CopCheck3ndCounter += 0.1f;
                if (CopCheck3ndCounter >= COP_CHECK_TIME / 8)
                {
                    CopCheck3ndCounter = 0f;

                    if (pending_answers_from.Count > 0)
                    {
                        //Check
                        Send_Officers_To_Check();
                        foreach (Cop cop in pending_answers_from)
                        {
                            cop.player.TriggerEvent("PoliceBasics:dispatch_officersSentToLKL");
                        }
                        pending_answers_from.Clear();
                    }
                    CopCheck1ndCounter = 0f;
                    CopCheck2ndCounter = 0f;
                    CopCheck3ndCounter = 0f;

                    pendingCops1 = false;
                    pendingCops2 = false;
                    pendingCops3 = false;
                }
            }
        }

        public static void Get_1197_CallbackFromOfficer(Cop cop)
        {
            cop.inTouch = true;
            if (pending_answers_from.Contains(cop))
            {
                pending_answers_from.Remove(cop);
            }

            if (pending_answers_from.Count == 0)
            {
                pendingCops1 = false;
                pendingCops2 = false;
                pendingCops3 = false;

                CopCheck1ndCounter = 0f;
                CopCheck2ndCounter = 0f;
                CopCheck3ndCounter = 0f;
            }
        }

        private static void Send_Officers_To_Check()
        {
            for (int i = 0; i < PoliceBase.CopsList.Count; i++)
            {
                Cop cop = PoliceBase.CopsList[i];
                if (cop.inTouch && cop.onDuty)
                {
                    foreach (Cop cop_to_check in pending_answers_from)
                    {
                        cop.player.TriggerEvent("PoliceBasics:dispatch_checkOfficer", cop_to_check.serverId);
                    }
                }
            }
        }

        private static void Send11_97_ToCops()
        {
            for (int i = 0; i < PoliceBase.CopsList.Count; i++)
            {
                Cop cop = PoliceBase.CopsList[i];
                if (cop.onDuty)
                {
                    cop.player.TriggerEvent("PoliceBasics:dispatch_1197");
                    cop.inTouch = false;
                    pending_answers_from.Add(cop);
                }
            }
        }
        private static void Send11_97_ToCops(List<Cop> cops)
        {
            for (int i = 0; i < cops.Count; i++)
            {
                cops[i].player.TriggerEvent("PoliceBasics:dispatch_1197");
            }
        }

        #endregion

        #endregion

    }
}
