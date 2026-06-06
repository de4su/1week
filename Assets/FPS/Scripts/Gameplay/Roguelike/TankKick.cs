using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace Unity.FPS.Roguelike
{
    public class TankKick : MonoBehaviour
    {
        public float KickDamage = 250f; // Massive damage
        public float KickRange = 3.5f;
        public float KickForce = 30f;
        public float Cooldown = 0.8f;

        private float m_LastKickTime;
        private Transform m_CameraTransform;
        private GameObject m_FootVisual;

        void Start()
        {
            Debug.Log("[Roguelike] TankKick component started on " + gameObject.name);
            m_CameraTransform = GetComponentInChildren<Camera>().transform;
            UpdateSettingsFromPerks();
            BuildFootVisual();
        }

        void UpdateSettingsFromPerks()
        {
            CombatAbilityPerk perk = null;
            var header = GameObject.Find("=======  perks modifiers =======");
            if (header != null)
            {
                foreach (Transform child in header.transform)
                {
                    if (child.name.Contains("Kick"))
                    {
                        perk = child.GetComponent<CombatAbilityPerk>();
                        break;
                    }
                }
            }

            if (perk != null)
            {
                KickDamage = perk.Damage;
                Cooldown = perk.Cooldown;
                KickForce = perk.Force;
                KickRange = perk.Range;
            }
        }

        void Update()
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                UpdateSettingsFromPerks();
                Debug.Log("[Roguelike] Q pressed for TankKick. Cooldown left: " + Mathf.Max(0, (m_LastKickTime + Cooldown) - Time.time));
                if (Time.time > m_LastKickTime + Cooldown)
                {
                    DoKick();
                }
            }
        }

        void BuildFootVisual()
        {
            m_FootVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            m_FootVisual.transform.SetParent(m_CameraTransform);
            // Position on the LEFT side now
            m_FootVisual.transform.localPosition = new Vector3(-0.6f, -0.6f, 0.4f);
            m_FootVisual.transform.localRotation = Quaternion.Euler(-15, 10, 0);
            m_FootVisual.transform.localScale = new Vector3(0.3f, 0.3f, 0.8f);
            m_FootVisual.GetComponent<Collider>().enabled = false;
            
            // Set material to something visible
            var renderer = m_FootVisual.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = new Color(0.4f, 0.2f, 0.1f); // Boot color

            m_FootVisual.SetActive(false);
        }

        void DoKick()
        {
            m_LastKickTime = Time.time;
            StartCoroutine(KickAnimation());

            // Sphere cast to hit multiple/easier
            Collider[] hits = Physics.OverlapSphere(m_CameraTransform.position + m_CameraTransform.forward * 1.5f, 1.5f);
            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;

                var health = hit.GetComponentInParent<Unity.FPS.Game.Health>();
                if (health)
                {
                    health.TakeDamage(KickDamage * PlayerStats.Instance.DamageMult, gameObject);
                }

                var rb = hit.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.AddForce(m_CameraTransform.forward * KickForce, ForceMode.Impulse);
                }
            }
        }

        IEnumerator KickAnimation()
        {
            m_FootVisual.SetActive(true);
            float elapsed = 0;
            float duration = 0.15f;
            Vector3 startPos = new Vector3(-0.6f, -0.6f, 0.4f);
            Vector3 endPos = new Vector3(-0.2f, -0.3f, 2.0f); // Kicks toward center-forward

            while (elapsed < duration)
            {
                m_FootVisual.transform.localPosition = Vector3.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            elapsed = 0;
            while (elapsed < duration)
            {
                m_FootVisual.transform.localPosition = Vector3.Lerp(endPos, startPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            m_FootVisual.SetActive(false);
        }
    }
}
