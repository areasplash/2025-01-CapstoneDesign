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
// Game Manager | Game State
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Game State")]
public class GameStateEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class GameStateEventNode : BaseEventNode {
			GameStateEvent I => target as GameStateEvent;

			public GameStateEventNode() : base() {
				mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultSize.x;
				var cyan = new StyleColor(new Color(38f / 255f, 152f / 255f, 152f / 255f));
				titleContainer.style.backgroundColor = cyan;
			}

			public override void ConstructData() {
				var state = new EnumField(GameState.Gameplay) { value = I.state };
				state.RegisterValueChangedCallback(evt => I.state = (GameState)evt.newValue);
				mainContainer.Add(state);
			}
		}
	#endif



	// Fields

	public GameState state = GameState.Gameplay;



	// Methods

	public override void End() {
		GameManager.GameState = state;
	}

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is GameStateEvent gameState) {
			state = gameState.state;
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Collect Gem
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Collect Gem")]
public class CollectGemEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class CollectGemEventNode : BaseEventNode {
			CollectGemEvent I => target as CollectGemEvent;

			public CollectGemEventNode() : base() {
				mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultSize.x;
			}

			public override void ConstructData() {
				var amount = new IntegerField() { value = I.amount };
				amount.RegisterValueChangedCallback(evt => I.amount = evt.newValue);
				mainContainer.Add(amount);
			}
		}
	#endif



	// Fields

	public int amount = 1;



	// Methods

	public override void End() {
		GameManager.CollectGem(amount);
	}

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is CollectGemEvent collectGem) {
			amount = collectGem.amount;
		}
	}
}



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

	public override BaseEvent GetNext() {
		// Get Index from UI Manager, User Selection
		var index = 0;
		foreach (var next in next) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) return next.data;
		}
		return null;
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
				mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultSize.x;
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

	public override MultimodalData GetMultimodalData() => data;
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Camera | Move Camera
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Camera/Move Camera")]
public class MoveCameraEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class MoveCameraEventNode : BaseEventNode {
			MoveCameraEvent I => target as MoveCameraEvent;

			public override void ConstructData() {
				var target   = new ObjectField("Target"  ) { value = I.target,   };
				var curve    = new CurveField ("Curve"   ) { value = I.curve,    };
				var duration = new FloatField ("Duration") { value = I.duration, };
				var async    = new Toggle     ("Async"   ) { value = I.async,    };
				target  .labelElement.style.minWidth = target  .labelElement.style.maxWidth =  60f;
				curve   .labelElement.style.minWidth = curve   .labelElement.style.maxWidth =  60f;
				duration.labelElement.style.minWidth = duration.labelElement.style.maxWidth =  60f;
				async   .labelElement.style.minWidth = async   .labelElement.style.maxWidth =  60f;
				target  .ElementAt(1).style.minWidth = target  .ElementAt(1).style.maxWidth = 144f;
				curve   .ElementAt(1).style.minWidth = curve   .ElementAt(1).style.maxWidth = 144f;
				duration.ElementAt(1).style.minWidth = duration.ElementAt(1).style.maxWidth = 144f;
				async   .ElementAt(1).style.minWidth = async   .ElementAt(1).style.maxWidth = 144f;
				target  .RegisterValueChangedCallback(evt => I.target   = evt.newValue as GameObject);
				curve   .RegisterValueChangedCallback(evt => I.curve    = evt.newValue);
				duration.RegisterValueChangedCallback(evt => I.duration = evt.newValue);
				async   .RegisterValueChangedCallback(evt => I.async    = evt.newValue);
				mainContainer.Add(target);
				mainContainer.Add(curve);
				mainContainer.Add(duration);
				mainContainer.Add(async);
			}
		}
	#endif



	// Fields

	public GameObject target;
	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	public float duration = 1f;



	// Methods

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is MoveCameraEvent moveCamera) {
			target   = moveCamera.target;
			curve.CopyFrom(moveCamera.curve);
			duration = moveCamera.duration;
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Camera | Track Camera
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Camera/Track Camera")]
public class TrackCameraEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class TrackCameraEventNode : BaseEventNode {
			TrackCameraEvent I => target as TrackCameraEvent;

			public override void ConstructData() {
				var root = new VisualElement();
				mainContainer.Add(root);
				var track = new Foldout() {
					text = "Track",
					value = true,
				};
				track.style.marginTop = 2f;
				for (int i = 0; i < I.track.Count; i++) {
					var index = i;

					var element = new VisualElement();
					element.style.flexDirection = FlexDirection.Row;
					track.Add(element);

					var value = I.track[index];
					var item1 = new Vector3Field() { value = value.Item1 };
					var item2 = new Toggle      () { value = value.Item2 };
					var x = item1.ElementAt(0).ElementAt(0);
					var y = item1.ElementAt(0).ElementAt(1);
					var z = item1.ElementAt(0).ElementAt(2);
					if (x != null) x.style.minWidth = x.style.maxWidth = 45f;
					if (y != null) y.style.minWidth = y.style.maxWidth = 45f;
					if (z != null) z.style.minWidth = z.style.maxWidth = 45f;
					item1.style.minWidth = item1.style.maxWidth = 145f;
					item2.style.minWidth = item2.style.maxWidth =  14f;
					item1.RegisterValueChangedCallback(evt => {
						value.Item1 = evt.newValue;
						I.track[index] = value;
					});
					item2.RegisterValueChangedCallback(evt => {
						value.Item2 = evt.newValue;
						I.track[index] = value;
					});
					element.Add(item1);
					element.Add(item2);

					var removeButton = new Button(() => {
						mainContainer.Remove(root);
						I.track.RemoveAt(index);
						ConstructData();
					}) { text = "-" };
					removeButton.style.width = 18f;
					element.Add(removeButton);
				}
				var addButton = new Button(() => {
					mainContainer.Remove(root);
					I.track.Add();
					ConstructData();
				}) { text = "Add" };
				track.Add(addButton);

				var anchor   = new ObjectField("Anchor"  ) { value = I.anchor,   };
				var curve    = new CurveField ("Curve"   ) { value = I.curve,    };
				var duration = new FloatField ("Duration") { value = I.duration, };
				var async    = new Toggle     ("Async"   ) { value = I.async,    };
				anchor  .labelElement.style.minWidth = anchor  .labelElement.style.maxWidth =  60f;
				curve   .labelElement.style.minWidth = curve   .labelElement.style.maxWidth =  60f;
				duration.labelElement.style.minWidth = duration.labelElement.style.maxWidth =  60f;
				async   .labelElement.style.minWidth = async   .labelElement.style.maxWidth =  60f;
				anchor  .ElementAt(1).style.minWidth = anchor  .ElementAt(1).style.maxWidth = 144f;
				curve   .ElementAt(1).style.minWidth = curve   .ElementAt(1).style.maxWidth = 144f;
				duration.ElementAt(1).style.minWidth = duration.ElementAt(1).style.maxWidth = 144f;
				async   .ElementAt(1).style.minWidth = async   .ElementAt(1).style.maxWidth = 144f;
				anchor  .RegisterValueChangedCallback(evt => I.anchor = evt.newValue as GameObject);
				curve   .RegisterValueChangedCallback(evt => I.curve    = evt.newValue);
				duration.RegisterValueChangedCallback(evt => I.duration = evt.newValue);
				async   .RegisterValueChangedCallback(evt => I.async    = evt.newValue);
				root.Add(anchor);
				root.Add(track);
				root.Add(curve);
				root.Add(duration);
				root.Add(async);
			}
		}
	#endif



	// Fields

	public GameObject anchor;
	public Track track = new();
	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	public float duration = 1f;



	// Methods

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is TrackCameraEvent trackCamera) {
			anchor = trackCamera.anchor;
			track.CopyFrom(trackCamera.track);
			curve.CopyFrom(trackCamera.curve);
			duration = trackCamera.duration;
		}
	}



	#if UNITY_EDITOR
		public override void DrawGizmos() {
			if (track.Count == 0) return;
			const float Height = 0.1f;

			var position = anchor ? anchor.transform.position : default;
			var prev = default(Vector3);
			for (float time = 0f; time <= duration; time += Time.deltaTime) {
				var s = curve.Evaluate(Mathf.Clamp01(time / duration)) * track.Distance;
				var next = position + track.Evaluate(s);
				if (time != 0f) Gizmos.DrawLine(prev, next);
				Gizmos.DrawLine(prev, prev + new Vector3(0f, -Height, 0f));
				prev = next;
			}
		}

		public override void DrawHandles() {
			if (track.Count == 0 || node == null) return;

			var position = anchor ? anchor.transform.position : default;
			var query = node.Query<Vector3Field>().ToList();
			for (int i = 0; i < query.Count; i++) {
				var handle = Handles.PositionHandle(position + query[i].value, Quaternion.identity);
				track[i] = (handle - position, track[i].Item2);
				query.ElementAt(i).value = handle - position;
			}
		}
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Camera | Shake Camera
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Camera/Shake Camera")]
public class ShakeCameraEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class ShakeCameraEventNode : BaseEventNode {
			ShakeCameraEvent I => target as ShakeCameraEvent;

			public ShakeCameraEventNode() : base() {
				mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultSize.x;
			}

			public override void ConstructData() {
				var strength = new FloatField("Strength") { value = I.strength };
				var duration = new FloatField("Duration") { value = I.duration };
				var width = DefaultSize.x * 0.5f - 5f;
				strength.labelElement.style.minWidth = strength.labelElement.style.maxWidth = width;
				duration.labelElement.style.minWidth = duration.labelElement.style.maxWidth = width;
				strength.ElementAt(1).style.minWidth = strength.ElementAt(1).style.maxWidth = width;
				duration.ElementAt(1).style.minWidth = duration.ElementAt(1).style.maxWidth = width;
				strength.RegisterValueChangedCallback(evt => I.strength = evt.newValue);
				duration.RegisterValueChangedCallback(evt => I.duration = evt.newValue);
				mainContainer.Add(strength);
				mainContainer.Add(duration);

				var async = new Toggle("Async") { value = I.async };
				async.labelElement.style.minWidth = async.labelElement.style.maxWidth = width;
				async.ElementAt(1).style.minWidth = async.ElementAt(1).style.maxWidth = width;
				async.RegisterValueChangedCallback(evt => I.async = evt.newValue);
				mainContainer.Add(async);
			}
		}
	#endif



	// Fields

	public float strength = 0f;
	public float duration = 0f;



	// Methods

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is ShakeCameraEvent shakeCamera) {
			strength = shakeCamera.strength;
			duration = shakeCamera.duration;
		}
	}
}
