using UnityEngine;

#if UNITY_EDITOR
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
