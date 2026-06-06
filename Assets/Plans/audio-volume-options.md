# Project Overview
- Game Title: RangeGrid (FPS Roguelike)
- High-Level Concept: FPS with roguelike elements, featuring many enemies and upgrade systems.
- Players: Single player
- Target Platform: PC (StandaloneWindows64)
- Render Pipeline: URP (Default_PipelineAsset)

# Game Mechanics
## Core Gameplay Loop
The player fights waves of enemies (hoverbots, turrets), gains experience, and chooses upgrades from a Roguelike UI.
## Controls and Input Methods
Standard FPS controls with a Pause Menu for settings.

# UI
- IntroMenu: Main menu with Start, Controls, and Options.
- InGameMenu: Pause menu with volume sliders, sensitivity, and gameplay toggles.
- Roguelike UI: For selecting upgrades.

# Key Asset & Context
- `Assets/myne.unity`: The primary working scene with enemies and gameplay systems.
- `Assets/FPS/Audio/MainAudioMixer.mixer`: Audio mixer with Master, Music (Ambient), and SFX groups.
- `Assets/FPS/Scripts/Game/AudioUtility.cs`: Utility for setting mixer parameters.
- `Assets/FPS/Scripts/UI/VolumeSlider.cs`: UI script for volume control.

# Implementation Steps
1. **Restore Scene & Music**: 
   - Ensure `Assets/myne.unity` is loaded as the active scene. (Assigned: developer | Dependency: None)
   - Set the `GameManager` AudioSource clip to `Steel Synapse.mp3` in `myne.unity`. (Assigned: developer | Dependency: Step 1)
2. **Verify Audio Setup**: 
   - Confirm that all SFX sources (Enemies, Weapons) are routed to the `SFX` group or its children. (Assigned: developer | Dependency: Step 1)
   - Confirm `Ambient` sources are routed to the `Ambient` group for Music control. (Assigned: developer | Dependency: Step 1)
3. **Verify UI Hookups**: 
   - Confirm `VolumeSlider` components in `IntroMenu` and `myne.unity` (Pause Menu) are correctly assigned to their respective VolumeTypes. (Assigned: developer | Dependency: Step 1)

# Verification & Testing
- Open `myne.unity` and verify all 39 hoverbots and the RoguelikeCanvas are present.
- Play the game and verify "Steel Synapse" plays as background music.
- Open the Options menu and verify that the Master, Music, and SFX sliders correctly affect the respective audio volumes.
- Verify that volume settings persist after restarting the game (via `PlayerPrefs`).
