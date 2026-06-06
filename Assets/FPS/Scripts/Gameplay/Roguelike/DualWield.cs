using UnityEngine;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using System.Reflection;

namespace Unity.FPS.Roguelike
{
    public class DualWield : MonoBehaviour
    {
        // Horizontal offset of the off-hand weapon from WeaponParentSocket
        const float k_OffHandXOffset = -0.50f;

        PlayerWeaponsManager m_WeaponsManager;
        WeaponController m_MainWeapon;
        GameObject m_OffHandSocket;
        WeaponController m_OffHandWeapon;
        Transform m_OffHandMuzzle;
        Animator m_OffHandAnimator;

        // Reflection fields for syncing state
        FieldInfo m_CurrentAmmoField;
        FieldInfo m_WantsToShootField;
        PropertyInfo m_CurrentChargeProp;
        PropertyInfo m_IsChargingProp;

        void Start()
        {
            m_WeaponsManager = GetComponent<PlayerWeaponsManager>();
            if (m_WeaponsManager == null)
            {
                Debug.LogError("[DualWield] PlayerWeaponsManager not found on player!");
                return;
            }

            // Prepare reflection
            m_CurrentAmmoField = typeof(WeaponController).GetField("m_CurrentAmmo", BindingFlags.NonPublic | BindingFlags.Instance);
            m_WantsToShootField = typeof(WeaponController).GetField("m_WantsToShoot", BindingFlags.NonPublic | BindingFlags.Instance);
            m_CurrentChargeProp = typeof(WeaponController).GetProperty("CurrentCharge");
            m_IsChargingProp = typeof(WeaponController).GetProperty("IsCharging");

            // Disable ADS for the duration this perk is active
            m_WeaponsManager.ForceNoAim = true;

            // Initialize with current weapon
            OnWeaponSwitched(m_WeaponsManager.GetActiveWeapon());

            // Subscribe to weapon switches
            m_WeaponsManager.OnSwitchedToWeapon += OnWeaponSwitched;

            Debug.Log("[Roguelike] Tank Special: Dual Wield activated.");
        }

        void OnWeaponSwitched(WeaponController newWeapon)
        {
            if (newWeapon == null) return;

            // Clean up old off-hand
            if (m_MainWeapon != null)
            {
                m_MainWeapon.OnShoot -= FireOffHand;
            }

            if (m_OffHandSocket != null)
            {
                Destroy(m_OffHandSocket);
            }

            m_MainWeapon = newWeapon;
            SetupOffHand();

            // Hook into the main weapon's fire event to fire the off-hand shot
            m_MainWeapon.OnShoot += FireOffHand;
        }

        void SetupOffHand()
        {
            if (m_MainWeapon == null || m_MainWeapon.SourcePrefab == null) return;

            // Parent to WeaponParentSocket so the off-hand inherits all bob, recoil, etc.
            m_OffHandSocket = new GameObject("OffHandSocket");
            m_OffHandSocket.transform.SetParent(m_WeaponsManager.WeaponParentSocket, false);
            m_OffHandSocket.transform.localPosition = new Vector3(k_OffHandXOffset, 0f, 0f);
            m_OffHandSocket.transform.localRotation = Quaternion.identity;
            
            // Mirror the weapon from right to left
            m_OffHandSocket.transform.localScale = new Vector3(-1, 1, 1);

            // Instantiate a visual copy of the current weapon
            GameObject offHandGo = Instantiate(m_MainWeapon.SourcePrefab, m_OffHandSocket.transform);
            offHandGo.transform.localPosition = Vector3.zero;
            offHandGo.transform.localRotation = Quaternion.identity;

            m_OffHandWeapon = offHandGo.GetComponent<WeaponController>();
            if (m_OffHandWeapon != null)
            {
                m_OffHandWeapon.Owner = gameObject;
                m_OffHandWeapon.SourcePrefab = m_MainWeapon.SourcePrefab;
                m_OffHandMuzzle = m_OffHandWeapon.WeaponMuzzle;
                m_OffHandAnimator = m_OffHandWeapon.WeaponAnimator;
                
                // Ensure the off-hand doesn't try to play "SwitchWeaponSfx" again and is immediately visible
                m_OffHandWeapon.ShowWeapon(true);
                
                // Disable AudioSource on off-hand to avoid double sounds (SFX handled by main weapon or manual triggers)
                AudioSource offHandAudio = m_OffHandWeapon.GetComponent<AudioSource>();
                if (offHandAudio) offHandAudio.enabled = false;
            }
            else
            {
                m_OffHandMuzzle = m_OffHandSocket.transform;
            }

            // Apply the FPS weapon layer
            if (m_WeaponsManager.FpsWeaponLayer.value > 0)
            {
                int layerIndex = Mathf.RoundToInt(Mathf.Log(m_WeaponsManager.FpsWeaponLayer.value, 2));
                foreach (Transform t in offHandGo.GetComponentsInChildren<Transform>(true))
                {
                    t.gameObject.layer = layerIndex;
                }
            }
        }

        void Update()
        {
            if (m_MainWeapon == null || m_OffHandWeapon == null) return;

            // Sync state for visual effects (overheating, charging, continuous beams)
            try
            {
                if (m_CurrentAmmoField != null)
                {
                    float ammo = (float)m_CurrentAmmoField.GetValue(m_MainWeapon);
                    m_CurrentAmmoField.SetValue(m_OffHandWeapon, ammo);
                }

                if (m_WantsToShootField != null)
                {
                    bool wantsToShoot = (bool)m_WantsToShootField.GetValue(m_MainWeapon);
                    m_WantsToShootField.SetValue(m_OffHandWeapon, wantsToShoot);
                }

                if (m_CurrentChargeProp != null)
                {
                    float charge = (float)m_CurrentChargeProp.GetValue(m_MainWeapon);
                    m_CurrentChargeProp.SetValue(m_OffHandWeapon, charge);
                }

                if (m_IsChargingProp != null)
                {
                    bool isCharging = (bool)m_IsChargingProp.GetValue(m_MainWeapon);
                    m_IsChargingProp.SetValue(m_OffHandWeapon, isCharging);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[DualWield] Sync failed: " + e.Message);
            }
        }

        void FireOffHand()
        {
            if (m_MainWeapon == null || m_OffHandMuzzle == null) return;

            // Calculate bullets per shot (handles shotgun types and charging weapons)
            int bulletsPerShotFinal = m_MainWeapon.ShootType == WeaponShootType.Charge
                ? Mathf.CeilToInt(m_MainWeapon.CurrentCharge * m_MainWeapon.BulletsPerShot)
                : m_MainWeapon.BulletsPerShot;

            for (int i = 0; i < bulletsPerShotFinal; i++)
            {
                // Use the main weapon's spread calculation from the off-hand muzzle position
                Vector3 shotDirection = m_MainWeapon.GetShotDirectionWithinSpread(m_OffHandMuzzle);

                // Spawn projectile
                if (m_MainWeapon.ProjectilePrefab != null)
                {
                    ProjectileBase newProjectile = Instantiate(
                        m_MainWeapon.ProjectilePrefab,
                        m_OffHandMuzzle.position,
                        Quaternion.LookRotation(shotDirection));

                    newProjectile.Shoot(m_MainWeapon);
                }
            }

            // Drain one unit of heat/ammo from the shared pool
            m_MainWeapon.UseAmmo(1f);

            // Trigger animations
            if (m_OffHandAnimator != null)
                m_OffHandAnimator.SetTrigger("Attack");

            // Muzzle flash
            if (m_MainWeapon.MuzzleFlashPrefab != null)
            {
                GameObject flash = Instantiate(
                    m_MainWeapon.MuzzleFlashPrefab,
                    m_OffHandMuzzle.position,
                    m_OffHandMuzzle.rotation,
                    m_OffHandMuzzle);

                Destroy(flash, 2f);
            }
        }

        void OnDisable()
        {
            if (m_WeaponsManager != null)
            {
                m_WeaponsManager.OnSwitchedToWeapon -= OnWeaponSwitched;
                m_WeaponsManager.ForceNoAim = false;
            }

            if (m_MainWeapon != null)
                m_MainWeapon.OnShoot -= FireOffHand;

            if (m_OffHandSocket != null)
                Destroy(m_OffHandSocket);
        }

        void OnDestroy()
        {
            OnDisable();
        }
    }
}
