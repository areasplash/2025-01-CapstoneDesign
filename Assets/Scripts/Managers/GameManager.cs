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

	readonly List<BaseEvent> m_Temp = new();
	readonly List<BaseEvent> m_ActiveEvents = new();
	readonly List<float> m_EventElapsed = new();

	Player m_Player;

	[SerializeField] int m_Gem;



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

	static List<BaseEvent> Temp => Instance.m_Temp;
	static List<BaseEvent> ActiveEvents => Instance.m_ActiveEvents;
	static List<float> EventElapsed => Instance.m_EventElapsed;



	public static Player Player => Instance.m_Player ??= FindAnyObjectByType<Player>();

	public static int Gem {
		get => Instance.m_Gem;
		private set => Instance.m_Gem = value;
	}



	// Methods

	public static void PlayEvent(EventGraphSO graph) {
		ActiveEvents.Add(graph.Entry);
		EventElapsed.Add(-1f);
	}

	static void SimulateEvents() {
		if (GameState == GameState.Paused) return;
		int i = 0;
		while (i < ActiveEvents.Count) {
			while (true) {
				if (ActiveEvents[i] == null) {
					ActiveEvents.RemoveAt(i);
					EventElapsed.RemoveAt(i);
					break;
				}
				if (EventElapsed[i] < 0f) {
					EventElapsed[i] = 0f;
					ActiveEvents[i].Start();
				}
				if (ActiveEvents[i].Update() == false) {
					EventElapsed[i] += Time.deltaTime;
					i++;
					break;
				} else {
					ActiveEvents[i].End();
					ActiveEvents[i].GetNext(Temp);
					if (Temp.Count == 0) ActiveEvents[i] = null;
					else {
						ActiveEvents[i] = Temp[0];
						EventElapsed[i] = -1f;
						for (int j = 1; j < Temp.Count; j++) {
							ActiveEvents.Add(Temp[j]);
							EventElapsed.Add(-1f);
						}
					}
				}
			}
		}
	}



	public static void CollectGem(int amount) {
		Gem += amount;
		UIManager.ShowGemCollectMessage("야호! {" + amount + "}마음 보석을 획득했어!");
	}



	// Lifecycle

	void Start() {
		GameState = GameState.Gameplay;
		UIManager.Initialize();
		UIManager.ShowGame();
	}

	void Update() {
		SimulateEvents();
	}
}
