using UnityEngine;
using UnityEngine.InputSystem;
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



	// Fields

	PlayerInput m_PlayerInput;

	uint m_KeyNext;
	uint m_KeyPrev;
	Vector2 m_MoveDirection;
	Vector2 m_PointPosition;
	Vector2 m_ScrollWheel;
	Vector2 m_Navigate;



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
		KeyNext = 0u;
		KeyPrev = 0u;
		MoveDirection = Vector2.zero;
		PointPosition = Vector2.zero;
		ScrollWheel   = Vector2.zero;
		Navigate      = Vector2.zero;
	}



	// Lifecycle

	void Start() => RegisterActionMap();
}
