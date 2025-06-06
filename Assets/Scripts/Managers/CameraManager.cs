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

				if (!MainCamera) {
					HelpBox("No camera found. Please add a camera to child object.");
					Space();
				} else {
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

	static Camera MainCamera {
		get {
			if (!Instance.m_MainCamera) for (int i = 0; i < Instance.transform.childCount; i++) {
				if (Instance.transform.GetChild(i).TryGetComponent(out Instance.m_MainCamera)) break;
			}
			return Instance.m_MainCamera;
		}
	}
	public static float OrthographicSize {
		get => MainCamera.orthographicSize;
		set => MainCamera.orthographicSize = Mathf.Clamp(value, 1f, 179f);
	}
}
