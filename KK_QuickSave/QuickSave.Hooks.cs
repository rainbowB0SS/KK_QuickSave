using ChaCustom;
using HarmonyLib;
using BepInEx;
using BepInEx.Logging;

namespace KK_QuickSave
{
    public partial class QuickSave
    {          
        private static class Hooks
        {  
            [HarmonyPostfix]
            [HarmonyPatch(typeof(CustomCharaFile), "Start")]
            public static void CustomCharaFile_Start(CustomCharaFile __instance)
            {
                Logger.Log(LogLevel.Debug, "got CustomCharaFile");
                charaFile = __instance;
            }
        }
    }
}
