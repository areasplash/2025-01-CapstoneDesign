using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using static EditorVisualElement;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Set Game State
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Set Game State")]
public class SetGameStateEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class SetGameStateEventNode : EventNodeBase {
		SetGameStateEvent I => target as SetGameStateEvent;

		public SetGameStateEventNode() : base() {
			mainContainer.style.width = Node1U;
			var cyan = new Color(180f, 0.75f, 0.60f).ToRGB();
			titleContainer.style.backgroundColor = cyan;
		}

		public override void ConstructData() {
			var gameState = EnumField(I.gameState, value => I.gameState = value);
			mainContainer.Add(gameState);
		}
	}
	#endif



	// Fields

	public GameState gameState = GameState.Gameplay;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetGameStateEvent setGameStateEvent) {
			gameState = setGameStateEvent.gameState;
		}
	}

	public override void End() {
		GameManager.GameState = gameState;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Get Int Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Get Int Value")]
public class GetIntValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class GetIntValueEventNode : EventNodeBase {
		GetIntValueEvent I => target as GetIntValueEvent;

		public GetIntValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var compare = EnumField(I.compare, value => I.compare = value);
			var value = IntField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			mainContainer.Add(key);
			mainContainer.Add(compare);
			mainContainer.Add(value);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output).portName = "True";
			CreatePort(Direction.Output).portName = "False";
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	public string key;
	public Compare compare;
	public int value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is GetIntValueEvent getIntValueEvent) {
			key = getIntValueEvent.key;
			compare = getIntValueEvent.compare;
			value = getIntValueEvent.value;
		}
	}

	public override void GetNext(List<EventBase> list) {
		int index = compare switch {
			Compare.Equal              => GameManager.IntValue[key] == value,
			Compare.NotEqual           => GameManager.IntValue[key] != value,
			Compare.LessThan           => GameManager.IntValue[key] <  value,
			Compare.LessThanOrEqual    => GameManager.IntValue[key] <= value,
			Compare.GreaterThan        => GameManager.IntValue[key] >  value,
			Compare.GreaterThanOrEqual => GameManager.IntValue[key] >= value,
			_ => default,
		} ? 0 : 1;
		foreach (var next in nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Set Int Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Set Int Value")]
public class SetIntValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class SetIntValueEventNode : EventNodeBase {
		SetIntValueEvent I => target as SetIntValueEvent;

		public SetIntValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var value = IntField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			mainContainer.Add(key);
			mainContainer.Add(value);
		}
	}
	#endif



	// Fields

	public string key;
	public int value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetIntValueEvent setIntValueEvent) {
			key = setIntValueEvent.key;
			value = setIntValueEvent.value;
		}
	}

	public override void End() {
		GameManager.IntValue[key] = value;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Add Int Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Add Int Value")]
public class AddIntValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class AddIntValueEventNode : EventNodeBase {
		AddIntValueEvent I => target as AddIntValueEvent;

		public AddIntValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var value = IntField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			mainContainer.Add(key);
			mainContainer.Add(value);
		}
	}
	#endif



	// Fields

	public string key;
	public int value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is AddIntValueEvent addIntValueEvent) {
			key = addIntValueEvent.key;
			value = addIntValueEvent.value;
		}
	}

	public override void End() {
		GameManager.IntValue[key] += value;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Get Float Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Get Float Value")]
public class GetFloatValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class GetFloatValueEventNode : EventNodeBase {
		GetFloatValueEvent I => target as GetFloatValueEvent;

		public GetFloatValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var compare = EnumField(I.compare, value => I.compare = value);
			var value = FloatField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			mainContainer.Add(key);
			mainContainer.Add(compare);
			mainContainer.Add(value);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output).portName = "True";
			CreatePort(Direction.Output).portName = "False";
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	public string key;
	public Compare compare;
	public float value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is GetFloatValueEvent getFloatValueEvent) {
			key     = getFloatValueEvent.key;
			compare = getFloatValueEvent.compare;
			value   = getFloatValueEvent.value;
		}
	}

	public override void GetNext(List<EventBase> list) {
		int index = compare switch {
			Compare.Equal              => GameManager.FloatValue[key] == value,
			Compare.NotEqual           => GameManager.FloatValue[key] != value,
			Compare.LessThan           => GameManager.FloatValue[key] <  value,
			Compare.LessThanOrEqual    => GameManager.FloatValue[key] <= value,
			Compare.GreaterThan        => GameManager.FloatValue[key] >  value,
			Compare.GreaterThanOrEqual => GameManager.FloatValue[key] >= value,
			_ => default,
		} ? 0 : 1;
		foreach (var next in nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Set Float Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Set Float Value")]
public class SetFloatValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class SetFloatValueEventNode : EventNodeBase {
		SetFloatValueEvent I => target as SetFloatValueEvent;

		public SetFloatValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var value = FloatField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			mainContainer.Add(key);
			mainContainer.Add(value);
		}
	}
	#endif



	// Fields

	public string key;
	public float value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetFloatValueEvent setFloatValueEvent) {
			key   = setFloatValueEvent.key;
			value = setFloatValueEvent.value;
		}
	}

	public override void End() {
		GameManager.FloatValue[key] = value;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Add Float Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Add Float Value")]
public class AddFloatValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class AddFloatValueEventNode : EventNodeBase {
		AddFloatValueEvent I => target as AddFloatValueEvent;

		public AddFloatValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var value = FloatField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			mainContainer.Add(key);
			mainContainer.Add(value);
		}
	}
	#endif



	// Fields

	public string key;
	public float value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is AddFloatValueEvent addFloatValueEvent) {
			key = addFloatValueEvent.key;
			value = addFloatValueEvent.value;
		}
	}

	public override void End() {
		GameManager.FloatValue[key] += value;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Get String Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Get String Value")]
public class GetStringValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class GetStringValueEventNode : EventNodeBase {
		GetStringValueEvent I => target as GetStringValueEvent;

		public GetStringValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var compare = EnumField(I.compare, value => I.compare = value);
			var value = TextField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			value.textEdition.placeholder = "Value";
			mainContainer.Add(key);
			mainContainer.Add(compare);
			mainContainer.Add(value);
		}

		public override void ConstructPort() {
			CreatePort(Direction.Input);
			CreatePort(Direction.Output).portName = "True";
			CreatePort(Direction.Output).portName = "False";
			RefreshExpandedState();
			RefreshPorts();
		}
	}
	#endif



	// Fields

	public string key;
	public Compare compare;
	public string value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is GetStringValueEvent getStringValueEvent) {
			key     = getStringValueEvent.key;
			compare = getStringValueEvent.compare;
			value   = getStringValueEvent.value;
		}
	}

	public override void GetNext(List<EventBase> list) {
		int index = compare switch {
			Compare.Equal    => GameManager.StringValue[key] == value,
			Compare.NotEqual => GameManager.StringValue[key] != value,
			_ => default,
		} ? 0 : 1;
		foreach (var next in nexts) if (next.oPortType == PortType.Default) {
			if (next.oPort == index) list.Add(next.eventBase);
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Set String Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Set String Value")]
public class SetStringValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class SetStringValueEventNode : EventNodeBase {
		SetStringValueEvent I => target as SetStringValueEvent;

		public SetStringValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var value = TextField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			value.textEdition.placeholder = "Value";
			mainContainer.Add(key);
			mainContainer.Add(value);
		}
	}
	#endif



	// Fields

	public string key;
	public string value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is SetStringValueEvent setStringValueEvent) {
			key   = setStringValueEvent.key;
			value = setStringValueEvent.value;
		}
	}

	public override void End() {
		GameManager.StringValue[key] = value;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Add String Value
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Add String Value")]
public class AddStringValueEvent : EventBase {

	// Node

	#if UNITY_EDITOR
	public class AddStringValueEventNode : EventNodeBase {
		AddStringValueEvent I => target as AddStringValueEvent;

		public AddStringValueEventNode() : base() {
			mainContainer.style.width = Node1U;
		}

		public override void ConstructData() {
			var key = TextField(I.key, value => I.key = value);
			var value = TextField(I.value, value => I.value = value);
			key.textEdition.placeholder = "Key";
			value.textEdition.placeholder = "Value";
			mainContainer.Add(key);
			mainContainer.Add(value);
		}
	}
	#endif



	// Fields

	public string key;
	public string value;



	// Methods

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is AddStringValueEvent addStringValueEvent) {
			key = addStringValueEvent.key;
			value = addStringValueEvent.value;
		}
	}

	public override void End() {
		GameManager.StringValue[key] += value;
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager | Collect Gem
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Game Manager/Collect Gem")]
public class CollectGemEvent : EventBase {

	// Node

	#if UNITY_EDITOR
		public class CollectGemEventNode : EventNodeBase {
			CollectGemEvent I => target as CollectGemEvent;

			public CollectGemEventNode() : base() {
				mainContainer.style.width = Node1U;
			}

			public override void ConstructData() {
				var amount = IntField(I.amount, value => I.amount = value);
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

	public override void CopyFrom(EventBase eventBase) {
		base.CopyFrom(eventBase);
		if (eventBase is CollectGemEvent collectGemEvent) {
			amount = collectGemEvent.amount;
		}
	}
}
