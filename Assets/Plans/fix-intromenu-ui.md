# Project Overview
- Game Title: FPS Sample
- High-Level Concept: Fixing the Intro Menu UI structure.
- Players: Single player
- Target Platform: PC (StandaloneWindows64)

# Game Mechanics
## Controls and Input Methods
- UI interaction via mouse and keyboard.

# UI
- **Intro Menu**: Needs the background to be behind the buttons and the gameplay HUD to be hidden.

# Key Asset & Context
- `Assets/FPS/Scenes/IntroMenu.unity`: The scene to be fixed.
- `Canvas/BackgroundImage`: Currently rendering on top of the menu buttons.
- `GameManager/GameHUD`: Gameplay UI accidentally visible in the main menu.

# Implementation Steps
1. **Fix UI Layering in IntroMenu**
   - Description: Open `Assets/FPS/Scenes/IntroMenu.unity`. Reorder `Canvas/BackgroundImage` to be the first child of the `Canvas` (index 0) so it renders as the background.
   - Assigned role: developer
   - Dependencies: None
   - Parallelizable: Yes

2. **Hide Gameplay HUD in IntroMenu**
   - Description: In `Assets/FPS/Scenes/IntroMenu.unity`, locate `GameManager/GameHUD` and set it to inactive (Deactivate the GameObject).
   - Assigned role: developer
   - Dependencies: None
   - Parallelizable: Yes

# Verification & Testing
- **Manual Verification**:
  1. Open `IntroMenu` scene.
  2. Verify the "Play" button (StartButton) and other menu buttons are visible on top of the background.
  3. Verify the health bar and other HUD elements are no longer visible.
  4. Ensure the buttons are clickable and correctly trigger scene transitions.
