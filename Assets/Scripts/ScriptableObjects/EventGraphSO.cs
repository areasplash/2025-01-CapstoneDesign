using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Callbacks;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Event Graph SO
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[CreateAssetMenu(fileName = "EventGraphSO", menuName = "Scriptable Objects/EventGraph")]
public class EventGraphSO : ScriptableObject {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(EventGraphSO))]
	class EventGraphSOEditor : EditorExtensions {
		EventGraphSO I => target as EventGraphSO;
		public override void OnInspectorGUI() {
			Begin("Event Graph");

			if (Button("Open Event Graph")) I.Open();
			Space();

			End();
		}
	}
	#endif



	// Fields

	[SerializeReference] EntryEvent m_Entry = new();
	EntryEvent m_Clone;



	// Properties

	public EntryEvent Entry {
		get => m_Entry;
	}
	public EntryEvent Clone {
		get => m_Clone;
		set => m_Clone = value;
	}



	// Methods

	#if UNITY_EDITOR
	public void Open() => EventGraphWindow.Open(name, this);

	[OnOpenAsset]
	public static bool OnOpen(int instanceID) {
		var target = EditorUtility.InstanceIDToObject(instanceID);
		if (target is EventGraphSO eventGraph) {
			eventGraph.Open();
			return true;
		} 
		return false;
	}
	#endif
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Event Graph Window
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

#if UNITY_EDITOR
public class EventGraphWindow : EditorWindow {

	// Fields

	Toolbar toolbar;
	EventGraphView eventGraphView;
	EventGraphSO eventGraph;



	// Methods

	public static void Open(string name, EventGraphSO eventGraph) {
		var windows = Resources.FindObjectsOfTypeAll<EventGraphWindow>();
		var existingWindow = windows.FirstOrDefault(window => window.eventGraph == eventGraph);
		if (existingWindow == null) {
			var dock = new[] { typeof(EventGraphWindow), typeof(SceneView) };
			var window = CreateWindow<EventGraphWindow>(name, dock);
			window.eventGraph = eventGraph;
			window.Initialize();
		} else existingWindow.Focus();
	}

	void Initialize() {
		var mainContainer = new VisualElement();
		mainContainer.style.flexDirection = FlexDirection.Column;
		mainContainer.style.flexGrow = 1;
		rootVisualElement.Clear();
		rootVisualElement.Add(mainContainer);

		toolbar = new Toolbar();
		toolbar.Add(new ToolbarButton(() => eventGraphView.Save()) { text = "Save" });
		toolbar.Add(new ToolbarButton(() => eventGraphView.Load()) { text = "Load" });
		mainContainer.Add(toolbar);

		eventGraphView = new EventGraphView(eventGraph);
		eventGraphView.style.flexGrow = 1;
		mainContainer.Add(eventGraphView);
	}



	// Lifecycle

	void OnGUI() {
		if (eventGraphView == null) Initialize();
	}
}
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Event Graph View
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

#if UNITY_EDITOR
public class EventGraphView : GraphView {

	// Fields

	bool isFramed;
	EventGraphSO eventGraph;



	// Constructor

	public EventGraphView(EventGraphSO eventGraph) {
		this.eventGraph = eventGraph;
		var contentZoomer = new ContentZoomer();
		this.AddManipulator(contentZoomer);
		this.AddManipulator(new ContentDragger());
		this.AddManipulator(new SelectionDragger());
		this.AddManipulator(new RectangleSelector());
		contentZoomer.minScale = 00.1f;
		contentZoomer.maxScale = 10.0f;
		var grid = new GridBackground();
		grid.StretchToParentSize();
		Insert(0, grid);

		RegisterCallback<GeometryChangedEvent>(evt => {
			if (isFramed == false) {
				isFramed = true;
				FrameAll();
			}
		});
		RegisterCallback<KeyDownEvent>(evt => {
			if ((evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.S) {
				evt.StopImmediatePropagation();
				Save();
			}
		});
		Load();
	}



	// Methods

	public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
		if (evt.target is GraphView) {
			var position = contentViewContainer.WorldToLocal(evt.localMousePosition);
			foreach (var dropdown in EventBase.EventNodeBase.Dropdown) {
				evt.menu.AppendAction("Create Node/"+ dropdown, _ => {
					var name = Regex.Replace(dropdown.Split('/')[^1], @"\s+", "");
					var type = Type.GetType(name + "Event");
					var node = CreateNode(type, position);
					node.ConstructData();
					node.ConstructPort();
				});
			}
			evt.menu.AppendSeparator();
		}
		base.BuildContextualMenu(evt);
	}

	EventBase.EventNodeBase CreateNode(Type type, Vector2 position) {
		if (type == null || !typeof(EventBase).IsAssignableFrom(type)) return null;
		var nodeType = Type.GetType(type.Name + "+" + type.Name + "Node");
		if (nodeType == null) return null;
		var node = Activator.CreateInstance(nodeType) as EventBase.EventNodeBase;
		node.SetPosition(new Rect(position, Vector2.zero));
		AddElement(node);
		return node;
	}

	public override List<Port> GetCompatiblePorts(Port startport, NodeAdapter adapter) {
		return ports.Where(port => {
			var match = true;
			if (match) match &= port.node != startport.node;
			if (match) match &= port.direction != startport.direction;
			if (match) match &= (byte)port.userData == (byte)startport.userData;
			if (match) {
				var connectedPorts = startport.connections.Select(edge => {
					return edge.output == startport ? edge.input : edge.output;
				}).ToList();
				match &= !connectedPorts.Contains(port);
			}
			return match;
		}).ToList();
	}



	// IO Methods

	public void Save() {
		if (eventGraph == null) return;
		foreach (var node in nodes.OfType<EventBase.EventNodeBase>()) {
			var eventBase = node.target;
			eventBase.prevs.Clear();
			eventBase.nexts.Clear();
			eventBase.position = node.GetPosition().position;

			var node_iPorts = node.inputContainer.Children().OfType<Port>().ToList();
			foreach (var port in node_iPorts) foreach (var edge in port.connections) {
				if (edge.output.node is EventBase.EventNodeBase prev) {
					var prev_oPorts = prev.outputContainer.Children().OfType<Port>().ToList();
					var iPort = (byte)node_iPorts.IndexOf(edge.input);
					var oPort = (byte)prev_oPorts.IndexOf(edge.output);
					eventBase.prevs.Add(new EventBase.Connection {
						eventBase = prev.target,
						iPort = iPort,
						oPort = oPort,
						iPortType = (PortType)node_iPorts[iPort].userData,
						oPortType = (PortType)prev_oPorts[oPort].userData,
					});
				}
			}
			var node_oPorts = node.outputContainer.Children().OfType<Port>().ToList();
			foreach (var port in node_oPorts) foreach (var edge in port.connections) {
				if (edge.input.node is EventBase.EventNodeBase next) {
					var next_iPorts = next.inputContainer.Children().OfType<Port>().ToList();
					var iPort = (byte)next_iPorts.IndexOf(edge.input);
					var oPort = (byte)node_oPorts.IndexOf(edge.output);
					eventBase.nexts.Add(new EventBase.Connection {
						eventBase = next.target,
						iPort = iPort,
						oPort = oPort,
						iPortType = (PortType)next_iPorts[iPort].userData,
						oPortType = (PortType)node_oPorts[oPort].userData,
					});
				}
			}
			eventBase.prevs.TrimExcess();
			eventBase.nexts.TrimExcess();
		}
		eventGraph.Entry.CopyFrom(eventGraph.Clone);
		EditorUtility.SetDirty(eventGraph);
		AssetDatabase.SaveAssets();
		Load();
	}

	public void Load() {
		if (eventGraph == null) return;
		DeleteElements(graphElements);
		var stack = new Stack<EventBase>();
		var cache = new Dictionary<string, EventBase.EventNodeBase>();

		stack.Push(eventGraph.Entry);
		while (0 < stack.Count) {
			var eventBase = stack.Pop();
			if (eventBase == null) continue;
			if (cache.ContainsKey(eventBase.guid)) continue;
			var node = CreateNode(eventBase.GetType(), eventBase.position);
			node.target.CopyFrom(eventBase);
			node.ConstructData();
			node.ConstructPort();
			cache.Add(eventBase.guid, node);
			foreach (var prev in eventBase.prevs) stack.Push(prev.eventBase);
			foreach (var next in eventBase.nexts) stack.Push(next.eventBase);
		}
		foreach (var (_, node) in cache) {
			var eventBase = node.target;
			var node_oPorts = node.outputContainer.Children().OfType<Port>().ToList();
			if (eventBase.nexts != null) for (int i = 0; i < eventBase.nexts.Count; i++) {
				if (eventBase.nexts[i].eventBase == null) continue;
				var next = cache[eventBase.nexts[i].eventBase.guid];
				var next_iPorts = next.inputContainer.Children().OfType<Port>().ToList();
				var nodeOPort = node_oPorts[eventBase.nexts[i].oPort];
				var nextIPort = next_iPorts[eventBase.nexts[i].iPort];
				AddElement(nodeOPort.ConnectTo(nextIPort));
			}
		}
		foreach (var (_, node) in cache) {
			var eventBase = node.target;
			var prev = new List<EventBase.Connection>();
			foreach (var connection in eventBase.prevs) {
				if (connection.eventBase == null) continue;
				prev.Add(new() {
					eventBase = cache[connection.eventBase.guid].target,
					iPort = connection.iPort,
					oPort = connection.oPort,
					iPortType = connection.iPortType,
					oPortType = connection.oPortType,
				});
			}
			eventBase.prevs = prev;
			var next = new List<EventBase.Connection>();
				foreach (var connection in eventBase.nexts) {
				if (connection.eventBase == null) continue;
				next.Add(new() {
					eventBase = cache[connection.eventBase.guid].target,
					iPort = connection.iPort,
					oPort = connection.oPort,
					iPortType = connection.iPortType,
					oPortType = connection.oPortType,
				});
			}
			eventBase.nexts = next;
		}
		eventGraph.Clone = cache[eventGraph.Entry.guid].target as EntryEvent;
	}
}
#endif
