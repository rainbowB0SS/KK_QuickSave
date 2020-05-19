using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using KKAPI.Maker;
using HarmonyLib;
using BepInEx.Harmony;
using ChaCustom;
using ADV.Commands.Base;
using StrayTech;
using Illusion.Game;
using System.Collections.Generic;
using System;

namespace KK_QuickSave
{
    [BepInProcess("Koikatu")]
    [BepInDependency(KKAPI.KoikatuAPI.GUID)]
    [BepInPlugin(GUID, "QuickSave", Version)]
    public partial class QuickSave : BaseUnityPlugin
    {
        public const string GUID = "rainbowB0SS.quicksave";
        public const string Version = "0.1";

        static new ManualLogSource Logger;
        public static string lastFilename;
        public static CustomCharaFile charaFile;

        internal static ConfigEntry<KeyboardShortcut> HotkeySave { get; set; }

        private void Awake()
        {
            Logger = base.Logger;

            HotkeySave = Config.Bind("", "Save the current card", new KeyboardShortcut(KeyCode.Alpha8));

            MakerAPI.ChaFileLoaded += MakerAPI_ChaFileLoaded;

            HarmonyWrapper.PatchAll(typeof(Hooks));
        }

        private void MakerAPI_ChaFileLoaded(object sender, ChaFileLoadedEventArgs e)
        {
            Logger.Log(LogLevel.Debug, $"char file loaded ${e.Filename}");
            lastFilename = e.Filename;
        }

        private void Update()
        {

            if (MakerAPI.InsideAndLoaded && HotkeySave.Value.IsDown())
            {
                if (lastFilename.IsNullOrEmpty())
                {
                    Logger.Log(LogLevel.Debug, "no filename set");
                    return;
                }
                if (charaFile == null)
                {
                    Logger.Log(LogLevel.Debug, "no charaFile");
                    return;
                }

                ChaFileControl chaFile = Traverse.Create(charaFile).Property("chaFile").GetValue<ChaFileControl>();
                if (chaFile == null)
                {
                    Logger.Log(LogLevel.Debug, "could not get chaFile");
                    return;
                }
                List<CustomFileInfo> lstFileInfo = Traverse.Create(charaFile).Field("listCtrl").Field("lstFileInfo").GetValue<List<CustomFileInfo>>();
                if (lstFileInfo == null)
                {
                    Logger.Log(LogLevel.Debug, "could not get CustomFileInfo list");
                    return;
                }

                for (int i = 0; i < lstFileInfo.Count; i++)
                {
                    if (lstFileInfo[i].FullPath == lastFilename)
                    {
                        Logger.Log(LogLevel.Debug, "found a matching chara");

                        ChaFileControl chaFileControl = new ChaFileControl();
                        if (!chaFileControl.LoadCharaFile(lstFileInfo[i].FullPath, 255, false, true))
                        {
                        }
                        if (chaFileControl.facePngData != null && chaFileControl.pngData != null)
                        {
                            chaFile.facePngData = chaFileControl.facePngData;
                            chaFile.pngData = chaFileControl.pngData;
                        }
                        chaFile.SaveCharaFile(lstFileInfo[i].FullPath, byte.MaxValue, false);
                        lstFileInfo[i].time = DateTime.Now;
                        lstFileInfo[i].name = chaFile.parameter.fullname;
                        lstFileInfo[i].DeleteThumb();

                        Utils.Sound.Play(SystemSE.ok_s);
                        break;
                    }
                }
            }
        }
    }
}