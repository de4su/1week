using UnityEngine;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;

namespace Unity.FPS.Roguelike
{
public enum PlayerMode { None, Tank, Agile }

    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        public PlayerMode CurrentMode = PlayerMode.None;
        
        [Header("Upgrade Pools")]
        public List<UpgradeData> TankUpgrades;
        public List<UpgradeData> AgileUpgrades;
        public List<UpgradeData> ModeSelectionUpgrades; // level 1 options
        
        public List<UpgradeData> ActiveUpgrades = new List<UpgradeData>();
        public UnityEngine.Events.UnityAction OnUpgradesUpdated;

        public UpgradeData TankShieldSpecial; // level 3
        public UpgradeData AgileHookSpecial; // level 3
        public UpgradeData GrenadeSpecial; // level 10
        public UpgradeData AgileFiredashSpecial; // level 10 agile
        public UpgradeData TankSpecial; // level 7
        public UpgradeData AgileSpecial;
        public UpgradeData DualWieldSpecial; // level 5, Tank only

        public UpgradeUI UpgradeUI;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Time.timeScale = 1f;
        }

        void Start()
        {
            EnsureUIAndSubscribe();
        }

        void EnsureUIAndSubscribe()
        {
            if (UpgradeUI == null)
            {
                UpgradeUI = FindAnyObjectByType<UpgradeUI>();
                
                // If still null, try to spawn from prefab
                if (UpgradeUI == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("RoguelikeCanvas");
                    if (prefab == null) 
                    {
                        // Fallback: look in standard paths
#if UNITY_EDITOR
                        prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/FPS/Prefabs/UI/RoguelikeCanvas.prefab");
#endif
                    }

                    if (prefab != null)
                    {
                        GameObject go = Instantiate(prefab);
                        UpgradeUI = go.GetComponent<UpgradeUI>();
                        Debug.Log("[Roguelike] Spawned RoguelikeCanvas from prefab.");
                    }
                }
            }
            
            if (XPManager.Instance != null)
            {
                // Unsubscribe first to avoid double registration in any case
                XPManager.Instance.OnLevelUp -= HandleLevelUp;
                XPManager.Instance.OnLevelUp += HandleLevelUp;
                Debug.Log("[Roguelike] Subscribed to XPManager.LevelUp");
            }
            else
            {
                Debug.LogWarning("[Roguelike] XPManager.Instance is null in Start! Retrying subscription...");
                Invoke(nameof(EnsureUIAndSubscribe), 0.1f);
            }
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void HandleLevelUp(int level)
        {
            Time.timeScale = 0f;
            
            // Disable player input to prevent cursor fighting
            var inputHandler = GetComponent<PlayerInputHandler>();
            if (inputHandler) inputHandler.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (level == 1)
            {
                UpgradeUI.ShowOptions(ModeSelectionUpgrades);
            }
            else if (level == 3)
            {
                if (CurrentMode == PlayerMode.Tank && TankShieldSpecial != null)
                {
                    List<UpgradeData> special = new List<UpgradeData> { TankShieldSpecial };
                    UpgradeUI.ShowOptions(special);
                }
                else if (CurrentMode == PlayerMode.Agile && AgileHookSpecial != null)
                {
                    List<UpgradeData> special = new List<UpgradeData> { AgileHookSpecial };
                    UpgradeUI.ShowOptions(special);
                }
                else
                {
                    ShowGenericPool();
                }
            }
            else if (level == 5)
            {
                if (CurrentMode == PlayerMode.Tank && DualWieldSpecial != null)
                {
                    List<UpgradeData> special = new List<UpgradeData> { DualWieldSpecial };
                    UpgradeUI.ShowOptions(special);
                }
                else
                {
                    ShowGenericPool();
                }
            }
            else if (level == 7)
            {
                List<UpgradeData> special = new List<UpgradeData>();
                special.Add(CurrentMode == PlayerMode.Tank ? TankSpecial : AgileSpecial);
                UpgradeUI.ShowOptions(special);
            }
            else if (level == 10)
            {
                List<UpgradeData> special = new List<UpgradeData>();
                if (CurrentMode == PlayerMode.Tank && GrenadeSpecial != null)
                {
                    special.Add(GrenadeSpecial);
                }
                else if (CurrentMode == PlayerMode.Agile && AgileFiredashSpecial != null)
                {
                    special.Add(AgileFiredashSpecial);
                }

                if (special.Count > 0)
                {
                    UpgradeUI.ShowOptions(special);
                }
                else
                {
                    ShowGenericPool();
                }
            }
else
            {
                ShowGenericPool();
            }
        }

        void ShowGenericPool()
        {
            List<UpgradeData> pool = CurrentMode == PlayerMode.Tank ? TankUpgrades : AgileUpgrades;
            List<UpgradeData> options = GetRandomUpgrades(pool, 3);
            UpgradeUI.ShowOptions(options);
        }

        List<UpgradeData> GetRandomUpgrades(List<UpgradeData> pool, int count)
        {
            List<UpgradeData> result = new List<UpgradeData>();
            List<UpgradeData> copy = new List<UpgradeData>(pool);
            for (int i = 0; i < count && copy.Count > 0; i++)
            {
                int index = Random.Range(0, copy.Count);
                result.Add(copy[index]);
                copy.RemoveAt(index);
            }
            return result;
        }

        public void SelectUpgrade(UpgradeData upgrade)
        {
            Debug.Log("[Roguelike] Selected upgrade: " + upgrade.Title + " (Type: " + upgrade.Type + ")");
            
            ActiveUpgrades.Add(upgrade);
            OnUpgradesUpdated?.Invoke();

            // Re-enable player input
            var inputHandler = GetComponent<PlayerInputHandler>();
            if (inputHandler) inputHandler.enabled = true;

            if (upgrade.Type == UpgradeType.SpecialAbility)
            {
                // If we are at Level 1 or selecting a mode upgrade, set the mode
                if (CurrentMode == PlayerMode.None || upgrade.Title.Contains("Mode"))
                {
                    if (upgrade.Title.Contains("Tank"))
                    {
                        CurrentMode = PlayerMode.Tank;
                        Debug.Log("[Roguelike] Switched to TANK Mode: Slow & Durable");
                        
                        float speedVal = -0.3f;
                        float healthVal = 0.5f;

                        var header = GameObject.Find("=======  perks modifiers =======");
                        if (header != null)
                        {
                            foreach (Transform child in header.transform)
                            {
                                if (child.name.Contains("Tank"))
                                {
                                    var speedMod = child.GetComponent<SpeedModifier>();
                                    var healthMod = child.GetComponent<HealthModifier>();
                                    if (speedMod) speedVal = speedMod.SpeedMultiplier - 1f;
                                    if (healthMod) healthVal = healthMod.HealthMultiplier - 1f;
                                    break;
                                }
                            }
                        }

                        UpgradeData tankSpeed = ScriptableObject.CreateInstance<UpgradeData>();
                        tankSpeed.Type = UpgradeType.PlayerSpeed;
                        tankSpeed.Value = speedVal;
                        PlayerStats.Instance.ApplyUpgrade(tankSpeed);

                        UpgradeData tankHealth = ScriptableObject.CreateInstance<UpgradeData>();
                        tankHealth.Type = UpgradeType.MaxHealth;
                        tankHealth.Value = healthVal;
                        PlayerStats.Instance.ApplyUpgrade(tankHealth);
                    }
                    else if (upgrade.Title.Contains("Agile"))
                    {
                        CurrentMode = PlayerMode.Agile;
                        Debug.Log("[Roguelike] Switched to AGILE Mode: Fast & Nimble");
                        
                        float speedVal = 0.25f;
                        float jumpVal = 0.2f;

                        var header = GameObject.Find("=======  perks modifiers =======");
                        if (header != null)
                        {
                            foreach (Transform child in header.transform)
                            {
                                if (child.name.Contains("Agile"))
                                {
                                    var speedMod = child.GetComponent<SpeedModifier>();
                                    var jumpMod = child.GetComponent<JumpModifier>();
                                    if (speedMod) speedVal = speedMod.SpeedMultiplier - 1f;
                                    if (jumpMod) jumpVal = jumpMod.JumpMultiplier - 1f;
                                    break;
                                }
                            }
                        }

                        UpgradeData agileSpeed = ScriptableObject.CreateInstance<UpgradeData>();
                        agileSpeed.Type = UpgradeType.PlayerSpeed;
                        agileSpeed.Value = speedVal;
                        PlayerStats.Instance.ApplyUpgrade(agileSpeed);

                        UpgradeData agileJump = ScriptableObject.CreateInstance<UpgradeData>();
                        agileJump.Type = UpgradeType.JumpHeight;
                        agileJump.Value = jumpVal;
                        PlayerStats.Instance.ApplyUpgrade(agileJump);
                    }
                }
                else
                {
                    // Special Ability selected (Level 3, Level 7, etc.)
                    ApplySpecial(upgrade);
                }
            }
else
            {
                PlayerStats.Instance.ApplyUpgrade(upgrade);
            }

            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void ApplySpecial(UpgradeData upgrade)
        {
            if (PlayerStats.Instance == null)
            {
                Debug.LogError("[Roguelike] Cannot apply special: PlayerStats.Instance is null!");
                return;
            }

            GameObject player = PlayerStats.Instance.gameObject;
            Debug.Log("[Roguelike] Applying special: " + upgrade.Title);
            
            if (upgrade.Title.Contains("Shield"))
            {
                if (player.GetComponent<TankShield>() == null)
                {
                    player.AddComponent<TankShield>();
                    Debug.Log("[Roguelike] Tank Special: Energy Shield Unlocked!");
                }
            }
            else if (upgrade.Title.Contains("Kick"))
            {
                if (player.GetComponent<TankKick>() == null)
                {
                    player.AddComponent<TankKick>();
                    Debug.Log("[Roguelike] Tank Special: Power Kick Unlocked!");
                }
            }
            else if (upgrade.Title.Contains("Hook") || upgrade.Title.Contains("Grapple"))
            {
                if (player.GetComponent<AgileGrapple>() == null)
                {
                    player.AddComponent<AgileGrapple>();
                    Debug.Log("[Roguelike] Agile Special: Grappling Hook Unlocked!");
                }
            }
            else if (upgrade.Title.Contains("Ghost") || upgrade.Title.Contains("Invis"))
            {
                if (player.GetComponent<AgileInvisibility>() == null)
                {
                    player.AddComponent<AgileInvisibility>();
                    Debug.Log("[Roguelike] Agile Special: Ghost Mode Unlocked!");
                }
            }
            else if (upgrade.Title.Contains("Grenade"))
            {
                if (player.GetComponent<GrenadeAbility>() == null)
                {
                    GrenadeAbility ga = player.AddComponent<GrenadeAbility>();
                    // Try to load from Resources or AssetDatabase
                    ga.GrenadePrefab = Resources.Load<GameObject>("Roguelike_Grenade");
                    if (ga.GrenadePrefab == null)
                    {
#if UNITY_EDITOR
                        ga.GrenadePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/FPS/Prefabs/Roguelike_Grenade.prefab");
#endif
                    }
Debug.Log("[Roguelike] Special: Grenade Ability Unlocked!");
                }
            }
            else if (upgrade.Title.Contains("Dash") || upgrade.Title.Contains("Fire"))
            {
                if (player.GetComponent<AgileFiredash>() == null)
                {
                    player.AddComponent<AgileFiredash>();
                    Debug.Log("[Roguelike] Agile Special: Fire Dash Unlocked!");
                }
            }
            else if (upgrade.Title.Replace(" ", "").Contains("DualWield"))
            {
                if (player.GetComponent<DualWield>() == null)
                {
                    player.AddComponent<DualWield>();
                    Debug.Log("[Roguelike] Tank Special: Dual Wield Unlocked!");
                }
            }
        }

        void Update()
        {
            if (UpgradeUI != null && UpgradeUI.UIContainer.activeSelf)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}
