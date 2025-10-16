# GameJam Base

![Unity](https://img.shields.io/badge/Unity-6.0+-000000?style=flat&logo=unity&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-blue.svg)
![URP](https://img.shields.io/badge/Render%20Pipeline-URP-orange)

Unity Game Jam template with audio, achievements & VFX systems, utilities, curated third-party assets.  
Designed for rapid prototyping with zero-friction setup.

**Try it here**  
[![Try GameJam Base](https://img.shields.io/badge/Try_it_on-itch.io-FA5C5C?style=for-the-badge&logo=itch.io&logoColor=white)](https://lombardidelavega.itch.io/gamejam-base)

Unity template used to create multiple games in game jams, like:  
[![Space Trains](https://img.shields.io/badge/Space_Trains-Itch.io-FA5C5C?style=flat&logo=itch.io&logoColor=white)](https://lombardidelavega.itch.io/space-trains)
[![Wheel Power](https://img.shields.io/badge/Wheel_Power-Itch.io-FA5C5C?style=flat&logo=itch.io&logoColor=white)](https://lombardidelavega.itch.io/wheel-power)

## Project structure

- `Assets/GAMEJAM/` **(rename this!)** - Your game-specific content goes here. Start by renaming this folder to your project name, then add your gameplay scripts, scenes, and assets.
- `Assets/CoreSystems/` - Foundational systems (audio manager, achievements, VFX, etc.). These are generic and reusable across projects.
- `Assets/ThirdParty/` - Curated asset library. **Feel free to delete unused assets** to keep your project lean. You don't need all 750+ assets for every jam!
- `Assets/Staging/` - Temporary workspace for experimental content that may be moved to your game folder or discarded. Contains menus, prefabs, shaders, utilities and other miscellaneous assets.
- `Licenses/` - Third-party asset licenses for compliance

**💡 Tip:** Start by cleaning out unused third-party assets based on your game's style and needs. Keep only what serves your vision!

## Included third-party assets

- **Audio**: 250+ SFX (Kenney, Duelyst) + 5 music tracks (Abstraction)
- **3D Models**: 145+ prototype models with animations (Kenney Prototype Kit)
- **2D Graphics**: 500+ icons, particles, patterns (Kenney collections)
- **Fonts**: 28+ retro/pixel fonts ready for TextMeshPro
- **Plugins**: DOTween, TextMeshPro, SoftMaskForUGUI

*All third-party assets use permissive licenses (CC0/CC BY) - perfect for commercial use*

## Dependencies

### Unity Packages
- **Universal Render Pipeline (URP)** 17.0.4+
- **Input System** 1.14.0+

### Third-party Packages
- **[SoftMaskForUGUI](https://github.com/mob-sakai/SoftMaskForUGUI)** - Used in Achievement UI prefabs for advanced masking
- **[Unity UI Rounded Corners](https://github.com/kirevdokimov/Unity-UI-Rounded-Corners)**
- **DOTween** - Smooth animations

## Static Event System

This template uses a **static event class** (`GameEvents.cs`) for rapid prototyping, a controversial but pragmatic choice for game jams.

**Perfect for Game Jams because:**

- **Instant SFX setup**  
  Typical end-of-jam scenario: _"We have 2 hours left, let's add audio!"_
    - ❌ **Without GameEvents**: Dig into classes, add audio reference, risk breaking jump logic, debug for 20 minutes...
    - ✅ **With GameEvents**: Just add `GameEvents.OnPlayerJump += PlayJumpSound;` → **Done in 30 seconds**
- **Zero configuration**  
      No references to drag, no null checks, works instantly, easy to use even in teams with varied skill levels or amateur developers
- **Ultra-fast prototyping**  
  Add new features without breaking existing code or chasing down dependencies

  
**Game Jam Strategy** 

Keep core gameplay explicit, use GameEvents for peripheral systems only:
- ✅ **Audio**: Sound effects & music triggers
- ✅ **Achievements**: Progress tracking, unlock notifications
- ✅ **Juice**: Screen shake, particles, UI animations, camera effects, ...
- ❌ **Core Logic**: Player movement, Gameplay, Collisions, complex systems, ... (keep these explicit for debugging)

Audio, Achievements, and Juice are typically added in the final hours when every minute counts for polish. Having everything centralized in GameEvents allows instant addition of SFX, micro-improvements, and juice effects without touching the core codebase, risking breaking something that works, or losing time digging through code.

That said, I keep in mind that this approach brings serious drawbacks. And that's why I only use it for peripheral systems in game jams. 

**The drawbacks:**
- Poor testability
- Invisible dependencies
- Sneaky memory leaks
- Refactoring is difficult
- Unpredictable execution order  
- Requires strict cleanup discipline
- Editor-specific pitfalls

But used strategically for peripheral systems in game jams, you get the best of both worlds: stable core logic + rapid peripheral execution.

### Quick Usage

1. Add events to `GameEvents.cs`:
   ```cs
   public static event Action OnPlayerJump;
   public static void TriggerPlayerJump() => OnPlayerJump?.Invoke();
   ```

2. Trigger in gameplay: `GameEvents.TriggerPlayerJump();`

3. Connect audio: `GameEvents.OnPlayerJump += PlayJumpSound;`

*See the existing GameEvents.cs for more examples.*


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Music Attribution

If you use any music by Abstraction (Benjamin Burnes) in your final product, you must include the following credit:
Music: "Track Title" by Abstraction (Benjamin Burnes), used under CC BY 4.0 — abstractionmusic.com
