using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Unity.FPS.Roguelike;
using Unity.FPS.Game;
using UnityEngine.UI;

namespace Unity.AI.Assistant.PlayModeTest
{
    [System.Serializable]
    public class TestResult {
        public bool success;
        public int initialXP;
        public int finalXP;
        public string[] logs;
    }

    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";

        private static readonly int WaitFrames = SessionState.GetInt("PlayModeTest.WaitFrames", 300);
        private static readonly float TestTimeout = SessionState.GetFloat("PlayModeTest.TestTimeout", 30.0f);

        private static List<string> _capturedLogs = new List<string>();

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");
            if (state == "WaitingForCompile")
            {
                EditorApplication.delayCall += () =>
                {
                    SessionState.SetString(StateKey, "EnteringPlayMode");
                    EditorApplication.isPlaying = true;
                };
            }
            else if (state == "EnteringPlayMode" && EditorApplication.isPlaying)
            {
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            }
            else if (state == "InPlayMode" && EditorApplication.isPlaying)
            {
                EditorApplication.update += WaitFramesThenRun;
            }
            else if (state == "Done")
            {
                EditorApplication.delayCall += SelfDestruct;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _testStartTime = 0;
        private static int _startXP = -1;
        private static bool _upgradeSelected = false;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;
            if (_testDone) return;

            if (!_setupDone)
            {
                _setupDone = true;
                Application.logMessageReceived += (m, s, t) => { if (_capturedLogs.Count < 50) _capturedLogs.Add("[" + t + "] " + m); };
                _testStartTime = EditorApplication.timeSinceStartup;
                var xpManager = XPManager.Instance;
                if (xpManager != null) _startXP = xpManager.CurrentXP;
                return;
            }

            float elapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            bool complete = Tick(elapsed);
            if (complete || elapsed >= TestTimeout) FinishTest();
        }

        private static void FinishTest()
        {
            _testDone = true;
            EditorApplication.update -= WaitFramesThenRun;
            TestResult resultObj = new TestResult { success = _upgradeSelected, initialXP = _startXP, finalXP = XPManager.Instance?.CurrentXP ?? -1, logs = _capturedLogs.ToArray() };
            SessionState.SetString(ResultKey, JsonUtility.ToJson(resultObj));
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static bool Tick(float elapsed)
        {
            if (!_upgradeSelected && Time.timeScale == 0)
            {
                var upgradeManager = UpgradeManager.Instance;
                if (upgradeManager != null && upgradeManager.UpgradeUI != null && upgradeManager.UpgradeUI.UIContainer.activeSelf)
                {
                    var upgrades = upgradeManager.ModeSelectionUpgrades;
                    if (upgrades != null && upgrades.Count > 0)
                    {
                        upgradeManager.SelectUpgrade(upgrades[0]);
                        _upgradeSelected = true;
                    }
                }
            }

            if (_upgradeSelected && Time.timeScale > 0)
            {
                var healths = Object.FindObjectsByType<Health>(FindObjectsSortMode.None);
                foreach (var h in healths)
                {
                    if (h.gameObject.name.Contains("HoverBot") || h.gameObject.name.Contains("Turret"))
                    {
                        h.TakeDamage(1000, null);
                        return true;
                    }
                }
            }
            return false;
        }

        private static void SelfDestruct()
        {
            string p = SessionState.GetString(ScriptPathKey, "");
            if (!string.IsNullOrEmpty(p)) AssetDatabase.DeleteAsset(p);
            SessionState.EraseString(StateKey);
            SessionState.EraseString(ScriptPathKey);
        }
    }
}
