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
                text.fontSize = 12; // Smaller font
                text.color = new Color(0, 0.8f, 1, 0.7f); // More transparent
                text.alignment = TMPro.TextAlignmentOptions.Center;
                
                // Load Orbitron font for sci-fi feel
#if UNITY_EDITOR
                var font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/ModAssets/Fonts/Orbitron-Regular SDF.asset");
                if (font != null) text.font = font;
#endif

                RectTransform rt = popup.GetComponent<RectTransform>();
                // Position it above the center to stay clear of main gameplay focus
                rt.anchoredPosition = new Vector2(Random.Range(-50, 50), 120 + Random.Range(-20, 20));
                
                StartCoroutine(AnimateXPPopup(popup, text, rt));
            }
        }

        System.Collections.IEnumerator AnimateXPPopup(GameObject popup, TMPro.TextMeshProUGUI text, RectTransform rt)
        {
            float duration = 1f;
            float elapsed = 0f;
            Vector2 startPos = rt.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0, 50f);
            Color startColor = text.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                if (rt != null) rt.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                if (text != null) text.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            if (popup != null) Destroy(popup);
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
