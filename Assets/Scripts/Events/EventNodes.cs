using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

#if UNITY_EDITOR
	using UnityEditor;
	using UnityEditor.UIElements;
	using UnityEditor.Experimental.GraphView;
#endif



// ━

public enum PortType : byte {
	Default,
	GameObject,
	MultimodalData,
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class NodeMenuAttribute : Attribute {
	public string Path { get; }
	public NodeMenuAttribute(string path) => Path = path;
}

public static class ListExtensions {
	public static void CopyFrom<T>(this List<T> a, List<T> b) {
		a.Clear();
		a.AddRange(b);
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Base Event
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Serializable]
public abstract class BaseEvent {

	// Node

	#if UNITY_EDITOR
		public abstract class BaseEventNode : Node {
			const Orientation Horizontal = Orientation.Horizontal;
			const Port.Capacity Single = Port.Capacity.Single;
			const Port.Capacity Multi  = Port.Capacity.Multi;

			public static readonly Vector2 DefaultSize = new(128f, 96f);
			public static List<string> Dropdown {
				get {
					var cache = TypeCache.GetTypesWithAttribute<NodeMenuAttribute>();
					var types = cache.Where(type => typeof(BaseEvent).IsAssignableFrom(type));
					var dropdown = types.Select(type => {
						var attribute = type.GetCustomAttributes(typeof(NodeMenuAttribute), false);
						return attribute.Cast<NodeMenuAttribute>().First().Path;
					}).OrderBy(path => path).ToList();
					return dropdown;
				}
			}



			public BaseEvent target;

			public BaseEventNode() {
				var name = ToString().Split(" ")[0][..^4];
				var type = Type.GetType(name);
				title  = Regex.Replace(name[..^5], "(?<=[a-z])(?=[A-Z])", " ");
				target = Activator.CreateInstance(type) as BaseEvent;
				target.node = this;
				mainContainer.style.backgroundColor = new StyleColor(new Color(0.14f, 0.14f, 0.14f));
			}



			public virtual void ConstructData() { }
			public virtual void ConstructPort() {
				CreatePort(Direction.Input );
				CreatePort(Direction.Output);
				RefreshExpandedState();
				RefreshPorts();
			}

			protected Port CreatePort(Direction direction, PortType type = PortType.Default) {
				var input = direction == Direction.Input;
				var port  = default(Port);
				switch (type) {
					case PortType.Default:
						port = InstantiatePort(Horizontal, direction, input ? Multi : Single, null);
						port.portColor = new Color(1.0f, 1.0f, 1.0f);
						port.portName = input ? "Prev" : "Next";
						break;
					case PortType.GameObject:
						port = InstantiatePort(Horizontal, direction, Multi, null);
						port.portColor = new Color(0.0f, 0.8f, 1.0f);
						port.portName = input ? "In" : "Out";
						break;
					case PortType.MultimodalData:
						port = InstantiatePort(Horizontal, direction, Single, null);
						port.portColor = new Color(0.8f, 0.5f, 1.0f);
						port.portName = input ? "In" : "Out";
						break;
				}
				if (port != null) {
					port.userData = type;
					if (input) inputContainer .Add(port);
					else       outputContainer.Add(port);
				}
				return port;
			}
		}
	#endif



	// Constants

	[Serializable]
	public class Connection {
		[SerializeReference] public BaseEvent data;
		public int iPort;
		public int oPort;
		public PortType iPortType;
		public PortType oPortType;
	}



	// Fields

	public string guid;
	[SerializeReference] public List<Connection> prev = new();
	[SerializeReference] public List<Connection> next = new();
	public Vector2 position;
	public bool async;

	#if UNITY_EDITOR
		[NonSerialized] public BaseEventNode node;
	#endif



	// Methods

	public BaseEvent() => guid = Guid.NewGuid().ToString();

	public List<BaseEvent> GetEvents() {
		var stack = new Stack<BaseEvent>();
		var list  = new List <BaseEvent>();
		stack.Push(this);
		while (0 < stack.Count) {
			var data = stack.Pop();
			if (list.Contains(data)) continue;
			else list.Add(data);
			if (data.prev != null) foreach (var prev in data.prev) stack.Push(prev.data);
			if (data.next != null) foreach (var next in data.next) stack.Push(next.data);
		}
		return list;
	}

	public virtual void CopyFrom(BaseEvent data) {
		guid = data.guid;
		prev = data.prev;
		next = data.next;
		position = data.position;
		async = data.async;
	}

	public virtual void Start () { }
	public virtual bool Update() => true;
	public virtual void End   () { }

	public virtual BaseEvent GetNext() {
		foreach (var next in next) if (next.oPortType == PortType.Default) {
			if (next.oPort == 0) return next.data;
		}
		return null;
	}
	public virtual GameObject     GetGameObject    () => null;
	public virtual MultimodalData GetMultimodalData() => null;

	#if UNITY_EDITOR
		public virtual void DrawGizmos () { }
		public virtual void DrawHandles() { }
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Entry
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class EntryEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class EntryEventNode : BaseEventNode {
			EntryEvent I => target as EntryEvent;

			public EntryEventNode() : base() {
				capabilities &= ~Capabilities.Deletable;
				var bluegreen = new StyleColor(new Color(38f / 255f, 152f / 255f, 114f / 255f));
				titleContainer.style.backgroundColor = bluegreen;
			}

			public override void ConstructPort() {
				CreatePort(Direction.Output);
				RefreshExpandedState();
				RefreshPorts();
			}
		}
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Debug | Log
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Debug/Log")]
public class LogEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class LogEventNode : BaseEventNode {
			LogEvent I => target as LogEvent;

			public LogEventNode() : base() {
				mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultSize.x;
			}

			public override void ConstructData() {
				var text = new TextField() { value = I.text, multiline = true };
				text.RegisterValueChangedCallback(evt => I.text = evt.newValue);
				mainContainer.Add(text);
			}
		}
	#endif



	// Fields

	public string text = "Debug";



	// Methods

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is LogEvent delay) {
			text = delay.text;
		}
	}

	#if UNITY_EDITOR
		public override void End() => Debug.Log(text);
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Logic | Delay
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Logic/Delay")]
public class DelayEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class DelayEventNode : BaseEventNode {
			DelayEvent I => target as DelayEvent;

			public DelayEventNode() : base() {
				mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultSize.x;
			}

			public override void ConstructData() {
				var time = new FloatField() { value = I.time };
				time.RegisterValueChangedCallback(evt => I.time = evt.newValue);
				mainContainer.Add(time);
			}
		}
	#endif



	// Fields

	public float time = 0.1f;

	float timer = 0f;



	// Methods

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is DelayEvent delay) {
			time = delay.time;
		}
	}

	public override void Start () =>  timer = time;
	public override bool Update() => (timer -= Time.deltaTime) <= 0f;
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Logic | Once Then
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Logic/Once Then")]
public class OnceThenEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class OnceThenEventNode : BaseEventNode {
			OnceThenEvent I => target as OnceThenEvent;

			public OnceThenEventNode() : base() {
				mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultSize.x;
			}

			public override void ConstructPort() {
				CreatePort(Direction.Input );
				CreatePort(Direction.Output).portName = "Once";
				CreatePort(Direction.Output).portName = "Then";
				RefreshExpandedState();
				RefreshPorts();
			}
		}
	#endif



	// Fields

	bool value = false;



	// Methods

	public override BaseEvent GetNext() {
		var index = !value ? 0 : 1;
		value = true;
		foreach (var next in next) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) return next.data;
		}
		return null;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Logic | Repeat
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Logic/Repeat")]
public class RepeatEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class RepeatEventNode : BaseEventNode {
			RepeatEvent I => target as RepeatEvent;

			public RepeatEventNode() : base() {
				mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultSize.x;
			}

			public override void ConstructData() {
				var count = new IntegerField() { value = I.count };
				count.RegisterValueChangedCallback(evt => I.count = evt.newValue);
				mainContainer.Add(count);
			}

			public override void ConstructPort() {
				CreatePort(Direction.Input );
				CreatePort(Direction.Output).portName = "While";
				CreatePort(Direction.Output).portName = "Break";
				RefreshExpandedState();
				RefreshPorts();
			}
		}
	#endif



	// Fields

	public int count = 1;

	int value = 0;



	// Methods

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is RepeatEvent repeat) {
			count = repeat.count;
		}
	}

	public override void Start () {
		if (count < value) value = 0;
	}

	public override BaseEvent GetNext() {
		var index = value++ < count ? 0 : 1;
		foreach (var next in next) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) return next.data;
		}
		return null;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Logic | Random
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Logic/Random")]
public class RandomEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class RandomEventNode : BaseEventNode {
			RandomEvent I => target as RandomEvent;

			public override void ConstructData() {
				var root = new VisualElement();
				mainContainer.Add(root);
				for (int i = 0; i < I.weights.Count; i++) {
					var index = i;

					var element = new VisualElement();
					element.style.flexDirection = FlexDirection.Row;
					root.Add(element);

					var weight = new FloatField() { value = I.weights[index] };
					weight.style.width = 120f;
					weight.RegisterValueChangedCallback(evt => {
						I.weights[index] = evt.newValue;
						UpdateProbability();
					});
					element.Add(weight);

					var removeButton = new Button(() => {
						mainContainer.Remove(root);
						I.weights.RemoveAt(index);
						ConstructData();
						outputContainer.RemoveAt(index);
						UpdateProbability();
					}) { text = "-" };
					removeButton.style.width = 18f;
					element.Add(removeButton);
				}
				var addButton = new Button(() => {
					mainContainer.Remove(root);
					I.weights.Add(1f);
					ConstructData();
					CreatePort(Direction.Output);
					UpdateProbability();
				}) { text = "Add" };
				root.Add(addButton);
			}

			public override void ConstructPort() {
				CreatePort(Direction.Input);
				for (int i = 0; i < I.weights.Count; i++) {
					CreatePort(Direction.Output);
				}
				UpdateProbability();
				RefreshExpandedState();
				RefreshPorts();
			}

			void UpdateProbability() {
				var sum = 0f;
				foreach (var weight in I.weights) sum += weight;
				if (sum == 0f) sum = 1f;
				var ports = outputContainer.Children().OfType<Port>().ToList();
				for (int i = 0; i < ports.Count; i++) {
					ports[i].portName = $"{(100f * I.weights[i] / sum).ToString("F1")}%";
				}
			}
		}
	#endif



	// Fields

	public List<float> weights = new() { 1f, 1f, };



	// Methods

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is RandomEvent random) {
			weights.CopyFrom(random.weights);
		}
	}

	public override BaseEvent GetNext() {
		var sum = 0f;
		foreach (var weight in weights) sum += weight;
		var random = Random.Range(0f, sum);
		var index = weights.FindIndex(weight => (random -= weight) <= 0f);
		if (index == -1) index = weights.Count - 1;
		foreach (var next in next) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) return next.data;
		}
		return null;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// GameObject | Object
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("GameObject/Object")]
public class ObjectEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class ObjectEventNode : BaseEventNode {
			ObjectEvent I => target as ObjectEvent;

			public ObjectEventNode() : base() {
				mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultSize.x;
				var skyblue = new StyleColor(new Color(38f / 255f, 114f / 255f, 152f / 255f));
				titleContainer.style.backgroundColor = skyblue;
			}

			public override void ConstructData() {
				var instance = new ObjectField() { value = I.instance };
				instance.RegisterValueChangedCallback(evt => I.instance = evt.newValue as GameObject);
				mainContainer.Add(instance);
			}

			public override void ConstructPort() {
				CreatePort(Direction.Output, PortType.GameObject);
				RefreshExpandedState();
				RefreshPorts();
			}
		}
	#endif



	// Fields

	public GameObject instance;



	// Methods

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is ObjectEvent gameObject) {
			instance = gameObject.instance;
		}
	}

	public override GameObject GetGameObject() => instance;
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// GameObject | Instantiate Object
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("GameObject/Instantiate Object")]
public class InstantiateObjectEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class InstantiateObjectEventNode : BaseEventNode {
			InstantiateObjectEvent I => target as InstantiateObjectEvent;

			public InstantiateObjectEventNode() : base() {
				var skyblue = new StyleColor(new Color(38f / 255f, 114f / 255f, 152f / 255f));
				titleContainer.style.backgroundColor = skyblue;
			}

			public override void ConstructData() {
				var prefab = new ObjectField ("Prefab") { value = I.prefab };
				var anchor = new ObjectField ("Anchor") { value = I.anchor };
				var offset = new Vector3Field("Offset") { value = I.offset };
				prefab.labelElement.style.minWidth = prefab.labelElement.style.maxWidth =  60f;
				anchor.labelElement.style.minWidth = anchor.labelElement.style.maxWidth =  60f;
				offset.labelElement.style.minWidth = offset.labelElement.style.maxWidth =  60f;
				prefab.ElementAt(1).style.minWidth = prefab.ElementAt(1).style.maxWidth = 144f;
				anchor.ElementAt(1).style.minWidth = anchor.ElementAt(1).style.maxWidth = 144f;
				offset.ElementAt(1).style.minWidth = offset.ElementAt(1).style.maxWidth = 144f;
				prefab.RegisterValueChangedCallback(evt => I.prefab = evt.newValue as GameObject);
				anchor.RegisterValueChangedCallback(evt => I.anchor = evt.newValue as GameObject);
				offset.RegisterValueChangedCallback(evt => I.offset = evt.newValue);
				mainContainer.Add(prefab);
				mainContainer.Add(anchor);
				mainContainer.Add(offset);
			}

			public override void ConstructPort() {
				CreatePort(Direction.Input );
				CreatePort(Direction.Output);
				CreatePort(Direction.Output, PortType.GameObject);
				RefreshExpandedState();
				RefreshPorts();
			}
		}
	#endif



	// Fields

	public GameObject prefab;
	public GameObject anchor;
	public Vector3    offset;

	GameObject instance;



	// Methods

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is InstantiateObjectEvent instantiate) {
			prefab = instantiate.prefab;
			anchor = instantiate.anchor;
			offset = instantiate.offset;
		}
	}

	public override void Start() {
		instance = null;
	}

	public override void End() {
		if (!instance && prefab) {
			var position = anchor ? anchor.transform.position : default;
			instance = Object.Instantiate(prefab, position + offset, anchor.transform.rotation);
		}
	}

	public override GameObject GetGameObject() {
		End();
		return instance;
	}



	#if UNITY_EDITOR
		public override void DrawGizmos() {
			if (!prefab) return;
			const float Sample = 8.0f;
			const float Radius = 0.5f;
			const float Height = 0.1f;
			float sin(float f) => Mathf.Sin(f) * Radius;
			float cos(float f) => Mathf.Cos(f) * Radius;
			var segments = Mathf.Max(3, Mathf.RoundToInt(Sample * 2f * Mathf.PI * Radius));
			var step = 2f * Mathf.PI / segments;

			var position = anchor ? anchor.transform.position : default;
			var prev = position + offset + new Vector3(cos(0), Height, sin(0));
			for (int i = 0; i < segments; i++) {
				float f = (i + 1) * step;
				var next = position + offset + new Vector3(cos(f), Height, sin(f));
				Gizmos.DrawLine(prev, next);
				prev = next;
			}
		}

		public override void DrawHandles() {
			if (!prefab | node == null) return;
			var position = anchor ? anchor.transform.position : default;
			var handle = Handles.PositionHandle(position + offset, Quaternion.identity);
			node.Q<Vector3Field>().value = offset = handle - position;
		}
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// GameObject | Destroy Object
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("GameObject/Destroy Object")]
public class DestroyObjectEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class DestroyObjectEventNode : BaseEventNode {
			DestroyObjectEvent I => target as DestroyObjectEvent;

			public DestroyObjectEventNode() : base() {
				mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultSize.x;
				var skyblue = new StyleColor(new Color(38f / 255f, 114f / 255f, 152f / 255f));
				titleContainer.style.backgroundColor = skyblue;
			}

			public override void ConstructPort() {
				CreatePort(Direction.Input );
				CreatePort(Direction.Output);
				CreatePort(Direction.Input, PortType.GameObject);
				RefreshExpandedState();
				RefreshPorts();
			}
		}
	#endif



	// Methods

	public override void End() {
		foreach (var prev in prev) {
			if (prev.oPortType == PortType.GameObject) {
				var instance = prev.data.GetGameObject();
				if (instance) Object.Destroy(instance);
			}
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Multimodal | Validate
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Multimodal/Validate Answer")]
public class ValidateAnswerEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class ValidateAnswerEventNode : BaseEventNode {
			ValidateAnswerEvent I => target as ValidateAnswerEvent;

			public ValidateAnswerEventNode() : base() {
				var purple = new StyleColor(new Color(114f / 255f, 38f / 255f, 152f / 255f));
				titleContainer.style.backgroundColor = purple;
			}

			public override void ConstructData() {
				var prompt = new TextField() { value = I.prompt, multiline = true };
				prompt.style.minWidth = prompt.style.maxWidth = 204f;
				prompt.style.whiteSpace = WhiteSpace.Normal;
				prompt.textEdition.placeholder = "Prompt";
				var field = prompt.Q<VisualElement>(className: "unity-text-field__input");
				if (field != null) field.style.minHeight = 71f;
				prompt.RegisterValueChangedCallback(evt => I.prompt = evt.newValue);
				mainContainer.Add(prompt);
			}

			public override void ConstructPort() {
				CreatePort(Direction.Input );
				CreatePort(Direction.Input, PortType.MultimodalData);
				var pass = CreatePort(Direction.Output);
				var fail = CreatePort(Direction.Output);
				pass.portName = "True";
				fail.portName = "False";
				RefreshExpandedState();
				RefreshPorts();
			}
		}
	#endif



	// Fields

	public string prompt;

	float timer;
	bool isValid;



	// Methods

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is ValidateAnswerEvent validateAnswer) {
			prompt = validateAnswer.prompt;
		}
	}

	public override void Start() {
		timer = 3f;
		// Send the prompt to the server
	}

	public override bool Update() {
		if (0f < timer) {
			timer -= Time.deltaTime;
			// wait for the server response
			if (timer <= 0f) {
				// timeout
				isValid = true;
				return true;
			}
		}
		isValid = true;
		return false;
	}

	public override BaseEvent GetNext() {
		var index = isValid ? 0 : 1;
		foreach (var next in next) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) return next.data;
		}
		return null;
	}
}
