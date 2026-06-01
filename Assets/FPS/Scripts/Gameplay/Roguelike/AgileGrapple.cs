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

        [Header("Momentum Settings")]
        public float MomentumReleaseMultiplier = 1.4f;

        private Vector3 m_GrapplePoint;
        private bool m_IsGrappling = false;
        private float m_CurrentSpeed;
        private Vector3 m_LastMovementVelocity;

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
            
            // Basic material
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            m_LineRenderer.material = new Material(shader);
            
            if (shader.name.Contains("Universal Render Pipeline"))
            {
                m_LineRenderer.material.SetFloat("_Surface", 1); // Transparent
                m_LineRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m_LineRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                m_LineRenderer.material.SetInt("_ZWrite", 0);
            }
            
            m_LineRenderer.material.color = new Color(0.7f, 0.9f, 1f, 0.8f);
            m_LineRenderer.enabled = false;
            
            m_OriginalMaxSpeedInAir = m_PlayerController.MaxSpeedInAir;
        }

        void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.eKey.wasPressedThisFrame)
                {
                    StartGrapple();
                }

                if (Keyboard.current.eKey.wasReleasedThisFrame)
                {
                    StopGrapple();
                }
            }

            if (m_IsGrappling)
            {
                ExecutePull();
            }
            else if (m_SpeedBoosted)
            {
                // Gradually return max speed to normal
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
                
                // Allow high speed in air while grappling
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

            // Direct velocity injection
            m_PlayerController.CharacterVelocity = m_LastMovementVelocity;

            if (distanceToPoint < 1.5f)
            {
                StopGrapple();
            }
        }

        void StopGrapple()
        {
            if (!m_IsGrappling) return;
            m_IsGrappling = false;
            m_LineRenderer.enabled = false;

            if (m_LastMovementVelocity.magnitude > 5f)
            {
                // Apply the momentum slingshot
                m_PlayerController.CharacterVelocity = m_LastMovementVelocity * MomentumReleaseMultiplier;
            }
        }
    }
}
