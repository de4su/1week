using UnityEngine;
using TMPro;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Roguelike
{
    public class GrenadeAbility : MonoBehaviour
    {
        public GameObject GrenadePrefab;
        public float Cooldown = 5f;
        public float ThrowForce = 15f;
        public float GrenadeDamage = 50f;
        
        private float m_NextGrenadeTime;
        private PlayerInputHandler m_InputHandler;
        private Camera m_PlayerCamera;

        private GameObject m_EnergyBarGo;
        private UnityEngine.UI.Image m_EnergyFill;
        private TextMeshProUGUI m_EnergyText;

        void Start()
        {
            m_InputHandler = GetComponent<PlayerInputHandler>();
            m_PlayerCamera = GetComponentInChildren<Camera>();
            
            UpdateSettingsFromPerks();
            SetupEnergyUI();
        }

        void UpdateSettingsFromPerks()
        {
            CombatAbilityPerk perk = null;
            var header = GameObject.Find("=======  perks modifiers =======");
            if (header != null)
            {
                // Find specifically the Grenade Ability perk
                foreach (Transform child in header.transform)
                {
                    if (child.name.Contains("Grenade"))
                    {
                        perk = child.GetComponent<CombatAbilityPerk>();
                        break;
                    }
                }
            }

            if (perk != null)
            {
                Cooldown = perk.Cooldown;
                ThrowForce = perk.Force;
                GrenadeDamage = perk.Damage;
            }
        }

        void SetupEnergyUI()
        {
            if (m_EnergyBarGo != null) return;
            GameObject canvas = GameObject.Find("RoguelikeCanvas");
            if (canvas == null) canvas = FindAnyObjectByType<UpgradeUI>()?.gameObject;
            if (canvas == null) { Invoke(nameof(SetupEnergyUI), 0.5f); return; }

            m_EnergyBarGo = new GameObject("GrenadeEnergyBar");
            m_EnergyBarGo.transform.SetParent(canvas.transform, false);
            RectTransform rt = m_EnergyBarGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0); rt.anchorMax = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 180); rt.sizeDelta = new Vector2(250, 20);

            GameObject bg = new GameObject("BG");
            bg.transform.SetParent(m_EnergyBarGo.transform, false);
            bg.AddComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.7f);
            bg.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(m_EnergyBarGo.transform, false);
            m_EnergyFill = fill.AddComponent<UnityEngine.UI.Image>();
            m_EnergyFill.color = new Color(1f, 0.2f, 0.2f, 1f);
            m_EnergyFill.type = UnityEngine.UI.Image.Type.Filled;
            m_EnergyFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
            fill.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;

            GameObject textGo = new GameObject("Text");
            textGo.transform.SetParent(m_EnergyBarGo.transform, false);
            m_EnergyText = textGo.AddComponent<TextMeshProUGUI>();
            m_EnergyText.fontSize = 14; m_EnergyText.alignment = TextAlignmentOptions.Center;
            m_EnergyText.fontStyle = FontStyles.Bold;
            m_EnergyText.color = Color.white;
            textGo.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 25);
            textGo.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 25);
        }

        void Update()
        {
            if (m_InputHandler.GetGrenadeInputDown() && Time.time >= m_NextGrenadeTime)
            {
                UpdateSettingsFromPerks(); // Refresh settings before throwing
                ThrowGrenade();
                m_NextGrenadeTime = Time.time + Cooldown;
            }

            // UI Update
            float remaining = Mathf.Max(0, m_NextGrenadeTime - Time.time);
            float ratio = 1f - (remaining / Cooldown);
            
            if (m_EnergyFill) m_EnergyFill.fillAmount = ratio;
            if (m_EnergyText) 
            {
                if (ratio >= 1f) m_EnergyText.text = "GRENADE READY";
                else m_EnergyText.text = $"RECHARGING: {Mathf.CeilToInt(ratio * 100)}%";
            }
        }

        void ThrowGrenade()
        {
            if (GrenadePrefab == null)
            {
                Debug.LogWarning("GrenadePrefab is null in GrenadeAbility!");
                return;
            }

            GameObject grenade = Instantiate(GrenadePrefab, m_PlayerCamera.transform.position + m_PlayerCamera.transform.forward, m_PlayerCamera.transform.rotation);
            PlayerGrenade pg = grenade.GetComponent<PlayerGrenade>();
            if (pg)
            {
                pg.Initialize(gameObject);
                pg.Damage = GrenadeDamage; // Apply the damage from the perk
            }

            Rigidbody rb = grenade.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddForce(m_PlayerCamera.transform.forward * ThrowForce, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
            }
        }
    }
}
