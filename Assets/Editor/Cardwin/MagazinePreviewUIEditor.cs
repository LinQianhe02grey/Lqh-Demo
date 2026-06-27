using UnityEditor;
using UnityEngine;

namespace Cardwin.UI.Editor
{
    [CustomEditor(typeof(MagazinePreviewUI))]
    public class MagazinePreviewUIEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MagazinePreviewUI preview = (MagazinePreviewUI)target;

            if (Application.isPlaying)
                return;

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Editor Animation Preview", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = preview.editorPreviewMode != MagazinePreviewUI.PreviewMode.Enemy;
            if (GUILayout.Button("Preview Enemy Consume", GUILayout.Height(30)))
            {
                preview.editorPreviewMode = MagazinePreviewUI.PreviewMode.Enemy;
                preview.editorAnimProgress = 0f;
                preview.RefreshEditorPreview();
                EditorApplication.update += AnimateEnemy;
            }

            GUI.enabled = preview.editorPreviewMode != MagazinePreviewUI.PreviewMode.Self;
            if (GUILayout.Button("Preview Self Consume", GUILayout.Height(30)))
            {
                preview.editorPreviewMode = MagazinePreviewUI.PreviewMode.Self;
                preview.editorAnimProgress = 0f;
                preview.RefreshEditorPreview();
                EditorApplication.update += AnimateSelf;
            }

            GUI.enabled = true;
            if (GUILayout.Button("Reset Preview", GUILayout.Height(30)))
            {
                EditorApplication.update -= AnimateEnemy;
                EditorApplication.update -= AnimateSelf;
                preview.ResetEditorPreview();
                EditorUtility.SetDirty(preview);
            }

            EditorGUILayout.EndHorizontal();

            if (preview.editorPreviewMode == MagazinePreviewUI.PreviewMode.Enemy)
            {
                EditorGUILayout.HelpBox("Enemy consume preview active — Slot 0 dissolving.", MessageType.Info);
            }
            else if (preview.editorPreviewMode == MagazinePreviewUI.PreviewMode.Self)
            {
                EditorGUILayout.HelpBox("Self consume preview active — Slot 0 flying to HP bar background.", MessageType.Info);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(preview);
            }
        }

        private void AnimateEnemy()
        {
            MagazinePreviewUI preview = (MagazinePreviewUI)target;
            if (preview == null || preview.editorPreviewMode != MagazinePreviewUI.PreviewMode.Enemy)
            {
                EditorApplication.update -= AnimateEnemy;
                return;
            }

            preview.editorAnimProgress += 0.02f;
            if (preview.editorAnimProgress >= 1f)
            {
                preview.editorAnimProgress = 1f;
                preview.RefreshEditorPreview();
                EditorApplication.update -= AnimateEnemy;

                EditorApplication.delayCall += () =>
                {
                    preview.ResetEditorPreview();
                    EditorUtility.SetDirty(preview);
                };
                return;
            }

            preview.RefreshEditorPreview();
            EditorUtility.SetDirty(preview);
        }

        private void AnimateSelf()
        {
            MagazinePreviewUI preview = (MagazinePreviewUI)target;
            if (preview == null || preview.editorPreviewMode != MagazinePreviewUI.PreviewMode.Self)
            {
                EditorApplication.update -= AnimateSelf;
                return;
            }

            preview.editorAnimProgress += 0.02f;
            if (preview.editorAnimProgress >= 1f)
            {
                preview.editorAnimProgress = 1f;
                preview.RefreshEditorPreview();
                EditorApplication.update -= AnimateSelf;

                EditorApplication.delayCall += () =>
                {
                    preview.ResetEditorPreview();
                    EditorUtility.SetDirty(preview);
                };
                return;
            }

            preview.RefreshEditorPreview();
            EditorUtility.SetDirty(preview);
        }

        private void OnDisable()
        {
            EditorApplication.update -= AnimateEnemy;
            EditorApplication.update -= AnimateSelf;
        }
    }
}
