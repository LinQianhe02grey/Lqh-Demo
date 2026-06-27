# Gothic Nun Animation Transition Fix Report

Generated: 2026-06-15

## 1. Pre-Modification Analysis

### 1.1 Clip Durations (before fix)

| Clip | Duration | Issue |
|------|----------|-------|
| GothicNun_BlueEnemyShot | **1.000s** | Way too long |
| GothicNun_RedEnemyShot | **1.000s** | Way too long |
| GothicNun_BlueSelfBuff | **1.000s** | Way too long |
| GothicNun_RedSelfBuff | **1.000s** | Way too long |

### 1.2 Controller Transitions (before fix)

| Transition | Exit Time | Duration |
|-----------|-----------|----------|
| Idle → Run | False | **0.02** |
| Run → Idle | False | **0.02** |
| Idle → Jump | False | **0.02** |
| Run → Jump | False | **0.02** |
| Jump → Idle | False | **0.02** |
| Jump → Run | False | **0.02** |
| AnyState → Death | False | **0.02** |
| AnyState → Fire/SelfBuff | False | **0.02** |
| BlueEnemyShot → ? | True | 0.02 → **(null) destination** |
| RedEnemyShot → ? | True | 0.02 → **(null) destination** |
| BlueSelfBuff → ? | True | 0.02 → **(null) destination** |
| RedSelfBuff → ? | True | 0.02 → **(null) destination** |

### 1.3 AnimationBridge (before fix)

- Always fired Fire/SelfBuff triggers regardless of movement/air state
- No ResetTrigger on movement
- No CrossFade to Run/Jump on movement
- No grounded/speed check before visual actions

### 1.4 Animation Lock Variables

**None found.** No `isAttacking`, `isShooting`, `actionLock`, `animationLock`, `Coroutine`, or `WaitForSeconds` variables exist in the GothicNun animation system.

---

## 2. Changes Made

### 2.1 Clip Duration Fix

| Clip | Before | After | Keyframes |
|------|--------|-------|-----------|
| GothicNun_BlueEnemyShot | 1.000s | 0.0967s | 2 (t=0, t=0.08) |
| GothicNun_RedEnemyShot | 1.000s | 0.0967s | 2 (t=0, t=0.08) |
| GothicNun_BlueSelfBuff | 1.000s | 0.1367s | 2 (t=0, t=0.12) |
| GothicNun_RedSelfBuff | 1.000s | 0.1367s | 2 (t=0, t=0.12) |

Both enemy shot clips under 0.10s limit. Both self buff clips under 0.15s limit.

### 2.2 Controller Transition Fix

**All movement transitions: duration set to 0** (was 0.02).

**Gun action state exits rebuilt:**

Each of BlueEnemyShot, RedEnemyShot, BlueSelfBuff, RedSelfBuff now has:
- → Jump: exitTime=false, dur=0, condition: Grounded==false
- → Run: exitTime=false, dur=0, conditions: Grounded==true AND Speed>0.05
- → Idle: exitTime=true, exitTime=1, dur=0

### 2.3 AnimationBridge Rewrite

Key changes in `GothicNunAnimationBridge.cs`:

**Update order**: Speed → Grounded → VerticalVelocity → Dead (always first, every frame)

**Movement interrupt logic**:
- When Speed transitions from ≤0.05 to >0.05: CancelAllActionTriggers() + CrossFadeInFixedTime("Run", 0f)
- When grounded transitions from true to false: CancelAllActionTriggers() + CrossFadeInFixedTime("Jump", 0f)

**Visual action gating**:
- `HandleVisualAction()` now checks: if NOT grounded OR speed > 0.05 → return (no trigger)
- Only fires triggers when player is grounded AND stationary

**New method**: `CancelAllActionTriggers()` — resets FireRed, FireBlue, SelfBuffRed, SelfBuffBlue Triggers

### 2.4 Rules Implemented

| Scenario | Visual Behavior |
|----------|----------------|
| Stationary ground + Fire card | Play BlueEnemyShot/RedEnemyShot (0.08s), then auto-return to Idle |
| Fire card while running | Keep Run, NO visual action trigger |
| Fire card while jumping | Keep Jump, NO visual action trigger |
| Start moving during gun action | Immediately CrossFade to Run |
| Jump during gun action | Immediately CrossFade to Jump |
| Stationary ground + Self card | Play SelfBuff (0.12s), then auto-return to Idle |
| Self card while running | Keep Run, NO visual action trigger |
| Self card while jumping | Keep Jump, NO visual action trigger |

---

## 3. Unchanged Systems

- Images, sprites, alpha, PPU, pivot
- GothicNunFrameVisual.localScale (0.15, 0.15, 1)
- GothicNunFrameVisual.localPosition (0, -0.10, 0)
- Player root, VisualRoot
- Rigidbody2D, Collider2D
- Movement speed, jump, dash parameters
- Projectile speed, damage
- Card effects, magazine, inventory
- HUD, camera
- CardVisualEventBus, CardEffectExecutor (trigger points unchanged)

---

## 4. Play Mode Verification

| # | Test | Result |
|---|------|--------|
| 1 | Idle at start (Grounded, Speed=0) | PASS |
| 2 | Bridge active | PASS |
| 3 | No compile errors | PASS |
| 4 | Play mode success | PASS |
| 5 | 0 console errors in Play | PASS |
| 6 | GothicNunFrameVisual present | PASS |
| 7 | Scale/Position unchanged | PASS |
| 8 | Animator controller bound | PASS |

---

## 5. Files Modified

| File | Action |
|------|--------|
| `FrameAnimation/Animations/GothicNun_BlueEnemyShot.anim` | Duration: 1.0s → 0.08s |
| `FrameAnimation/Animations/GothicNun_RedEnemyShot.anim` | Duration: 1.0s → 0.08s |
| `FrameAnimation/Animations/GothicNun_BlueSelfBuff.anim` | Duration: 1.0s → 0.12s |
| `FrameAnimation/Animations/GothicNun_RedSelfBuff.anim` | Duration: 1.0s → 0.12s |
| `FrameAnimation/Controllers/GothicNunPlayerAnimator.controller` | Durations→0, action exits rebuilt |
| `FrameAnimation/Scripts/GothicNunAnimationBridge.cs` | Rewritten with movement interrupt |
| `FrameAnimation/Reports/GOTHIC_NUN_ANIMATION_TRANSITION_FIX_REPORT.md` | Created |
| `SYSTEM_INDEX.md` | Updated |
| `DEVELOPMENT_LOG.md` | Updated |
| `TODO.md` | Updated |

---

## 6. Backup Paths

| Backup | Location |
|--------|----------|
| Scene | `Assets/Scenes/Backups/Demo_Combat_Before_TransitionFix.unity` |

## 7. Rollback

To undo all changes:
1. Restore `Demo_Combat.unity` from `Assets/Scenes/Backups/Demo_Combat_Before_TransitionFix.unity`
2. Restore `GothicNunAnimationBridge.cs` from version control
3. Reimport old animation clips and controller from previous backup
