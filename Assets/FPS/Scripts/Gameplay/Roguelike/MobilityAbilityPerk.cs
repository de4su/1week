using UnityEngine;

namespace Unity.FPS.Roguelike
{
    public class MobilityAbilityPerk : MonoBehaviour
    {
        public float MaxDistance = 100f;
        public float BaseSpeed = 15f;
        public float MaxSpeed = 45f;
        public float Acceleration = 30f;
        public float MomentumMultiplier = 1.4f;
        public bool SwingingMode = false;
    }
}
