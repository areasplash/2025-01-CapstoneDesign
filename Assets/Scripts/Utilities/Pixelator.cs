using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Pixelator
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Utility/Pixelator")]
[ExecuteAlways, RequireComponent(typeof(Camera))]
public class Pixelator : MonoBehaviour {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(Pixelator))]
	public class PixelatorEditor : EditorExtensions {
		Pixelator I => target as Pixelator;
		public override void OnInspectorGUI() {
			Begin();

			LabelField("Camera", EditorStyles.boldLabel);
			I.DollyDistance = Slider("Dolly Distance", I.DollyDistance, 0f, 64f);
			I.FieldOfView = FloatField("Field Of View", I.FieldOfView);
			I.OrthographicSize = FloatField("Orthographic Size", I.OrthographicSize);
			I.Projection = Slider("Projection", I.Projection, 0f, 1f);
			BeginHorizontal();
			PrefixLabel(" ");
			var l = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft };
			var r = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
			var s = new GUIStyle(GUI.skin.label) { fixedWidth = 50 };
			GUILayout.Label("< Perspective ", l);
			GUILayout.Label("Orthographic >", r);
			GUILayout.Label(" ", s);
			EndHorizontal();
			Space();

			LabelField("Pixelator", EditorStyles.boldLabel);
			I.TargetObject = ObjectField("Target Object", I.TargetObject);
			if (I.TargetObject) {
				var prev = LayerMask.LayerToName(I.TargetObjectLayer);
				var next = LayerMask.LayerToName(I.TargetObject.layer);
				if (string.IsNullOrEmpty(prev)) prev = "Empty";
				if (string.IsNullOrEmpty(next)) next = "Empty";
				var text = string.Empty;
				text += $"{I.TargetObject.name} layer changed ";
				text += $"{prev}({I.TargetObjectLayer}) to {next}({I.TargetObject.layer}).";
				HelpBox(text);
				BeginVertical(EditorStyles.helpBox);
				if (true) {
					LabelField("Transform", EditorStyles.boldLabel);
					var position = I.TargetObject.transform.localPosition;
					var rotation = I.TargetObject.transform.localRotation;
					I.TargetObject.transform.localPosition = Vector3Field("Position", position);
					I.TargetObject.transform.localRotation = EulerField("Rotation", rotation);
					Space();
				}
				EndVertical();
			}
			I.TargetPath = TextField("Target Path", I.TargetPath);
			I.TextureSize = IntSlider("Texture Size", I.TextureSize, 16, 256);
			BeginHorizontal();
			PrefixLabel("Save Pixelated");
			if (Button("Save")) I.SavePixelated(I.TargetPath);
			if (Button("Save As")) {
				var name = "Save Pixelated";
				var path = EditorUtility.SaveFilePanel(name, "Assets", "pixelated.png", "png");
				if (!string.IsNullOrEmpty(path)) I.SavePixelated(path);
			}
			EndHorizontal();
			Space();

			LabelField("Preview", EditorStyles.boldLabel);
			var rect = GUILayoutUtility.GetAspectRect(1.0f);
			EditorGUI.DrawPreviewTexture(rect, I.RenderTexture);
			Space();

			End();
		}

		void OnSceneGUI() {
			if (I.TargetObject) {
				EditorGUI.BeginChangeCheck();
				var position = I.TargetObject.transform.localPosition;
				var rotation = I.TargetObject.transform.localRotation;
				var positionHandle = Handles.PositionHandle(position, rotation);
				var rotationHandle = Handles.RotationHandle(rotation, position);
				I.TargetObject.transform.localPosition = positionHandle;
				I.TargetObject.transform.localRotation = rotationHandle;
				if (EditorGUI.EndChangeCheck()) Repaint();
			}
		}
	}
	#endif



	// Constants

	const TextureFormat Format = TextureFormat.ARGB32;
	const RenderTextureFormat RenderFormat = RenderTextureFormat.ARGB32;



	// Fields

	Camera m_Camera;
	[SerializeField] float m_DollyDistance = 0f;
	[SerializeField] float m_Projection = 1f;

	RenderTexture m_RenderTexture;
	[SerializeField] GameObject m_TargetObject;
	[SerializeField] int m_TargetObjectLayer;
	[SerializeField] string m_TargetPath = "Assets/pixelated.png";
	[SerializeField] int m_TextureSize = 64;



	// Properties

	Camera Camera => !m_Camera ?
		m_Camera = TryGetComponent(out Camera camera) ? camera : null :
		m_Camera;

	float DollyDistance {
		get => m_DollyDistance;
		set {
			float delta = value - m_DollyDistance;
			if (m_DollyDistance != value) {
				m_DollyDistance = value;
				transform.localPosition -= delta * transform.forward;
			}
		}
	}

	float FieldOfView {
		get => Camera.fieldOfView;
		set => Camera.fieldOfView = value;
	}
	float OrthographicSize {
		get => Camera.orthographicSize;
		set => Camera.orthographicSize = value;
	}
	float Projection {
		get => m_Projection;
		set {
			value = Mathf.Clamp(value, 0f, 1f);
			if (m_Projection != value) {
				float fov    = Camera.fieldOfView;
				float aspect = Camera.aspect;
				float zNear  = Camera.nearClipPlane;
				float zFar   = Camera.farClipPlane;
				float wHalf  = Camera.orthographicSize * aspect;
				float hHalf  = Camera.orthographicSize;

				m_Projection = value;
				var matrix = Camera.projectionMatrix;
				var a = Matrix4x4.Perspective(fov, aspect, zNear, zFar);
				var b = Matrix4x4.Ortho(-wHalf, wHalf, -hHalf, hHalf, zNear, zFar);
				float t = Mathf.Pow(Mathf.Max(0.01f, value), 0.03f);
				for (int i = 0; i < 16; i++) matrix[i] = Mathf.Lerp(a[i], b[i], t);
				Camera.projectionMatrix = matrix;
			}
		}
	}



	RenderTexture RenderTexture {
		get => m_RenderTexture;
		set => m_RenderTexture = value;
	}

	GameObject TargetObject {
		get => m_TargetObject;
		set {
			if (m_TargetObject != value) {
				if (m_TargetObject != null) {
					m_TargetObject.layer = TargetObjectLayer;
					TargetObjectLayer = default;
				}
				if (value != null) {
					Camera.cullingMask = 1 << 31;
					TargetObjectLayer = value.layer;
					value.layer = 31;
				} else Camera.cullingMask = -1;
				m_TargetObject = value;
			}
		}
	}
	int TargetObjectLayer {
		get => m_TargetObjectLayer;
		set => m_TargetObjectLayer = value;
	}
	string TargetPath {
		get => m_TargetPath;
		set => m_TargetPath = value;
	}

	int TextureSize {
		get => m_TextureSize;
		set {
			value = value / 2 * 2;
			if (m_TextureSize != value) {
				m_TextureSize = value;
				RenderTexture.Release();
				RenderTexture.width = value;
				RenderTexture.height = value;
				RenderTexture.Create();
			}
		}
	}



	// Methods

	void SavePixelated(string path) {
		var color = Camera.backgroundColor;
		Camera.backgroundColor = Color.clear;
		Camera.Render();
		Camera.backgroundColor = color;

		var active = RenderTexture.active;
		RenderTexture.active = RenderTexture;
		var texture = new Texture2D(TextureSize, TextureSize, Format, false);
		texture.ReadPixels(new Rect(0, 0, TextureSize, TextureSize), 0, 0);
		texture.Apply();
		RenderTexture.active = active;
		try {
			var bytes = texture.EncodeToPNG();
			File.WriteAllBytes(path, bytes);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		} finally {
			DestroyImmediate(texture);
		}
	}



	// Lifecycle

	void Awake() {
		Camera.orthographic = true;
		Camera.clearFlags = CameraClearFlags.SolidColor;
		Camera.backgroundColor = new(0.16f, 0.16f, 0.16f, 1f);
		Camera.targetTexture = new(TextureSize, TextureSize, 16, RenderFormat) {
			name = "Render Texture",
			antiAliasing = 1,
			useMipMap = false,
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Point,
		};
		RenderTexture = Camera.targetTexture;
		RenderTexture.Create();
	}

	void OnDestroy() {
		if (RenderTexture != null) {
			Camera.targetTexture = null;
			if (RenderTexture.IsCreated()) RenderTexture.Release();
			DestroyImmediate(RenderTexture);
			RenderTexture = null;
		}
		if (TargetObject != null) {
			TargetObject.layer = TargetObjectLayer;
		}
	}
}
