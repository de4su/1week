using UnityEngine;
using Unity.FPS.Gameplay;
using Unity.FPS.Game;

namespace Unity.FPS.Roguelike
{
    public class PlayerStats : MonoBehaviour
    {
        public static PlayerStats Instance { get; private set; }

        public float SpeedMult = 1f;
        public float JumpMult = 1f;
        public float FireRateMult = 1f;
        public float DamageMult = 1f;
        public float MaxHealthMult = 1f;
        public float AmmoMult = 1f;

        PlayerCharacterController m_Controller;
        PlayerWeaponsManager m_WeaponsManager;
        Health m_Health;

        void Awake()
        {
            Instance = this;
            m_Controller = GetComponent<PlayerCharacterController>();
            m_WeaponsManager = GetComponent<PlayerWeaponsManager>();
            m_Health = GetComponent<Health>();
        }

        public void ApplyUpgrade(UpgradeData upgrade)
        {
            switch (upgrade.Type)
            {
                case UpgradeType.PlayerSpeed:
                    SpeedMult += upgrade.Value;
                    ApplySpeed();
                    break;
                case UpgradeType.JumpHeight:
                    JumpMult += upgrade.Value;
                    ApplyJump();
                    break;
                case UpgradeType.FireRate:
                    FireRateMult += upgrade.Value;
                    ApplyFireRate(upgrade.Value);
                    break;
                case UpgradeType.Damage:
                    DamageMult += upgrade.Value;
                    break;
                case UpgradeType.MaxHealth:
                    float oldMax = m_Health.MaxHealth;
                    m_Health.MaxHealth *= (1 + upgrade.Value);
                    m_Health.Heal(m_Health.MaxHealth - oldMax);
                    break;
                case UpgradeType.AmmoClip:
                    AmmoMult += upgrade.Value;
                    ApplyAmmo(upgrade.Value);
                    break;
            }
        }

        void ApplyFireRate(float addValue)
        {
            foreach (var weapon in m_WeaponsManager.GetInventoryWeapons())
            {
                weapon.DelayBetweenShots /= (1 + addValue);
            }
        }

        void ApplyAmmo(float addValue)
        {
            foreach (var weapon in m_WeaponsManager.GetInventoryWeapons())
            {
                weapon.MaxAmmo = Mathf.RoundToInt(weapon.MaxAmmo * (1 + addValue));
            }
        }

        void ApplySpeed()
        {
            if (m_Controller)
            {
                // In FPS template, PlayerCharacterController has MaxSpeedOnGround
                // We might need to adjust based on exact variable names
                m_Controller.MaxSpeedOnGround *= SpeedMult;
                m_Controller.MaxSpeedInAir *= SpeedMult;
            }
        }

        void ApplyJump()
        {
            if (m_Controller)
            {
                m_Controller.JumpForce *= JumpMult;
            }
        }
    }
}
