using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.IO;

namespace Client
{
    static class Localization
    {
        //Worst localization ever. My skills back in 2015 didn't allow me to use serialization in FiveM

        private const string LocalizationPath = "localization/local.ini";

        public static string Coroner_answer_request { get; private set; }
        public static string Police_notification_already_cop { get; private set; }
        public static string Police_notification_now_cop { get; private set; }
        public static string PoliceBase_no_online_cops { get; private set; }
        public static string Error_No_dep { get; private set; }
        public static string Error_not_cop { get; private set; }
        public static string String_Success { get; private set; }
        public static string String_Failure { get; private set; }
        public static string PoliceBase_joined_department { get; private set; }
        public static string PoliceBase_left_department { get; private set; }
        public static string Menues_Backup_title { get; private set; }
        #region Main menu
        public static string Menues_Main_title { get; private set; }
        public static string Menues_Main_GoCop { get; private set; }
        public static string Menues_Main_GoCop_Description { get; private set; }
        public static string Menues_Main_CopList { get; private set; }
        public static string Menues_Main_DepList { get; private set; }
        public static string Menues_Main_JoinDep { get; private set; }
        public static string Menues_Main_LeaveDep { get; private set; }
        public static string Menues_Main_CODList { get; private set; }
        #endregion
        public static string Menues_Dispatch_Title { get; private set; }
        #region Other codes
        public static string Menues_Dispatch_Code1 { get; private set; }
        public static string Menues_Dispatch_Code2 { get; private set; }
        public static string Menues_Dispatch_Code3 { get; private set; }
        public static string Menues_Dispatch_Code4 { get; private set; }
        public static string Menues_Dispatch_Code6 { get; private set; }
        public static string Menues_Dispatch_Code8 { get; private set; }
        public static string Menues_Dispatch_Code9 { get; private set; }
        public static string Menues_Dispatch_Code10 { get; private set; }
        public static string Menues_Dispatch_Code20 { get; private set; }
        public static string Menues_Dispatch_Code30 { get; private set; }
        #endregion
        #region 10 Codes
        public static string Menues_Dispatch_Code10_4 { get; private set; }
        public static string Menues_Dispatch_Code10_8 { get; private set; }
        public static string Menues_Dispatch_Code10_7 { get; private set; }
        public static string Menues_Dispatch_Code10_9 { get; private set; }
        public static string Menues_Dispatch_Code10_22 { get; private set; }
        public static string Menues_Dispatch_Code10_23 { get; private set; }
        public static string Menues_Dispatch_Code10_97 { get; private set; }
        public static string Menues_Dispatch_Code10_26 { get; private set; }
        public static string Menues_Dispatch_Code10_20 { get; private set; }
        public static string Menues_Dispatch_Code10_39 { get; private set; }
        public static string Menues_Dispatch_Code10_10 { get; private set; }
        public static string Menues_Dispatch_Code10_53 { get; private set; }
        public static string Menues_Dispatch_Code10_66 { get; private set; }
        public static string Menues_Dispatch_Code10_71 { get; private set; }
        public static string Menues_Dispatch_Code10_72 { get; private set; }
        public static string Menues_Dispatch_Code10_79 { get; private set; }
        public static string Menues_Dispatch_Code10_15 { get; private set; }
        public static string Menues_Dispatch_Code10_16 { get; private set; }
        public static string Menues_Dispatch_Code10_17 { get; private set; }
        public static string Menues_Dispatch_Code10_19 { get; private set; }
        public static string Menues_Dispatch_Code10_96 { get; private set; }
        #endregion
        #region 11 Codes
        public static string Menues_Dispatch_Code11_54 { get; private set; }
        public static string Menues_Dispatch_Code11_41 { get; private set; }
        public static string Menues_Dispatch_Code11_42 { get; private set; }
        public static string Menues_Dispatch_Code11_55 { get; private set; }
        public static string Menues_Dispatch_Code11_56 { get; private set; }
        public static string Menues_Dispatch_Code11_57 { get; private set; }
        public static string Menues_Dispatch_Code11_85 { get; private set; }
        public static string Menues_Dispatch_Code11_94 { get; private set; }
        public static string Menues_Dispatch_Code11_95 { get; private set; }
        public static string Menues_Dispatch_Code11_96 { get; private set; }
        public static string Menues_Dispatch_Code11_97 { get; private set; }
        #endregion

        public static string Menues_Dispatch_Codes10_title { get; private set; }
        public static string Menues_Dispatch_Codes11_title { get; private set; }
        public static string Dispatch_Panic_Message { get; private set; }
        public static string Dispatch_Officer_Not_Answering { get; private set; }
        public static string Dispatch_Officers_Sent_To_LKL { get; private set; }
        public static string Menues_Body_Menu_Head { get; private set; }
        public static string Menues_Body_CallEMS { get; private set; }
        public static string Menues_Body_DragBody { get; private set; }
        public static string Notification_Body_DeliveredToHospital { get; private set; }
        //public static string Menues_Body_DragBody { get; private set; }
        //public static string Coroner_answer_request { get; private set; }
        //public static string Coroner_answer_request { get; private set; }
        //public static string Coroner_answer_request { get; private set; }
        //public static string Coroner_answer_request { get; private set; }
        //public static string Coroner_answer_request { get; private set; }

        private static void SaveLocalizedText(string[] loctext)
        {
            Coroner_answer_request = loctext[0];
            Police_notification_already_cop = loctext[1];
            Police_notification_now_cop = loctext[2];
            PoliceBase_no_online_cops = loctext[3];
            Error_No_dep = loctext[4];
            Error_not_cop = loctext[5];
            PoliceBase_joined_department = loctext[6];
            PoliceBase_left_department = loctext[7];

            Menues_Backup_title = loctext[8];

            Menues_Main_title = loctext[9];
            Menues_Main_GoCop = loctext[10];
            Menues_Main_GoCop_Description = loctext[11];
            Menues_Main_CopList = loctext[12];
            Menues_Main_DepList = loctext[13];
            Menues_Main_JoinDep = loctext[14];
            Menues_Main_LeaveDep = loctext[15];
            Menues_Main_CODList = loctext[16];

            Menues_Dispatch_Title = loctext[17];
            Menues_Dispatch_Code1 = loctext[18];
            Menues_Dispatch_Code2 = loctext[19];
            Menues_Dispatch_Code3 = loctext[20];
            Menues_Dispatch_Code4 = loctext[21];
            Menues_Dispatch_Code6 = loctext[22];
            Menues_Dispatch_Code8 = loctext[23];
            Menues_Dispatch_Code9 = loctext[24];
            Menues_Dispatch_Code10 = loctext[25];
            Menues_Dispatch_Code20 = loctext[26];
            Menues_Dispatch_Code30 = loctext[27];

            Menues_Dispatch_Code10_4 = loctext[28];
            Menues_Dispatch_Code10_7 = loctext[29];
            Menues_Dispatch_Code10_8 = loctext[30];
            Menues_Dispatch_Code10_9 = loctext[31];
            Menues_Dispatch_Code10_22 = loctext[32];
            Menues_Dispatch_Code10_23 = loctext[33];
            Menues_Dispatch_Code10_97 = loctext[34];
            Menues_Dispatch_Code10_26 = loctext[35];
            Menues_Dispatch_Code10_20 = loctext[36];
            Menues_Dispatch_Code10_39 = loctext[37];
            Menues_Dispatch_Code10_10 = loctext[38];
            Menues_Dispatch_Code10_53 = loctext[39];
            Menues_Dispatch_Code10_66 = loctext[40];
            Menues_Dispatch_Code10_71 = loctext[41];
            Menues_Dispatch_Code10_72 = loctext[42];
            Menues_Dispatch_Code10_79 = loctext[43];
            Menues_Dispatch_Code10_15 = loctext[44];
            Menues_Dispatch_Code10_16 = loctext[45];
            Menues_Dispatch_Code10_17 = loctext[46];
            Menues_Dispatch_Code10_19 = loctext[47];
            Menues_Dispatch_Code10_96 = loctext[48];
            Menues_Dispatch_Code11_54 = loctext[49];
            Menues_Dispatch_Code11_41 = loctext[50];
            Menues_Dispatch_Code11_42 = loctext[51];
            Menues_Dispatch_Code11_55 = loctext[52];
            Menues_Dispatch_Code11_56 = loctext[53];
            Menues_Dispatch_Code11_57 = loctext[54];
            Menues_Dispatch_Code11_85 = loctext[55];
            Menues_Dispatch_Code11_94 = loctext[56];
            Menues_Dispatch_Code11_95 = loctext[57];
            Menues_Dispatch_Code11_96 = loctext[58];
            Menues_Dispatch_Code11_97 = loctext[59];

            Menues_Dispatch_Codes10_title = loctext[60];
            Menues_Dispatch_Codes11_title = loctext[61];

            Dispatch_Panic_Message = loctext[62];

            Menues_Body_Menu_Head = loctext[63];
            Menues_Body_CallEMS = loctext[64];
            Menues_Body_DragBody = loctext[65];

            Notification_Body_DeliveredToHospital = loctext[66];
            Dispatch_Officer_Not_Answering = loctext[67];
            Dispatch_Officers_Sent_To_LKL = loctext[68];

            String_Success = loctext[69];
            String_Failure = loctext[70];
        }

        public static bool LoadLocalization()
        {
            string[] loctext = null;
            try
            {
                String data = Function.Call<string>(Hash.LOAD_RESOURCE_FILE, "PoliceBasics", LocalizationPath);
                loctext = data.Split('\n');

            }
            catch (Exception ex)
            {
                Debug.WriteLine("[PoliceBasics] Cought exception: " + ex.Message);
                return false;
            }

            if (loctext == null) return false;

            SaveLocalizedText(loctext);

            return true;
        }

    }
}
