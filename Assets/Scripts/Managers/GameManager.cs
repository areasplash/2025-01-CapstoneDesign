using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif



// Game States

public enum GameState : byte {
	None,
	Gameplay,
	Cutscene,
	Paused,
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Game Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/Game Manager")]
public class GameManager : MonoSingleton<GameManager> {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(GameManager))]
	class GameManagerEditor : EditorExtensions {
		GameManager I => target as GameManager;
		public override void OnInspectorGUI() {
			Begin("Game Manager");
			I.TrySetInstance();

			LabelField("Debug", EditorStyles.boldLabel);
			BeginDisabledGroup();
			TextField("Game State", GameState.ToString());
			EndDisabledGroup();
			Space();

			End();
		}
	}
	#endif



	// Constants

	public const float GridXMultiplier = 1.0f;
	public const float GridYMultiplier = 0.5f;
	public static Vector2 GridMultiplier => new(GridXMultiplier, GridYMultiplier);



	// Fields

	GameState m_GameState;
	Player m_Player;
	[SerializeField] HashMap<string, int> m_Inventory = new();
	[SerializeField] int m_Gem;
	public bool m_Negative = false;

	Dictionary<uint, byte> m_Events = new();
	List<(uint, BaseEvent, float)> m_EventList = new();
	List<BaseEvent> m_EventBuffer = new();
	uint m_EventID;



	// Properties

	public static GameState GameState {
		get => Instance.m_GameState;
		set {
			if (Instance.m_GameState != value) {
				Instance.m_GameState = value;
				InputManager.SwitchActionMap(value switch {
					GameState.Gameplay => ActionMap.Player,
					GameState.Cutscene => ActionMap.UI,
					GameState.Paused   => ActionMap.UI,
					_ => default,
				});
			}
		}
	}

	public static Player Player => Instance.m_Player ??= FindAnyObjectByType<Player>();

	public static HashMap<string, int> Inventory {
		get => Instance.m_Inventory;
		set => Instance.m_Inventory = value;
	}

	public static int Gem {
		get => Instance.m_Gem;
		private set => Instance.m_Gem = value;
	}



	static Dictionary<uint, byte> Events => Instance.m_Events;
	static List<(uint, BaseEvent, float)> EventList => Instance.m_EventList;
	static List<BaseEvent> EventBuffer => Instance.m_EventBuffer;

	static uint EventID {
		get => Instance.m_EventID;
		set => Instance.m_EventID = value;
	}



	// Methods

	public static void CollectGem(int amount) {
		Gem += amount;
		UIManager.ShowGemCollectMessage("야호! {" + amount + "}마음 보석을 획득했어!");
	}



	// Instance Methods

	static uint AddInstance(BaseEvent baseEvent) {
		if (baseEvent == null) return default;
		while (++EventID == default || Events.ContainsKey(EventID));
		Events.Add(EventID, 1);
		EventList.Add((EventID, baseEvent, default));
		return EventID;
	}

	static void RemoveInstances(uint id) {
		byte numEvents = Events[id];
		Events.Remove(id);
		for (int i = EventList.Count; 0 < i--;) {
			if (EventList[i].Item1 == id) {
				EventList.RemoveAt(i);
				if (--numEvents == 0) break;
			}
		}
	}

	static void UpdateInstances() {
		if (GameState == GameState.Paused) return;
		int i = 0;
		while (i < EventList.Count) {
			var (id, eventBase, startTime) = EventList[i];
			if (startTime == default) {
				eventBase.Start();
				EventList[i] = (id, eventBase, Time.time);
				continue;
			}
			if (eventBase.Update() == false) {
				i++;
				continue;
			}
			eventBase.End();
			eventBase.GetNext(EventBuffer);
			int numNexts = EventBuffer.Count;
			if (numNexts == 0) {
				if (--Events[id] == 0) Events.Remove(id);
				EventList.RemoveAt(i);
			} else {
				if (1 < numNexts) Events[id] += (byte)(numNexts - 1);
				EventList[i] = (id, EventBuffer[0], default);
				for (int j = 1; j < numNexts; j++) EventList.Add((id, EventBuffer[j], default));
				EventBuffer.Clear();
			}
		}
	}



	// Event Methods

	public static uint PlayEvent(EventGraphSO eventGraph) {
		uint id = AddInstance(eventGraph?.Entry);
		return id;
	}

	public static bool IsEventPlaying(uint id = default) {
		return id == default ? 0 < Events.Count : Events.ContainsKey(id);
	}

	public static void StopEvent(uint id) {
		if (Events.ContainsKey(id)) RemoveInstances(id);
	}



	// Lifecycle

	void Start() {
		GameState = GameState.Gameplay;
		UIManager.Initialize();
		UIManager.OpenGame();
	}

	void Update() {
		UpdateInstances();
	}
}
