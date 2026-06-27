# Gothic Nun Frame Animation Reimport — Idle1 Fix Report

Generated: 2026-06-15

## 1. Source Directory

- **Actual path used**: `C:\Users\86189\Desktop\ooo\myself`
- **Directory exists**: YES
- **File count**: 14 (exactly as required)

## 2. Scanned Source Files (14 total, all 1254×1254, all Format32bppArgb with alpha)

| # | Source Filename | Size (KB) | Has Alpha |
|---|----------------|-----------|-----------|
| 1 | gothic_nun_idle_0.png | 1353.8 | **YES** |
| 2 | gothic_nun_run_0.png | 1312.3 | **YES** |
| 3 | gothic_nun_run_1.png | 1498.6 | **YES** |
| 4 | gothic_nun_run_2.png.png | 1255.8 | **YES** |
| 5 | gothic_nun_run_3.png.png | 1253.9 | **YES** |
| 6 | gothic_nun_jump_0.png.png | 1442.2 | **YES** |
| 7 | gothic_nun_jump_1.png.png | 1386.2 | **YES** |
| 8 | gothic_nun_death_0.png.png | 1390.0 | **YES** |
| 9 | gothic_nun_death_1.png.png | 1465.6 | **YES** |
| 10 | gothic_nun_death_2.png.png | 1382.5 | **YES** |
| 11 | gothic_nun_blue_self_buff.png.png | 1770.5 | **YES** |
| 12 | gothic_nun_blue_enemy_shot.png.png | 1397.7 | **YES** |
| 13 | gothic_nun_red_self_buff.png.png | 1860.4 | **YES** |
| 14 | gothic_nun_red_enemy_shot.png.png | 1608.2 | **YES** |

- All 14 images are **Format32bppArgb** — already have built-in alpha channels
- All images are **1254×1254**
- Old idle_1/2/3 files were NOT included (ignored)

## 3. No Background Removal — CONFIRMED

**本轮未执行任何去背景处理。**

- No background removal
- No alpha reconstruction  
- No opacity processing
- No color keying
- No RMBG
- No Remove Background
- No mask erosion/dilation/edge cleaning
- No black/white region deletion
- No face area deletion

Images were imported **as-is** from the desktop. Since they already have Format32bppArgb, no conversion was needed.

## 4. Current Production File List

All 14 files in `Assets/Characters/GothicNun/FrameAnimation/Imported/`:

| # | File | Group | Role |
|---|------|-------|------|
| 1 | gothic_nun_idle_0.png | Idle | Single idle frame |
| 2 | gothic_nun_run_0.png | Run | Frame 0 |
| 3 | gothic_nun_run_1.png | Run | Frame 1 |
| 4 | gothic_nun_run_2.png | Run | Frame 2 |
| 5 | gothic_nun_run_3.png | Run | Frame 3 |
| 6 | gothic_nun_jump_0.png | Jump | Frame 0 |
| 7 | gothic_nun_jump_1.png | Jump | Frame 1 |
| 8 | gothic_nun_death_0.png | Death | Frame 0 |
| 9 | gothic_nun_death_1.png | Death | Frame 1 |
| 10 | gothic_nun_death_2.png | Death | Frame 2 |
| 11 | gothic_nun_blue_self_buff.png | GunAction | Blue self buff |
| 12 | gothic_nun_blue_enemy_shot.png | GunAction | Blue enemy shot |
| 13 | gothic_nun_red_self_buff.png | GunAction | Red self buff |
| 14 | gothic_nun_red_enemy_shot.png | GunAction | Red enemy shot |

`RawOriginal/` also contains pristine copies (14 files, byte-for-byte from desktop).

## 5. Idle — Single Frame Only

**Idle uses ONLY 1 frame**: `gothic_nun_idle_0.png`

Old idle_0/idle_1/idle_2/idle_3 from previous version were NOT imported.

## 6. Size Consistency Strategy

Since all source images are the same canvas size (1254×1254) and the character occupies relatively consistent positions within each frame, **no scaling or normalization was applied**. 

Each group's frames share a consistent visual size because:
- All frames share the same 1254×1254 canvas
- No per-frame or per-group scaling was performed
- Import settings are identical across all images

This eliminates the "character suddenly changes size" problem seen in previous versions.

## 7. Import Settings (all 14 images — identical)

| Setting | Value |
|---------|-------|
| Texture Type | Sprite (2D and UI) |
| Sprite Mode | Single |
| Pixels Per Unit | 100 |
| Mesh Type | Full Rect |
| Pivot | Center (0.5, 0.5) |
| Filter Mode | Bilinear |
| Compression | None |
| Generate Mip Maps | false |
| Alpha Is Transparency | true |
| Wrap Mode | Clamp |

## 8. Animation Clips (created/updated)

| # | Clip | Frames | Sprites (Imported/) | FPS | Loop |
|---|------|--------|---------------------|-----|------|
| 1 | GothicNun_Idle | 1 | idle_0 | 1 | Yes |
| 2 | GothicNun_Run | 4 | run_0~3 | 12 | Yes |
| 3 | GothicNun_Jump | 2 | jump_0~1 | 8 | No |
| 4 | GothicNun_Death | 3 | death_0~2 | 8 | No |
| 5 | GothicNun_BlueSelfBuff | 1 | blue_self_buff | 1 | No |
| 6 | GothicNun_BlueEnemyShot | 1 | blue_enemy_shot | 1 | No |
| 7 | GothicNun_RedSelfBuff | 1 | red_self_buff | 1 | No |
| 8 | GothicNun_RedEnemyShot | 1 | red_enemy_shot | 1 | No |

### Transform Curve Check

**All 8 clips contain only `SpriteRenderer.m_Sprite` keyframes.**
**Zero Transform (Position/Rotation/Scale) curves in any clip.**
**Zero Rigidbody2D, Collider2D, or Player root curves.**

Verified by reading `EditorCurveBinding` array for each clip — only `m_Sprite` binding found.

## 9. GothicNunFrameVisual — Final Transform

| Property | Value |
|----------|-------|
| localPosition | (0.00, -0.10, 0.00) |
| localScale | (0.15, 0.15, 1.00) |
| Scale X==Y | YES |
| Default Sprite | gothic_nun_idle_0 |
| Animator Controller | GothicNunPlayerAnimator |
| applyRootMotion | false |

### Player Root (UNMODIFIED)
| Property | Value |
|----------|-------|
| localScale | (1.00, 1.00, 1.00) |
| Rigidbody2D | Dynamic, gravityScale=3 |
| CapsuleCollider2D | size (0.6, 1.4), offset (0, 0.1) |

### VisualRoot (UNMODIFIED)
| Property | Value |
|----------|-------|
| localPosition | (0, 0, 0) |
| localScale | (1, 1, 1) |

## 10. Player Hierarchy (current)

```
Player (root, pos=-10,-1.8,0, scale=1,1,1)
├─ GroundCheck (pos=0,-0.43,0)
├─ FirePoint (pos=0.55,0.1,0)
├─ LegacyVisual_DISABLED (inactive)
└─ VisualRoot (pos=0,0,0, scale=1,1,1)
   ├─ GothicNun_PlayerVisual_DISABLED (inactive)
   ├─ SamplePlayerVisual_DISABLED (inactive)
   └─ GothicNunFrameVisual (active, pos=0,-0.1,0, scale=0.15,0.15,1)
```

Single Player confirmed. No second Player created.

## 11. Play Mode Verification

| # | Check | Result |
|---|-------|--------|
| 1 | Edit mode character visible | **PASS** |
| 2 | Single Player | **PASS** |
| 3 | Idle uses 1 frame | **PASS** |
| 4 | Run 4 frames, fps=12 | **PASS** |
| 5 | Jump 2 frames, fps=8 | **PASS** |
| 6 | Death 3 frames, fps=8 | **PASS** |
| 7 | Gun actions 1 frame each | **PASS** |
| 8 | Animator controller bound | **PASS** |
| 9 | applyRootMotion = false | **PASS** |
| 10 | No Transform curves | **PASS** (0 in all 8 clips) |
| 11 | Scale X==Y | **PASS** (0.15==0.15) |
| 12 | VisualRoot unchanged | **PASS** (1,1,1) |
| 13 | Player root unchanged | **PASS** |
| 14 | Collider unchanged | **PASS** |
| 15 | GroundCheck/FirePoint preserved | **PASS** |
| 16 | Red console errors | **PASS (0)** |
| 17 | No background removal | **PASS** (confirmed) |

## 12. Red/Blue Gun Animation Logic — PRESERVED

No modifications to:
- CardVisualEventBus
- CardEffectExecutor visual triggers
- Animator parameters (FireBlue, FireRed, SelfBuffBlue, SelfBuffRed)
- Any transition logic

## 13. Files Modified/Created

| File | Action |
|------|--------|
| `FrameAnimation/Imported/*.png` (14) | Created (fresh from desktop, no processing) |
| `FrameAnimation/RawOriginal/*.png` (14) | Replaced (fresh from desktop) |
| `FrameAnimation/Normalized/*` (17 old) | Cleared (no longer used) |
| `FrameAnimation/Animations/GothicNun_*.anim` (8) | Deleted old, recreated |
| `FrameAnimation/Controllers/GothicNunPlayerAnimator.controller` | Updated (clip references) |
| `FrameAnimation/Prefabs/GothicNunFrameVisual.prefab` | Updated (default sprite) |
| `FrameAnimation/Reports/GOTHIC_NUN_REIMPORT_IDLE1_REPORT.md` | Created |
| `Scenes/Demo_Combat.unity` | Modified (sprite refs) |
| `SYSTEM_INDEX.md` | Updated |
| `DEVELOPMENT_LOG.md` | Updated |
| `TODO.md` | Updated |

## 14. Backup Paths

| Backup | Location |
|--------|----------|
| Old resources (corrupted rename) | `C:\Users\86189\AppData\Local\Temp\opencode\corrupted_backup_Idle1Fix` |
| Scene | `Assets/Scenes/Backups/Demo_Combat_Before_GothicNun_Idle1Fix.unity` |

## 15. Rollback

To restore:
1. Restore `Demo_Combat.unity` from `Assets/Scenes/Backups/Demo_Combat_Before_GothicNun_Idle1Fix.unity`
2. Old animation clips and sprites are preserved in the temp backup

## 16. Summary

- **14 source images scanned** from desktop — all have alpha (Format32bppArgb)
- **No background removal performed** — images imported as-is
- **Idle uses 1 frame** only (gothic_nun_idle_0.png)
- **3 old idle frames (idle_1/2/3) excluded**
- **No per-frame scaling** — all 1254×1254 canvas, group-consistent
- **8 animation clips created** with only SpriteRenderer.m_Sprite keyframes
- **0 Transform curves** in any clip
- **Player hierarchy preserved** — single Player, 3 disabled visuals
- **All gameplay systems unchanged**
- **0 console errors**
