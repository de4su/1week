using UnityEngine;
using UnityEngine.InputSystem;
using Unity.FPS.Gameplay;
using Unity.FPS.Game;

namespace Unity.FPS.Roguelike
{
    public class AgileGrapple : MonoBehaviour
    {
        [Header("Grapple Settings")]
        public float MaxGrappleDistance = 100f;
        public float BasePullSpeed = 15f;
        public float MaxPullSpeed = 45f;
        public float AccelerationRate = 30f;
        public LayerMask GrappleableLayers = -1;

        [Header("Swinging Settings")]
        public bool SwingingMode = false;
        public float SwingForce = 15f;
        public float RopeElasticity = 5f;

        [Header("Momentum Settings")]
        public float MomentumReleaseMultiplier = 1.4f;

        private Vector3 m_GrapplePoint;
        private bool m_IsGrappling = false;
        private float m_CurrentSpeed;
        private Vector3 m_LastMovementVelocity;
        private float m_CurrentRopeLength;

        private PlayerCharacterController m_PlayerController;
        private Camera m_PlayerCamera;
        private LineRenderer m_LineRenderer;
        private Transform m_HookOrigin;
        private float m_OriginalMaxSpeedInAir;
        private bool m_SpeedBoosted = false;

        void Start()
        {
            m_PlayerController = GetComponent<PlayerCharacterController>();
            m_PlayerCamera = m_PlayerController.PlayerCamera;
            
            // Sync with Perk if it exists in the hierarchy/manager
            UpdateSettingsFromPerks();

            // Create a visual origin for the rope
            GameObject originGo = new GameObject("GrappleOrigin");
            originGo.transform.SetParent(m_PlayerCamera.transform);
            originGo.transform.localPosition = new Vector3(0.5f, -0.4f, 0.5f);
            m_HookOrigin = originGo.transform;

            m_LineRenderer = gameObject.AddComponent<LineRenderer>();
            m_LineRenderer.startWidth = 0.04f;
            m_LineRenderer.endWidth = 0.04f;
            m_LineRenderer.positionCount = 2;
            m_LineRenderer.useWorldSpace = true;
            
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            m_LineRenderer.material = new Material(shader);
            
            if (shader.name.Contains("Universal Render Pipeline"))
            {
                m_LineRenderer.material.SetFloat("_Surface", 1);
                m_LineRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m_LineRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                m_LineRenderer.material.SetInt("_ZWrite", 0);
            }
            
            m_LineRenderer.material.color = new Color(0.7f, 0.9f, 1f, 0.8f);
            m_LineRenderer.enabled = false;
            
            m_OriginalMaxSpeedInAir = m_PlayerController.MaxSpeedInAir;
        }

        void UpdateSettingsFromPerks()
        {
            // Try to find the modifier in the scene
            MobilityAbilityPerk perk = null;
            var header = GameObject.Find("=======  perks modifiers =======");
            if (header != null)
            {
                perk = header.GetComponentInChildren<MobilityAbilityPerk>();
            }

            if (perk != null)
            {
                MaxGrappleDistance = perk.MaxDistance;
                BasePullSpeed = perk.BaseSpeed;
                MaxPullSpeed = perk.MaxSpeed;
                AccelerationRate = perk.Acceleration;
                MomentumReleaseMultiplier = perk.MomentumMultiplier;
                SwingingMode = perk.SwingingMode;
            }
        }

        void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    UpdateSettingsFromPerks(); // Refresh settings on each use
                    StartGrapple();
                }

                if (Keyboard.current.eKey.wasReleasedThisFrame)
                {
                    StopGrapple();
                }
            }

            if (m_IsGrappling)
            {
                if (SwingingMode)
                    ExecuteSwing();
                else
                    ExecutePull();
            }
            else if (m_SpeedBoosted)
            {
                m_PlayerController.MaxSpeedInAir = Mathf.Lerp(m_PlayerController.MaxSpeedInAir, m_OriginalMaxSpeedInAir, Time.deltaTime * 2f);
                if (Mathf.Abs(m_PlayerController.MaxSpeedInAir - m_OriginalMaxSpeedInAir) < 0.1f)
                {
                    m_PlayerController.MaxSpeedInAir = m_OriginalMaxSpeedInAir;
                    m_SpeedBoosted = false;
                }
            }
        }

        void LateUpdate()
        {
            if (m_IsGrappling && m_LineRenderer != null && m_HookOrigin != null)
            {
                m_LineRenderer.SetPosition(0, m_HookOrigin.position);
                m_LineRenderer.SetPosition(1, m_GrapplePoint);
            }
        }

        void StartGrapple()
        {
            RaycastHit hit;
            if (Physics.Raycast(m_PlayerCamera.transform.position, m_PlayerCamera.transform.forward, out hit, MaxGrappleDistance, GrappleableLayers, QueryTriggerInteraction.Ignore))
            {
                m_GrapplePoint = hit.point;
                m_IsGrappling = true;
                m_CurrentSpeed = BasePullSpeed;
                m_LineRenderer.enabled = true;
                
                m_CurrentRopeLength = Vector3.Distance(transform.position, m_GrapplePoint);
                
                m_PlayerController.MaxSpeedInAir = MaxPullSpeed * 2f;
                m_SpeedBoosted = true;
            }
        }

        void ExecutePull()
        {
            Vector3 direction = (m_GrapplePoint - transform.position).normalized;
            float distanceToPoint = Vector3.Distance(transform.position, m_GrapplePoint);

            m_CurrentSpeed += AccelerationRate * Time.deltaTime;
            m_CurrentSpeed = Mathf.Min(m_CurrentSpeed, MaxPullSpeed);

            m_LastMovementVelocity = direction * m_CurrentSpeed;
            m_PlayerController.CharacterVelocity = m_LastMovementVelocity;

            if (distanceToPoint < 1.5f)
            {
                StopGrapple();
            }
        }

        void ExecuteSwing()
        {
            Vector3 playerToPoint = m_GrapplePoint - transform.position;
            float currentDistance = playerToPoint.magnitude;
            
            // Calculate tether direction
            Vector3 tetherDirection = playerToPoint.normalized;
            
            // Get current velocity from player controller
            Vector3 velocity = m_PlayerController.CharacterVelocity;

            // If we are further than the rope length, apply tether constraint
            if (currentDistance > m_CurrentRopeLength)
            {
                // Velocity component towards/away from the hook
                float velocityTowardsPoint = Vector3.Dot(velocity, tetherDirection);
                
                // If moving away, cancel that part of the velocity and pull back
                if (velocityTowardsPoint < 0)
                {
                    // Project velocity onto the sphere surface (perpendicular to rope)
                    velocity -= tetherDirection * velocityTowardsPoint;
                }
                
                // Pull player back to the rope radius (elasticity)
                velocity += tetherDirection * (currentDistance - m_CurrentRopeLength) * RopeElasticity;
            }

            // Apply air control / "Pumping" the swing
            Vector3 inputDir = m_PlayerController.transform.TransformVector(new Vector3(Keyboard.current.aKey.isPressed ? -1 : (Keyboard.current.dKey.isPressed ? 1 : 0), 0, Keyboard.current.wKey.isPressed ? 1 : (Keyboard.current.sKey.isPressed ? -1 : 0)));
            velocity += inputDir * SwingForce * Time.deltaTime;

            // Update player velocity
            m_PlayerController.CharacterVelocity = velocity;
            m_LastMovementVelocity = velocity;
        }

        void StopGrapple()
        {
            if (!m_IsGrappling) return;
            m_IsGrappling = false;
            m_LineRenderer.enabled = false;

            if (m_LastMovementVelocity.magnitude > 5f)
            {
                m_PlayerController.CharacterVelocity = m_LastMovementVelocity * MomentumReleaseMultiplier;
            }
        }
    }
}
