using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Roguelike
{
    public class TankShield : MonoBehaviour
    {
        public bool IsShieldActive { get; private set; }
        public float Energy = 100f;
        public float MaxEnergy = 100f;
        public float DrainRate = 25f;
        public float RegenRate = 15f;

        private GameObject m_ShieldVisual;
        private GameObject m_EnergyBarGo;
        private UnityEngine.UI.Image m_EnergyFill;
        private TextMeshProUGUI m_EnergyText;
        
        private Health m_PlayerHealth;
        private PlayerWeaponsManager m_WeaponsManager;
        private PlayerInputHandler m_InputHandler;

        private Transform m_CameraTransform;

        void Start()
        {
            m_PlayerHealth = GetComponent<Health>();
            m_WeaponsManager = GetComponent<PlayerWeaponsManager>();
            m_InputHandler = GetComponent<PlayerInputHandler>();
            m_CameraTransform = GetComponentInChildren<Camera>().transform;
            
            UpdateSettingsFromPerks();
            SetupShieldVisual();
            SetupEnergyUI();
        }

        void SetupShieldVisual()
        {
            m_ShieldVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            m_ShieldVisual.transform.SetParent(m_CameraTransform, false);
            m_ShieldVisual.transform.localPosition = new Vector3(0, 0, 1.2f);
            m_ShieldVisual.transform.localScale = new Vector3(2.5f, 1.8f, 0.1f);
            
            Destroy(m_ShieldVisual.GetComponent<Collider>());
            
            var renderer = m_ShieldVisual.GetComponent<Renderer>();
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            
            Material mat = new Material(shader);
            if (shader.name.Contains("Universal Render Pipeline"))
            {
                mat.SetFloat("_Surface", 1);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_ZWrite", 0);
                mat.renderQueue = 3100;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0f) * 2f);
            }
            mat.color = new Color(1f, 0.5f, 0f, 0.2f);
            renderer.material = mat;
            
            m_ShieldVisual.SetActive(false);
        }

        void SetupEnergyUI()
        {
            if (m_EnergyBarGo != null) return;
            GameObject canvas = GameObject.Find("RoguelikeCanvas");
            if (canvas == null) canvas = FindAnyObjectByType<UpgradeUI>()?.gameObject;
            if (canvas == null) { Invoke(nameof(SetupEnergyUI), 0.5f); return; }

            m_EnergyBarGo = new GameObject("ShieldEnergyBar");
            m_EnergyBarGo.transform.SetParent(canvas.transform, false);
            RectTransform rt = m_EnergyBarGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0); rt.anchorMax = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 150); rt.sizeDelta = new Vector2(250, 20);

            GameObject bg = new GameObject("BG");
            bg.transform.SetParent(m_EnergyBarGo.transform, false);
            bg.AddComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.7f);
            bg.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(m_EnergyBarGo.transform, false);
            m_EnergyFill = fill.AddComponent<UnityEngine.UI.Image>();
            m_EnergyFill.color = new Color(1f, 0.6f, 0f, 1f);
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

            m_EnergyBarGo.SetActive(false);
        }

        void UpdateSettingsFromPerks()
        {
            EnergyAbilityPerk perk = null;
            var header = GameObject.Find("=======  perks modifiers =======");
            if (header != null)
            {
                foreach (Transform child in header.transform)
                {
                    if (child.name.Contains("Shield"))
                    {
                        perk = child.GetComponent<EnergyAbilityPerk>();
                        break;
                    }
                }
            }

            if (perk != null)
            {
                MaxEnergy = perk.MaxEnergy;
                DrainRate = perk.DrainRate;
                RegenRate = perk.RegenRate;
                // If current energy is higher than new max, cap it
                Energy = Mathf.Min(Energy, MaxEnergy);
            }
        }

        void Update()
        {
            UpdateSettingsFromPerks();
            bool inputPressed = Keyboard.current != null && Keyboard.current.eKey.isPressed;
            bool wantsShield = inputPressed && Energy > 5f;

            if (IsShieldActive && (!inputPressed || Energy <= 0))
            {
                SetShield(false);
            }
            else if (!IsShieldActive && wantsShield)
            {
                SetShield(true);
            }

            if (IsShieldActive)
            {
                Energy -= DrainRate * Time.deltaTime;
                if (Energy <= 0)
                {
                    Energy = 0;
                    SetShield(false);
                }
            }
            else
            {
                Energy = Mathf.Min(MaxEnergy, Energy + RegenRate * Time.deltaTime);
            }

            // UI Update
            if (m_EnergyFill) m_EnergyFill.fillAmount = Energy / MaxEnergy;
            if (m_EnergyText) m_EnergyText.text = $"SHIELD: {Mathf.CeilToInt(Energy / MaxEnergy * 100)}%";
            if (m_EnergyBarGo) m_EnergyBarGo.SetActive(IsShieldActive || Energy < MaxEnergy);
        }

        void SetShield(bool active)
        {
            if (IsShieldActive == active) return;
            IsShieldActive = active;
            
            if (m_ShieldVisual) m_ShieldVisual.SetActive(active);
            if (m_PlayerHealth) m_PlayerHealth.Invincible = active;
            
            if (m_InputHandler)
            {
                m_InputHandler.FireInputBlocked = active;
            }
        }
    }
}
