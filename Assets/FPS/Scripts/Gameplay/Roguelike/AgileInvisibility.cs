using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using TMPro;
using Unity.FPS.Game;

namespace Unity.FPS.Roguelike
{
    public class AgileInvisibility : MonoBehaviour
    {
        public bool IsInvisible { get; private set; }
        public float Energy = 100f;
        public float MaxEnergy = 100f;
        public float DrainRate = 20f;
        public float RegenRate = 12f;

        private List<Renderer> m_Renderers = new List<Renderer>();
        private List<Material[]> m_OriginalMaterials = new List<Material[]>();
        private Material m_InvisMaterial;
        
        private GameObject m_EnergyBarGo;
        private UnityEngine.UI.Image m_EnergyFill;
        private TextMeshProUGUI m_EnergyText;

        void Start()
        {
            Debug.Log("[Roguelike] AgileInvisibility: Starting on " + gameObject.name);
            SetupInvisMaterial();
            SetupEnergyUI();
            RefreshRenderers();
        }

        void SetupInvisMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            
            m_InvisMaterial = new Material(shader);
            if (shader.name.Contains("Universal Render Pipeline"))
            {
                m_InvisMaterial.SetFloat("_Surface", 1);
                m_InvisMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m_InvisMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                m_InvisMaterial.SetInt("_ZWrite", 0);
                m_InvisMaterial.renderQueue = 3000;
            }
            m_InvisMaterial.color = new Color(0.2f, 0.8f, 1f, 0.25f);
        }

        void SetupEnergyUI()
        {
            if (m_EnergyBarGo != null) return;
            GameObject canvas = GameObject.Find("RoguelikeCanvas");
            if (canvas == null) canvas = FindAnyObjectByType<UpgradeUI>()?.gameObject;
            if (canvas == null) { Invoke(nameof(SetupEnergyUI), 0.5f); return; }

            m_EnergyBarGo = new GameObject("InvisEnergyBar");
            m_EnergyBarGo.transform.SetParent(canvas.transform, false);
            RectTransform rt = m_EnergyBarGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0); rt.anchorMax = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 120); rt.sizeDelta = new Vector2(250, 20);

            GameObject bg = new GameObject("BG");
            bg.transform.SetParent(m_EnergyBarGo.transform, false);
            bg.AddComponent<UnityEngine.UI.Image>().color = new Color(0, 0, 0, 0.7f);
            bg.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(m_EnergyBarGo.transform, false);
            m_EnergyFill = fill.AddComponent<UnityEngine.UI.Image>();
            m_EnergyFill.color = new Color(0, 0.8f, 1f, 1f);
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
            Debug.Log("[Roguelike] Invisibility UI Initialized.");
        }

        void Update()
        {
            // Toggle input
            if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
            {
                if (!IsInvisible && Energy > 10f) SetInvisibility(true);
                else if (IsInvisible) SetInvisibility(false);
            }

            // Energy Logic
            if (IsInvisible)
            {
                Energy -= DrainRate * Time.deltaTime;
                // Debug.Log("[Roguelike] Invisible. Energy: " + Energy + " Delta: " + Time.deltaTime);
                if (Energy <= 0)
{
                    Energy = 0;
                    SetInvisibility(false);
                }
            }
            else
            {
                Energy = Mathf.Min(MaxEnergy, Energy + RegenRate * Time.deltaTime);
            }

            // UI Update
            if (m_EnergyFill) m_EnergyFill.fillAmount = Energy / MaxEnergy;
            if (m_EnergyBarGo) m_EnergyBarGo.SetActive(IsInvisible || Energy < MaxEnergy);
            if (m_EnergyText) m_EnergyText.text = $"GHOST MODE: {Mathf.CeilToInt(Energy / MaxEnergy * 100)}%";
        }

        public void RefreshRenderers()
        {
            m_Renderers.Clear();
            m_OriginalMaterials.Clear();
            m_Renderers.AddRange(GetComponentsInChildren<Renderer>(true));
            foreach (var r in m_Renderers) m_OriginalMaterials.Add(r.sharedMaterials);
        }

        public void SetInvisibility(bool state)
        {
            if (IsInvisible == state) return;
            
            // Capture original materials only when turning invisible
            if (state) RefreshRenderers();

            IsInvisible = state;
            
            Debug.Log("[Roguelike] Ghost Mode " + (IsInvisible ? "ENABLED" : "DISABLED") + ". Energy: " + Energy);
            
            for (int i = 0; i < m_Renderers.Count; i++)
            {
                if (m_Renderers[i] == null) continue;
                if (IsInvisible)
                {
                    Material[] mats = new Material[m_Renderers[i].sharedMaterials.Length];
                    for (int j = 0; j < mats.Length; j++) mats[j] = m_InvisMaterial;
                    m_Renderers[i].materials = mats;
                }
                else 
                {
                    if (i < m_OriginalMaterials.Count)
                        m_Renderers[i].materials = m_OriginalMaterials[i];
                }
            }
        }
    }
}
