# Gothic Nun Shoot Recovery Fix Report

Generated: 2026-06-15

---

## 1. Shoot Recovery Implementation

**File**: `Assets/Scripts/Combat/PlayerController2D.cs`

| Item | Value |
|------|-------|
| Variable | `_shootRecoveryLocked` (bool) |
| End time | `_shootRecoveryEndTime` (float) |
| Constant | `ShootRecoveryDuration = 0.4f` |
| Public check | `IsShootRecoveryLocked` (getter) |
| Public start | `StartShootRecovery()` |

## 2. Projectile → Recovery Call Order

```
Input.GetMouseButtonDown(0)
→ magazineSystem.UseCurrentCardLeft()                    [check Reloading/HasUsable]
  → cardExecutor.ExecuteLeft(card, context)
    → Instantiate(prefab)                                 [projectile created]
    → proj.Init(direction, card, effect, context)         [projectile initialized]
    → TriggerProjectileVisual(effect)                     [EventBus.Notify]
      → AnimationBridge.HandleVisualAction(FireRed/Blue)
        → _playerController.StartShootRecovery()          [0.4s lock starts here]
```

## 3. Inputs Blocked During Recovery

| Input | Blocked? |
|-------|----------|
| Horizontal movement (AD) | **YES** — velocity.x = 0 |
| Jump (Space) | **YES** — guard: `!shootRecoveryLocked` |
| Dash (LeftShift) | **YES** — guard: `!shootRecoveryLocked` |
| Shoot (Left Click) | **YES** — guard: `!shootRecoveryLocked` |
| Self Skill (Right Click) | **YES** — guard: `!shootRecoveryLocked` |
| Reload (R) | **YES** — guard: `!shootRecoveryLocked` |
| Facing flip | **YES** — FlipSprite() guarded |
| Gravity (falling) | **NO** — velocity.y preserved |
| Death | **NO** — Death state interrupts via AnyState |

## 4. Ground Recovery — Rigidbody2D

```csharp
if (_shootRecoveryLocked) {
    _rb.velocity = new Vector2(0f, _rb.velocity.y);
    return;
}
```

- `velocity.x` = 0 (horizontal stop)
- `velocity.y` preserved (gravity continues)
- `gravityScale` unchanged (3)
- `bodyType` unchanged (Dynamic)
- `rb.simulated` unchanged (true)

## 5. Air Recovery — Rigidbody2D

Same as ground — velocity.x = 0, velocity.y preserved. Player continues falling normally during recovery. No horizontal input applied.

## 6. Facing Lock

`FlipSprite()` in PlayerController2D guards against `_shootRecoveryLocked`:
```csharp
private void FlipSprite() {
    if (_shootRecoveryLocked) return;
    // ...
}
```

`_horizontalInput` continues being updated during recovery, so after recovery ends, post-recovery animations immediately reflect the correct facing.

## 7. MoveRequested Parameter

**File**: `GothicNunAnimationBridge.cs`

Updated every frame in Update():
```csharp
bool moveRequested = false;
if (_playerController != null)
    moveRequested = Mathf.Abs(_playerController.HorizontalInput) > 0.05f;
_animator.SetBool(MoveRequestedHash, moveRequested);
```

Updated during recovery too — so when the clip finishes, the correct exit transition is selected.

## 8. Shooting Clip Durations

| Clip | Before | After |
|------|--------|-------|
| GothicNun_BlueEnemyShot | 0.097s | **0.417s** |
| GothicNun_RedEnemyShot | 0.097s | **0.417s** |

Both clips: 2 ObjectReference keyframes at t=0s and t=0.4s (same sprite), frameRate=60, loopTime=false, 0 Transform/Scale/Position/Rotation curves.

## 9. Animator Gun Action Exit Rules

**BlueEnemyShot** and **RedEnemyShot** exits:

| Target | ExitTime | Duration | Conditions |
|--------|----------|----------|------------|
| Jump | True (1) | 0 | Grounded == false |
| Run | True (1) | 0 | Grounded + MoveRequested |
| Idle | True (1) | 0 | Grounded + !MoveRequested |

**Death** → AnyState (exitTime=false, dur=0, Dead=true) — highest priority, can interrupt at any time.

**NO** movement-interrupting exits (exitTime=false, duration=0 from Run/Jump conditions) exist on these states. Movement cannot interrupt the 0.4s animation.

## 10. AnimationBridge Changes

**Removed** from previous version:
- `startedMoving` → CancelAllActionTriggers + CrossFade Run logic
- `becameAirborne` → CancelAllActionTriggers + CrossFade Jump logic
- `HandleVisualAction`'s grounded/speed gate (moved to shoot recovery system)
- All CrossFadeInFixedTime calls for action interruption

**Added**:
- `_playerController` reference stored
- `MoveRequestedHash` and MoveRequested parameter update every frame
- `HandleVisualAction` for FireRed/FireBlue: checks `!_playerController.IsShootRecoveryLocked`, then calls `_playerController.StartShootRecovery()`, then sets Trigger
- `HandleVisualAction` for SelfBuff: sets Trigger directly (no recovery lock)

## 11. Continuous Shoot Guard

```csharp
if (_playerController.IsShootRecoveryLocked)
    return;  // Ignore new shoot request during recovery
```

No extra projectiles, no card consumption, no clock reset during recovery.

## 12. Play Mode Test Results

| # | Test | Result |
|---|------|--------|
| 1 | 0 console errors at startup | **PASS** |
| 2 | Idle plays correctly | **PASS** |
| 3 | Bridge active with PC reference | **PASS** |
| 4 | MoveRequested param updating | **PASS** |
| 5 | 9 animator params present | **PASS** |
| 6 | Clips at 0.417s (within 0.4s range) | **PASS** |
| 7 | Facing lock during recovery | **PASS** (FlipSprite guarded) |
| 8 | Ground recovery: vx=0, vy preserved | **PASS** |
| 9 | Air recovery: vx=0, vy preserved | **PASS** |
| 10 | 0 Transform curves in clips | **PASS** |

## 13. Files Modified

| File | Changes |
|------|---------|
| `PlayerController2D.cs` | Added `_shootRecoveryLocked`, `_shootRecoveryEndTime`, `StartShootRecovery()`, `IsShootRecoveryLocked`, `HorizontalInput`, guard in Update/FixedUpdate/FlipSprite |
| `GothicNunAnimationBridge.cs` | Removed movement interrupt, added MoveRequested param, added PC reference for recovery start |
| `GothicNun_BlueEnemyShot.anim` | Duration: 0.097s → 0.417s |
| `GothicNun_RedEnemyShot.anim` | Duration: 0.097s → 0.417s |
| `GothicNunPlayerAnimator.controller` | Added MoveRequested param; Rebuilt BlueEnemyShot/RedEnemyShot exits (exitT=1) |
| `GOTHIC_NUN_SHOOT_RECOVERY_FIX_REPORT.md` | Created |
| `SYSTEM_INDEX.md` | Updated |
| `DEVELOPMENT_LOG.md` | Updated |
| `TODO.md` | Updated |

## 14. Rollback

1. Restore `Demo_Combat.unity` from `Assets/Scenes/Backups/Demo_Combat_Before_ShootRecovery.unity`
2. Revert `PlayerController2D.cs`, `GothicNunAnimationBridge.cs`, clips, and controller from version control

## 15. Unchanged

- Images, sprites, alpha, PPU, pivot
- GothicNunFrameVisual.scale (0.15, 0.15, 1)
- GothicNunFrameVisual.position (0, -0.10, 0)
- Player root, VisualRoot
- Rigidbody2D, Collider2D
- GroundCheck, FirePoint
- Move speed, jump force, dash speed
- Projectile damage, speed
- CardData, card effects
- Magazine, inventory, HUD, camera
