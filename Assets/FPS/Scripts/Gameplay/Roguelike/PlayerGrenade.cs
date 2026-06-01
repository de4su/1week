using UnityEngine;
using Unity.FPS.Game;

namespace Unity.FPS.Roguelike
{
    public class PlayerGrenade : MonoBehaviour
    {
        public float FuseDelay = 3f;
        public float ExplosionRadius = 7f;
        public float ExplosionForce = 700f;
        public float Damage = 50f;
        public GameObject ExplosionEffect;

        private bool m_HasExploded = false;
        private GameObject m_Owner;

        public void Initialize(GameObject owner)
        {
            m_Owner = owner;
        }

        void Start()
        {
            Invoke(nameof(Explode), FuseDelay);
        }

        void Explode()
        {
            if (m_HasExploded) return;
            m_HasExploded = true;

            if (ExplosionEffect != null)
            {
                Instantiate(ExplosionEffect, transform.position, transform.rotation);
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, ExplosionRadius);
            foreach (Collider nearbyObject in colliders)
            {
                // Physics force
                Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(ExplosionForce, transform.position, ExplosionRadius);
                }

                // Damage
                Health health = nearbyObject.GetComponentInParent<Health>();
                if (health != null && health.gameObject != m_Owner)
                {
                    health.TakeDamage(Damage * PlayerStats.Instance.DamageMult, m_Owner);
                }
            }

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, ExplosionRadius);
        }
    }
}
