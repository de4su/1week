# Project Overview
- Game Title: FPS Sample
- High-Level Concept: First-person shooter with menu systems and distinct scenes for gameplay, setup, and results.
- Players: Single player
- Inspiration / Reference Games: Standard FPS titles
- Tone / Art Direction: Realistic/Sci-fi (based on existing assets)
- Target Platform: PC (StandaloneWindows64)
- Screen Orientation / Resolution: Landscape
- Render Pipeline: Built-in (Default_PipelineAsset)

# Game Mechanics
## Core Gameplay Loop
- Navigate menus, configure enemy placement, play the main game scene, and handle win/loss states.
## Controls and Input Methods
- Mouse and Keyboard (New Input System). 'Tab' key opens the in-game menu.

# UI
- **Intro Menu**: The initial screen where players start the game.
- **Lose Scene**: Screen shown when the player loses.
- **Options (Tab) Menu**: In-game overlay for settings and pausing.

# Key Asset & Context
- `Assets/FPS/Art/Textures/Backgrounds/play.png`: Background for IntroMenu.
- `Assets/FPS/Art/Textures/Backgrounds/options.png`: Background for Tab menu.
- `Assets/FPS/Art/Textures/Backgrounds/losescene.png`: Background for LoseScene.
- `Assets/FPS/Scenes/IntroMenu.unity`: Scene for the start menu.
- `Assets/FPS/Scenes/LoseScene.unity`: Scene for the loss screen.
- `Assets/FPS/Prefabs/UI/InGameMenu.prefab`: Prefab for the Tab menu used across play scenes.

# Implementation Steps
1. **Configure Texture Import Settings**
   - Description: Change the texture type of `play.png`, `options.png`, and `losescene.png` to `Sprite (2D and UI)`.
   - Assigned role: developer
   - Dependencies: None
   - Parallelizable: Yes

2. **Update IntroMenu Background**
   - Description: Open `Assets/FPS/Scenes/IntroMenu.unity`, find `Canvas/BackgroundImage`, and set its `Image.sprite` to `play.png`.
   - Assigned role: developer
   - Dependencies: Step 1
   - Parallelizable: Yes

3. **Update LoseScene Background**
   - Description: Open `Assets/FPS/Scenes/LoseScene.unity`, find `Canvas/BackgroundImage`, and set its `Image.sprite` to `losescene.png`.
   - Assigned role: developer
   - Dependencies: Step 1
   - Parallelizable: Yes

4. **Update InGameMenu Prefab Background**
   - Description: Modify `Assets/FPS/Prefabs/UI/InGameMenu.prefab`. Find `Canvas/Background`, set its `Image.sprite` to `options.png`, and change its `Image.color` to `Color.white` (fully opaque white).
   - Assigned role: developer
   - Dependencies: Step 1
   - Parallelizable: Yes

# Verification & Testing
- **Manual Verification**:
  1. Open `IntroMenu` scene and verify the new background is visible in the Editor and Play mode.
  2. Play the game and trigger a loss to verify the `LoseScene` background.
  3. During gameplay, press `Tab` to verify the `InGameMenu` background.
- **Automated Check**:
  - Run a script to verify that the `Image` components on the targeted objects now reference the correct sprites and have the expected colors.
