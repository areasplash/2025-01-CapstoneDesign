using UnityEngine;
using UnityEngine.Rendering.Universal;

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
					RenderTextureSize = Vector2Field("Render Texture Size", RenderTextureSize);
					FocusDistance     = Slider      ("Focus Distance",      FocusDistance, 0f, 255f);
					FieldOfView       = FloatField  ("Field Of View",       FieldOfView);
					OrthographicSize  = FloatField  ("Orthographic Size",   OrthographicSize);
					Space();
				}
				if (CameraData) {
                    LabelField("URP Camera", EditorStyles.boldLabel);
					PostProcessing = Toggle("Post Processing", PostProcessing);
					BeginDisabledGroup(!PostProcessing);
					AntiAliasing = Toggle("Anti Aliasing", AntiAliasing);
					EndDisabledGroup();
					Space();
				}

				End();
			}
		}
	#endif



	// Fields

	Camera m_MainCamera;
	UniversalAdditionalCameraData m_CameraData;

	[SerializeField] float m_FocusDistance;



	// Properties

	public static Vector3 Position {
		get => Instance.transform.position;
		set => Instance.transform.position = value;
	}
	public static Quaternion Rotation {
		get => Instance.transform.rotation;
		set => Instance.transform.rotation = value;
	}
	public static Vector3 EulerRotation {
		get => Rotation.eulerAngles;
		set => Rotation = Quaternion.Euler(value);
	}
	public static float Yaw {
		get => EulerRotation.y;
		set => EulerRotation = new Vector3(EulerRotation.x, value, EulerRotation.z);
	}
	public static Vector3 Right   => Rotation * Vector3.right;
	public static Vector3 Up      => Rotation * Vector3.up;
	public static Vector3 Forward => Rotation * Vector3.forward;



	static Camera MainCamera {
		get {
			if (!Instance.m_MainCamera) for (int i = 0; i < Instance.transform.childCount; i++) {
				if (Instance.transform.GetChild(i).TryGetComponent(out Instance.m_MainCamera)) break;
			}
			return Instance.m_MainCamera;
		}
	}
	static UniversalAdditionalCameraData CameraData {
		get {
			if (!Instance.m_CameraData) for (int i = 0; i < Instance.transform.childCount; i++) {
				if (Instance.transform.GetChild(i).TryGetComponent(out Instance.m_CameraData)) break;
			}
			return Instance.m_CameraData;
		}
	}

	public static Vector2 RenderTextureSize {
		get {
			var target = MainCamera.targetTexture;
			if (target) return new Vector2(target.width, target.height);
			else        return new Vector2(Screen.width, Screen.height);
		}
		set {
			var target = MainCamera.targetTexture;
			if (target) {
				target.Release();
				target.width  = (int)Mathf.Max(1f, value.x);
				target.height = (int)Mathf.Max(1f, value.y);
				target.Create();
			}
		}
	}
	public static float FocusDistance {
		get => Instance.m_FocusDistance;
		set {
			value = Mathf.Clamp(value, 0f, 255f);
			Instance.m_FocusDistance = value;
			MainCamera.transform.localPosition = new Vector3(0, 0, -value);
		}
	}
	public static float FieldOfView {
		get => MainCamera.fieldOfView;
		set => MainCamera.fieldOfView = Mathf.Clamp(value, 1f, 179f);
	}
	public static float OrthographicSize {
		get => MainCamera.orthographicSize;
		set => MainCamera.orthographicSize = Mathf.Clamp(value, 1f, 179f);
	}

	static bool PostProcessing {
		get => CameraData.renderPostProcessing;
		set => CameraData.renderPostProcessing = value;
	}
	static bool AntiAliasing {
		get => CameraData.antialiasing != AntialiasingMode.None;
		set {
			const AntialiasingMode None = AntialiasingMode.None;
			const AntialiasingMode SMAA = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
			CameraData.antialiasing = value ? SMAA : None;
		}
	}
}
