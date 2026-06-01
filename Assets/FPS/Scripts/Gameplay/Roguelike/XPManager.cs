using UnityEngine;
using UnityEngine.Events;

namespace Unity.FPS.Roguelike
{
    public class XPManager : MonoBehaviour
    {
        public static XPManager Instance { get; private set; }

        public int CurrentXP = 0;
        public int XPToNextLevel = 100;
        public int Level = 0;

        public UnityAction<int> OnXPChanged;
        public UnityAction<int> OnLevelUp;

        void Awake()
        {
            Instance = this;
            // Safety: ensure time is running
            if (Time.timeScale == 0) Time.timeScale = 1f;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void AddXP(int amount)
        {
            CurrentXP += amount;
            if (CurrentXP >= XPToNextLevel)
            {
                LevelUp();
            }
            OnXPChanged?.Invoke(CurrentXP);
            
            // Spawn XP Pop-up
            ShowXPPopup(amount);
        }

        void ShowXPPopup(int amount)
        {
            GameObject canvasGo = GameObject.Find("RoguelikeCanvas");
            if (canvasGo)
            {
                GameObject popup = new GameObject("XPPopup");
                popup.transform.SetParent(canvasGo.transform, false);
                var text = popup.AddComponent<TMPro.TextMeshProUGUI>();
                text.text = "+" + amount + " XP";
                text.fontSize = 32;
                text.color = new Color(0, 0.8f, 1, 1);
                text.alignment = TMPro.TextAlignmentOptions.Center;
                
                RectTransform rt = popup.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(Random.Range(-100, 100), Random.Range(-50, 50));
                
                Object.Destroy(popup, 1f);
                // Simple float up effect script or just let it sit
            }
        }

        void LevelUp()
        {
            CurrentXP -= XPToNextLevel;
            Level++;
            XPToNextLevel = Mathf.RoundToInt(XPToNextLevel * 1.2f);
            OnLevelUp?.Invoke(Level);
        }
    }
}
