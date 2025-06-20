using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif



// Input Actions

public enum ActionMap : byte {
	Player,
	UI,
}

public enum KeyAction : byte {
	Move,
	Jump,
	Interact,

	Point,
	Click,
	MiddleClick,
	RightClick,
	Navigate,
	ScrollWheel,
	Submit,
	Cancel,
	TrackedDevicePosition,
	TrackedDeviceOrientation,
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Input Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/Input Manager")]
[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoSingleton<InputManager> {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(InputManager))]
	class InputManagerEditor : EditorExtensions {
		InputManager I => target as InputManager;
		public override void OnInspectorGUI() {
			Begin("Input Manager");

			LabelField("Web Cam", EditorStyles.boldLabel);
			RawImage = ObjectField("Web Cam Image", RawImage);
			Space();
			LabelField("Debug", EditorStyles.boldLabel);
			BeginDisabledGroup();
			var actionMap = Application.isPlaying ? PlayerInput.currentActionMap.name : "None";
			TextField("Action Map", actionMap);
			EndDisabledGroup();
			Space();

			End();
		}
	}
	#endif



	// Constants

	const float WebCamUpdateInterval = 10f;



	// Fields

	PlayerInput m_PlayerInput;

	uint m_KeyNext;
	uint m_KeyPrev;
	Vector2 m_MoveDirection;
	Vector2 m_PointPosition;
	Vector2 m_ScrollWheel;
	Vector2 m_Navigate;

	WebCamTexture m_WebCamTexture;
	Texture2D m_CachedWebCamTexture;
	[SerializeField] RawImage m_RawImage;



	// Properties

	static PlayerInput PlayerInput =>
		Instance.m_PlayerInput || Instance.TryGetComponent(out Instance.m_PlayerInput) ?
		Instance.m_PlayerInput : null;

	static InputActionAsset InputActionAsset => PlayerInput.actions;



	public static uint KeyNext {
		get         => Instance.m_KeyNext;
		private set => Instance.m_KeyNext = value;
	}
	public static uint KeyPrev {
		get         => Instance.m_KeyPrev;
		private set => Instance.m_KeyPrev = value;
	}
	public static Vector2 MoveDirection {
		get         => Instance.m_MoveDirection;
		private set => Instance.m_MoveDirection = value;
	}
	public static Vector2 PointPosition {
		get         => Instance.m_PointPosition;
		private set => Instance.m_PointPosition = value;
	}
	public static Vector2 ScrollWheel {
		get         => Instance.m_ScrollWheel;
		private set => Instance.m_ScrollWheel = value;
	}
	public static Vector2 Navigate {
		get         => Instance.m_Navigate;
		private set => Instance.m_Navigate = value;
	}



	static WebCamTexture WebCamTexture {
		get => Instance.m_WebCamTexture;
		set => Instance.m_WebCamTexture = value;
	}
	public static Texture2D CachedWebCamTexture {
		get => Instance.m_CachedWebCamTexture;
		set => Instance.m_CachedWebCamTexture = value;
	}
	static RawImage RawImage {
		get => Instance.m_RawImage;
		set => Instance.m_RawImage = value;
	}



	// Key State Methods

	static void RegisterActionMap() {
		if (InputActionAsset == null) return;
		foreach (var inputActionMap in InputActionAsset.actionMaps) {
			if (!Enum.TryParse(inputActionMap.name, out ActionMap actionMap)) continue;
			foreach (var inputAction in inputActionMap.actions) {
				if (!Enum.TryParse(inputAction.name, out KeyAction keyAction)) continue;

				int index = (int)keyAction;
				inputAction.performed += (KeyAction)index switch {
					KeyAction.Move        => callback => MoveDirection = callback.ReadValue<Vector2>(),
					KeyAction.Point       => callback => PointPosition = callback.ReadValue<Vector2>(),
					KeyAction.ScrollWheel => callback => ScrollWheel   = callback.ReadValue<Vector2>(),
					KeyAction.Navigate    => callback => Navigate      = callback.ReadValue<Vector2>(),
					_ => callback => _ = callback.action.IsPressed() switch {
						true  => KeyNext |=  (1u << index),
						false => KeyNext &= ~(1u << index),
					},
				};
				inputAction.started  += callback => KeyNext |=  (1u << index);
				inputAction.canceled += callback => KeyNext &= ~(1u << index);
			}
		}
		InputSystem.onBeforeUpdate += () => KeyPrev = KeyNext;
	}

	static bool GetKeyNext(KeyAction key) => (KeyNext & (1u << (int)key)) != 0u;
	static bool GetKeyPrev(KeyAction key) => (KeyPrev & (1u << (int)key)) != 0u;

	public static bool GetKey(KeyAction key) => GetKeyNext(key);
	public static bool GetKeyDown(KeyAction key) => GetKeyNext(key) && !GetKeyPrev(key);
	public static bool GetKeyUp(KeyAction key) => !GetKeyNext(key) && GetKeyPrev(key);

	public static void SwitchActionMap(ActionMap actionMap) {
		if (InputActionAsset == null) return;
		PlayerInput.currentActionMap = InputActionAsset.FindActionMap(actionMap.ToString());
		MoveDirection = PointPosition = ScrollWheel = Navigate = default;
		KeyNext = KeyPrev = default;
	}



	// Lifecycle

	void Start() => RegisterActionMap();

	void Update() {
		float time = Time.realtimeSinceStartup;
		if ((time + Time.unscaledDeltaTime) % WebCamUpdateInterval < time % WebCamUpdateInterval) {
			if (!WebCamTexture && 0 < WebCamTexture.devices.Length) {
				WebCamTexture = new WebCamTexture(WebCamTexture.devices[0].name);
				WebCamTexture.Play();
			}
			if (WebCamTexture)
			{
				CachedWebCamTexture ??= new Texture2D(WebCamTexture.width, WebCamTexture.height);
				CachedWebCamTexture.SetPixels(WebCamTexture.GetPixels());
				CachedWebCamTexture.Apply();
				if (RawImage) RawImage.material.mainTexture = CachedWebCamTexture;
				
				ServerRequestManager.Instance.RequestImageAnalysis(CachedWebCamTexture, (result) => {
					if (result != null) {
						Debug.Log("분석 감정: " + result.emotion_result.emotion);
						if (result.emotion_result.emotion == "sadness" || result.emotion_result.emotion == "anger") {
							GameManager.Instance.m_Negative = true;
						}
						else {
							GameManager.Instance.m_Negative = false;
						}
					}
					else {
						Debug.LogWarning("이미지 분석 실패!");
					}
            	});
			}
		}
	}
}
