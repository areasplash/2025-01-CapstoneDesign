using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UI Manager | Dialogue
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("UI Manager/Dialogue")]
public class DialogueEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class DialogueEventNode : BaseEventNode {
			DialogueEvent I => target as DialogueEvent;

			public override void ConstructData() {
				var root = new VisualElement();
				mainContainer.Add(root);
				for (int i = 0; i < I.texts.Count; i++) {
					var index = i;

					var element = new VisualElement();
					element.style.flexDirection = FlexDirection.Row;
					root.Add(element);

					var name = new TextField() { value = I.names[index] };
					name.style.minWidth = name.style.maxWidth = 180f;
					name.textEdition.placeholder = "Name";
					name.RegisterValueChangedCallback(evt => I.names[index] = evt.newValue);
					element.Add(name);

					var removeButton = new Button(() => {
						mainContainer.Remove(root);
						I.names.RemoveAt(index);
						I.texts.RemoveAt(index);
						ConstructData();
					}) { text = "-" };
					removeButton.style.width = 18f;
					element.Add(removeButton);

					var text = new TextField() { value = I.texts[index], multiline = true };
					text.style.minWidth = text.style.maxWidth = 204f;
					text.style.whiteSpace = WhiteSpace.Normal;
					text.textEdition.placeholder = "Text";
					var field = text.Q<VisualElement>(className: "unity-text-field__input");
					if (field != null) field.style.minHeight = 46f;
					text.RegisterValueChangedCallback(evt => I.texts[index] = evt.newValue);
					root.Add(text);
				}
				var addButton = new Button(() => {
					mainContainer.Remove(root);
					I.names.Add("");
					I.texts.Add("");
					ConstructData();
				}) { text = "Add Element" };
				root.Add(addButton);
			}
		}
	#endif



	// Fields

	public List<string> names = new() { "", };
	public List<string> texts = new() { "", };
	bool end;



	// Methods

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is DialogueEvent dialogue) {
			names.Clear();
			texts.Clear();
			names.AddRange(dialogue.names);
			texts.AddRange(dialogue.texts);
		}
	}

	public override void Start() {
		end = false;
		for (int i = 0; i < texts.Count; i++) {
			Action onEnd = (i == texts.Count - 1) ? () => end = true : null;
			UIManager.EnqueueDialogue(names[i], texts[i], onEnd);
		}
	}

	public override bool Update() {
		return end;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UI Manager | Branch
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("UI Manager/Branch")]
public class BranchEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class BranchEventNode : BaseEventNode {
			BranchEvent I => target as BranchEvent;

			public override void ConstructData() {
				var root = new VisualElement();
				mainContainer.Add(root);
				for (int i = 0; i < I.texts.Count; i++) {
					var index = i;

					var element = new VisualElement();
					element.style.flexDirection = FlexDirection.Row;
					root.Add(element);

					var text = new TextField() { value = I.texts[index] };
					text.style.minWidth = text.style.maxWidth = 180f;
					text.textEdition.placeholder = "Text";
					text.RegisterValueChangedCallback(evt => {
						I.texts[index] = evt.newValue;
						(outputContainer[index] as Port).portName = evt.newValue;
					});
					element.Add(text);

					var removeButton = new Button(() => {
						mainContainer.Remove(root);
						I.texts.RemoveAt(index);
						ConstructData();
						outputContainer.RemoveAt(index);
					}) { text = "-" };
					removeButton.style.width = 18f;
					element.Add(removeButton);
				}
				var addButton = new Button(() => {
					mainContainer.Remove(root);
					I.texts.Add("");
					ConstructData();
					var port = CreatePort(Direction.Output);
					port.portName = I.texts[^1];
					outputContainer.Add(port);
				}) { text = "Add Element" };
				root.Add(addButton);
			}

			public override void ConstructPort() {
				CreatePort(Direction.Input);
				for (int i = 0; i < I.texts.Count; i++) {
					var port = CreatePort(Direction.Output);
					port.style.maxWidth = 154f;
					port.portName = I.texts[i];
				}
				RefreshExpandedState();
				RefreshPorts();
			}
		}
	#endif



	// Fields

	public List<string> texts = new() { "", "", };



	// Methods

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is BranchEvent branch) {
			texts.Clear();
			texts.AddRange(branch.texts);
		}
	}

	public override void GetNext(List<BaseEvent> list) {
		list ??= new();
		list.Clear();
		// Get Index from UI Manager, User Selection
		int index = 0;
		foreach (var next in next) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.data);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UI Manager | Dialogue Input
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("UI Manager/Dialogue Input")]
public class DialogueInputEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
	public class DialogueInputEventNode : BaseEventNode {
		DialogueInputEvent I => target as DialogueInputEvent;

		public DialogueInputEventNode() : base() {
			mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultNodeWidth;
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			CreatePort(Direction.Output, PortType.MultimodalData);
		}
	}
	#endif



	// Fields

	MultimodalData data;
	bool end;



	// Methods

	public override void Start() {
		end = false;
		UIManager.BeginDialogueInput(data => {
			this.data = data;
			end = true;
		});
	}

	public override bool Update() {
		return end;
	}

	public override void GetMultimodalData(List<MultimodalData> list) {
		list ??= new();
		list.Add(data);
	}
}
