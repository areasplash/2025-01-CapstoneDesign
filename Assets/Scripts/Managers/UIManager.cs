using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UI Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/UI Manager")]
public class UIManager : MonoSingleton<UIManager> {

	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(UIManager))]
		class UIManagerEditor : EditorExtensions {
			UIManager I => target as UIManager;
			public override void OnInspectorGUI() {
				Begin("UI Manager");

				End();
			}
		}
	#endif



	// Fields

	GameCanvas      m_GameCanvas;
	DialogueCanvas  m_DialogueCanvas;
	InventoryCanvas m_InventoryCanvas;

	BaseCanvas m_MainCanvas;
	readonly Stack<BaseCanvas> m_OverlayCanvas = new();



	// Properties

	static RectTransform Transform => Instance.transform as RectTransform;

	static GameCanvas GameCanvas {
		get {
			if (!Instance.m_GameCanvas) for (int i = 0; i < Transform.childCount; i++) {
				if (Transform.GetChild(i).TryGetComponent(out Instance.m_GameCanvas)) break;
			}
			return Instance.m_GameCanvas;
		}
	}
	static DialogueCanvas DialogueCanvas {
		get {
			if (!Instance.m_DialogueCanvas) for (int i = 0; i < Transform.childCount; i++) {
				if (Transform.GetChild(i).TryGetComponent(out Instance.m_DialogueCanvas)) break;
			}
			return Instance.m_DialogueCanvas;
		}
	}
	static InventoryCanvas InventoryCanvas {
		get {
			if (!Instance.m_InventoryCanvas) for (int i = 0; i < Transform.childCount; i++) {
				if (Transform.GetChild(i).TryGetComponent(out Instance.m_InventoryCanvas)) break;
			}
			return Instance.m_InventoryCanvas;
		}
	}



	static BaseCanvas MainCanvas {
		get => Instance.m_MainCanvas;
		set => Instance.m_MainCanvas = value;
	}
	static Stack<BaseCanvas> OverlayCanvas => Instance.m_OverlayCanvas;

	public static BaseCanvas CurrentCanvas {
		get => OverlayCanvas.TryPeek(out var overlayCanvas) ? overlayCanvas : MainCanvas;
	}
	public static bool IsUIActive => CurrentCanvas != GameCanvas;



	public static GameObject SelectedGameObject {
		get => EventSystem.current.currentSelectedGameObject;
		set => EventSystem.current.SetSelectedGameObject(value);
	}



	// Methods

	public static void Initialize() {
		GameCanvas.Hide();
		DialogueCanvas.Hide();
		InventoryCanvas.Hide();
	}

	public static void Back() {
		switch (CurrentCanvas) {
			case global::GameCanvas:
				// show menu
				break;
			case global::DialogueCanvas:
				// do nothing
				break;
			default:
				if (OverlayCanvas.TryPop (out var next)) next.Hide();
				if (OverlayCanvas.TryPeek(out var prev)) prev.Show();
				break;
		}
	}

	public static void ForceBack() {
		if (OverlayCanvas.TryPop (out var next)) next.Hide();
		if (OverlayCanvas.TryPeek(out var prev)) prev.Show();
	}



	// Canvas Methods

	public static void ShowGame() => ShowMainCanvas(GameCanvas);

	static void ShowMainCanvas(BaseCanvas mainCanvas) {
		if (MainCanvas) {
			if (MainCanvas == mainCanvas) return;
			MainCanvas.Hide();
		}
		while (OverlayCanvas.TryPop(out var canvas)) canvas.Hide();
		MainCanvas = mainCanvas;
		mainCanvas.Show();
	}

	public static void ShowDialogue () => ShowOverlayCanvas(DialogueCanvas);
	public static void ShowInventory() => ShowOverlayCanvas(InventoryCanvas);

	static void ShowOverlayCanvas(BaseCanvas overlayCanvas) {
		if (OverlayCanvas.TryPeek(out var canvas)) {
			if (canvas == overlayCanvas) return;
			canvas.Hide(true);
		}
		OverlayCanvas.Push(overlayCanvas);
		overlayCanvas.Show();
	}



	// Game Canvas Methods

	public static void ShowGemCollectMessage(string message) {
		GameCanvas.ShowGemCollectMessage(message);
	}



	// Dialogue Canvas Methods

	public static void EnqueueDialogue(string name, string text, Action onEnd = null) {
		if (!DialogueCanvas.gameObject.activeSelf) ShowDialogue();
		DialogueCanvas.EnqueueDialogue(name, text, onEnd);
	}

	public static void BeginDialogueInput(Action<MultimodalData> onEnd = null) {
		if (!DialogueCanvas.gameObject.activeSelf) ShowDialogue();
		DialogueCanvas.BeginDialogueInput(onEnd);
	}



	// Lifecycle

	void Update() {
		
	}
}
