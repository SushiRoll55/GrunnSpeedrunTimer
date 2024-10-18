using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using Unity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Reflection;
using UnityEngine.EventSystems;
using TMPro;

namespace GrunnSpeedrunTimer
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class GrunnSpeedrunTimer : BaseUnityPlugin
    {
        public const string pluginGuid = "sushiroll.grunn.grunnspeedruntimer";
        public const string pluginName = "Grunn Speedrun Timer";
        public const string pluginVersion = "0.1";

        Harmony harmony;

        GameObject timerObj;
        TMP_Text timerText;

        public static bool showMs;

        #region Prefix/Postfix creation helper functions
        void PatchPrefix(Type origType, string origName, string concat = "_Patch")
        {
            MethodInfo original = AccessTools.Method(origType, origName);
            MethodInfo patch = AccessTools.Method(typeof(GrunnSpeedrunTimer), origName + concat);
            harmony.Patch(original, new HarmonyMethod(patch));
        }

        void PatchPostfix(Type origType, string origName, string concat = "_Patch")
        {
            MethodInfo original = AccessTools.Method(origType, origName);
            MethodInfo patch = AccessTools.Method(typeof(GrunnSpeedrunTimer), origName + concat);
            harmony.Patch(original, null, new HarmonyMethod(patch));
        }
		#endregion

		public void Awake()
		{
            Logger.LogInfo("Speedrun timer mod loaded!");

            harmony = new Harmony(pluginGuid);

            showMs = PlayerPrefs.GetInt("showMs") == 1 ? true : false;
		}

        public void Update()
		{
            if (InGameState())
			{
                if (timerObj == null) InstantiateTimerObj();
                if (!timerObj.activeSelf) timerObj.SetActive(true);
                UpdateTimer();
            }
            else
			{
                if (timerObj != null) timerObj.SetActive(false);
			}

            if (Keyboard.current.mKey.wasPressedThisFrame)
			{
                showMs = !showMs;
                PlayerPrefs.SetInt("showMs", showMs ? 1 : 0);
			}
		}

        void InstantiateTimerObj()
		{
            var mgr = UIManager.instance;

            timerObj = Instantiate(mgr.timeRect.gameObject, mgr.timeRect.transform.parent);
            timerObj.name = "SpeedrunTimer";

            timerObj.GetComponent<ContentSizeFitter>().enabled = false;
            var rect = timerObj.GetComponent<RectTransform>();
            rect.transform.localPosition = new Vector3(80, 330, 0);
            rect.anchoredPosition = new Vector2(-480, -30);
            rect.sizeDelta = new Vector2(300, 85);

            timerText = timerObj.GetComponentInChildren<TMP_Text>();
            timerText.text = "00:00";
            timerText.enableAutoSizing = true;
        }

        void UpdateTimer()
		{
            if (timerText != null)
            {
                if (showMs)
                {
                    timerText.alignment = TextAlignmentOptions.Left;
                    timerText.margin = new Vector4(15, 0, 0, 0);
                }
                else
                {
                    timerText.alignment = TextAlignmentOptions.Center;
                    timerText.margin = new Vector4(0, 0, 0, 0);
                }
            }

            float time = SaveManager.progressDataCheck.playTime;
            int hours = (int)(time / 3600);
            int minutes = (int)((time % 3600) / 60);
            int seconds = (int)(time % 60);
            float msDecimal = (time - Mathf.Floor(time)) * 1000;
            int ms = Mathf.FloorToInt(msDecimal);

            string timeStr = "";

            if (hours > 0) timeStr = $"{hours}:";

            timeStr += string.Format("{0:D2}:{1:D2}", minutes, seconds);

            if (showMs) timeStr += string.Format(":{0:D3}", ms);

            timerText.text = timeStr;
		}

        public static bool InGameState()
        {
            var curState = GameManager.CurGameState;
            return curState == GameManager.GameState.Game || curState == GameManager.GameState.Paused;
        }

        //Unused
        public static void GetEndingString_Patch(ref string ___endingString)
		{
            float time = SaveManager.progressDataCheck.playTime;
            float msDecimal = (time - Mathf.Floor(time)) * 1000;
            int ms = Mathf.FloorToInt(msDecimal);
            ___endingString += string.Format("{0:D3}ms", ms);
		}
    }
}
