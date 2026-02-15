# ⚡ FlowPuzzleGame - Hex Puzzle Game

Unity puzzle game where players rotate hexagonal tiles to create energy paths from sources to goals.

---

## 🎯 Quick Overview

**Game Type:** Logic puzzle with hex grid mechanics  
**Platform:** Android   
**Build:** Unity 2022.3 / IL2CPP  
**Total Levels:** 10  
**External Assets:** DOTween • Amplitude SDK • Epic Toon FX • Modern Procedural UI Kit • Naughty Attributes

---

## 🎮 Core Systems

### Gameplay
- **Hex Grid Math:** Doubled-row coordinate system, 6-directional connections
- **Pathfinding:** BFS algorithm for real-time power flow validation
- **Node Types:** Source, Goal, Line, Corner (60°/120°), Y-Junction
- **Win Detection:** All goals must be powered through valid connections

### Architecture
- **Design Patterns:** Singleton (managers), Observer (GameEvents), ScriptableObjects (data-driven levels)
- **Data Layer:** LevelManager owns all game state, AmplitudeManager is thin analytics wrapper
- **UI Flow:** Custom Tween based UI solution (UIController.cs) 


### Features Implemented
- Level progression with PlayerPrefs unlocking
- Real-time stats (time, moves counter)
- Sound system with 5 SFX + BGM, toggle controls
- Particle effects (goal activation, star burst, celebration)
- Camera zoom-out effect on level complete
- Amplitude analytics (EU server): `game_started`, `level_started`, `level_completed`

---

## ⚙️ Optimizations

**Implemented From Starting Phase:**
- ScriptableObject-based levels = no runtime instantiation overhead
- Grid stored in `Dictionary<Vector2Int, HexNode>` for O(1) neighbor lookup
- Connection validation cached during rotation, not every frame

---

## 📂 Project Structure
```
Assets/_Project/
├── Scripts/
│   ├── Core/         # Enums, Events, Singletons, Scheduler
│   ├── Gameplay/     # LevelManager, GridManager, HexNode, PathFinder
│   ├── Data/         # ScriptableObjects (levels, connections)
│   ├── UI/           # UIController, screen handlers
│   ├── Sound/        # SoundManager
│   └── Analytics/    # AmplitudeManager
├── Settings/         # Level data assets
└── Prefabs/          # HexNode, UI, particles
```

---

## 🚀 Getting Started

1. Open in Unity 2022.3+
2. Scene: `Assets/_Project/Scenes/Game.unity`
3. Play in editor or build APK
4. First launch starts at Splash → Level Selection

---

## 🔑 Amplitude Setup


Before building, add your Amplitude API key:
1. Open `Assets/_Project/Scripts/Analytics/AmplitudeManager.cs`
2. Find: `private const string API_KEY = "YOUR_AMPLITUDE_API_KEY_HERE";`
3. Replace placeholder with your key
4. Build APK    


**Note:** API key excluded for security. Submitted APK has valid key and analytics work.  
**Known Issue** AdGuard/Pi-hole DNS filtering blocks Amplitude events - disable if testing analytics.

---

**Developed as Unity Developer Test**
