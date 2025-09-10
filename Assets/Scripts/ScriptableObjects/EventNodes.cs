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
using static EditorVisualElement;
#endif



// ━

public enum PortType : byte {
	Default,
	Object,
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
// Event Base
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[Serializable]
public abstract class EventBase {

	// Node

	#if UNITY_EDITOR
	public abstract class EventNodeBase : Node {
		public static List<string> Dropdown {
			get {
				var cache = TypeCache.GetTypesWithAttribute<NodeMenuAttribute>();
				var types = cache.Where(type => typeof(EventBase).IsAssignableFrom(type));
				var dropdown = types.Select(type => {
					var attribute = type.GetCustomAttributes(typeof(NodeMenuAttribute), false);
					return attribute.Cast<NodeMenuAttribute>().First().Path;
				}).OrderBy(path => path).ToList();
				return dropdown;
			}
		}



		public EventBase target;

		public EventNodeBase() {
			var name = ToString().Split(" ")[0][..^4];
			var type = Type.GetType(name);
			title = Regex.Replace(name[..^5], "(?<=[a-z])(?=[A-Z])", " ");
			target = Activator.CreateInstance(type) as EventBase;
			target.node = this;
			var gray = new Color(0.2f, 0.2f, 0.2f);
			mainContainer.style.backgroundColor = gray;
		}



		public virtual void ConstructData() { }
		public virtual void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			RefreshExpandedState();
			RefreshPorts();
		}

		protected Port CreatePort(Direction direction, PortType type = PortType.Default) {
			var isInput = direction == Direction.Input;
			var port = default(Port);
			switch (type) {
				case PortType.Default: {
					var orientation = Orientation.Horizontal;
					var capacity = Port.Capacity.Multi;
					port = InstantiatePort(orientation, direction, capacity, null);
					port.portColor = new Color(1.0f, 1.0f, 1.0f);
					port.portName = isInput ? "Prev" : "Next";
				} break;
				case PortType.Object: {
					var orientation = Orientation.Horizontal;
					var capacity = Port.Capacity.Multi;
					port = InstantiatePort(orientation, direction, capacity, typeof(GameObject));
					port.portColor = new Color(0.0f, 0.8f, 1.0f);
					port.portName = isInput ? "In" : "Out";
				} break;
				case PortType.MultimodalData: {
					var orientation = Orientation.Horizontal;
					var capacity = Port.Capacity.Multi;
					port = InstantiatePort(orientation, direction, capacity, null);
					port.portColor = new Color(0.8f, 0.2f, 0.8f);
					port.portName = isInput ? "In" : "Out";
				} break;
			}
			port.userData = type;
			switch (isInput) {
				case true:  inputContainer.Add(port);  break;
				case false: outputContainer.Add(port); break;
			}
			return port;
		}
	}
	#endif



	// Constants

	[Serializable]
	public struct Connection {
		[SerializeReference] public EventBase eventBase;
		public byte iPort;
		public byte oPort;
		public PortType iPortType;
		public PortType oPortType;
	}



	// Fields

	#if UNITY_EDITOR
	public string guid;
	public EventNodeBase node;
	public Vector2 position;
	#endif

	[SerializeField] public List<Connection> prevs = new();
	[SerializeField] public List<Connection> nexts = new();



	// Methods

	public EventBase() => guid = Guid.NewGuid().ToString();

	public virtual void CopyFrom(EventBase eventBase) {
		guid = eventBase.guid;
		position = eventBase.position;
		prevs.CopyFrom(eventBase.prevs);
		nexts.CopyFrom(eventBase.nexts);
	}

	public virtual void Start() { }
	public virtual bool Update() => true;
	public virtual void End() { }

	public virtual void GetNext(List<EventBase> list) {
		foreach (var next in nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == 0) list.Add(next.eventBase);
		}
	}
	public virtual void GetObjects(List<GameObject> list) { }
	public virtual void GetMultimodalData(List<MultimodalData> list) { }



	#if UNITY_EDITOR
	public List<EventBase> GetEvents() {
		var queue = new Queue<EventBase>();
		var stack = new Stack<EventBase>();
		stack.Push(this);
		while (stack.TryPop(out var eventBase)) {
			if (queue.Contains(eventBase)) continue;
			queue.Enqueue(eventBase);
			foreach (var prev in eventBase.prevs) stack.Push(prev.eventBase);
			foreach (var next in eventBase.nexts) stack.Push(next.eventBase);
		}
		return queue.ToList();
	}

	public virtual void DrawGizmos() { }
	public virtual void DrawHandles() { }
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Entry
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public sealed class EntryEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public sealed class EntryEventNode : EventNodeBase {
		EntryEvent I => target as EntryEvent;

		public EntryEventNode() : base() {
			capabilities &= ~Capabilities.Deletable;
			var bluegreen = new Color(160f, 0.75f, 0.60f).ToRGB();
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
public sealed class LogEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public sealed class LogEventNode : EventNodeBase {
		LogEvent I => target as LogEvent;

		public LogEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var message = TextField(I.message, value => I.message = value);
			message.textEdition.placeholder = "Message";
			message.multiline = true;
			mainContainer.Add(message);
		}
	}
	#endif



	// Fields

	public string message = string.Empty;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is LogEvent logEvent) {
			message = logEvent.message;
		}
	}

	public override void End() {
		Debug.Log(message);
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Logic | Delay
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Logic/Delay")]
public sealed class DelayEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public sealed class DelayEventNode : EventNodeBase {
		DelayEvent I => target as DelayEvent;

		public DelayEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var delay = FloatField(I.delay, value => I.delay = value);
			mainContainer.Add(delay);
		}
	}
	#endif



	// Fields

	public float delay = 0.1f;

	float time = 0f;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is DelayEvent delayEvent) {
			delay = delayEvent.delay;
		}
	}

	public override void Start() {
		time = Time.time;
	}

	public override bool Update() {
		return delay <= (Time.time - time);
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Logic | Once Then
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Logic/Once Then")]
public sealed class OnceThenEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public sealed class OnceThenEventNode : EventNodeBase {
		OnceThenEvent I => target as OnceThenEvent;

		public OnceThenEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
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

	public override void GetNext(List<EventBase> list) {
		int index = !value ? 0 : 1;
		foreach (var next in nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
		value = true;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Logic | Repeat
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Logic/Repeat")]
public sealed class RepeatEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public sealed class RepeatEventNode : EventNodeBase {
		RepeatEvent I => target as RepeatEvent;

		public RepeatEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var count = IntField(I.count, value => I.count = value);
			mainContainer.Add(count);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
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

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is RepeatEvent repeatEvent) {
			count = repeatEvent.count;
		}
	}

	public override void Start() {
		value = (value <= count) ? value : 0;
	}

	public override void GetNext(List<EventBase> list) {
		int index = (value++ < count) ? 0 : 1;
		foreach (var next in nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Logic | Randomize
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Logic/Randomize")]
public sealed class RandomizeEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public sealed class RandomizeEventNode : EventNodeBase {
		RandomizeEvent I => target as RandomizeEvent;

		public RandomizeEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var weights = new VisualElement();
			for (int i = 0; i < I.weights.Count; i++) {
				int index = i;
				var element = new VisualElement();
				element.style.flexDirection = FlexDirection.Row;
				var weight = FloatField(I.weights[index], value => {
					I.weights[index] = value;
					UpdateProbability();
				});
				weight.style.width = Node1U - 11f - 18f;
				var remove = Button("-", () => {
					I.weights.RemoveAt(index);
					I.weights.TrimExcess();
					mainContainer.Remove(weights);
					ConstructData();
					var port = outputContainer.ElementAt(index) as Port;
					var graphView = port.GetFirstAncestorOfType<GraphView>();
					graphView.DeleteElements(port.connections);
					outputContainer.RemoveAt(index);
					UpdateProbability();
				});
				remove.style.marginTop = remove.style.marginBottom = 0f;
				remove.style.marginLeft = remove.style.marginRight = 0f;
				remove.style.width = 18f;
				element.Add(weight);
				element.Add(remove);
				weights.Add(element);
			}
			var add = Button("Add", () => {
				I.weights.Add(1f);
				I.weights.TrimExcess();
				mainContainer.Remove(weights);
				ConstructData();
				CreatePort(Direction.Output);
				UpdateProbability();
			});
			weights.Add(add);
			mainContainer.Add(weights);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			foreach (var weight in I.weights) CreatePort(Direction.Output);
			UpdateProbability();
			RefreshExpandedState();
			RefreshPorts();
		}

		void UpdateProbability() {
			float sum = 0f;
			foreach (float weight in I.weights) sum += weight;
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

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is RandomizeEvent randomizeEvent) {
			weights.CopyFrom(randomizeEvent.weights);
		}
	}

	public override void GetNext(List<EventBase> list) {
		float sum = 0f;
		foreach (float weight in weights) sum += weight;
		float random = Random.Range(0f, sum);
		int index = weights.FindIndex(weight => (random -= weight) <= 0f);
		if (index == -1) index = weights.Count - 1;
		foreach (var next in nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// GameObject | Object
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("GameObject/Object")]
public sealed class ObjectEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public sealed class ObjectEventNode : EventNodeBase {
		ObjectEvent I => target as ObjectEvent;

		public ObjectEventNode() : base() {
			mainContainer.style.width = Node1U;
			var skyblue = new Color(200f, 0.75f, 0.60f).ToRGB();
			titleContainer.style.backgroundColor = skyblue;
		}

		public override void ConstructData() {
			var instance = ObjectField(I.instance, value => I.instance = value);
			mainContainer.Add(instance);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Output, PortType.Object);
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	public GameObject instance;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is ObjectEvent objectEvent) {
			instance = objectEvent.instance;
		}
	}

	public override void GetObjects(List<GameObject> list) {
		if (instance != null) list.Add(instance);
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// GameObject | Instantiate Object
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("GameObject/Instantiate Object")]
public sealed class InstantiateObjectEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public sealed class InstantiateObjectEventNode : EventNodeBase {
		InstantiateObjectEvent I => target as InstantiateObjectEvent;

		public InstantiateObjectEventNode() : base() {
			mainContainer.style.width = Node2U;
			var skyblue = new Color(200f, 0.75f, 0.60f).ToRGB();
			titleContainer.style.backgroundColor = skyblue;
		}

		public override void ConstructData() {
			var prefab = ObjectField("Prefab", I.prefab, value => I.prefab = value);
			var anchor = ObjectField("Anchor", I.anchor, value => I.anchor = value);
			var offset = Vector3Field("Offset", I.offset, value => I.offset = value);
			mainContainer.Add(prefab);
			mainContainer.Add(anchor);
			mainContainer.Add(offset);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			CreatePort(Direction.Output, PortType.Object);
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	public GameObject prefab;
	public GameObject anchor;
	public Vector3 offset;

	GameObject instance;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is InstantiateObjectEvent instantiateObjectEvent) {
			prefab = instantiateObjectEvent.prefab;
			anchor = instantiateObjectEvent.anchor;
			offset = instantiateObjectEvent.offset;
		}
	}

	public override void Start() {
		instance = null;
	}

	public override void End() {
		if (instance == null && prefab) {
			var position = anchor ? anchor.transform.TransformPoint(offset) : offset;
			var rotation = anchor ? anchor.transform.rotation : Quaternion.identity;
			instance = Object.Instantiate(prefab, position, rotation);
		}
	}

	public override void GetObjects(List<GameObject> list) {
		End();
		if (instance != null) list.Add(instance);
	}



	#if UNITY_EDITOR
	public override void DrawGizmos() {
		var position = anchor ? anchor.transform.TransformPoint(offset) : offset;
		Gizmos.DrawIcon(position, "d_GameObject Icon", true, Gizmos.color);
	}

	public override void DrawHandles() {
		var position = anchor ? anchor.transform.TransformPoint(offset) : offset;
		var handle = Handles.PositionHandle(position, Quaternion.identity);
		offset = anchor ? anchor.transform.InverseTransformPoint(handle) : handle;
		if (node != null) node.Q<Vector3Field>().value = offset;
	}
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// GameObject | Destroy Object
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("GameObject/Destroy Object")]
public sealed class DestroyObjectEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public sealed class DestroyObjectEventNode : EventNodeBase {
		DestroyObjectEvent I => target as DestroyObjectEvent;

		public DestroyObjectEventNode() : base() {
			mainContainer.style.width = Node1U;
			var skyblue = new Color(200f, 0.75f, 0.60f).ToRGB();
			titleContainer.style.backgroundColor = skyblue;
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output);
			CreatePort(Direction.Input, PortType.Object);
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	List<GameObject> list = new();



	// Methods

	public override void End() {
		foreach (var prev in prevs) if (prev.oPortType == PortType.Object) {
			prev.eventBase.GetObjects(list);
			foreach (var instance in list) Object.Destroy(instance);
			list.Clear();
		}
	}
}





// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Multimodal | Validate
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Multimodal/Validate Answer")]
public class ValidateAnswerEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class ValidateAnswerEventNode : EventNodeBase {
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
			CreatePort(Direction.Input);
			CreatePort(Direction.Input, PortType.MultimodalData);
			CreatePort(Direction.Output).portName = "True";
			CreatePort(Direction.Output).portName = "False";
			CreatePort(Direction.Output).portName = "Exception";
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	public string prompt;

	float timer;
	bool isValid;
	bool isException;



	// Methods

	public override void CopyFrom(EventBase data) {
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

	public override void GetNext(List<EventBase> list) {
		list ??= new();
		list.Clear();
		int index = isException ? 2 : (isValid ? 0 : 1);
		foreach (var next in nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}
