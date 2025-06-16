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

	GameCanvas m_GameCanvas;
	DialogueCanvas m_DialogueCanvas;
	InventoryCanvas m_InventoryCanvas;

	BaseCanvas m_MainCanvas;
	Stack<BaseCanvas> m_OverlayCanvas = new();



	// Properties

	static GameCanvas GameCanvas =>
		Instance.m_GameCanvas || TryGetComponentInChildren(out Instance.m_GameCanvas) ?
		Instance.m_GameCanvas : null;

	static DialogueCanvas DialogueCanvas =>
		Instance.m_DialogueCanvas || TryGetComponentInChildren(out Instance.m_DialogueCanvas) ?
		Instance.m_DialogueCanvas : null;

	static InventoryCanvas InventoryCanvas =>
		Instance.m_InventoryCanvas || TryGetComponentInChildren(out Instance.m_InventoryCanvas) ?
		Instance.m_InventoryCanvas : null;



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
		var transform = Instance.transform;
		for (int i = 0; i < transform.childCount; i++) {
			if (transform.GetChild(i).TryGetComponent(out BaseCanvas canvas)) canvas.Hide();
		}
		MainCanvas = null;
		OverlayCanvas.Clear();
	}

	public static void Back() => CurrentCanvas?.Back();

	public static void PopOverlay() {
		if (OverlayCanvas.TryPop(out var next)) next.Hide();
		if (OverlayCanvas.TryPeek(out var prev)) prev.Show();
	}



	// Canvas Methods

	public static void ShowGame() => ShowMainCanvas(GameCanvas);

	static void ShowMainCanvas(BaseCanvas mainCanvas) {
		if (mainCanvas == CurrentCanvas) return;
		if (MainCanvas) MainCanvas.Hide();
		while (OverlayCanvas.TryPop(out var canvas)) canvas.Hide();
		MainCanvas = mainCanvas;
		mainCanvas.Show();
	}

	public static void OpenDialogue()  => OpenOverlayCanvas(DialogueCanvas);
	public static void OpenInventory() => OpenOverlayCanvas(InventoryCanvas);

	static void OpenOverlayCanvas(BaseCanvas overlayCanvas) {
		if (overlayCanvas == CurrentCanvas) return;
		if (OverlayCanvas.TryPeek(out var canvas)) canvas.Hide(true);
		OverlayCanvas.Push(overlayCanvas);
		overlayCanvas.Show();
	}



	// Game Canvas Methods

	public static void ShowGemCollectMessage(string message) {
		GameCanvas.ShowGemCollectMessage(message);
	}



	// Dialogue Canvas Methods

	public static void EnqueueDialogue(string name, string text, Action onEnd = null) {
		if (!DialogueCanvas.gameObject.activeSelf) OpenDialogue();
		DialogueCanvas.EnqueueDialogue(name, text, onEnd);
	}

	public static void BeginDialogueInput(Action<MultimodalData> onEnd = null) {
		if (!DialogueCanvas.gameObject.activeSelf) OpenDialogue();
		DialogueCanvas.BeginDialogueInput(onEnd);
	}



	// Lifecycle

	void Update() {
		
	}
}
