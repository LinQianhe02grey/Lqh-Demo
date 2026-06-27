using UnityEngine;

namespace Cardwin.Boss
{
    /// <summary>
    /// Stage 46 — single source of truth for the Mirror Angel boss visual facing and the
    /// BeamOrigin mirror. ONLY the Body sprite (flipX) and the BeamOrigin localPosition are
    /// mirrored; the root GameObject / Rigidbody2D / Collider2D are NEVER flipped, so the
    /// physics body and hurtbox stay put.
    ///
    /// Other scripts must drive facing exclusively through this controller:
    ///   - locomotion (mover): FaceMoveDirection / FaceTarget while !IsFacingLocked
    ///   - skill (cast):       LockFacing(sign) ... UnlockFacing()
    /// This removes the previous "two writers" bug where the mover faced the player while
    /// movement (patrol / dash / post-cast) went the other way, making the boss walk
    /// backwards. Animation clips only swap the sprite (no flipX / scale curves), so the
    /// Animator can never re-mirror the body.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MirrorAngelFacingController : MonoBehaviour
    {
        [Header("Visual (Body only — never the root)")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private SpriteRenderer bodySpriteRenderer;
        [Tooltip("Prefer SpriteRenderer.flipX. If false, mirror visualRoot.localScale.x instead.")]
        [SerializeField] private bool useSpriteFlipX = true;
        [Tooltip("The MirrorAngel source art faces RIGHT by default.")]
        [SerializeField] private bool artDefaultFacesRight = true;
        [Tooltip("Flip the visual result if the art ends up mirrored the wrong way.")]
        [SerializeField] private bool invertVisualFacing = false;

        [Header("Beam Origin")]
        [SerializeField] private Transform beamOrigin;
        [Tooltip("Flip which side the BeamOrigin sits on, if the mirror is on the other side.")]
        [SerializeField] private bool invertBeamOriginSide = false;

        [Header("Facing")]
        [Tooltip("Below this |dx| / |moveX| the facing is left unchanged (avoids jitter).")]
        [SerializeField] private float facingDeadZone = 0.05f;

        private int _facingSign = 1;          // +1 = face right, -1 = face left
        private bool _isFacingLocked;
        private Vector3 _visualBaseScale = Vector3.one;
        private Vector3 _beamOriginBaseLocalPos;
        private bool _initialized;

        public bool IsFacingLocked => _isFacingLocked;
        public int CurrentFacingSign => _facingSign;

        private void Awake()
        {
            EnsureRefs();
        }

        private void Start()
        {
            // Initialise the visual + beam origin to the current sign so nothing is left
            // in a stale mirrored state from edit time / a previous run.
            ApplyVisual();
            ApplyBeamOrigin();
        }

        private void EnsureRefs()
        {
            if (_initialized)
                return;
            if (visualRoot == null)
                visualRoot = transform.Find("Body");
            if (bodySpriteRenderer == null && visualRoot != null)
                bodySpriteRenderer = visualRoot.GetComponent<SpriteRenderer>();
            if (beamOrigin == null)
                beamOrigin = transform.Find("BeamOrigin");
            if (visualRoot != null)
            {
                _visualBaseScale = visualRoot.localScale;
                _visualBaseScale.x = Mathf.Abs(_visualBaseScale.x);
            }
            if (beamOrigin != null)
                _beamOriginBaseLocalPos = beamOrigin.localPosition;
            _initialized = true;
        }

        /// <summary>Facing sign (+1 right / -1 left) the boss WOULD use to face a target now.</summary>
        public int GetFacingToTarget(Transform target)
        {
            if (target == null)
                return _facingSign;
            float dx = target.position.x - transform.position.x;
            if (Mathf.Abs(dx) < facingDeadZone)
                return _facingSign;
            return dx > 0f ? 1 : -1;
        }

        /// <summary>Face a world target (ignored while locked).</summary>
        public void FaceTarget(Transform target)
        {
            if (_isFacingLocked || target == null)
                return;
            float dx = target.position.x - transform.position.x;
            if (Mathf.Abs(dx) < facingDeadZone)
                return;
            SetFacing(dx > 0f ? 1 : -1);
        }

        /// <summary>Face the current move direction (ignored while locked). Preferred during walk.</summary>
        public void FaceMoveDirection(float moveX)
        {
            if (_isFacingLocked)
                return;
            if (Mathf.Abs(moveX) < facingDeadZone)
                return;
            SetFacing(moveX > 0f ? 1 : -1);
        }

        /// <summary>Set the absolute facing sign and apply it to the body + beam origin.</summary>
        public void SetFacing(int sign)
        {
            EnsureRefs();
            _facingSign = sign >= 0 ? 1 : -1;
            ApplyVisual();
            ApplyBeamOrigin();
        }

        /// <summary>Lock facing to a sign (skill cast). Movement-driven facing is suspended.</summary>
        public void LockFacing(int sign)
        {
            SetFacing(sign);
            _isFacingLocked = true;
        }

        /// <summary>Release the facing lock so locomotion controls facing again.</summary>
        public void UnlockFacing()
        {
            _isFacingLocked = false;
        }

        private void ApplyVisual()
        {
            bool faceRight = _facingSign > 0;
            if (useSpriteFlipX && bodySpriteRenderer != null)
            {
                bool flip = artDefaultFacesRight ? !faceRight : faceRight;
                if (invertVisualFacing) flip = !flip;
                bodySpriteRenderer.flipX = flip;
                // Defensive: never leave the body mirrored via scale (legacy state).
                if (visualRoot != null && visualRoot.localScale.x < 0f)
                {
                    Vector3 s = visualRoot.localScale;
                    s.x = Mathf.Abs(s.x);
                    visualRoot.localScale = s;
                }
            }
            else if (visualRoot != null)
            {
                float visualSign = artDefaultFacesRight ? _facingSign : -_facingSign;
                if (invertVisualFacing) visualSign = -visualSign;
                Vector3 s = _visualBaseScale;
                s.x = _visualBaseScale.x * visualSign;
                visualRoot.localScale = s;
            }
        }

        private void ApplyBeamOrigin()
        {
            if (beamOrigin == null)
                return;
            int sideSign = invertBeamOriginSide ? -_facingSign : _facingSign;
            Vector3 p = _beamOriginBaseLocalPos;
            p.x = Mathf.Abs(_beamOriginBaseLocalPos.x) * sideSign;
            beamOrigin.localPosition = p;
        }
    }
}
