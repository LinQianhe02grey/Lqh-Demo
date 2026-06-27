# Gothic Nun Rig Report (Rebuilt)

Generated: 2026-06-13 18:16:46

## 1. Baseline Source

**Assembly prefab used**: `Assets/Characters/GothicNun/Prefabs/GothicNun_Assembly.prefab`

The Assembly prefab contains hand-adjusted sprite positions. All Rig sprites preserve these exact world positions.

## 2. Skeleton Hierarchy

```
GothicNun_Rig (SortingGroup: Character)
└─ Root (0.00, 0.00)
   └─ Pelvis (0.30, 1.80)
      ├─ TorsoJoint (0.15, 2.50)
      │  ├─ NeckJoint (0.00, 3.00)
      │  │  └─ HeadJoint (0.00, 3.28)
      │  │     └─ Head [order=60]
      │  ├─ Shoulder_L (2.18, 2.99)
      │  │  └─ Elbow_L (2.20, 2.17)
      │  │     └─ Wrist_L (2.18, 1.63)
      │  │        └─ LeftHand [order=51]
      │  │     └─ LeftForearm [order=43]
      │  │  └─ LeftUpperArm [order=41]
      │  ├─ Shoulder_R (-0.20, 2.99)
      │  │  └─ Elbow_R (-1.00, 2.20)
      │  │     └─ Wrist_R (-1.60, 1.63)
      │  │        └─ RightHand [order=50]
      │  │     └─ RightForearm [order=42]
      │  │  └─ RightUpperArm [order=40]
      │  └─ Torso [order=30]
      ├─ Hip_L (1.50, 1.55)
      │  └─ Knee_L (1.55, 0.15)
      │     └─ Ankle_L (1.59, -0.50)
      │        └─ LeftFoot [order=24]
      │     └─ (no calf sprite)
      │  └─ LeftThigh [order=22]
      ├─ Hip_R (0.30, 1.47)
      │  └─ Knee_R (0.60, 0.10)
      │     └─ Ankle_R (0.60, -0.50)
      │        └─ RightFoot [order=23]
      │     └─ (no calf sprite)
      │  └─ RightThigh [order=21]
      └─ Hip [order=20]
```

## 3. Sprite Bindings

| Sprite | Joint | Local Position | World Position (preserved) |
|--------|-------|---------------|---------------------------|
| Head | HeadJoint | (0.000, -3.280) | (0.000, 0.000) |
| Torso | TorsoJoint | (-0.004, -1.369) | (0.146, 1.131) |
| Hip | Pelvis | (-1.905, 0.351) | (-1.605, 2.151) |
| RightUpperArm | Shoulder_R | (-0.449, -1.761) | (-0.649, 1.229) |
| RightForearm | Elbow_R | (-0.649, -1.624) | (-1.649, 0.576) |
| RightHand | Wrist_R | (-0.015, -0.474) | (-1.615, 1.156) |
| LeftUpperArm | Shoulder_L | (0.001, -2.049) | (2.181, 0.941) |
| LeftForearm | Elbow_L | (0.079, -1.081) | (2.279, 1.089) |
| LeftHand | Wrist_L | (0.000, -0.132) | (2.180, 1.498) |
| RightThigh | Hip_R | (0.328, 0.245) | (0.628, 1.715) |
| RightFoot | Ankle_R | (0.001, 2.830) | (0.601, 2.330) |
| LeftThigh | Hip_L | (0.013, 0.399) | (1.513, 1.949) |
| LeftFoot | Ankle_L | (0.002, 2.781) | (1.592, 2.281) |

## 4. Joint Positions

Joint positions computed from sprite pixel bound analysis (PPU=100):

| Joint | World Position | Basis |
|-------|---------------|-------|
| Pelvis | (0.30, 1.80) | Between torso and hips |
| TorsoJoint | (0.15, 2.50) | Center of torso content |
| NeckJoint | (0.00, 3.00) | Between head and torso |
| HeadJoint | (0.00, 3.28) | Head rotation center |
| Shoulder_L | (2.18, 2.99) | Top of LeftUpperArm content |
| Elbow_L | (2.20, 2.17) | UpperArm bottom ~ Forearm top |
| Wrist_L | (2.18, 1.63) | Forearm bottom ~ Hand top |
| Shoulder_R | (-0.20, 2.99) | Top of RightUpperArm content |
| Elbow_R | (-1.00, 2.20) | UpperArm bottom ~ Forearm top |
| Wrist_R | (-1.60, 1.63) | Forearm bottom ~ Hand top |
| Hip_L | (1.50, 1.55) | Top of LeftThigh content |
| Knee_L | (1.55, 0.15) | Thigh bottom ~ Foot top |
| Ankle_L | (1.59, -0.50) | Foot pivot area |
| Hip_R | (0.30, 1.47) | Top of RightThigh content |
| Knee_R | (0.60, 0.10) | Thigh bottom ~ Foot top |
| Ankle_R | (0.60, -0.50) | Foot pivot area |

## 5. Rotation Test Results

| Joint | Test Angle | Result |
|-------|-----------|--------|
| HeadJoint | ±10° | PASS |
| Shoulder_L | ±10° | PASS |
| Elbow_L | ±15° | PASS |
| Shoulder_R | ±10° | PASS |
| Elbow_R | ±15° | PASS |
| Hip_L | ±8° | PASS |
| Knee_L | ±12° | PASS |
| Hip_R | ±8° | PASS |
| Knee_R | ±12° | PASS |

## 6. Integrity Check

- All 13 sprites preserve exact baseline world positions: **PASS**
- All rotations reset to zero after tests: **PASS**
- Default pose matches Assembly: **PASS**
- No sprites at UnassignedParts: **PASS** (13/13 assigned)
- SortingLayer (Character) preserved: **PASS**
- SortingOrder preserved: **PASS**

## 7. Missing Parts

- No calf/shin sprites (LeftCalf, RightCalf) → Knee→Ankle chain bare
- No hair sprites (front/back hair)
- No veil/headpiece sprites
- No shoulder armor sprites
- No facial feature sprites
- No reference image available

## 8. Gap Analysis

| Area | Severity | Notes |
|------|----------|-------|
| Knee joints (L/R) | HIGH | Thigh directly to foot; no calf to fill gap during rotation |
| Shoulder joints (L/R) | MEDIUM | Arm-to-torso connection may gap at extreme angles |
| Elbow joints (L/R) | LOW | Good overlap between upper/lower arm sprites |
| Hip joints (L/R) | LOW | Thigh-to-pelvis connection has reasonable coverage |

## 9. Files Modified/Created

| File | Action |
|------|--------|
| `GothicNun_Rig.prefab` | Rebuilt from Assembly baseline |
| `GothicNun_RigTest.unity` | Rebuilt with rig instance + pose test |
| `GothicNunRigPoseTest.cs` | Existing, verified |
| `GothicNun_Assembly.prefab` | NOT modified |
| `GOTHIC_NUN_RIG_REPORT.md` | Updated |

## 10. Console Compilation Status

- No new compilation errors
- No runtime errors during rig creation or testing

## 11. Readiness Assessment

**Can proceed to Idle animation stage?** YES

Caveats:
- Knee joints will expose large gaps during rotation (no calf sprites)
- Joint positions estimated from pixel bounds; artist review recommended
- Add calf/shin sprites before Walk/Attack animation

