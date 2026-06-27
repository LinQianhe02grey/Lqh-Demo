# Gothic Nun Air Shooting Feel Fix Report

Generated: 2026-06-15

---

## 1. Air vs Ground Shoot Differentiation

**File**: `Assets/Scripts/Combat/PlayerController2D.cs`

**Ground shooting** (unchanged):
- `_shootStartedInAir = false`
- Full 0.4s: `rb.velocity.x = 0`, `rb.velocity.y` preserved
- No movement, jump, dash, shoot during recovery

**Air shooting** (new):
- `_shootStartedInAir = true`
- Two-phase recovery
- Horizontal velocity NOT zeroed
- Gravity continues normally

## 2. Air Shoot Phase Breakdown

| Phase | Time Range | Duration | Behavior |
|-------|-----------|----------|----------|
| Phase 1 — Action Lock | 0.00 – 0.10s | 0.10s | Preserve existing momentum; no new horizontal input applied; gravity normal; facing locked |
| Phase 2 — Limited Control | 0.10 – 0.40s | 0.30s | Allow air control at 45% multiplier; gravity normal; facing locked |
| Recovery End | 0.40s+ | — | Full control restored; facing updates from input |

**Serialized parameters** (all in PlayerController2D):
- `airShotFullLockDuration = 0.10f`
- `airShotRecoveryDuration = 0.40f`  
- `airShotControlMultiplier = 0.45f`

## 3. Horizontal Velocity — Air

**Phase 1**: No new acceleration. `_airShotInitialVelocityX` recorded. `rb.velocity.x` preserved as-is (with recoil applied once at start).

**Phase 2**: `_rb.velocity = new Vector2(_horizontalInput * moveSpeed * airShotControlMultiplier, _rb.velocity.y)`

Code in `FixedUpdate()`:
```csharp
if (_shootRecoveryLocked)
{
    if (_shootStartedInAir)
    {
        float elapsed = ShootRecoveryDuration - (_shootRecoveryEndTime - Time.time);
        if (elapsed >= airShotFullLockDuration)
        {
            float targetVx = _horizontalInput * moveSpeed * airShotControlMultiplier;
            _rb.velocity = new Vector2(targetVx, _rb.velocity.y);
        }
        return;
    }
    _rb.velocity = new Vector2(0f, _rb.velocity.y);
    return;
}
```

## 4. Vertical Velocity

**NEVER modified during recovery.** `rb.velocity.y` is always preserved (read, not set). Gravity scale never changes. `rb.simulated` stays true. `bodyType` stays Dynamic.

## 5. Recoil

**Applied once at shoot start** (inside `StartShootRecovery()`):

```csharp
float recoilDir = _facingRight ? -1f : 1f;
_rb.velocity = new Vector2(
    _rb.velocity.x + recoilDir * airShotRecoilSpeed,
    _rb.velocity.y
);
```

**Final value**: `airShotRecoilSpeed = 0.6f`

Applied only for air shots. Ground shots don't get recoil (vx=0 already).

## 6. Facing Lock

`FlipSprite()` in PlayerController2D returns early when `_shootRecoveryLocked == true`. Facing direction locked at `_facingRight` set at moment of shot. Restored when recovery ends.

## 7. Landing During Air Shoot

- `_shootStartedInAir` stays true (recorded at start)
- The original 0.4s timer continues — no restart
- Phase 2 air control continues applying
- When grounded and no input: vx stays near 0
- No double recoil, no extra projectile, no timer reset

## 8. Consecutive Shoot Guard

`HandleVisualAction()` in GothicNunAnimationBridge checks `_playerController.IsShootRecoveryLocked` and returns early. No second projectile, no card consumed, no timer reset during recovery.

## 9. Input Sampling During Recovery

`_horizontalInput` continuously sampled in `Update()` (line 159). Not zeroed during recovery. Used by:
- Phase 2 air control calculation
- `MoveRequested` animator parameter
- Post-recovery facing via `FlipSprite()`

## 10. Animator Exit Rules (unchanged from Stage 13H)

BlueEnemyShot / RedEnemyShot exits:
- → Death: AnyState, Dead=true (highest priority)
- → Jump: exitTime=true(1), Grounded=false
- → Run: exitTime=true(1), Grounded+MoveRequested
- → Idle: exitTime=true(1), Grounded+!MoveRequested

## 11. Files Modified

| File | Changes |
|------|---------|
| `PlayerController2D.cs` | Added air shoot fields (`_shootStartedInAir`, `_airShotInitialVelocityX`, `airShotFullLockDuration`, `airShotControlMultiplier`, `airShotRecoilSpeed`); Modified `StartShootRecovery()` for air/ground differentiation and recoil; Modified `FixedUpdate()` for two-phase air recovery |
| `GOTHIC_NUN_AIR_SHOOT_FEEL_FIX_REPORT.md` | Created |
| `Scenes/Backups/Demo_Combat_Before_AirShootFix.unity` | Created (backup) |
| `SYSTEM_INDEX.md` | Updated |
| `DEVELOPMENT_LOG.md` | Updated |
| `TODO.md` | Updated |

## 12. Play Mode Verification

| # | Check | Result |
|---|-------|--------|
| 1 | 0 compile errors | PASS |
| 2 | 0 console errors in Play | PASS |
| 3 | Idle plays at start | PASS |
| 4 | Grounded detected correctly | PASS |
| 5 | airShotRecoilSpeed = 0.6 | PASS |
| 6 | airShotControlMultiplier = 0.45 | PASS |
| 7 | airShotFullLockDuration = 0.1 | PASS |
| 8 | Not recovery locked at start | PASS |
| 9 | MoveRequested param updating | PASS |
| 10 | Clip lengths unchanged (0.417s) | PASS |
| 11 | 0 Transform curves in clips | PASS |
| 12 | Gravity scale unchanged (3) | PASS |
| 13 | Rigidbody2D Dynamic preserved | PASS |

## 13. Rollback

1. Restore `Demo_Combat.unity` from `Assets/Scenes/Backups/Demo_Combat_Before_AirShootFix.unity`
2. Revert `PlayerController2D.cs` from version control (previous Stage 13H version)

## 14. Unchanged

- Images, sprites, alpha, PPU, pivot
- GothicNunFrameVisual scale/position
- Player root, VisualRoot
- Collider, GroundCheck, FirePoint
- Move speed, jump force, dash params
- Gravity scale, Rigidbody2D settings
- Projectile damage, speed
- CardData, card effects
- Magazine, inventory, HUD, camera
- Animator Controller (same exit transitions)
- Animation clips (same 0.4s frames)
