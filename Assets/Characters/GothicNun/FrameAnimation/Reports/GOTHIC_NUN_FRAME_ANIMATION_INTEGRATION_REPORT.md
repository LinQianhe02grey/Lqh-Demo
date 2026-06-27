# Gothic Nun Frame Animation Integration Report

Generated: 2026-06-14

## 1. Source Material

### Directory Check
- **Requested path**: `C:\Users\86189\Desktop\ooo\myself`
- **Directory.Exists result**: **TRUE** — directory found at exact requested path
- **Resolved full path**: `C:\Users\86189\Desktop\ooo\myself` (confirmed via PowerShell)

### Files Found (17 total — correct count)

| # | Source Filename | Normalized Target | Action Type | Width | Height | Format |
|---|-----------------|-------------------|-------------|-------|--------|--------|
| 1 | gothic_nun_idle_0.png.png | gothic_nun_idle_0.png | Idle frame 0 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 2 | gothic_nun_idle_1.png.png | gothic_nun_idle_1.png | Idle frame 1 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 3 | gothic_nun_idle_2.png.png | gothic_nun_idle_2.png | Idle frame 2 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 4 | gothic_nun_idle_3.png.png | gothic_nun_idle_3.png | Idle frame 3 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 5 | gothic_nun_run_0.png.png | gothic_nun_run_0.png | Run frame 0 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 6 | gothic_nun_run_1.png.png | gothic_nun_run_1.png | Run frame 1 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 7 | gothic_nun_run_2.png.png | gothic_nun_run_2.png | Run frame 2 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 8 | gothic_nun_run_3.png.png | gothic_nun_run_3.png | Run frame 3 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 9 | gothic_nun_jump_0.png.png | gothic_nun_jump_0.png | Jump frame 0 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 10 | gothic_nun_jump_1.png.png | gothic_nun_jump_1.png | Jump frame 1 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 11 | gothic_nun_death_0.png.png | gothic_nun_death_0.png | Death frame 0 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 12 | gothic_nun_death_1.png.png | gothic_nun_death_1.png | Death frame 1 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 13 | gothic_nun_death_2.png.png | gothic_nun_death_2.png | Death frame 2 | 1254 | 1254 | 24bppRgb→RGBA32 |
| 14 | gothic_nun_blue_self_buff.png.png | gothic_nun_blue_self_buff.png | Blue self buff | 1254 | 1254 | 24bppRgb→RGBA32 |
| 15 | gothic_nun_blue_enemy_shot.png.png | gothic_nun_blue_enemy_shot.png | Blue enemy shot | 1254 | 1254 | 24bppRgb→RGBA32 |
| 16 | gothic_nun_red_self_buff.png.png | gothic_nun_red_self_buff.png | Red self buff | 1254 | 1254 | 24bppRgb→RGBA32 |
| 17 | gothic_nun_red_enemy_shot.png.png | gothic_nun_red_enemy_shot.png | Red enemy shot | 1254 | 1254 | 24bppRgb→RGBA32 |

### Naming Note
All source files had double `.png.png` extension. Copies in project use single `.png`.

### Dimension Consistency
All 17 images: **1254 × 1254** — consistent dimensions.

## 2. Alpha Channel Processing

### Initial State
- **All 17 images**: Format24bppRgb — **NO alpha channel**
- **Background color**: Solid off-white (~RGB 0.95-0.99) at all corners and edges
- **No baked checkerboard**: Background is uniform near-white, not gridded
- **Character in center**: Dark pixels (R≈0.03-0.19) at center positions confirm character occupies canvas center

### Alpha Processing Method
1. Made each texture readable via TextureImporter
2. Sampled all pixels: if R>0.85 AND G>0.85 AND B>0.85 → alpha=0 (transparent)
3. All other pixels → alpha=1 (opaque)
4. Encoded as RGBA32 PNG, overwriting project copy
5. Reimported with `alphaIsTransparency=true`

### Processing Results
| Image | % Transparent | Alpha After |
|-------|---------------|-------------|
| gothic_nun_blue_enemy_shot.png | 81.7% | True |
| gothic_nun_blue_self_buff.png | 78.7% | True |
| gothic_nun_death_0.png | 82.6% | True |
| gothic_nun_death_1.png | 86.7% | True |
| gothic_nun_death_2.png | 89.3% | True |
| gothic_nun_idle_0.png | 80.5% | True |
| gothic_nun_idle_1.png | 84.0% | True |
| gothic_nun_idle_2.png | 86.9% | True |
| gothic_nun_idle_3.png | 84.4% | True |
| gothic_nun_jump_0.png | 83.9% | True |
| gothic_nun_jump_1.png | 82.1% | True |
| gothic_nun_red_enemy_shot.png | 80.4% | True |
| gothic_nun_red_self_buff.png | 73.7% | True |
| gothic_nun_run_0.png | 83.2% | True |
| gothic_nun_run_1.png | 82.4% | True |
| gothic_nun_run_2.png | 86.2% | True |
| gothic_nun_run_3.png | 86.0% | True |

**All 17 images now have valid alpha channels.**

## 3. Import Settings

All 17 textures configured with:
- **Texture Type**: Sprite (2D and UI)
- **Sprite Mode**: Single
- **Pixels Per Unit**: 100
- **Mesh Type**: Full Rect (default)
- **Pivot**: Center (0.5, 0.5)
- **Filter Mode**: Bilinear
- **Compression**: None
- **Mip Maps**: Disabled
- **Alpha Is Transparency**: true
- **Wrap Mode**: Clamp
- **Max Size**: 2048

## 4. Animation Clips

| Clip | Frames | FPS | Duration | Loop | Final Path |
|------|--------|-----|----------|------|------------|
| GothicNun_Idle | 4 (idle_0~3) | 8 | 0.5s | **Yes** | `FrameAnimation/Animations/GothicNun_Idle.anim` |
| GothicNun_Run | 4 (run_0~3) | 12 | 0.33s | **Yes** | `FrameAnimation/Animations/GothicNun_Run.anim` |
| GothicNun_Jump | 2 (jump_0~1) | 8 | 0.25s | **No** | `FrameAnimation/Animations/GothicNun_Jump.anim` |
| GothicNun_Death | 3 (death_0~2) | 8 | 0.375s | **No** | `FrameAnimation/Animations/GothicNun_Death.anim` |
| GothicNun_BlueSelfBuff | 1 (blue_self_buff) | 5 | 0.2s | **No** | `FrameAnimation/Animations/GothicNun_BlueSelfBuff.anim` |
| GothicNun_BlueEnemyShot | 1 (blue_enemy_shot) | 7 | ~0.14s | **No** | `FrameAnimation/Animations/GothicNun_BlueEnemyShot.anim` |
| GothicNun_RedSelfBuff | 1 (red_self_buff) | 5 | 0.2s | **No** | `FrameAnimation/Animations/GothicNun_RedSelfBuff.anim` |
| GothicNun_RedEnemyShot | 1 (red_enemy_shot) | 7 | ~0.14s | **No** | `FrameAnimation/Animations/GothicNun_RedEnemyShot.anim` |

All clips only contain `SpriteRenderer.m_Sprite` keyframes. No Transform/Rigidbody2D/Collider properties.

## 5. Animator Controller

### Path
`Assets/Characters/GothicNun/FrameAnimation/Controllers/GothicNunPlayerAnimator.controller`

### Parameters
| Name | Type | Default |
|------|------|---------|
| Speed | Float | 0 |
| Grounded | Bool | true |
| VerticalVelocity | Float | 0 |
| Dead | Bool | false |
| FireBlue | Trigger | — |
| FireRed | Trigger | — |
| SelfBuffBlue | Trigger | — |
| SelfBuffRed | Trigger | — |

### States
| State | Clip | Position |
|-------|------|----------|
| Idle (default) | GothicNun_Idle | (300, 100) |
| Run | GothicNun_Run | (300, 200) |
| Jump | GothicNun_Jump | (300, 300) |
| Death | GothicNun_Death | (550, 100) |
| BlueEnemyShot | GothicNun_BlueEnemyShot | (550, 200) |
| RedEnemyShot | GothicNun_RedEnemyShot | (550, 300) |
| BlueSelfBuff | GothicNun_BlueSelfBuff | (50, 200) |
| RedSelfBuff | GothicNun_RedSelfBuff | (50, 300) |

### Transitions
| From | To | Conditions | Duration |
|------|----|------------|----------|
| Idle → Run | Speed > 0.05, no exit time | 0.02 |
| Run → Idle | Speed < 0.05, no exit time | 0.02 |
| Idle → Jump | Grounded == false, no exit time | 0.02 |
| Run → Jump | Grounded == false, no exit time | 0.02 |
| Jump → Idle | Grounded == true AND Speed < 0.05 | 0.02 |
| Jump → Run | Grounded == true AND Speed > 0.05 | 0.02 |
| Any State → Death | Dead == true (highest priority) | 0.02 |
| Any State → BlueEnemyShot | FireBlue trigger | 0.02 |
| Any State → RedEnemyShot | FireRed trigger | 0.02 |
| Any State → BlueSelfBuff | SelfBuffBlue trigger | 0.02 |
| Any State → RedSelfBuff | SelfBuffRed trigger | 0.02 |
| BlueEnemyShot → Exit | Exit time 0.95 | 0.02 |
| RedEnemyShot → Exit | Exit time 0.95 | 0.02 |
| BlueSelfBuff → Exit | Exit time 0.95 | 0.02 |
| RedSelfBuff → Exit | Exit time 0.95 | 0.02 |

- Death has highest priority
- Action states exit after clip ends (exit time 0.95)
- All transition durations 0.02s

## 6. Visual Prefab

### Path
`Assets/Characters/GothicNun/FrameAnimation/Prefabs/GothicNunFrameVisual.prefab`

### Structure
```
GothicNunFrameVisual
├─ SpriteRenderer (default: gothic_nun_idle_0, sortingLayer=Character, order=10)
├─ Animator (GothicNunPlayerAnimator.controller, applyRootMotion=false)
└─ GothicNunAnimationBridge
```

### Components NOT included
- No Rigidbody2D
- No Collider2D
- No PlayerController
- No input system
- No Health
- No Magazine/Inventory
- No CardEffectExecutor
- No Projectile generation
- No GroundCheck

## 7. Animation Bridge

### GothicNunAnimationBridge.cs
- **Path**: `Assets/Characters/GothicNun/FrameAnimation/Scripts/GothicNunAnimationBridge.cs`
- **Base**: Inspired by SamplePlayerAnimationBridge, rewritten for GothicNun frame system
- **Responsibilities**:
  - Reads Player Rigidbody2D.velocity.x → Speed (absolute)
  - Reads Player Rigidbody2D.velocity.y → VerticalVelocity
  - Reuses PlayerController2D.groundCheck/groundLayer for Grounded
  - Subscribes Health.OnDeath → Dead
  - Subscribes CardVisualEventBus.OnVisualAction → Triggers
  - NO input reading, NO card consumption, NO projectile creation, NO damage/heal/buff

### CardVisualEventBus.cs
- **Path**: `Assets/Characters/GothicNun/FrameAnimation/Scripts/CardVisualEventBus.cs`
- **Purpose**: Static event bus connecting CardEffectExecutor → GothicNunAnimationBridge
- **Action types**: FireRed, FireBlue, SelfBuffRed, SelfBuffBlue

## 8. CardEffectExecutor Modifications

### File modified: `Assets/Scripts/Cards/CardEffectExecutor.cs`

### Changes:
1. Added `using Cardwin.Characters;`
2. `ExecuteLeft()`: After projectile Init() success → calls `TriggerProjectileVisual(effect)`
3. `ExecuteRight()`: After ApplyEffectToTarget() success → calls `TriggerSelfVisual(effect)`
4. `TriggerProjectileVisual()`: If effect == Damage → FireRed, else → FireBlue
5. `TriggerSelfVisual()`: If effect == Damage → SelfBuffRed, else → SelfBuffBlue

### Call chain (left-click damage projectile):
```
PlayerController2D.Update() → Input.GetMouseButtonDown(0)
→ MagazineSystem.UseCurrentCardLeft()
→ CardEffectExecutor.ExecuteLeft()
→ Instantiate projectile → TriggerProjectileVisual(Damage)
→ CardVisualEventBus.Notify(FireRed)
→ GothicNunAnimationBridge.HandleVisualAction()
→ Animator.SetTrigger("FireRed")
→ GothicNun_RedEnemyShot plays
```

### Call chain (left-click support projectile):
```
Same as above → TriggerProjectileVisual(non-Damage)
→ CardVisualEventBus.Notify(FireBlue)
→ GothicNun_BlueEnemyShot plays
```

### Call chain (right-click self effect):
```
PlayerController2D.Update() → Input.GetMouseButtonDown(1)
→ MagazineSystem.UseCurrentCardRight()
→ CardEffectExecutor.ExecuteRight()
→ ApplyEffectToTarget() → TriggerSelfVisual(effect)
→ CardVisualEventBus.Notify(SelfBuffBlue/SelfBuffRed)
→ GothicNun_BlueSelfBuff or GothicNun_RedSelfBuff plays
```

## 9. Card Classification (Real Data)

### Project cards (12 total from CardDatabase)
| Card ID | Name | CardType | LeftClickEffect | RightClickEffect | useTarget | IsOffensive | Visual Action (Left/Right) |
|---------|------|----------|-----------------|------------------|-----------|-------------|---------------------------|
| C001 | Strike | Attack | Damage | Damage | Enemy | True | FireRed / SelfBuffRed |
| C002 | Pierce | Attack | Damage | Damage | Enemy | True | FireRed / SelfBuffRed |
| C003 | Burst | Attack | Damage | Damage | Enemy | True | FireRed / SelfBuffRed |
| C004 | Guard | Defense | Block | Block | Self | False | FireBlue / SelfBuffBlue |
| C005 | Heal | Support | Heal | Heal | Self | False | FireBlue / SelfBuffBlue |
| C006 | Focus | Support | Focus | Focus | Self | False | FireBlue / SelfBuffBlue |
| C007 | Evil Shot | Attack | Damage | Damage | Enemy | True | FireRed / SelfBuffRed |
| C008 | Mercy Shield | Defense | Block | Block | Self | False | FireBlue / SelfBuffBlue |
| C009 | Combo Spark | Support | ComboSpark | ComboSpark | Self | False | FireBlue / SelfBuffBlue |
| C010 | Quick Reload | Support | QuickReload | QuickReload | Self | False | FireBlue / SelfBuffBlue |
| C011 | Weakness Mark | Debuff | WeaknessMark | WeaknessMark | Enemy | False | FireBlue / SelfBuffBlue |
| C012 | Aerial Mark | Attack | AerialMark | AerialMark | Enemy | True | FireBlue / SelfBuffBlue |

### FireRed (Damage projectile) cards: Strike, Pierce, Burst, Evil Shot
### FireBlue (Support projectile) cards: Guard, Heal, Focus, Mercy Shield, Combo Spark, Quick Reload, Weakness Mark, Aerial Mark
### SelfBuffRed (Damage right-click) cards: Strike, Pierce, Burst, Evil Shot
### SelfBuffBlue (Support right-click) cards: Guard, Heal, Focus, Mercy Shield, Combo Spark, Quick Reload, Weakness Mark, Aerial Mark

**SelfBuffRed has REAL call sources**: Strike, Pierce, Burst, Evil Shot all have rightClickEffect=Damage

## 10. Scene Modifications

### Backup
- Created: `Assets/Scenes/Backups/Demo_Combat_Before_GothicNunFrameAnimation.unity`

### Player Hierarchy Changes

**Before:**
```
Player (root)
├─ GroundCheck
├─ FirePoint
├─ LegacyVisual_DISABLED
└─ VisualRoot (pos 0,0,0 scale 1,1,1)
   ├─ GothicNun_PlayerVisual_DISABLED (inactive)
   └─ SamplePlayerVisual (active, sprite+anim, scale 0.3)
```

**After:**
```
Player (root)
├─ GroundCheck
├─ FirePoint
├─ LegacyVisual_DISABLED
└─ VisualRoot (pos 0,0,0 scale 1,1,1)
   ├─ GothicNun_PlayerVisual_DISABLED (inactive)
   ├─ SamplePlayerVisual_DISABLED (inactive, renamed)
   └─ GothicNunFrameVisual (active, scale 0.15, pos 0,-0.1,0)
```

### GothicNunFrameVisual Final Transform
| Property | Value |
|----------|-------|
| localPosition | (0, -0.10, 0) |
| localScale | (0.15, 0.15, 1.0) |
| localRotation | (0, 0, 0) |
| Scale X==Y | YES (0.15 == 0.15) |
| Z scale | 1.0 |

### Preserved Systems
- Player root GameObject: NOT deleted, NOT renamed
- Rigidbody2D: Dynamic, gravityScale=3, mass=1
- CapsuleCollider2D: size (0.6, 1.4), offset (0, 0.1)
- GroundCheck: local (0, -0.43)
- FirePoint: local (0.55, 0.1)
- PlayerController2D: all params unchanged
- Health: unchanged
- MagazineSystem: unchanged
- InventorySystem: unchanged
- CardEffectExecutor: modified (visual events added)
- PlayerAlignment: unchanged
- ComboRatingSystem: unchanged
- RewardManager: unchanged
- SpawnPoint_Player: unchanged
- MainCamera: unchanged
- CameraFollow2D: unchanged
- HUD/Canvas: unchanged
- SamplePlayerVisual: preserved (disabled, renamed)
- GothicNun_PlayerVisual_DISABLED: preserved (disabled)

### NOT Modified
- Move Speed (7)
- Jump Force (13)
- Max Jumps (2)
- Dash Speed (18)
- Dash Duration (0.15)
- Dash Cooldown (0.6)
- Rigidbody2D.gravityScale (3)
- Rigidbody2D.mass (1)
- Collider Size/Offset
- GroundCheck radius/position
- Projectile speed/damage
- CardData values
- Buff/Heal values
- Magazine capacity
- Camera follow params
- SpawnPoint_Player
- HUD layout

## 11. Files Created

| File | Type |
|------|------|
| `Assets/Characters/GothicNun/FrameAnimation/Raw/gothic_nun_*.png` (17) | Images (alpha-processed) |
| `Assets/Characters/GothicNun/FrameAnimation/Animations/GothicNun_Idle.anim` | AnimationClip |
| `Assets/Characters/GothicNun/FrameAnimation/Animations/GothicNun_Run.anim` | AnimationClip |
| `Assets/Characters/GothicNun/FrameAnimation/Animations/GothicNun_Jump.anim` | AnimationClip |
| `Assets/Characters/GothicNun/FrameAnimation/Animations/GothicNun_Death.anim` | AnimationClip |
| `Assets/Characters/GothicNun/FrameAnimation/Animations/GothicNun_BlueSelfBuff.anim` | AnimationClip |
| `Assets/Characters/GothicNun/FrameAnimation/Animations/GothicNun_BlueEnemyShot.anim` | AnimationClip |
| `Assets/Characters/GothicNun/FrameAnimation/Animations/GothicNun_RedSelfBuff.anim` | AnimationClip |
| `Assets/Characters/GothicNun/FrameAnimation/Animations/GothicNun_RedEnemyShot.anim` | AnimationClip |
| `Assets/Characters/GothicNun/FrameAnimation/Controllers/GothicNunPlayerAnimator.controller` | AnimatorController |
| `Assets/Characters/GothicNun/FrameAnimation/Prefabs/GothicNunFrameVisual.prefab` | Prefab |
| `Assets/Characters/GothicNun/FrameAnimation/Scripts/GothicNunAnimationBridge.cs` | Script |
| `Assets/Characters/GothicNun/FrameAnimation/Scripts/CardVisualEventBus.cs` | Script |
| `Assets/Characters/GothicNun/FrameAnimation/Reports/GOTHIC_NUN_FRAME_ANIMATION_INTEGRATION_REPORT.md` | Report |
| `Assets/Scenes/Backups/Demo_Combat_Before_GothicNunFrameAnimation.unity` | Scene Backup |

## 12. Files Modified

| File | Modification |
|------|-------------|
| `Assets/Scripts/Cards/CardEffectExecutor.cs` | Added visual event triggers after ExecuteLeft/ExecuteRight |
| `Assets/Scenes/Demo_Combat.unity` | Disabled+renamed SamplePlayerVisual, added GothicNunFrameVisual |

## 13. Play Mode Verification

| # | Check | Result |
|---|-------|--------|
| 1 | Single Player in scene | **PASS** (1 instance, Tag=Player) |
| 2 | GothicNunFrameVisual visible in edit mode | **PASS** (sprite=gothic_nun_idle_0) |
| 3 | SamplePlayerVisual disabled but preserved | **PASS** (renamed to SamplePlayerVisual_DISABLED) |
| 4 | GothicNun_PlayerVisual_DISABLED preserved | **PASS** (still inactive under VisualRoot) |
| 5 | Animator controller bound | **PASS** (GothicNunPlayerAnimator) |
| 6 | AnimationBridge present and bound | **PASS** (GothicNunAnimationBridge) |
| 7 | Rigidbody2D not changed | **PASS** (Dynamic, gravityScale=3) |
| 8 | CapsuleCollider2D not changed | **PASS** (size 0.6x1.4, offset 0,0.1) |
| 9 | GroundCheck not changed | **PASS** |
| 10 | FirePoint not changed | **PASS** |
| 11 | Move speed unchanged | **PASS** (7) |
| 12 | Jump force unchanged | **PASS** (13) |
| 13 | Dash unchanged | **PASS** (18, 0.15s) |
| 14 | Magazine system working | **PASS** (8 cards loaded) |
| 15 | VisionRoot scale X==Y | **PASS** (0.15 == 0.15) |
| 16 | No second Player | **PASS** |
| 17 | Console errors at startup | **PASS** (0 errors, 0 warnings) |
| 18 | CardVisualEventBus has subscribers | **VERIFIED** (via reflection check) |
| 19 | Animator parameters initialized | **PASS** (Speed=0, Grounded=False, VV=-1.17, Dead=False) |
| 20 | FlipSprite via VisualRoot inheritance | **PASS** (Player root scale.x flip, children auto-mirror) |
| 21 | Death transitions reserved | **PASS** (Dead param configured) |
| 22 | Four gun actions as independent states | **PASS** (separate AnyState transitions) |
| 23 | No damage/attack clip merged | **PASS** (4 independent single-frame clips) |

## 14. Known Limitations

1. **Visual scale tuning**: Scale 0.15 is tentative; may need fine-tuning after visual review.
2. **Foot alignment**: localPosition Y=-0.10 is approximate; may need adjustment per visual review.
3. **Action clip duration**: Single-frame clips at 5-7 FPS give ~0.14-0.2s durations; may feel fast.
4. **Right-click self-damage cards** (Strike, Pierce, Burst, Evil Shot): These cards apply Damage to self via right-click. The system correctly triggers SelfBuffRed for these, but the visual shows "self buff" while the effect damages the player. This is a CARDS design issue, not an animation issue. The animation system faithfully reflects the effect classification (Damage→Red, non-Damage→Blue).
5. **WeaknessMark/AerialMark right-click**: These cards have useTarget=Enemy but ExecuteRight always applies to Player. The system classifies them as SelfBuffBlue (non-Damage). This is a design limitation in CardEffectExecutor, not introduced by this integration.

## 15. Rollback Procedure

To restore previous visual:
1. Disable `Player/VisualRoot/GothicNunFrameVisual`
2. Enable and rename `Player/VisualRoot/SamplePlayerVisual_DISABLED` back to `SamplePlayerVisual`
3. (Optional) Enable `GothicNun_PlayerVisual_DISABLED` for rigid-bone GothicNun

To completely undo CardEffectExecutor changes:
1. Remove `using Cardwin.Characters;`
2. Remove `TriggerProjectileVisual()` and `TriggerSelfVisual()` calls and methods

## 16. Readiness Assessment

**Can proceed to next stage**: YES

All core systems operational:
- Frame animation clips created and assigned
- Animator controller with correct transitions
- Animation bridge reading real player data
- Visual event bus connecting card execution to animation
- CardEffectExecutor modified to trigger correct visual action types
- Scene backup created
- Sample visual preserved and disabled
- GothicNun rigid-bone visual preserved
- No physics parameters changed
- No gameplay systems altered
- Zero console errors

**Recommendations for next stage**:
- Visual size/position fine-tuning
- Testing Idle/Run/Jump/Death animation transitions during gameplay
- Testing all 12 cards for correct visual triggers
- Edge case testing (no ammo, reloading, death, input lock, bag open)
