using UnityEngine;

namespace Unity.FPS.Roguelike
{
    public class FiredashPerk : MonoBehaviour
    {
        [Header("Energy Settings")]
        public float MaxEnergy = 100f;
        public float DashCost = 30f;
        public float RegenRate = 20f;

        [Header("Dash Settings")]
        public float DashSpeed = 40f;
        public float DashDuration = 0.3f;
        public float DamageRadius = 2.0f;
        public float DamageAmount = 250f;
    }
}
