# Inner Duel - Technical Design Document (TDD)

## 1. PROJECT OVERVIEW

**Game Name:** Inner Duel  
**Genre:** 2D Fighting Game  
**Engine:** Unity  
**Language:** C#  
**Platform:** PC  

### Concept
"Inner Duel" is a 2D fighting game where characters represent opposing mental states (e.g., Discipline vs. Spontaneity). The gameplay loop involves selecting a battlefield, choosing characters for two players, and engaging in a high-stakes duel until one player's health is depleted.

### Gameplay Loop
1.  **Preparation:** Players select a map and their respective characters.
2.  **Combat:** Players engage in real-time combat using movement, attacks, blocks, and dashes.
3.  **Resolution:** The winner is declared in a result screen, with options to rematch or return to character selection.

---

## 2. GAME FLOW ARCHITECTURE

The game follows a linear scene transition flow managed by specific UI managers:

1.  **MainMenuScene:** Entry point. Access to settings, credits, and the game start.
2.  **MapSelectScene:** Players choose the arena for the duel.
3.  **CharacterSelectScene:** Both Player 1 and Player 2 select their characters.
4.  **LoadingScene:** Asynchronously loads the gameplay scene while displaying tips.
5.  **MainGameScene:** The core combat arena where the duel takes place.
6.  **ResultScene:** Displays the winner and match statistics.

---

## 3. SCENE STRUCTURE

### MainMenuScene
*   **Purpose:** Initial landing page.
*   **UI Hierarchy:** Main Menu Panel (Play, Options, Credits, Quit), Options Panel, Credits Panel.
*   **Main Scripts:** `MainMenuManager.cs`.
*   **Interactions:** Button clicks to navigate panels or start the map selection.

### MapSelectScene
*   **Purpose:** Arena selection.
*   **UI Hierarchy:** Map Preview Image, Map Name Text, Map Description, Navigation Buttons.
*   **Main Scripts:** `MapSelectManager.cs`.
*   **Interactions:** Keyboard (WASD/Arrows) or Buttons to browse maps; Enter/Confirm to save selection to `GameData`.

### CharacterSelectScene
*   **Purpose:** Dual-player character selection.
*   **UI Hierarchy:** P1 Selection Area, P2 Selection Area, Character Portals/Names, Status Indicators (READY/SELECTING).
*   **Main Scripts:** `CharacterSelectManager.cs`.
*   **Interactions:** P1 uses WASD+F; P2 uses Arrows+Enter. Both must confirm to proceed.

### LoadingScene
*   **Purpose:** Seamless transition to gameplay.
*   **UI Hierarchy:** Progress Bar, Progress Text, Random Tips.
*   **Main Scripts:** `LoadingSceneManager.cs`.
*   **Interactions:** Automated transition using `AsyncOperation`.

### MainGameScene
*   **Purpose:** Core gameplay execution.
*   **UI Hierarchy:** Health Bars, Combo Counters, Timer, Intro/Ending Text.
*   **Main Scripts:** `GameManager.cs`, `UIManager.cs`, `CameraController.cs`.
*   **Interactions:** Player movement and combat controls.

### ResultScene
*   **Purpose:** Match summary.
*   **UI Hierarchy:** Winner Name, Winner Portrait, Rematch/Menu Buttons.
*   **Main Scripts:** `ResultScreenManager.cs`.

---

## 4. UI SYSTEM

The UI system is modular, using dedicated managers for each scene.

*   **Navigation:** Uses a mix of Mouse-based Button clicks and Keyboard-based navigation (Input System).
*   **Persistence:** UI managers read from and write to `GameData` to maintain state across scenes.
*   **Pause Menu:** Implemented via `PauseMenuManager` in the `MainGameScene`. It toggles `Time.timeScale` and provides options to restart or quit.

---

## 5. CHARACTER SYSTEM

Characters are implemented using a data-driven approach.

### CharacterData (ScriptableObject)
Located at `Assets/_Project/Scripts/Character/CharacterType.cs`, this SO defines:
*   **Identity:** `CharacterType` enum, Name, Description.
*   **Stats:** Max Health, Move Speed, Defense, Jump Force.
*   **Combat:** Attack Damage, Ranges, Cooldowns, Dash Multipliers.
*   **Visuals:** Main Color, Effect Color.
*   **Flags:** `canBlock`, `canDash`, `canCounterAttack`, etc.

### Character Prefabs
Located at `Assets/_Project/Prefabs/Prefabs/Characters/`.
Each prefab contains:
*   `InnerCharacterController`: Handles movement and combat logic.
*   `Animator`: For character-specific animations.
*   `CharacterData`: A reference to its specific SO.

### Selection & Spawning
*   `CharacterSelectManager` saves the selected SOs to `GameData.player1Character` and `GameData.player2Character`.
*   `GameManager` assigns these SOs to the `InnerCharacterController` instances in the `MainGameScene` and calls `InitializeFromData()`.

---

## 6. MAP SYSTEM

### MapData (ScriptableObject)
Located at `Assets/_Project/Scripts/Core/MapData.cs`.
*   Fields: `mapName`, `previewImage`, `mapPrefab`, `description`.

### Map Loading
*   `MapSelectManager` populates a list of `availableMaps` and saves the selection to `GameData.selectedMap`.
*   In `MainGameScene`, `GameManager.SpawnMap()` instantiates the prefab stored in `GameData.selectedMap.mapPrefab`.

---

## 7. GAME DATA MANAGEMENT

### GameData (Static Class)
Located at `Assets/_Project/Scripts/Core/GameData.cs`.
This static class acts as the "Blackboard" for the game:
*   **Character Selection:** `player1Character`, `player2Character`.
*   **Map Selection:** `selectedMap`.
*   **Match Results:** `winnerPlayerID`, `winnerName`.
*   **Scene Constants:** Strings for scene names to prevent typos.

Why use Static Data? It provides a simple, high-performance way to pass information between independent scenes without requiring complex singleton persistence (DDOL) for every data point.

---

## 8. GAMEPLAY INITIALIZATION

Inside `MainGameScene`, the `GameManager.InitializeGame()` method follows these steps:
1.  **Spawn Map:** Instantiates the selected map prefab.
2.  **Setup Players:** Recovers player controllers from the scene or assigns IDs to pre-placed prefabs.
3.  **Apply Data:** Injects `CharacterData` from `GameData` into the controllers.
4.  **Link Camera:** Sets the camera targets to follow both players.
5.  **Initialize UI:** Sets up health bars and names via `UIManager`.
6.  **Start Intro:** Triggers the intro state (movement locked, countdown text).

---

## 9. SCRIPT ARCHITECTURE

| Script | Responsibility | Key Methods |
| :--- | :--- | :--- |
| `GameData` | Global state storage. | `ResetData()` |
| `MainMenuManager` | Navigation for the start screen. | `PlayGame()`, `QuitGame()` |
| `MapSelectManager` | Arena browsing and selection. | `ConfirmSelection()`, `ChangeSelection()` |
| `CharacterSelectManager` | Dual-player character selection logic. | `ConfirmSelection()`, `StartGame()` |
| `LoadingSceneManager` | Async loading with visual progress. | `LoadSceneAsync()` |
| `GameManager` | Main game loop and state machine. | `InitializeGame()`, `OnCharacterDied()` |
| `InnerCharacterController`| Character movement, physics, and combat. | `InitializeFromData()`, `TakeDamage()` |
| `PauseMenuManager` | Handles pausing and mid-game options. | `Pause()`, `Resume()`, `RestartMatch()` |
| `ResultScreenManager` | Displays post-match summary. | `Rematch()`, `MainMenu()` |

---

## 10. PROJECT FOLDER STRUCTURE

The project follows a standard Unity organization pattern:

```text
Assets/_Project
├ Art                 # Sprites, Animations, Visual Assets
├ Audio               # Sound Effects and Music
├ Data                # ScriptableObject instances (.asset)
├ Prefabs             # Player, Map, and UI Prefabs
├ Scenes              # .unity files for all game states
├ Scripts             # C# Logic
│  ├ Camera           # Camera control
│  ├ Character        # Combat, Controller, Factory
│  ├ Core             # Global data, Input, Utilities
│  ├ Effects          # Particles and visual feedback
│  ├ Game             # Main managers (GameManager)
│  └ UI               # Menu and HUD managers
└ Settings            # Input Actions and project configs
```

**Reasoning:** This structure isolates game-specific code and assets from 3rd-party plugins, ensuring a clean workspace for multiple developers.

---

## 11. TEAM DEVELOPMENT WORKFLOW

### Character Development
1.  **Logic:** Modify `CharacterData.cs` to add new flags or stats.
2.  **Creation:** Create a new `CharacterData` asset in `Assets/_Project/Data`.
3.  **Prefab:** Clone a base character prefab, link the new `CharacterData`, and adjust the `Animator`.
4.  **Integration:** Add the new `CharacterData` to the `availableCharacters` list in the `CharacterSelectManager`.

### Map Development
1.  **Design:** Create an arena prefab.
2.  **Data:** Create a `MapData` asset and link the prefab.
3.  **Integration:** Add to `availableMaps` in `MapSelectManager`.

### Git Workflow
*   **Scene Separation:** Use separate scenes for menus and gameplay to minimize merge conflicts.
*   **Prefab-Based:** UI and Characters are prefabs, allowing team members to work on them without touching the main scene files.

---

## 12. FUTURE EXTENSIONS

1.  **Online Mode:** Implement Mirror or Photon for remote multiplayer.
2.  **Animation Preview:** Show character animations/moves in the `CharacterSelectScene`.
3.  **Expanded Roster:** Implementation of more mental states (e.g., Logic vs. Creativity).
4.  **Advanced Map Features:** Hazards (traps, platforms) based on the map's theme.
5.  **Input Customization:** Allow players to rebind controls via the Options panel.

---

*Document generated on March 16, 2026.*  
*Reflects Project Version: 1.0.0 (Internal Build)*
