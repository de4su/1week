using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Roguelike
{
    public class AgileFiredash : MonoBehaviour
    {
        [Header("Ability Settings (Overridden by Perk if found)")]
        public float MaxEnergy = 100f;
        public float Energy = 100f;
        public float DashCost = 30f;
        public float RegenRate = 20f;
        public float DashSpeed = 40f;
        public float DashDuration = 0.3f;
        public float DamageRadius = 2.0f;
        public float DamageAmount = 250f;
        public GameObject FireTrailPrefab;

        private PlayerCharacterController m_PlayerController;
        private PlayerInputHandler m_InputHandler;
        private GameObject m_CurrentTrail;
        private bool m_IsDashing;
        private float m_DashEndTime;
        private Vector3 m_DashDirection;
        private System.Collections.Generic.HashSet<Health> m_HitTargets = new System.Collections.Generic.HashSet<Health>();

        private GameObject m_EnergyTextGo;
        private TextMeshProUGUI m_EnergyText;

        void Start()
        {
            m_PlayerController = GetComponent<PlayerCharacterController>();
            m_InputHandler = GetComponent<PlayerInputHandler>();

            if (FireTrailPrefab == null)
            {
#if UNITY_EDITOR
                FireTrailPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ModAssets/Prefabs/Trail Prefabs/FireTrail.prefab");
#endif
            }

            SyncWithPerk();
            SetupUI();
        }

        void SyncWithPerk()
        {
            FiredashPerk perk = null;
            var header = GameObject.Find("=======  perks modifiers =======");
            if (header != null)
            {
                perk = header.GetComponentInChildren<FiredashPerk>();
            }

            if (perk != null)
            {
                MaxEnergy = perk.MaxEnergy;
                DashCost = perk.DashCost;
                RegenRate = perk.RegenRate;
                DashSpeed = perk.DashSpeed;
                DashDuration = perk.DashDuration;
                DamageRadius = perk.DamageRadius;
                DamageAmount = perk.DamageAmount;
                Debug.Log("[Roguelike] AgileFiredash: Settings synced from Perk.");
            }
        }

        void SetupUI()
        {
            if (m_EnergyTextGo != null) return;
            GameObject canvas = GameObject.Find("RoguelikeCanvas");
            if (canvas == null)
            {
                UpgradeUI upgradeUI = FindAnyObjectByType<UpgradeUI>();
                if (upgradeUI != null) canvas = upgradeUI.gameObject;
            }

            if (canvas == null)
            {
                Invoke(nameof(SetupUI), 0.5f);
                return;
            }

            m_EnergyTextGo = new GameObject("FiredashText");
            m_EnergyTextGo.transform.SetParent(canvas.transform, false);
            RectTransform rt = m_EnergyTextGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0); 
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 240); 
            rt.sizeDelta = new Vector2(250, 25);

            m_EnergyText = m_EnergyTextGo.AddComponent<TextMeshProUGUI>();
            m_EnergyText.fontSize = 18; 
            m_EnergyText.alignment = TextAlignmentOptions.Center;
            m_EnergyText.fontStyle = FontStyles.Bold;
            m_EnergyText.color = new Color(1f, 0.3f, 0f, 1f); // Fire color
            
            m_EnergyTextGo.SetActive(false);
        }

        void Update()
        {
            if (!m_IsDashing)
            {
                Energy = Mathf.Min(MaxEnergy, Energy + RegenRate * Time.deltaTime);
            }

            bool dashRequested = m_InputHandler && m_InputHandler.GetSprintInputDown();

            if (dashRequested && !m_IsDashing && Energy >= DashCost)
            {
                SyncWithPerk(); // Refresh settings on each dash
                StartDash();
            }

            if (m_IsDashing)
            {
                if (Time.time >= m_DashEndTime)
                {
                    StopDash();
                }
                else
                {
                    ExecuteDash();
                }
            }

            UpdateUI();
        }

        void StartDash()
        {
            m_IsDashing = true;
            Energy -= DashCost;
            m_DashEndTime = Time.time + DashDuration;
            m_HitTargets.Clear();
            
            m_DashDirection = transform.forward;
            Vector3 moveInput = m_InputHandler ? m_InputHandler.GetMoveInput() : Vector3.zero;
            if (moveInput.magnitude > 0.1f)
            {
                m_DashDirection = transform.TransformDirection(moveInput).normalized;
            }

            if (FireTrailPrefab && m_CurrentTrail == null)
            {
                m_CurrentTrail = Instantiate(FireTrailPrefab, transform);
                m_CurrentTrail.transform.localPosition = Vector3.zero;
            }
        }

        void ExecuteDash()
        {
            if (m_PlayerController)
            {
                m_PlayerController.CharacterVelocity = m_DashDirection * DashSpeed;
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, DamageRadius);
            foreach (var col in colliders)
            {
                if (col.transform.root == transform.root) continue;

                Health health = col.GetComponentInParent<Health>();
                if (health && !m_HitTargets.Contains(health))
                {
                    m_HitTargets.Add(health);
                    health.TakeDamage(DamageAmount, gameObject);
                }
            }
        }

        void StopDash()
        {
            m_IsDashing = false;
            if (m_CurrentTrail != null)
            {
                Destroy(m_CurrentTrail);
                m_CurrentTrail = null;
            }
        }

        void UpdateUI()
        {
            if (m_EnergyTextGo)
            {
                m_EnergyTextGo.SetActive(m_IsDashing || Energy < MaxEnergy);
                if (m_EnergyText)
                {
                    int percent = Mathf.CeilToInt(Energy / MaxEnergy * 100);
                    m_EnergyText.text = $"FIRE DASH: {percent}%";
                }
            }
        }

        private void OnDisable()
        {
            if (m_IsDashing) StopDash();
        }
    }
}
