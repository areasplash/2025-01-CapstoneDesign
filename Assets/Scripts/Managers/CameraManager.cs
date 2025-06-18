using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Camera Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/Camera Manager")]
public class CameraManager : MonoSingleton<CameraManager> {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(CameraManager))]
	class CameraManagerEditor : EditorExtensions {
		CameraManager I => target as CameraManager;
		public override void OnInspectorGUI() {
			Begin("Camera Manager");

			if (MainCamera) {
				LabelField("Camera", EditorStyles.boldLabel);
				OrthographicSize = FloatField("Orthographic Size", OrthographicSize);
				BeginDisabledGroup(true);
				TextField("Reference Size", $"{270f / 16f * 0.5f}");
				EndDisabledGroup();
				Space();
			}
			End();
		}
	}
	#endif



	// Fields

	Camera m_MainCamera;



	// Properties

	public static Vector3 Position {
		get => Instance.transform.position;
		set => Instance.transform.position = value;
	}

	public static Camera MainCamera =>
		Instance.m_MainCamera || TryGetComponentInChildren(out Instance.m_MainCamera) ?
		Instance.m_MainCamera : null;

	public static float OrthographicSize {
		get => MainCamera.orthographicSize;
		set => MainCamera.orthographicSize = Mathf.Clamp(value, 1f, 179f);
	}
}
