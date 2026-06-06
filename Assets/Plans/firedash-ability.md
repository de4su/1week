# Project Overview
- Game Title: FPS Roguelike (based on Unity FPS Template)
- High-Level Concept: An FPS with a Roguelike upgrade system featuring two paths: Tank (Durable/Slow) and Agile (Fast/Nimble).
- Players: Single player
- Inspiration: Roguelike shooters
- Tone / Art Direction: Sci-fi / FPS Template style
- Target Platform: PC (StandaloneWindows64)
- Screen Orientation: Landscape 1920x1080
- Render Pipeline: URP (Default_PipelineAsset)

# Game Mechanics
## Core Gameplay Loop
Players kill enemies, gain XP, and level up to choose upgrades. At specific levels (1, 3, 7, 10), they receive special path-specific abilities.

## New Ability: Agile Firedash (Level 10 Agile Special)
- **Activation**: Holding the Sprint key (Shift) while having energy.
- **Effect**: 
    - Dramatically increased movement speed.
    - Leaves a fire trail (using `FireTrail.prefab`).
    - One-hit kill on contact: Any enemy touched while dashing takes 250 damage (similar to Tank Kick).
- **Resource**: Draining energy bar. Recharges automatically when not dashing.

# UI
- **Firedash Status Text**: A centered text element (e.g., "FIRE DASH: 100%") showing remaining energy, positioned to avoid overlapping other HUD elements.

# Key Asset & Context
- `Assets/FPS/Scripts/Gameplay/Roguelike/AgileFiredash.cs`: Ability logic and burst dash implementation.
- `Assets/FPS/Scripts/Gameplay/Roguelike/FiredashPerk.cs`: Configurable settings object in the hierarchy.
- `Assets/FPS/Scripts/Gameplay/Roguelike/UpgradeManager.cs`: Updated to handle Level 10 unlock and icon assignment.
- `Assets/ModAssets/Prefabs/Trail Prefabs/FireTrail.prefab`: Dash visual effect.

# Implementation Steps
## Step 1: Core Ability (AgileFiredash.cs)
- **Description**: Implement burst dash triggered by Shift.
    - Uses a fixed cost per dash (energy-based).
    - One-hit kill damage logic (250 damage to parent health).
    - Status text UI positioned at `y = 180` to avoid overlap with Ghost Mode (at `y = 120`).
- **Assigned role**: developer

## Step 2: Configurable Perk (FiredashPerk.cs)
- **Description**: Create a perk script and add it to the `======= perks modifiers =======` header in the scene for easy tuning.
- **Assigned role**: developer

## Step 3: Upgrade System Integration
- **Description**: Update `UpgradeManager.cs` to offer Firedash at Level 10 for Agile path.
    - Assign the provided icon (InstanceID: 259138) to the `UpgradeData`.
    - Ensure the texture importer for the icon is set to **Sprite (2D and UI)** so it registers in the HUD.
- **Assigned role**: developer

## Step 4: XP Popup Refinement (XPManager.cs)
- **Description**: Modify the XP popup to be less intrusive.
    - Reduce font size (e.g., from 32 to 18).
    - Add a simple float-up and fade-out effect for better visual feedback.
- **Assigned role**: developer

# Verification & Testing
1. **Selection**: Run the game, reach level 1 (choose Agile), then reach level 10. Verify that "Firedash" appears as the special upgrade.
2. **Activation**: Hold Shift. Verify that:
    - The player moves significantly faster.
    - The energy bar appears and drains.
    - The Fire Trail effect is visible.
3. **Combat**: Run into an enemy while holding Shift. Verify the enemy is one-hit killed (250 damage).
4. **Regeneration**: Release Shift. Verify the energy bar recharges.
5. **Exhaustion**: Hold Shift until energy reaches 0. Verify the dash effect stops and speed returns to normal.
