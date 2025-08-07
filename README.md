- `Assets/GAMEJAM/` (to be renamed) contains all the core content of the game - gameplay, systems, assets, etc.
- `Assets/CoreSystems/` contains foundational systems used across the entire project (e.g. audio manager, save system, input, etc.). These are meant to be generic, reusable, and decoupled from specific gameplay features.
- `Assets/Staging/` is a temporary workspace for experimental or undecided content (scripts, textures, sounds, etc.) that may be moved into `GameJam/` or discarded later.
- `Licenses` third-party licenses

If you use any music by Abstraction (Benjamin Burnes) in your final product, you must include the following credit:
Music: "Track Title" by Abstraction (Benjamin Burnes), used under CC BY 4.0 — abstractionmusic.com


## GameEvents

**Perfect for Game Jams because:**

- **Instant SFX setup**  
  Typical end-of-jam scenario: _"We have 2 hours left, let's add audio!"_
    - ✅ **With GameEvents**: Just add `GameEvents.OnPlayerJump += PlayJumpSound;` → **Done in 30 seconds**
    - ❌ **Without GameEvents**: Dig into classes, add audio reference, risk breaking jump logic, debug for 20 minutes...
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

The drawbacks:
  - Poor testability
  - Invisible dependencies
  - Sneaky memory leaks
  - Refactoring is difficult
  - Unpredictable execution order  
  - Requires strict cleanup discipline
  - Editor-specific pitfalls

But used strategically for peripheral systems in game jams, you get the best of both worlds: stable core logic + rapid peripheral execution.



### How to

To benefit from this system, add your game-specific events to `GameEvents.cs` (optionally even before implementing them)

  ```cs
  public static class GameEvents
  {
      // Interface "wishlist" - even if not used yet
      public static event Action OnPlayerJump;
      public static event Action<string> OnEnemyKilled;
      public static event Action<Vector3> OnExplosion;
      public static event Action OnLevelComplete;
      
      // Triggers (can be empty at first)
      public static void TriggerPlayerJump() => OnPlayerJump?.Invoke();
      public static void TriggerEnemyKilled(string type) => OnEnemyKilled?.Invoke(type);
      // ...
  }
  ```
  
  Then: Add triggers during gameplay development
  ```cs
  public void Jump()
  {
      rb.AddForce(Vector3.up * jumpForce);
      GameEvents.TriggerPlayerJump(); // ← 1 line, done!
  }
  ```
  
  Finally: use, example with audio design -> 30 seconds to connect
  ```cs
  void Start()
  {
      GameEvents.OnPlayerJump += PlayJumpSound;     // ← Already ready!
      GameEvents.OnEnemyKilled += PlayDeathSound;   // ← Interface exists
      GameEvents.OnExplosion += PlayExplosionSound; // ← Zero friction
  }
  ```

  