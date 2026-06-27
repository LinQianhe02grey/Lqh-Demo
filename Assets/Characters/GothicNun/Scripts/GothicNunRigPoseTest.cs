using System.Collections.Generic;
using UnityEngine;

namespace Cardwin.Characters
{
    [ExecuteAlways]
    public class GothicNunRigPoseTest : MonoBehaviour
    {
        [Header("Head")]
        [SerializeField] [Range(-30f, 30f)] private float _headRotation;
        [SerializeField] private Transform _headJoint;

        [Header("Torso")]
        [SerializeField] [Range(-20f, 20f)] private float _torsoRotation;
        [SerializeField] private Transform _torsoJoint;

        [Header("Left Arm")]
        [SerializeField] [Range(-60f, 60f)] private float _leftShoulderRotation;
        [SerializeField] private Transform _shoulderL;
        [SerializeField] [Range(-90f, 90f)] private float _leftElbowRotation;
        [SerializeField] private Transform _elbowL;
        [SerializeField] [Range(-60f, 60f)] private float _leftWristRotation;
        [SerializeField] private Transform _wristL;

        [Header("Right Arm")]
        [SerializeField] [Range(-60f, 60f)] private float _rightShoulderRotation;
        [SerializeField] private Transform _shoulderR;
        [SerializeField] [Range(-90f, 90f)] private float _rightElbowRotation;
        [SerializeField] private Transform _elbowR;
        [SerializeField] [Range(-60f, 60f)] private float _rightWristRotation;
        [SerializeField] private Transform _wristR;

        [Header("Left Leg")]
        [SerializeField] [Range(-45f, 45f)] private float _leftHipRotation;
        [SerializeField] private Transform _hipL;
        [SerializeField] [Range(-60f, 60f)] private float _leftKneeRotation;
        [SerializeField] private Transform _kneeL;
        [SerializeField] [Range(-45f, 45f)] private float _leftAnkleRotation;
        [SerializeField] private Transform _ankleL;

        [Header("Right Leg")]
        [SerializeField] [Range(-45f, 45f)] private float _rightHipRotation;
        [SerializeField] private Transform _hipR;
        [SerializeField] [Range(-60f, 60f)] private float _rightKneeRotation;
        [SerializeField] private Transform _kneeR;
        [SerializeField] [Range(-45f, 45f)] private float _rightAnkleRotation;
        [SerializeField] private Transform _ankleR;

        [Header("Test Presets")]
        [SerializeField] private bool _applyTpose;
        [SerializeField] private bool _applyShoulderTest;
        [SerializeField] private bool _applyElbowTest;
        [SerializeField] private bool _applyHipTest;
        [SerializeField] private bool _applyKneeTest;
        [SerializeField] private bool _applyHeadTest;
        [SerializeField] private bool _resetAll;

        private Dictionary<Transform, Quaternion> _initialRotations = new Dictionary<Transform, Quaternion>();
        private HashSet<string> _warnedNullJoints = new HashSet<string>();
        private bool _bound;

        void OnEnable()
        {
            RebindAndCapture();
            _bound = true;
        }

        void OnDisable()
        {
            _bound = false;
        }

        void OnValidate()
        {
            EnsureBound();
            if (!_bound) return;
            if (Application.isPlaying) return;
            ApplyPose();
        }

        void Update()
        {
            if (!_bound) return;
            if (!Application.isPlaying) return;
            ApplyPose();
        }

        void EnsureBound()
        {
            if (_bound) return;
            if (gameObject.scene.IsValid() && gameObject.scene.isLoaded)
            {
                RebindAndCapture();
                _bound = true;
            }
        }

        void RebindAndCapture()
        {
            AutoBindJoints();
            SaveInitialRotations();
        }

        void AutoBindJoints()
        {
            if (_headJoint == null) _headJoint = FindJoint("HeadJoint");
            if (_torsoJoint == null) _torsoJoint = FindJoint("TorsoJoint");
            if (_shoulderL == null) _shoulderL = FindJoint("Shoulder_L");
            if (_elbowL == null) _elbowL = FindJoint("Elbow_L");
            if (_wristL == null) _wristL = FindJoint("Wrist_L");
            if (_shoulderR == null) _shoulderR = FindJoint("Shoulder_R");
            if (_elbowR == null) _elbowR = FindJoint("Elbow_R");
            if (_wristR == null) _wristR = FindJoint("Wrist_R");
            if (_hipL == null) _hipL = FindJoint("Hip_L");
            if (_kneeL == null) _kneeL = FindJoint("Knee_L");
            if (_ankleL == null) _ankleL = FindJoint("Ankle_L");
            if (_hipR == null) _hipR = FindJoint("Hip_R");
            if (_kneeR == null) _kneeR = FindJoint("Knee_R");
            if (_ankleR == null) _ankleR = FindJoint("Ankle_R");
        }

        Transform FindJoint(string name)
        {
            var found = GetComponentsInChildren<Transform>(true);
            foreach (var t in found)
            {
                if (t.name == name) return t;
            }
            return null;
        }

        void SaveInitialRotations()
        {
            _initialRotations.Clear();
            Transform[] joints = {
                _headJoint, _torsoJoint,
                _shoulderL, _elbowL, _wristL,
                _shoulderR, _elbowR, _wristR,
                _hipL, _kneeL, _ankleL,
                _hipR, _kneeR, _ankleR
            };
            foreach (var j in joints)
            {
                if (j != null && !_initialRotations.ContainsKey(j))
                {
                    _initialRotations[j] = j.localRotation;
                }
            }
        }

        void ApplyPose()
        {
            if (!_bound)
            {
                EnsureBound();
                if (!_bound) return;
            }

            if (_resetAll)
            {
                _resetAll = false;
                ResetToZero();
                return;
            }

            if (_applyTpose) { _applyTpose = false; ApplyTPose(); }
            if (_applyShoulderTest) { _applyShoulderTest = false; _leftShoulderRotation = -15f; _rightShoulderRotation = 15f; }
            if (_applyElbowTest) { _applyElbowTest = false; _leftElbowRotation = 25f; _rightElbowRotation = -25f; }
            if (_applyHipTest) { _applyHipTest = false; _leftHipRotation = 10f; _rightHipRotation = -10f; }
            if (_applyKneeTest) { _applyKneeTest = false; _leftKneeRotation = 20f; _rightKneeRotation = -20f; }
            if (_applyHeadTest) { _applyHeadTest = false; _headRotation = 10f; }

            ApplyJointRotation(_headJoint, _headRotation, "_headJoint");
            ApplyJointRotation(_torsoJoint, _torsoRotation, "_torsoJoint");
            ApplyJointRotation(_shoulderL, _leftShoulderRotation, "_shoulderL");
            ApplyJointRotation(_elbowL, _leftElbowRotation, "_elbowL");
            ApplyJointRotation(_wristL, _leftWristRotation, "_wristL");
            ApplyJointRotation(_shoulderR, _rightShoulderRotation, "_shoulderR");
            ApplyJointRotation(_elbowR, _rightElbowRotation, "_elbowR");
            ApplyJointRotation(_wristR, _rightWristRotation, "_wristR");
            ApplyJointRotation(_hipL, _leftHipRotation, "_hipL");
            ApplyJointRotation(_kneeL, _leftKneeRotation, "_kneeL");
            ApplyJointRotation(_ankleL, _leftAnkleRotation, "_ankleL");
            ApplyJointRotation(_hipR, _rightHipRotation, "_hipR");
            ApplyJointRotation(_kneeR, _rightKneeRotation, "_kneeR");
            ApplyJointRotation(_ankleR, _rightAnkleRotation, "_ankleR");
        }

        void ApplyJointRotation(Transform joint, float angleDeg, string fieldName)
        {
            if (joint == null)
            {
                WarnOnce($"[GothicNunRigPoseTest] Joint reference is null: {fieldName}. " +
                    "Bind the Transform in the Inspector or ensure the Rig prefab has the correct joint hierarchy.",
                    fieldName);
                return;
            }

            if (!_initialRotations.TryGetValue(joint, out var initRot))
            {
                _initialRotations[joint] = joint.localRotation;
                initRot = joint.localRotation;
            }

            joint.localRotation = initRot * Quaternion.Euler(0, 0, angleDeg);
        }

        void WarnOnce(string message, string key)
        {
            if (_warnedNullJoints.Contains(key)) return;
            _warnedNullJoints.Add(key);
            Debug.LogWarning(message, this);
        }

        void ResetToZero()
        {
            _headRotation = 0; _torsoRotation = 0;
            _leftShoulderRotation = 0; _leftElbowRotation = 0; _leftWristRotation = 0;
            _rightShoulderRotation = 0; _rightElbowRotation = 0; _rightWristRotation = 0;
            _leftHipRotation = 0; _leftKneeRotation = 0; _leftAnkleRotation = 0;
            _rightHipRotation = 0; _rightKneeRotation = 0; _rightAnkleRotation = 0;

            Transform[] joints = {
                _headJoint, _torsoJoint,
                _shoulderL, _elbowL, _wristL,
                _shoulderR, _elbowR, _wristR,
                _hipL, _kneeL, _ankleL,
                _hipR, _kneeR, _ankleR
            };
            foreach (var j in joints)
            {
                if (j != null && _initialRotations.TryGetValue(j, out var initRot))
                    j.localRotation = initRot;
            }
        }

        void ApplyTPose()
        {
            _leftShoulderRotation = -15f; _rightShoulderRotation = 15f;
            _leftElbowRotation = 0; _rightElbowRotation = 0;
            _leftHipRotation = 10f; _rightHipRotation = -10f;
            _headRotation = 0; _torsoRotation = 0;
            _leftKneeRotation = 0; _rightKneeRotation = 0;
            _leftAnkleRotation = 0; _rightAnkleRotation = 0;
            _leftWristRotation = 0; _rightWristRotation = 0;
        }
    }
}
