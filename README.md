# Gore Sandbox VR

A bloody, gory VR sandbox inspired by Blade & Sorcery / Blood, Guts & Bones, with **Gorilla Tag locomotion**.

Punch people and NPCs, spawn NPCs, and spawn items: **brass knuckles, swords, bricks, vases, plates, cups**. Blood, guts, bones, dismemberment, ragdolls, breakables.

## Features
- Gorilla Tag arm-swing locomotion (no teleport). Swing your arms to move, let go to carry momentum, jump.
- Full-body gore: blood splatters, blood decals on surfaces, blood pooling, ragdoll death.
- Dismemberment: slash hard enough and limbs detach.
- Melee combat: punching velocity-based damage; weapons add damage/knockback.
- Spawn menu: spawn NPCs and items (brass knuckles, sword, brick, vase, plate, cup).
- Breakables: vases, plates, cups shatter on hard impact.
- Works in Unity Editor with keyboard/mouse fallback; builds to Android APK (Meta Quest / OpenXR).

## Setup (Unity)

1. **Unity 2022.3 LTS or 2023.2+** (3D Core / URP).
2. Create a new 3D project and copy this repo into it (or clone into the project root).
3. Install packages via **Window > Package Manager**:
   - `com.unity.inputsystem` (Input System)
   - `com.unity.xr.pluginmanagement` (XR Plugin Management)
   - `com.unity.xr.openxr` (OpenXR) — for Quest/PC VR
   - optionally `com.unity.xr.oculus` for Meta Quest
4. **Player Settings > XR Plug-in Management**: enable OpenXR (and Oculus if targeting Quest).
5. **Player Settings > Android**: set Package Name, Minimum API Level 24+, Target API Level 30+, enable **Virtual Reality Supported**.
6. **Assets > Editor > SceneBuilder > Create Scene** to build the demo scene (floor, lights, player rig, spawn menu).
7. Press **Play**. Build > Android > Build & Run for the APK.

## Controls

| Action | VR (Quest/OpenXR) | Keyboard fallback |
|---|---|---|
| Move | Swing your arms; **collide your hand with a wall/floor/object to push off** and launch | **WASD** |
| Jump | **A / X** button | **Space** |
| Grab item | **Grip** + touch | **E** near item |
| Throw | Release grip | **F** |
| Punch | Swing hand into target | Click into target |
| Open spawn menu | **Menu / B** button | **M** |
| Spawn (in menu) | Trigger on highlighted item | **1-6** keys |

Spawn keys: `1` NPC, `2` Brass Knuckles, `3` Sword, `4` Brick, `5` Vase, `6` Plate, `7` Cup.

Locomotion note: there is **no hold-to-grab toggle**. Move your hands to propel yourself, and bump your hands into surfaces to push off them (Gorilla Tag style).

## Project layout
- `Assets/Scripts/` — all gameplay code (locomotion, combat, gore, spawning, items, NPC AI).
- `Assets/Editor/SceneBuilder.cs` — one-click demo scene creator.
- `Packages/manifest.json` — package dependencies (edit versions as needed).

## Push to GitHub
```bash
cd GoreSandboxVR
git init
git add .
git commit -m "Initial commit: gory VR sandbox with Gorilla Tag locomotion"
git branch -M main
git remote add origin https://github.com/YOURUSER/YOURREPO.git
git push -u origin main
```

## APK build (you do this — I can't compile Unity here)

I don't have Unity installed, so I can't compile the APK binary for you. You build it in the Unity Editor:

1. Open `C:\Users\hujhgughu\GoreSandboxVR` in **Unity 2022.3 LTS / 2023.2+** (3D Core / URP).
2. **Window > Package Manager**: install Input System, XR Plugin Management, OpenXR (+ Oculus for Quest).
3. **Player Settings > XR Plug-in Management**: enable OpenXR (and Oculus if targeting Quest).
4. **Player Settings > Android**: enable **Virtual Reality Supported**, set Package Name, Min API Level 24+, Target API Level 30+.
5. **Tools > Gore Sandbox > Create Scene** to build the demo scene.
6. **File > Build Settings > Android > Switch Platform > Build** → choose a folder → produces `GoreSandboxVR.apk`.
7. Install: `adb install GoreSandboxVR.apk` or side-load onto Quest.

No Unity license is required for the free Personal edition to build and test.
