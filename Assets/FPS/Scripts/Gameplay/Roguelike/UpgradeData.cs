using UnityEngine;

namespace Unity.FPS.Roguelike
{
    public enum UpgradeType
    {
        FireRate,
        PlayerSpeed,
        JumpHeight,
        MaxHealth,
        Damage,
        AmmoClip,
        SpecialAbility
    }

    [CreateAssetMenu(menuName = "Roguelike/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        public string Title;
        public string Description;
        public UpgradeType Type;
        public float Value;
        public Sprite Icon;
    }
}
