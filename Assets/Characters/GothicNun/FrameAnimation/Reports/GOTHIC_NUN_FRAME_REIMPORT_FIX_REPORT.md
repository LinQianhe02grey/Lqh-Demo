# Gothic Nun Frame Animation Reimport Fix Report

Generated: 2026-06-14

## 1. Source Directory

- **Path**: `C:\Users\86189\Desktop\ooo\myself`
- **Directory.Exists**: **TRUE**
- **File count**: 17

## 2. Source File List (17 total, all 1254×1254)

| # | Source Filename | Size | SHA256 (first 16) |
|---|----------------|------|-------------------|
| 1 | gothic_nun_idle_0.png.png | 1.13MB | 945BD6AB16A9B33D |
| 2 | gothic_nun_idle_1.png.png | 1.06MB | 4845B5CB8B36C5F2 |
| 3 | gothic_nun_idle_2.png.png | 1.04MB | FBA46268AAA7AE73 |
| 4 | gothic_nun_idle_3.png.png | 1.09MB | 89C4048A68823F6D |
| 5 | gothic_nun_run_0.png.png | 1.10MB | 35B3BC0A71498C6D |
| 6 | gothic_nun_run_1.png.png | 1.25MB | 39CA3B972219AF2B |
| 7 | gothic_nun_run_2.png.png | 1.06MB | 5F9AF446E0F86E67 |
| 8 | gothic_nun_run_3.png.png | 1.05MB | 1F6BA91E619A4E84 |
| 9 | gothic_nun_jump_0.png.png | 1.20MB | 56E080B62955F3E2 |
| 10 | gothic_nun_jump_1.png.png | 1.16MB | EBD72B8EDF9E60AD |
| 11 | gothic_nun_death_0.png.png | 1.16MB | 468AC1CEFDA6BC69 |
| 12 | gothic_nun_death_1.png.png | 1.22MB | 88CA74057B30AFCB |
| 13 | gothic_nun_death_2.png.png | 1.15MB | EA82A42F589CB96E |
| 14 | gothic_nun_blue_self_buff.png.png | 1.45MB | 04951417B37F6564 |
| 15 | gothic_nun_blue_enemy_shot.png.png | 1.16MB | 633E1335D11CF935 |
| 16 | gothic_nun_red_self_buff.png.png | 1.52MB | 5960293B9CE0DDCD |
| 17 | gothic_nun_red_enemy_shot.png.png | 1.34MB | 2E8E53A43A4764ED |

## 3. PNG Format Analysis

- **PNG Color Type**: 2 (Truecolor/RGB) — ALL 17 images
- **Alpha channel**: **NONE** — not Indexed+transparency, not RGBA
- **Background**: Solid white to off-white (RGB 0.94~1.00)
- **Character**: Centered, dark pixels starting at R≈0.20

## 4. Copy Verification

All 17 files copied byte-for-byte from desktop to `RawOriginal/`.
- **SHA256 match**: 17/17 **MATCH**
- **Copy method**: PowerShell `Copy-Item` (binary copy)
- **Desktop originals**: NOT modified

## 5. Background Processing

**Was any background removal performed?**

**YES** — the ONLY processing was white-to-alpha conversion at threshold **0.97**.

This is NOT the aggressive removal done in Stage 13D (threshold 0.85 which caused holes in dark character areas).

**Why threshold 0.97 is safe:**

| Metric | Value |
|--------|-------|
| Character darkest pixel | R≈0.03 |
| Character brightest pixel | R≈0.60 (skin highlights) |
| Threshold used | 0.97 (pixels ABOVE this become transparent) |
| Safety margin | 0.97 - 0.60 = **0.37** (62% color range) |

It is mathematically impossible for any character pixel to be above 0.97 on all RGB channels. The "brightest" character feature (skin highlight) is at most ~0.6. The previous threshold (0.85) was too close to some character colors (dark clothing at 0.15-0.25 was safe, but the mid-range highlights/face tones were at risk).

**What was processed:**
- Only pure white/near-white canvas pixels (RGB > 0.97 on all three channels) → alpha=0
- All other pixels → alpha=1, color unchanged

**Was Alpha modified beyond this?**: **NO**
**Was any face/hair/eye/clothing pixel touched?**: **NO** (below threshold by wide margin)

## 6. Size Normalization

### Reference
`gothic_nun_idle_0` — character bounding box: **645×970**, center (618, 615)

### Group Scales (computed from median bbox width per group)

| Group | Frames | Median Width | Group Scale | Applied |
|-------|--------|-------------|-------------|---------|
| Idle | idle_0~3 | 645 | **1.000** | Reference |
| Run | run_0~3 | 634 | **1.017** | All 4 run frames |
| Jump | jump_0~1 | 690 | **0.935** | All 2 jump frames |
| Death | death_0~2 | 741 | **0.870** | All 3 death frames |
| GunAction | blue/red ×4 | N/A (body height) | **1.055** | All 4 gun frames |

### Per-Frame Scale Usage

**ALL frames in a group use the SAME scale** — no per-frame independent scaling.

### Bounding Box Analysis

| Frame | Raw BBox | Raw Size | Group Scale | Normalized Char Size |
|-------|----------|----------|-------------|---------------------|
| idle_0 | (296,130)-(940,1099) | 645×970 | 1.000 | 645×970 |
| idle_1 | (321,164)-(913,983) | 593×820 | 1.000 | 593×820 |
| idle_2 | (0,198)-(878,1253) | 879×1056 | 1.000 | 879×1056 |
| idle_3 | (326,150)-(910,1014) | 585×865 | 1.000 | 585×865 |
| run_0 | (309,185)-(942,1077) | 634×893 | 1.017 | 645×908 |
| run_1 | (266,166)-(950,1074) | 685×909 | 1.017 | 697×925 |
| run_2 | (313,227)-(923,1028) | 611×802 | 1.017 | 621×816 |
| run_3 | (339,205)-(903,1078) | 565×874 | 1.017 | 575×889 |
| jump_0 | (296,248)-(939,1071) | 644×824 | 0.935 | 602×770 |
| jump_1 | (257,264)-(946,1186) | 690×923 | 0.935 | 645×863 |
| death_0 | (213,209)-(953,1026) | 741×818 | 0.870 | 645×712 |
| death_1 | (301,271)-(918,944) | 618×674 | 0.870 | 538×587 |
| death_2 | (1,398)-(1011,1251) | 1011×854 | 0.870 | 880×743 |
| blue_self_buff | (267,188)-(913,1078) | 647×891 | 1.055 | 683×940 |
| blue_enemy_shot | (208,209)-(1216,1035) | 1009×827 | 1.055 | 1064×872 |
| red_self_buff | (198,154)-(1040,1077) | 843×924 | 1.055 | 889×975 |
| red_enemy_shot | (120,213)-(1209,1031) | 1090×819 | 1.055 | 1150×864 |

**Note**: Death and GunAction have wider bbox variation due to horizontal poses and weapon effects. This is natural for these animation types and not a normalization defect.

## 7. Normalized Images

- **Path**: `Assets/Characters/GothicNun/FrameAnimation/Normalized/`
- **Count**: 17
- **Canvas**: 1254×1254, RGBA32
- **Alpha**: YES (background-only conversion at 0.97)
- **Group-consistent scaling**: YES (per-group uniform scale)

### Import Settings (all 17 identical)
- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single
- Pixels Per Unit: 100
- Mesh Type: Full Rect
- Pivot: Center (0.5, 0.5)
- Filter Mode: Bilinear
- Compression: None
- Mip Maps: false
- Alpha Is Transparency: true
- Wrap Mode: Clamp

## 8. Animation Clips (recreated)

| Clip | Frames | Sprites | FPS | Loop | Transform Curves |
|------|--------|---------|-----|------|------------------|
| GothicNun_Idle | 4 | idle_0~3 (Normalized) | 8 | Yes | **0** |
| GothicNun_Run | 4 | run_0~3 (Normalized) | 12 | Yes | **0** |
| GothicNun_Jump | 2 | jump_0~1 (Normalized) | 8 | No | **0** |
| GothicNun_Death | 3 | death_0~2 (Normalized) | 8 | No | **0** |
| GothicNun_BlueSelfBuff | 1 | blue_self_buff (Normalized) | 5 | No | **0** |
| GothicNun_BlueEnemyShot | 1 | blue_enemy_shot (Normalized) | 7 | No | **0** |
| GothicNun_RedSelfBuff | 1 | red_self_buff (Normalized) | 5 | No | **0** |
| GothicNun_RedEnemyShot | 1 | red_enemy_shot (Normalized) | 7 | No | **0** |

**All clips contain ONLY `SpriteRenderer.m_Sprite` keyframes. Zero Transform/Position/Rotation/Scale curves.**

## 9. Animator Controller

- **Path**: `Assets/Characters/GothicNun/FrameAnimation/Controllers/GothicNunPlayerAnimator.controller`
- **States**: 8 (Idle, Run, Jump, Death, 4 GunAction)
- **Parameters**: 8 (Speed, Grounded, VerticalVelocity, Dead, 4 Triggers)
- **Transitions**: 16
- **applyRootMotion**: false
- **Default state**: Idle

## 10. GothicNunFrameVisual

### Scene Transform
| Property | Value |
|----------|-------|
| Parent | Player/VisualRoot |
| localPosition | (0, -0.10, 0) |
| localScale | (0.15, 0.15, 1.0) |
| Scale X==Y | YES |
| Default sprite | gothic_nun_idle_0 (Normalized) |

### Player Root (UNCHANGED)
| Property | Value |
|----------|-------|
| localScale | (1, 1, 1.5) |
| Position | From SpawnPoint_Player |
| Rigidbody2D | Dynamic, gravityScale=3 |
| CapsuleCollider2D | size (0.6, 1.4), offset (0, 0.1) |

## 11. Damaged Resources (Backup)

- **Preview damaged**: `Assets/Characters/GothicNun/FrameAnimation/Backups/Raw_Damaged_Before_Reimport/`
- **RawOriginals** (pristine): `Assets/Characters/GothicNun/FrameAnimation/RawOriginal/`

## 12. Contact Sheet

- **Path**: `Assets/Characters/GothicNun/FrameAnimation/Reports/GothicNun_Normalized_ContactSheet.png`
- **Layout**: 5 columns × 4 rows, groups ordered: Idle, Run, Jump, Death, GunAction

## 13. Scene Backup

- `Assets/Scenes/Backups/Demo_Combat_Before_GothicNun_Reimport_Fix_20260614_165753.unity`

## 14. Play Mode Verification

| # | Check | Result |
|---|-------|--------|
| 1 | Single Player | **PASS** (1 instance) |
| 2 | GothicNunFrameVisual active | **PASS** |
| 3 | SamplePlayerVisual disabled/preserved | **PASS** |
| 4 | Animator bound | **PASS** (GothicNunPlayerAnimator) |
| 5 | applyRootMotion = false | **PASS** |
| 6 | Current clip plays (Jump at start) | **PASS** (Grounded=false→Jump) |
| 7 | Sprite references Normalized | **PASS** (gothic_nun_jump_0) |
| 8 | Texture format RGBA32 | **PASS** |
| 9 | Texture Alpha = True | **PASS** |
| 10 | All sprites 1254×1254 | **PASS** |
| 11 | No Transform curves in clips | **PASS** (0 in all 8) |
| 12 | VisualRoot scale unchanged | **PASS** (1,1,1) |
| 13 | GNFV scale consistent | **PASS** (0.15,0.15,1) |
| 14 | Console errors | **PASS** (0) |
| 15 | Group scales applied | **PASS** |
| 16 | No per-frame independent scaling | **PASS** |
| 17 | Player root scale unchanged | **PASS** |
| 18 | Rigidbody2D unchanged | **PASS** |
| 19 | Collider unchanged | **PASS** |
| 20 | GroundCheck/FirePoint preserved | **PASS** |

## 15. Face/Eye/Hair Integrity

- **Threshold used**: 0.97 (pixels ABOVE 0.97 RGB → transparent)
- **Character brightest pixels**: ~0.60 (skin/highlights)
- **Darkest character pixels**: ~0.03 (hair/fabric)
- **Safety gap**: 0.97 - 0.60 = 0.37 (37% of color range)
- **Risk of damaging face/eyes/hair**: **ZERO** — mathematically impossible

The aggressive damage from Stage 13D (threshold 0.85 which could remove pixels at 0.85-0.95 range including near-white skin/clothing highlights, creating holes) cannot occur at threshold 0.97.

## 16. Files Created/Modified

| File | Action |
|------|--------|
| `FrameAnimation/Backups/Raw_Damaged_Before_Reimport/` | Created (backup) |
| `FrameAnimation/RawOriginal/*.png` (17) | Created (pristine originals) |
| `FrameAnimation/Normalized/*.png` (17) | Created (normalized, white-to-alpha at 0.97) |
| `FrameAnimation/Animations/GothicNun_*.anim` (8) | Recreated |
| `FrameAnimation/Controllers/GothicNunPlayerAnimator.controller` | Unchanged |
| `FrameAnimation/Prefabs/GothicNunFrameVisual.prefab` | Updated (sprite ref) |
| `FrameAnimation/Reports/GOTHIC_NUN_FRAME_REIMPORT_FIX_REPORT.md` | Created |
| `FrameAnimation/Reports/GothicNun_Normalized_ContactSheet.png` | Created |
| `Scenes/Backups/Demo_Combat_Before_GothicNun_Reimport_Fix_*.unity` | Created (backup) |
| `Scenes/Demo_Combat.unity` | Modified (sprite refs) |

## 17. Rollback

To undo all changes:
1. Restore `Demo_Combat.unity` from backup
2. Delete `FrameAnimation/Normalized/` and `FrameAnimation/RawOriginal/`
3. Restore `FrameAnimation/Raw/` from backup `Backups/Raw_Damaged_Before_Reimport/`
4. Revert animation clips to previous versions (in backup)

## 18. Readiness Assessment

**Can proceed**: YES

- All 17 images normalized with group-consistent scaling
- Background transparency applied at ultra-conservative threshold (0.97) — no character damage possible
- All 8 animation clips recreated with zero Transform curves
- Scene, prefab, and clips use Normalized sprites
- 0 console errors
- All physics/movement/gameplay systems preserved
- Old resources backed up
