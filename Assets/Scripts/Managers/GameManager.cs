using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif



// Game States

public enum GameState : byte {
	Gameplay,
	Cutscene,
	Paused,
}

public enum Compare : byte {
	Equal,
	NotEqual,
	LessThan,
	LessThanOrEqual,
	GreaterThan,
	GreaterThanOrEqual,
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

			LabelField("Game Data", EditorStyles.boldLabel);
			DictionaryField("Int Value", IntValue, (list, index) => {
				var pair = list[index];
				pair.key = TextField(pair.key);
				pair.value = IntField(pair.value);
				list[index] = pair;
			}, ($"New Key {IntValue.Count}", default));
			DictionaryField("Float Value", FloatValue, (list, index) => {
				var pair = list[index];
				pair.key = TextField(pair.key);
				pair.value = FloatField(pair.value);
				list[index] = pair;
			}, ($"New Key {FloatValue.Count}", default));
			DictionaryField("String Value", StringValue, (list, index) => {
				var pair = list[index];
				pair.key = TextField(pair.key);
				pair.value = TextField(pair.value);
				list[index] = pair;
			}, ($"New Key {StringValue.Count}", default));
			Space();
			BeginHorizontal();
			if (Button("Save Data")) SaveData();
			if (Button("Load Data")) LoadData();
			EndHorizontal();
			Space();

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
	[SerializeField] int m_Gem;
	public bool m_Negative = false;

	[SerializeField] HashMap<string, int> m_IntValue = new();
	[SerializeField] HashMap<string, float> m_FloatValue = new();
	[SerializeField] HashMap<string, string> m_StringValue = new();

	Dictionary<uint, byte> m_Events = new();
	List<(uint, EventBase, float)> m_EventList = new();
	List<EventBase> m_EventBuffer = new();
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
	public static float TimeScale {
		get => Time.timeScale;
		set => Time.timeScale = Mathf.Clamp(value, 0f, 10f);
	}

	public static Player Player => Instance.m_Player ??= FindAnyObjectByType<Player>();

	public static int Gem {
		get => Instance.m_Gem;
		private set => Instance.m_Gem = value;
	}



	public static HashMap<string, int> IntValue {
		get => Instance.m_IntValue;
	}
	public static HashMap<string, float> FloatValue {
		get => Instance.m_FloatValue;
	}
	public static HashMap<string, string> StringValue {
		get => Instance.m_StringValue;
	}



	static Dictionary<uint, byte> Events => Instance.m_Events;
	static List<(uint, EventBase, float)> EventList => Instance.m_EventList;
	static List<EventBase> EventBuffer => Instance.m_EventBuffer;

	static uint EventID {
		get => Instance.m_EventID;
		set => Instance.m_EventID = value;
	}



	// Methods

	public static void SaveData() {
		foreach (var (key, value) in IntValue) {
			PlayerPrefs.SetInt(key, value);
		}
		foreach (var (key, value) in FloatValue) {
			PlayerPrefs.SetFloat(key, value);
		}
		foreach (var (key, value) in StringValue) {
			PlayerPrefs.SetString(key, value);
		}
	}

	public static void LoadData() {
		foreach (var key in IntValue.Keys) {
			IntValue[key] = PlayerPrefs.GetInt(key);
		}
		foreach (var key in FloatValue.Keys) {
			FloatValue[key] = PlayerPrefs.GetFloat(key);
		}
		foreach (var key in StringValue.Keys) {
			StringValue[key] = PlayerPrefs.GetString(key);
		}
	}



	public static void CollectGem(int amount) {
		Gem += amount;
		UIManager.ShowGemCollectMessage("야호! {" + amount + "}마음 보석을 획득했어!");
	}



	// Instance Methods

	static uint AddInstance(EventBase baseEvent) {
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

	public static void Quit() {
		#if UNITY_EDITOR
		EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
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
