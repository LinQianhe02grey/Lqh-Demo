using UnityEngine;

namespace OrbitBreaker
{
    [DisallowMultipleComponent, RequireComponent(typeof(OrbitBreakerPlayerMotor), typeof(MeshRenderer))]
    public sealed class OrbitBreakerPlayerFeedback : MonoBehaviour
    {
        public const float ImpactFlashDuration = 0.12f;
        public const float MinimumImpactSpeed = 2f;
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private OrbitBreakerPlayerMotor motor;
        private MeshRenderer mesh;
        private MaterialPropertyBlock properties;
        private MaterialPropertyBlock originalProperties;
        private float impactUntil;
        private bool showingFeedback;
        private Material orbitMaterial;
        private TrailRenderer orbitTrail;
        private LineRenderer orbitArc;
        private readonly Vector3[] arcPoints = new Vector3[25];
        private float previousAngle, spinGlow;
        private bool wasAttached;
        public bool OrbitVfxActive => orbitArc != null && orbitArc.enabled;
        public int OrbitTrailPoints => orbitTrail == null ? 0 : orbitTrail.positionCount;
        public bool OrbitTrailVisible => orbitTrail != null && orbitTrail.enabled;
        public bool IsImpactFlashing => motor != null && motor.OrbitBreakerGameManager != null &&
            motor.OrbitBreakerGameManager.IsPlaying && Time.time < impactUntil;

        private void Awake()
        {
            motor = GetComponent<OrbitBreakerPlayerMotor>();
            mesh = GetComponent<MeshRenderer>();
            properties = new MaterialPropertyBlock();
            originalProperties = new MaterialPropertyBlock();
            mesh.GetPropertyBlock(originalProperties);
        }

        private void OnEnable() => motor.Impact += OnImpact;
        private void OnDisable()
        {
            motor.Impact -= OnImpact;
            impactUntil = 0f;
            Restore();
            ClearOrbitVfx();
        }

        private void OnImpact(Collision collision, bool wasDashing)
        {
            if (wasDashing || collision.relativeVelocity.sqrMagnitude >= MinimumImpactSpeed * MinimumImpactSpeed)
                impactUntil = Time.time + ImpactFlashDuration;
        }

        private void LateUpdate()
        {
            UpdateOrbitVfx();
            if (motor.OrbitBreakerGameManager == null || !motor.OrbitBreakerGameManager.IsPlaying) impactUntil = 0f;
            if (!IsImpactFlashing && !motor.IsDashing && !motor.IsStunned && !motor.IsInvulnerable && !motor.IsAttached) { Restore(); return; }
            mesh.GetPropertyBlock(properties);
            properties.SetColor(ColorId, motor.IsStunned ? new Color(1f, 0.18f, 0.3f) :
                IsImpactFlashing ? new Color(1f, 0.65f, 0.12f) :
                motor.IsAttached ? Color.Lerp(new Color(0.12f, 1f, 0.65f), Color.white, motor.AttachmentCharge) :
                motor.IsDashing ? new Color(0.25f, 1f, 1f) : new Color(0.5f, 0.7f, 1f));
            mesh.SetPropertyBlock(properties);
            showingFeedback = true;
        }

        private void CreateOrbitVfx()
        {
            if (orbitArc != null) return;
            // Runtime-only, unlit geometry: no collider, physics writer or imported package.
            orbitMaterial = new Material(Shader.Find("Sprites/Default")) { hideFlags = HideFlags.DontSave };
            var arc = new GameObject("OrbitEnergyArc") { hideFlags = HideFlags.DontSave };
            arc.transform.SetParent(transform, false);
            orbitArc = arc.AddComponent<LineRenderer>();
            orbitArc.sharedMaterial = orbitMaterial;
            orbitArc.useWorldSpace = true;
            orbitArc.positionCount = arcPoints.Length;
            orbitArc.numCapVertices = 4;
            orbitArc.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            orbitArc.receiveShadows = false;
            var trail = new GameObject("OrbitEnergyTrail") { hideFlags = HideFlags.DontSave };
            trail.transform.SetParent(transform, false);
            trail.transform.localPosition = new Vector3(0f, 0f, -0.15f);
            orbitTrail = trail.AddComponent<TrailRenderer>();
            orbitTrail.sharedMaterial = orbitMaterial;
            orbitTrail.time = 0.38f;
            orbitTrail.minVertexDistance = 0.15f;
            orbitTrail.numCapVertices = 4;
            orbitTrail.numCornerVertices = 4;
            orbitTrail.alignment = LineAlignment.TransformZ;
            orbitTrail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            orbitTrail.receiveShadows = false;
            orbitTrail.emitting = false;
        }

        private void UpdateOrbitVfx()
        {
            if (!motor.IsAttached) { ClearOrbitVfx(); return; }
            CreateOrbitVfx();
            Vector2 center = motor.CurrentAttachment.Center;
            Vector2 radial = (Vector2)motor.Position - center;
            float angle = Mathf.Atan2(radial.y, radial.x) * Mathf.Rad2Deg;
            float angularSpeed = wasAttached ? Mathf.Abs(Mathf.DeltaAngle(previousAngle, angle)) / Mathf.Max(Time.deltaTime, 0.001f) : 0f;
            if (!wasAttached) orbitTrail.Clear();
            wasAttached = true;
            previousAngle = angle;
            spinGlow = Mathf.MoveTowards(spinGlow, Mathf.Clamp01(angularSpeed / 180f), Time.deltaTime * 6f);
            Color glow = Color.Lerp(new Color(0.1f, 1f, 0.8f), new Color(1f, 0.9f, 0.35f), motor.AttachmentCharge);
            orbitArc.enabled = true;
            orbitArc.startWidth = 0.025f;
            orbitArc.endWidth = 0.07f + spinGlow * 0.055f;
            orbitArc.startColor = new Color(glow.r, glow.g, glow.b, 0.02f);
            orbitArc.endColor = new Color(glow.r, glow.g, glow.b, 0.3f + spinGlow * 0.65f);
            for (int i = 0; i < arcPoints.Length; i++)
            {
                float phase = (angle - 135f + 120f * i / (arcPoints.Length - 1)) * Mathf.Deg2Rad;
                arcPoints[i] = new Vector3(center.x + Mathf.Cos(phase) * (OrbitBreakerAttachmentPoint.OrbitRadius + 0.12f),
                    center.y + Mathf.Sin(phase) * (OrbitBreakerAttachmentPoint.OrbitRadius + 0.12f), -0.3f);
            }
            orbitArc.SetPositions(arcPoints);
            orbitTrail.emitting = angularSpeed > 25f;
            orbitTrail.enabled = true;
            orbitTrail.startWidth = 0.16f + motor.AttachmentCharge * 0.08f;
            orbitTrail.endWidth = 0f;
            orbitTrail.startColor = new Color(glow.r, glow.g, glow.b, 0.85f);
            orbitTrail.endColor = new Color(glow.r, glow.g, glow.b, 0f);
        }

        private void ClearOrbitVfx()
        {
            if (orbitArc != null) orbitArc.enabled = false;
            if (orbitTrail != null) { orbitTrail.emitting = false; orbitTrail.enabled = false; if (wasAttached) orbitTrail.Clear(); }
            wasAttached = false;
            spinGlow = 0f;
        }

        private void OnDestroy() { if (orbitMaterial != null) Destroy(orbitMaterial); }

        private void Restore()
        {
            if (!showingFeedback || mesh == null) return;
            mesh.SetPropertyBlock(originalProperties.isEmpty ? null : originalProperties);
            showingFeedback = false;
        }
    }
}
