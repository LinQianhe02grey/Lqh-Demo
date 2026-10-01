using System;
using System.Collections.Generic;
using UnityEngine;

namespace OrbitBreaker
{
    [DefaultExecutionOrder(-50), DisallowMultipleComponent, RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public sealed class OrbitBreakerPlayerMotor : MonoBehaviour
    {
        public const float MaximumSpeed = 24f;
        public const int MaximumShells = 3;
        public int DashCapacity => gameManager==null || gameManager.Config==null ? MaximumDashCharges : Mathf.Clamp(gameManager.Config.maxDashCharges,1,6);
        private float rewardEndsAt;
        public const float RewardDuration=7f, RewardSpeedMultiplier=1.35f;
        public float RewardRemaining => gameManager!=null && gameManager.IsPlaying ? Mathf.Max(0,rewardEndsAt-Time.time) : 0;
        public bool HasRewardBoost => RewardRemaining>0;
        public bool IsSlowed => CanControl && !IsAttached && OrbitBreakerSlimeAura.Contains(Position);
        public float MovementFactor => IsSlowed ? OrbitBreakerSlimeAura.SpeedFactor : 1f;
        public float LaunchSpeedFactor => HasRewardBoost ? RewardSpeedMultiplier : 1f;
        public OrbitBreakerEncounterConfig Rules => gameManager.Rules;
        public float BaseSpeedLimit => Rules == null ? MaximumSpeed : Rules.maximumSpeed;
        public float DamageThreshold => Rules == null ? 12f : Rules.damageThreshold;
        public float CurrentSpeed => body == null || body.isKinematic ? 0 : ((Vector2)body.velocity).magnitude;
        public float CurrentSpeedLimit => BaseSpeedLimit*LaunchSpeedFactor*(IsDashing || !IsLaunched ? MovementFactor : 1f);
        public bool IsPiercing => IsAttackActive && CurrentSpeed >= BaseSpeedLimit*LaunchSpeedFactor-.15f;
        public Vector2 IncomingVelocity { get; private set; }
        public float ImpactSpeed { get; private set; }
        public int ImpactDamage => DamageForSpeed(ImpactSpeed);
        public int DamageForSpeed(float speed)
        {
            if(speed<=DamageThreshold)return 0;
            float min=Rules==null?24:Rules.thresholdDamage, max=Rules==null?80:Rules.maximumSpeedDamage;
            return Mathf.RoundToInt(Mathf.LerpUnclamped(min,max,(speed-DamageThreshold)/(BaseSpeedLimit-DamageThreshold))*ComboMultiplier);
        }
        private bool kineticFlight, bossBounceQueued;
        private bool dashCruise, burstArmed;
        private float activeDashSpeed;
        public int DashBurstCount {get;private set;}
        public const float FocusDuration=2f, FocusScale=.2f;
        private bool focusActive;
        private float focusEnd, savedTimeScale=1, savedFixedStep=.02f;
        public bool IsFocusActive=>focusActive;
        public float FocusRemaining=>focusActive?Mathf.Max(0,focusEnd-Time.unscaledTime):0;
        public bool TryBeginFocus()
        {
            if(!CanControl || focusActive)return false;
            focusActive=true;focusEnd=Time.unscaledTime+FocusDuration;
            savedTimeScale=Time.timeScale;savedFixedStep=Time.fixedDeltaTime;
            Time.timeScale=FocusScale;Time.fixedDeltaTime=savedFixedStep*FocusScale;
            return true;
        }
        public void EndFocus()
        {
            if(!focusActive)return;
            focusActive=false;Time.timeScale=savedTimeScale;Time.fixedDeltaTime=savedFixedStep;
        }
        private void Update(){if(focusActive && (!CanControl || Time.unscaledTime>=focusEnd))EndFocus();}
        public bool TryDashBurst(Vector3 center,int impactDamage)
        {
            if(!IsDashing || !burstArmed || impactDamage<=0)return false;
            burstArmed=false;DashBurstCount++;
            int extra=Mathf.Max(1,Mathf.RoundToInt(impactDamage*OrbitBreakerDashBurst.DamageFactor));
            // Snapshot: a lethal explosion can remove an enemy from the registry.
            foreach(var enemy in new List<OrbitBreakerEnemyController>(OrbitBreakerEnemyController.Active))
                if(enemy!=null && Vector2.Distance(enemy.Position,center)<=OrbitBreakerDashBurst.Radius)
                    enemy.TryExplosionDamage(this,extra,center);
            var boss=FindObjectOfType<OrbitBreakerOctopusBoss>();
            if(boss!=null && Vector2.Distance(boss.transform.position,center)<=OrbitBreakerDashBurst.Radius+OrbitBreakerOctopusBoss.Radius)
                boss.TryExplosionDamage(extra);
            OrbitBreakerDashBurst.Spawn(center);return true;
        }
        private Vector2 bossBounceVelocity;
        private readonly Collider[] attackContacts=new Collider[64];
        public bool QueueBossRebound(Vector2 incoming)
        {
            if(!gameManager.IsPlaying || incoming.magnitude<=DamageThreshold)return false;
            ReleaseAttachment();CancelPendingDash();boardImpulseQueued=false;
            dashCruise=false;dashEndsAt=0;
            bossBounceVelocity=-incoming.normalized*Mathf.Min(Mathf.Max(incoming.magnitude,DamageThreshold+2),BaseSpeedLimit*LaunchSpeedFactor);
            bossBounceQueued=true;return true;
        }
        private void PrepareSpeedContacts()
        {
            IncomingVelocity=body.velocity;
            foreach(var enemy in OrbitBreakerEnemyController.Active)enemy.SetPlayerPassThrough(IsAttached||IsPiercing);
            if(!IsPiercing || IsAttached)return;
            int count=Physics.OverlapCapsuleNonAlloc(body.position,body.position+body.velocity*Time.fixedDeltaTime,
                GetComponent<SphereCollider>().radius*transform.lossyScale.x,attackContacts,~0,QueryTriggerInteraction.Ignore);
            for(int i=0;i<count;i++)
            {
                var enemy=attackContacts[i].GetComponentInParent<OrbitBreakerEnemyController>();
                if(enemy!=null && enemy.TryDamage(this))RegisterDashContact(attackContacts[i],CurrentSpeed);
            }
        }
        public event Action ShellRestored, RewardStarted;
        public bool TryRestoreShell()
        {
            if(gameManager==null || !gameManager.IsPlaying || Shells<=0 || Shells>=MaximumShells)return false;
            Shells++;ShellRestored?.Invoke();return true;
        }
        public void GrantRewardBoost()
        {
            if(gameManager==null || !gameManager.IsPlaying)return;
            rewardEndsAt=Time.time+RewardDuration;RewardStarted?.Invoke();OrbitBreakerCombatAudio.Play(CombatSound.Reward);
        }
        public int Shells { get; private set; } = MaximumShells;
        public event Action ShellBroken;
        private float nextHoleDamageAt;
        public bool TryReceiveBlackHole(Vector2 center)
        {
            if (!isActiveAndEnabled || gameManager == null || !gameManager.IsPlaying || Time.time < nextHoleDamageAt || Shells <= 0) return false;
            nextHoleDamageAt = Time.time + 2f;
            ReleaseAttachment(); CancelPendingDash();
            jackpotEndsAt = dashEndsAt = launchEndsAt = 0f; combo = 0; comboEndsAt = 0f;
            boardImpulseQueued = hitQueued = false; stunnedUntil = 0f;
            invulnerableUntil = Time.time + 2f;
            Shells--; ShellBroken?.Invoke(); HitReceived?.Invoke();
            if (Shells == 0) { gameManager.LoseGame(); return true; }
            Vector2 away = (Vector2)Position-center;
            if (away.sqrMagnitude < .01f) away = Vector2.up;
            // Ejection is queued through the sole player physics writer, including offensive states.
            TryRequestBoardImpulse(away.normalized, 20f);
            return true;
        }
        public const float DashDuration = 0.3f;
        public const float LaunchProtection = 1.25f, ComboWindow = 8f;
        private float launchEndsAt, comboEndsAt, jackpotEndsAt;
        private Vector2 jackpotCenter;
        private bool magnetLaunchQueued;
        private int combo;
        public const int ContactsPerDash = 4, MaximumDashCharges = 3;
        public int DashCharges { get; private set; }
        public int DashContactProgress { get; private set; }
        public int TotalQualifiedContacts { get; private set; }
        private readonly Dictionary<int,float> contactTimes = new Dictionary<int,float>();
        private readonly HashSet<int> rewardedKills = new HashSet<int>();
        public event Action DashChargeEarned;
        internal void RegisterDashContact(Collider source, float speed)
        {
            if (gameManager == null || !gameManager.IsPlaying || source == null || speed < 2f) return;
            int id = source.GetInstanceID();
            if (contactTimes.TryGetValue(id, out float last) && Time.time - last < .22f) return;
            contactTimes[id] = Time.time; TotalQualifiedContacts++;
            if (++DashContactProgress < ContactsPerDash) return;
            DashContactProgress = 0; EarnDash();
        }
        internal void RewardEnemyKill(OrbitBreakerEnemyController enemy)
        {
            if (gameManager == null || !gameManager.IsPlaying || enemy == null || !enemy.IsDead || enemy.Health != 0 || !rewardedKills.Add(enemy.GetInstanceID())) return;
            EarnDash();
        }
        private void EarnDash()
        {
            if (DashCharges >= DashCapacity) return;
            DashCharges++; DashChargeEarned?.Invoke();
        }
        public int ComboCount => gameManager != null && gameManager.IsPlaying && Time.time < comboEndsAt ? combo : 0;
        public float ComboRemaining => ComboCount > 0 ? Mathf.Max(0f, comboEndsAt - Time.time) : 0f;
        public float ComboMultiplier => 1f + Mathf.Min(ComboCount, 8) * 0.25f;
        public bool IsLaunched => gameManager != null && gameManager.IsPlaying && Time.time < launchEndsAt;
        public bool IsInJackpot => gameManager != null && gameManager.IsPlaying && Time.time < jackpotEndsAt;
        public bool IsAttackActive => gameManager != null && gameManager.IsPlaying && CurrentSpeed > DamageThreshold;
        public int AttackDamage => DamageForSpeed(CurrentSpeed);
        private void RegisterLaunch() { combo = ComboCount + 1; comboEndsAt = Time.time + ComboWindow; launchEndsAt = Time.time + LaunchProtection; }
        public bool TryCaptureJackpot(Vector2 center)
        {
            if (!CanControl || IsInJackpot || body == null || body.isKinematic) return false;
            ReleaseAttachment(); CancelPendingDash(); boardImpulseQueued = false; dashEndsAt = launchEndsAt = 0f;
            jackpotCenter = center; jackpotEndsAt = Time.time + 0.65f; return true;
        }
        public const float HitDownwardSpeed = 6f;
        public const float RecoveryProtection = 0.35f;
        public const RigidbodyConstraints PlaneConstraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        [SerializeField] private OrbitBreakerGameManager gameManager;

        private Rigidbody body;
        private Quaternion lockedRotation;
        private Vector2 moveInput;
        private Vector2 lastDirection = Vector2.up;
        private Vector2 queuedDashDirection;
        private bool dashQueued;
        private float dashEndsAt;
        private float nextDashAt;
        private bool boardImpulseQueued;
        private Vector2 boardImpulseDirection;
        private float boardExitSpeed;
        private bool hitQueued;
        private float stunnedUntil;
        private float invulnerableUntil;
        public const float OrbitDegreesPerSecond = 360f;
        public const float FullChargeSpinTime = 0.6f;
        public const float ReattachCooldown = 0.5f;
        private OrbitBreakerAttachmentPoint attachment, blockedAttachment;
        private bool attached;
        private float attachmentEndsAt, nextAttachmentAt, spinTime, previousOrbitAngle;
        private Vector2 attachmentAim;
        private bool aimValid;
        private float queuedDashSpeed;
        private readonly Collider[] orbitContacts = new Collider[32];
        public event Action<bool> AttachmentChanged;
        public event Action OrbitRepelled;
        public float AttachmentCooldownRemaining => Mathf.Max(0f, nextAttachmentAt - Time.time);
        public bool MustLeaveAttachment => blockedAttachment != null;
        public bool IsAttached => attached && CanControl;
        public OrbitBreakerAttachmentPoint CurrentAttachment => IsAttached ? attachment : null;
        public float AttachmentRemaining => IsAttached ? Mathf.Max(0f, attachmentEndsAt - Time.time) : 0f;
        public float OrbitRampTime => Rules==null?2:Rules.orbitRampSeconds;
        public float AttachmentCharge => IsAttached ? Mathf.Clamp01(spinTime / OrbitRampTime) : 0f;
        public Vector2 AttachmentAim => attachmentAim;
        public bool HasAttachmentAim => aimValid;
        public bool IsStunned => isActiveAndEnabled && gameManager != null && gameManager.IsPlaying && Time.time < stunnedUntil;
        public bool IsInvulnerable => isActiveAndEnabled && gameManager != null && gameManager.IsPlaying && (Time.time < invulnerableUntil || IsAttackActive || IsInJackpot);
        public float StunRemaining => IsStunned ? Mathf.Max(0f, stunnedUntil - Time.time) : 0f;
        // Attachment suspends steering and is protected; jackpot capture owns movement separately.
        public bool CanControl => isActiveAndEnabled && gameManager != null && gameManager.IsPlaying && !IsStunned && !IsInJackpot;
        public event Action HitReceived;
        public OrbitBreakerGameManager OrbitBreakerGameManager => gameManager;
        public Vector2 MoveInput => moveInput;
        public Vector3 Position => body == null ? transform.position : body.position;
        public Vector2 DashDirection { get; private set; } = Vector2.up;
        public bool IsDashing => isActiveAndEnabled && gameManager != null && gameManager.IsPlaying && Time.time < dashEndsAt;
        public float DashCooldownRemaining => Mathf.Max(0f, nextDashAt - Time.time);
        public bool CanDash => isActiveAndEnabled && gameManager != null && gameManager.IsPlaying &&
            !IsStunned && !IsInJackpot && !dashQueued && !boardImpulseQueued && !IsDashing && DashCharges > 0 && (aimValid || (GetComponent<OrbitBreakerTargetLock>()!=null && GetComponent<OrbitBreakerTargetLock>().HasTarget)) && DashCooldownRemaining <= 0f;
        public event Action DashStarted;
        public event Action BoardImpulseApplied;
        // The bool preserves offensive-state qualification across collision callback order.
        public event Action<Collision, bool> Impact;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            lockedRotation = body.rotation;
            body.useGravity = false;
            body.constraints = PlaneConstraints;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.drag = 0f;
            var position = body.position;
            position.z = 0f;
            body.position = position;
            Freeze();
            if (gameManager == null || gameManager.Config == null)
            {
                Debug.LogError("OrbitBreakerPlayerMotor needs a OrbitBreakerGameManager with OrbitBreakerCombatConfig.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (gameManager == null || body == null) return;
            gameManager.StateChanged += OnGameStateChanged;
            OnGameStateChanged(gameManager.State);
        }

        private void OnDisable()
        {
            if (gameManager != null) gameManager.StateChanged -= OnGameStateChanged;
            if (body != null) Freeze();
            moveInput = Vector2.zero;
        }

        // Input, dash and later abilities request motion; only this component writes physics.
        public void SetMoveInput(Vector2 direction)
        {
            if (IsStunned || IsAttached) { moveInput = Vector2.zero; return; }
            moveInput = Vector2.ClampMagnitude(direction, 1f);
            if (moveInput.sqrMagnitude > 0.001f) lastDirection = moveInput.normalized;
        }

        public bool TryRequestDash()
        {
            if (!CanDash) return false;
            var target=GetComponent<OrbitBreakerTargetLock>();
            if(target!=null && target.HasTarget)SetAttachmentAim(target.Target.position);
            Vector2 mouseDirection = attachmentAim - (Vector2)body.position;
            if (mouseDirection.sqrMagnitude < .01f) return false;
            float carriedSpeed=CurrentSpeed;
            bool focusRedirect=IsFocusActive;
            if (IsAttached)
            {
                Vector2 aim = attachmentAim - (Vector2)body.position;
                if (!aimValid || aim.sqrMagnitude < 0.01f) return false;
                magnetLaunchQueued = true;
                queuedDashDirection = aim.normalized;
                queuedDashSpeed = Mathf.Lerp(Mathf.Min(gameManager.Config.dashSpeed, BaseSpeedLimit), BaseSpeedLimit, AttachmentCharge) * LaunchSpeedFactor;
                ReleaseAttachment();
            }
            else
            {
                magnetLaunchQueued = false;
                queuedDashDirection = mouseDirection.normalized;
                queuedDashSpeed = Mathf.Min(gameManager.Config.dashSpeed, BaseSpeedLimit) * LaunchSpeedFactor * MovementFactor;
            }
            // A focus redirect changes direction, not the speed the player had when pressing Space.
            queuedDashSpeed=focusRedirect?carriedSpeed:Mathf.Max(carriedSpeed,queuedDashSpeed);
            dashQueued = true;
            return true;
        }

        public void CancelPendingDash() { dashQueued = magnetLaunchQueued = false; }

        public void SetAttachmentAim(Vector2 point, bool valid = true)
        {
            aimValid = valid && !float.IsNaN(point.x) && !float.IsNaN(point.y) && !float.IsInfinity(point.x) && !float.IsInfinity(point.y);
            if (aimValid) attachmentAim = point;
        }

        public bool TryAttach(OrbitBreakerAttachmentPoint point)
        {
            if (!CanAttach(point)) return false;
            attachment = point;
            attached = true;
            attachmentEndsAt = Time.time + gameManager.Config.attachDuration;
            spinTime = 0f;
            Vector2 radial = (Vector2)body.position - point.Center;
            previousOrbitAngle = Mathf.Atan2(radial.y, radial.x) * Mathf.Rad2Deg;
            moveInput = Vector2.zero;
            CancelPendingDash();
            dashEndsAt = 0f;
            AttachmentChanged?.Invoke(true);
            return true;
        }

        public bool CanAttach(OrbitBreakerAttachmentPoint point)
        {
            return !(!CanControl || attached || body == null || body.isKinematic || point == null || !point.isActiveAndEnabled ||
                boardImpulseQueued || Time.time < nextAttachmentAt || point == blockedAttachment ||
                Vector2.Distance(body.position, point.Center) > OrbitBreakerAttachmentPoint.CaptureRadius);
        }

        public void ReleaseAttachment()
        {
            if (!attached) return;
            blockedAttachment = attachment;
            var used=attachment;
            nextAttachmentAt = Time.time + ReattachCooldown;
            attached = false;
            attachment = null;
            spinTime = 0f;
            AttachmentChanged?.Invoke(false);
            if(used!=null)used.Consume();
        }

        private bool UpdateAttachment()
        {
            if (!attached) return false;
            if (attachment == null || !attachment.isActiveAndEnabled || IsStunned || Time.time >= attachmentEndsAt ||
                Vector2.Distance(body.position, attachment.Center) > OrbitBreakerAttachmentPoint.CaptureRadius + 0.25f)
            { ReleaseAttachment(); return false; }
            Vector2 radial = (Vector2)body.position - attachment.Center;
            float angle = Mathf.Atan2(radial.y, radial.x) * Mathf.Rad2Deg;
            float traveled = Mathf.Abs(Mathf.DeltaAngle(previousOrbitAngle, angle));
            if(Mathf.Abs(radial.magnitude-OrbitBreakerAttachmentPoint.OrbitRadius)<.2f && traveled>.1f)
                spinTime=Mathf.Min(OrbitRampTime,spinTime+Time.fixedDeltaTime);
            previousOrbitAngle=angle;
            float targetSpeed=Mathf.Lerp(6f,BaseSpeedLimit*LaunchSpeedFactor,AttachmentCharge);
            // Chord angle gives the requested actual linear speed on the circular orbit.
            float step=2*Mathf.Asin(Mathf.Clamp(targetSpeed*Time.fixedDeltaTime/(2*OrbitBreakerAttachmentPoint.OrbitRadius),0,.95f));
            float next=angle*Mathf.Deg2Rad+step;
            Vector2 destination=attachment.Center+new Vector2(Mathf.Cos(next),Mathf.Sin(next))*OrbitBreakerAttachmentPoint.OrbitRadius;
            body.velocity=Vector2.ClampMagnitude((destination-(Vector2)body.position)/Time.fixedDeltaTime,targetSpeed);
            kineticFlight=true;
            PrepareSpeedContacts();
            // OrbitBreakerEnemy solid pairs are ignored only during attachment. Sweep the actual next step
            // separately so defense never pushes the player or misses a fast orbit contact.
            int count = Physics.OverlapCapsuleNonAlloc(body.position, body.position + body.velocity * Time.fixedDeltaTime,
                GetComponent<SphereCollider>().radius * transform.lossyScale.x + 0.06f, orbitContacts, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var enemy = orbitContacts[i].attachedRigidbody == null ? null : orbitContacts[i].attachedRigidbody.GetComponent<OrbitBreakerEnemyController>();
                if (enemy != null && enemy.TryOrbitRepel(this)) OrbitRepelled?.Invoke();
            }
            return true;
        }

        // Accept state immediately so multiple contacts in the same step cannot stack hits.
        // Physics is deferred to this motor's FixedUpdate, before any queued rescue launch.
        public bool TryReceiveHit()
        {
            if (!CanControl || IsInvulnerable || body == null || body.isKinematic) return false;
            Shells--; ShellBroken?.Invoke();
            if (Shells == 0) { HitReceived?.Invoke(); gameManager.LoseGame(); return true; }
            ReleaseAttachment();
            combo = 0; comboEndsAt = launchEndsAt = 0f;
            stunnedUntil = Time.time + gameManager.Config.stunDuration;
            invulnerableUntil = stunnedUntil + RecoveryProtection;
            hitQueued = true;
            CancelPendingDash();
            dashEndsAt = 0f;
            moveInput = Vector2.zero;
            HitReceived?.Invoke();
            return true;
        }

        // Board contacts can rescue a falling ball independently of movement/dash input.
        // One accepted launch per physics step prevents overlapping devices from stacking kicks.
        public bool TryRequestBoardImpulse(Vector2 direction, float minimumExitSpeed)
        {
            if (!isActiveAndEnabled || gameManager == null || !gameManager.IsPlaying || body.isKinematic ||
                IsInJackpot || boardImpulseQueued || direction.sqrMagnitude < 0.001f || minimumExitSpeed <= 0f) return false;
            boardImpulseDirection = direction.normalized;
            ReleaseAttachment();
            boardExitSpeed = Mathf.Min(minimumExitSpeed, BaseSpeedLimit) * LaunchSpeedFactor;
            boardImpulseQueued = true;
            CancelPendingDash();
            dashEndsAt = 0f;
            return true;
        }

        private void FixedUpdate()
        {
            if (gameManager == null || !gameManager.IsPlaying || body.isKinematic) return;
            DashCharges=Mathf.Min(DashCharges,DashCapacity);
            if(bossBounceQueued){bossBounceQueued=false;body.velocity=bossBounceVelocity;kineticFlight=true;RegisterLaunch();PrepareSpeedContacts();return;}
            var config = gameManager.Config;
            if (jackpotEndsAt > 0f)
            {
                if (IsInJackpot) { body.velocity = Vector2.ClampMagnitude((jackpotCenter - (Vector2)body.position) * 12f, 12f); return; }
                jackpotEndsAt = 0f; TryRequestBoardImpulse(Vector2.down, 18f);
            }
            // Keep the sphere orientation stable even after externally requested torque impulses.
            body.angularVelocity = Vector3.zero;
            body.rotation = lockedRotation;
            if (blockedAttachment != null && Vector2.Distance(body.position, blockedAttachment.Center) > OrbitBreakerAttachmentPoint.CaptureRadius + 0.25f)
                blockedAttachment = null;
            if (UpdateAttachment()) return;
            if (hitQueued)
            {
                hitQueued = false;
                body.velocity = Vector3.ClampMagnitude(body.velocity + Vector3.down * HitDownwardSpeed, CurrentSpeedLimit);
            }
            if (dashQueued)
            {
                dashQueued = false;
                EndFocus();dashCruise=true;burstArmed=true;activeDashSpeed=queuedDashSpeed;
                kineticFlight=true;
                GetComponent<OrbitBreakerTargetLock>()?.Consume();
                DashCharges--; // Consume only when the queued dash actually starts; cancellation costs nothing.
                if (magnetLaunchQueued) { magnetLaunchQueued = false; RegisterLaunch(); }
                DashDirection = queuedDashDirection;
                dashEndsAt = Time.time + DashDuration;
                nextDashAt = Time.time + config.dashCooldown;
                body.velocity = new Vector3(DashDirection.x, DashDirection.y, 0f) * queuedDashSpeed;
                DashStarted?.Invoke();
            }
            // Keep launch momentum for the initial dash; gravity must not eat upward launch speed.
            if(IsDashing && dashCruise && !boardImpulseQueued && !IsStunned)
            {body.velocity=DashDirection*activeDashSpeed;PrepareSpeedContacts();return;}
            var velocity = body.velocity;
            velocity.z = 0f;
            body.velocity = Vector3.ClampMagnitude(velocity, CurrentSpeedLimit);
            bool launched = boardImpulseQueued;
            if (launched)
            {
                boardImpulseQueued = false;
                kineticFlight=true;RegisterLaunch();
                var direction = (Vector3)boardImpulseDirection;
                float speedChange = Mathf.Max(0f, boardExitSpeed - Vector3.Dot(body.velocity, direction));
                var targetVelocity = Vector3.ClampMagnitude(body.velocity + direction * speedChange, BaseSpeedLimit * LaunchSpeedFactor);
                body.velocity=targetVelocity;
                BoardImpulseApplied?.Invoke();
            }
            // Real physical momentum also counts; never discard a fast collision impulse as manual motion.
            if(CurrentSpeed>OrbitBreakerEncounterConfig.ManualSpeed+.1f)kineticFlight=true;
            if(CurrentSpeed<OrbitBreakerEncounterConfig.ManualSpeed && !IsDashing && !launched)kineticFlight=false;
            Vector2 nextVelocity=(Vector2)body.velocity+Vector2.down*config.gravityStrength*Time.fixedDeltaTime;
            // WASD steers a fast ball, but cannot add speed above the manual ceiling.
            if(!IsStunned && !IsDashing && !launched)
            {
                Vector2 acceleration=moveInput*config.moveAcceleration*MovementFactor*Time.fixedDeltaTime;
                if(kineticFlight && nextVelocity.magnitude>OrbitBreakerEncounterConfig.ManualSpeed)
                {
                    float before=nextVelocity.magnitude;
                    nextVelocity=Vector2.ClampMagnitude(nextVelocity+acceleration,before);
                }
                else nextVelocity+=acceleration;
            }
            float limit=kineticFlight?CurrentSpeedLimit:OrbitBreakerEncounterConfig.ManualSpeed*MovementFactor;
            body.velocity=Vector2.ClampMagnitude(nextVelocity,limit);
            PrepareSpeedContacts();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (gameManager == null || !gameManager.IsPlaying) return;
            if (IsAttached && collision.rigidbody != null && collision.rigidbody.GetComponent<OrbitBreakerEnemyController>() != null) return;
            ImpactSpeed=IncomingVelocity.magnitude;
            bool wasDashing = ImpactSpeed>DamageThreshold;
            RegisterDashContact(collision.collider, collision.relativeVelocity.magnitude);
            if (wasDashing) invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + 0.08f);
            dashCruise=false; // Solid contacts are allowed to redirect momentum naturally.
            ReleaseAttachment();
            Impact?.Invoke(collision, wasDashing);
        }

        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Playing)
            {
                Shells = MaximumShells; nextHoleDamageAt = 0f;
                body.isKinematic = false;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.WakeUp();
                // An opening serve starts the rally; it grants no dash charge or combo credit.
                body.velocity = Vector3.up * 17f;
                kineticFlight=true;IncomingVelocity=body.velocity;
                launchEndsAt = Time.time + LaunchProtection;
            }
            else
            {
                moveInput = Vector2.zero;
                Freeze();
            }
        }

        private void Freeze()
        {
            EndFocus();dashCruise=burstArmed=false;DashBurstCount=0;
            ReleaseAttachment();
            kineticFlight=bossBounceQueued=false;IncomingVelocity=Vector2.zero;ImpactSpeed=0;
            foreach(var enemy in OrbitBreakerEnemyController.Active)enemy.SetPlayerPassThrough(false);
            DashCharges = DashContactProgress = TotalQualifiedContacts = 0;
            contactTimes.Clear(); rewardedKills.Clear();
            rewardEndsAt = 0f;
            combo = 0; comboEndsAt = launchEndsAt = jackpotEndsAt = 0f; magnetLaunchQueued = false;
            attached = false;
            attachment = blockedAttachment = null;
            attachmentEndsAt = nextAttachmentAt = spinTime = 0f;
            aimValid = false;
            hitQueued = false;
            stunnedUntil = invulnerableUntil = 0f;
            boardImpulseQueued = false;
            dashQueued = false;
            dashEndsAt = nextDashAt = 0f;
            lastDirection = DashDirection = Vector2.up;
            if (!body.isKinematic)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            body.collisionDetectionMode = CollisionDetectionMode.Discrete;
            body.isKinematic = true;
        }
    }
}
