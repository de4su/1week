using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Unity.FPS.Roguelike
{
    public class XPBarUI : MonoBehaviour
    {
        public UnityEngine.UI.Image XPFillImage; 
        public UnityEngine.UI.Image XPGainFlashImage; 
        public TMPro.TextMeshProUGUI LevelText;
        public TMPro.TextMeshProUGUI ProgressText; // Percent or XP/MaxXP
        public float LerpSpeed = 5f;

        private float m_TargetFill;
        private Vector3 m_OriginalScale;
        private float m_PulseTimer;

        void Start()
        {
            m_OriginalScale = transform.localScale;
            Initialize();
        }

        void Initialize()
        {
            if (XPManager.Instance != null)
            {
                XPManager.Instance.OnXPChanged -= UpdateXPBar;
                XPManager.Instance.OnXPChanged += UpdateXPBar;
                XPManager.Instance.OnLevelUp -= UpdateLevelText;
                XPManager.Instance.OnLevelUp += UpdateLevelText;
                
                m_TargetFill = (float)XPManager.Instance.CurrentXP / XPManager.Instance.XPToNextLevel;
                if (XPFillImage != null) XPFillImage.fillAmount = m_TargetFill;
                UpdateLevelText(XPManager.Instance.Level);
                Debug.Log("[Roguelike] XPBarUI initialized and subscribed.");
            }
            else
            {
                Invoke(nameof(Initialize), 0.1f);
            }
        }

        void Update()
        {
            if (XPFillImage != null)
            {
                XPFillImage.fillAmount = Mathf.MoveTowards(XPFillImage.fillAmount, m_TargetFill, Time.deltaTime * LerpSpeed);
                
                // Update percentage text based on current fill (smooth) or target
                if (ProgressText != null)
                {
                    int pct = Mathf.RoundToInt(XPFillImage.fillAmount * 100f);
                    ProgressText.text = pct + "%";
                }
            }
            
            if (XPGainFlashImage != null)
            {
                XPGainFlashImage.fillAmount = Mathf.Lerp(XPGainFlashImage.fillAmount, XPFillImage.fillAmount, Time.deltaTime * 2f);
            }

            if (m_PulseTimer > 0)
            {
                m_PulseTimer -= Time.deltaTime;
                float scale = 1f + Mathf.Sin(m_PulseTimer * 15f) * 0.08f; 
                transform.localScale = m_OriginalScale * scale;
            }
            else
            {
                transform.localScale = m_OriginalScale;
            }
        }

        void UpdateXPBar(int currentXP)
        {
            if (XPManager.Instance != null)
            {
                m_TargetFill = (float)currentXP / XPManager.Instance.XPToNextLevel;
                m_PulseTimer = 0.4f; 
            }
        }

        void UpdateLevelText(int level)
        {
            if (LevelText != null)
            {
                LevelText.text = "LEVEL " + level;
            }
            if (XPFillImage != null) XPFillImage.fillAmount = 0;
            m_TargetFill = 0;
        }
    }
}
