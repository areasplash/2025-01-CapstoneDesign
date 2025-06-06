using UnityEngine;
using System;
using System.Collections.Generic;

using TMPro;

#if UNITY_EDITOR
	using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Dialogue Canvas
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI/Dialogue Canvas")]
public class DialogueCanvas : BaseCanvas {

	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(DialogueCanvas))]
		class DialogueCanvasEditor : EditorExtensions {
			DialogueCanvas I => target as DialogueCanvas;
			public override void OnInspectorGUI() {
				Begin("Dialogue Canvas");

				LabelField("Speaker Name", EditorStyles.boldLabel);
				I.NameTransform = ObjectField("Name Transform", I.NameTransform);
				I.NameTextUGUI  = ObjectField("Name Text UGUI", I.NameTextUGUI);
				if (I.NameTextUGUI) I.NameText = TextField("Name Text", I.NameText);
				Space();
				LabelField("Dialogue Text", EditorStyles.boldLabel);
				I.TextTransform = ObjectField("Text Transform", I.TextTransform);
				I.TextTextUGUI  = ObjectField("Text Text UGUI", I.TextTextUGUI);
				if (I.TextTextUGUI) I.TextText = TextArea("Text Text", I.TextText);
				Space();
				LabelField("Player Input", EditorStyles.boldLabel);
				I.InputField = ObjectField("Input Field", I.InputField);
				if (I.InputField) I.InputText   = TextField("Input Text",   I.InputText);
				if (I.InputField) I.EnableInput = Toggle   ("Enable Input", I.EnableInput);
				Space();
				LabelField("Settings", EditorStyles.boldLabel);
				I.TextDisplayDelay = Slider("Text Display Delay", I.TextDisplayDelay, 0.01f, 0.10f);
				I.AutoPlay         = Toggle("Auto Play",          I.AutoPlay);
				if (I.AutoPlay) I.AutoPlayDelay = Slider("Auto Play Delay", I.AutoPlayDelay, 1f, 10f);
				Space();

				End();
			}
		}
	#endif



	// Fields

	[SerializeField] RectTransform   m_NameTransform;
	[SerializeField] TextMeshProUGUI m_NameTextUGUI;
	[SerializeField] RectTransform   m_TextTransform;
	[SerializeField] TextMeshProUGUI m_TextTextUGUI;
	[SerializeField] TMP_InputField  m_InputField;

	[SerializeField] float m_TextDisplayDelay = 0.04f;
	float m_TextDisplayTimer;
	int   m_TextIndex;
	[SerializeField] bool  m_AutoPlay;
	[SerializeField] float m_AutoPlayDelay = 2.0f;

	Queue<(string name, string text, Action onEnd)> m_DialogueQueue = new();



	// Properties

	RectTransform NameTransform {
		get => m_NameTransform;
		set => m_NameTransform = value;
	}
	TextMeshProUGUI NameTextUGUI {
		get => m_NameTextUGUI;
		set => m_NameTextUGUI = value;
	}
	string NameText {
		get => NameTextUGUI.text;
		set {
			NameTextUGUI.text = value;
			if (NameTransform) {
				float width = Mathf.Max(80f, NameTextUGUI.GetPreferredValues().x);
				NameTransform.sizeDelta = new Vector2(width, NameTransform.sizeDelta.y);
			}
		}
	}

	RectTransform TextTransform {
		get => m_TextTransform;
		set => m_TextTransform = value;
	}
	TextMeshProUGUI TextTextUGUI {
		get => m_TextTextUGUI;
		set => m_TextTextUGUI = value;
	}
	string TextText {
		get => TextTextUGUI.text;
		set => TextTextUGUI.text = value;
	}

	TMP_InputField InputField {
		get => m_InputField;
		set => m_InputField = value;
	}
	string InputText {
		get => InputField.text;
		set => InputField.text = value;
	}
	bool EnableInput {
		get => InputField.gameObject.activeSelf;
		set {
			InputField.gameObject.SetActive(value);
			if (TextTransform) {
				TextTransform.anchoredPosition = new Vector2(0f, EnableInput ? 0f : -12f);
			}
		}
	}



	float TextDisplayDelay {
		get => m_TextDisplayDelay;
		set => m_TextDisplayDelay = value;
	}
	float TextDisplayTimer {
		get => m_TextDisplayTimer;
		set => m_TextDisplayTimer = value;
	}
	int TextIndex {
		get => m_TextIndex;
		set => m_TextIndex = value;
	}

	bool AutoPlay {
		get => m_AutoPlay;
		set => m_AutoPlay = value;
	}
	float AutoPlayDelay {
		get => m_AutoPlayDelay;
		set => m_AutoPlayDelay = value;
	}



	Queue<(string name, string text, Action onEnd)> DialogueQueue => m_DialogueQueue;



	// Methods

	public void EnqueueDialogue(string name, string text, Action onEnd = null) {
		DialogueQueue.Enqueue((name, text, onEnd));
	}

	public void BeginDialogueInput(Action<MultimodalData> onEnd = null) {
		InputText = "";
		EnableInput = true;
		InputField.onEndEdit.RemoveAllListeners();
		InputField.onEndEdit.AddListener(input => {
			EnableInput = false;
			onEnd?.Invoke(new MultimodalData { text = input });
		});
		UIManager.SelectedGameObject = InputField.gameObject;
	}



	bool TryGetFunction(string text, int start, out int end, out string func, out string[] args) {
		static bool IsValid(int a, int b) => (0 <= a) && (a < b);
		int a = text.IndexOf('{', start);
		int b = text.IndexOf('}', a + 1);
		if (IsValid(a, b)) {
			var fullCommand = text[(a + 1)..b];
			int c = fullCommand.    IndexOf('(');
			int d = fullCommand.LastIndexOf(')');
			bool isValid = IsValid(c, d);
			end  = b + 1;
			func = isValid ? fullCommand[..c] : fullCommand;
			args = isValid ? fullCommand[(c + 1)..d].Split(',') : null;
			return true;
		} else {
			end  = -1;
			func = null;
			args = null;
			return false;
		}
	}

	void UpdateDialogue() {
		TextDisplayTimer -= Time.deltaTime;
		if (DialogueQueue.TryPeek(out var value)) {
			var (name, text, onEnd) = value;
			bool initialize = TextIndex == 0;
			if (initialize) {
				NameText = name;
				TextText = "";
			}
			bool displayInstantly = false;
			displayInstantly |= InputManager.GetKeyDown(KeyAction.Submit);
			displayInstantly |= InputManager.GetKeyDown(KeyAction.Cancel);
			while (TextIndex < text.Length && (TextDisplayTimer <= 0f || displayInstantly)) {
				char next = text[TextIndex];
				bool flag = next == '{';
				if (flag && TryGetFunction(text, TextIndex, out int end, out var func, out var args)) {
					bool isArgsValid = args != null && 0 < args.Length;
					switch (func) {
						case "Sleep":
							if (isArgsValid && float.TryParse(args[0], out float delay)) {
								TextDisplayTimer = delay;
							}
							break;
					}
					TextIndex = end;
				} else {
					TextText += next;
					TextDisplayTimer = TextDisplayDelay;
					TextIndex++;
				}
				if (TextIndex == text.Length) {
					TextDisplayTimer = AutoPlayDelay;
				}
			}
			bool dequeueDialogue = false;
			dequeueDialogue |= TextIndex == text.Length && InputManager.GetKeyDown(KeyAction.Submit);
			dequeueDialogue |= TextIndex == text.Length && AutoPlay && (TextDisplayTimer <= 0f);
			if (dequeueDialogue) {
				TextDisplayTimer = 0f;
				TextIndex = 0;
				DialogueQueue.Dequeue();
				onEnd?.Invoke();
			}
		} else if (EnableInput) {
			TextDisplayTimer = 0f;
		} else {
			UIManager.ForceBack();
		}
	}



	// Lifecycle

	void Start() {
		EnableInput = false;	
	}

	void Update() {
		UpdateDialogue();
	}
}
