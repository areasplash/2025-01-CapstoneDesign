using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
	using UnityEditor;
#endif



// ━

public enum GameState : byte {
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

				LabelField("Config", EditorStyles.boldLabel);
				BeginDisabledGroup(Application.isPlaying);
				StartGameDirectly = Toggle("Start Game Directly", StartGameDirectly);
				EndDisabledGroup();
				Space();

				LabelField("Debug", EditorStyles.boldLabel);
				BeginDisabledGroup();
				var gameState = Regex.Replace($"{GameState}", "(?<=[a-z])(?=[A-Z])", " ");
				TextField("Game State", $"{gameState}");
				EndDisabledGroup();
				Space();

				End();
			}
		}
	#endif



	// Fields

	[SerializeField] bool m_StartGameDirectly = false;

	GameState m_GameState = GameState.Paused;

	readonly List<BaseEvent> m_ActiveEvents = new();
	readonly List<float    > m_EventElapsed = new();



	// Properties

	static bool StartGameDirectly {
		get => Instance.m_StartGameDirectly;
		set => Instance.m_StartGameDirectly = value;
	}

	public static GameState GameState {
		get => Instance.m_GameState;
		private set {
			var flag = GameState != value;
			Instance.m_GameState  = value;
			if (flag) InputManager.SwitchActionMap(value switch {
				GameState.Gameplay => ActionMap.Player,
				GameState.Cutscene => ActionMap.UI,
				GameState.Paused   => ActionMap.UI,
				_ => default,
			});
		}
	}

	static List<BaseEvent> ActiveEvents => Instance.m_ActiveEvents;
	static List<float    > EventElapsed => Instance.m_EventElapsed;



	// Methods

	public static void PlayEvent(EventGraphSO graph) {
		ActiveEvents.Add(graph.Entry);
		EventElapsed.Add(-1f);
	}

	static void SimulateEvents() {
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
					if (ActiveEvents[i].async) {
						ActiveEvents.Add(ActiveEvents[i]);
						EventElapsed.Add(EventElapsed[i]);
					}
				}
				if (ActiveEvents[i].Update() == false && EventElapsed[i] < 20f) {
					EventElapsed[i] += Time.deltaTime;
					i++;
					break;
				} else {
					ActiveEvents[i].End();
					ActiveEvents[i] = ActiveEvents[i].GetNext();
					EventElapsed[i] = -1f;
				}
			}
		}
	}



	// Lifecycle

	void Start() {
		var startGameDirectly = false;
		#if UNITY_EDITOR
			startGameDirectly = StartGameDirectly;
		#endif
		if (startGameDirectly == false) {
			GameState = GameState.Paused;
		} else {
			GameState = GameState.Gameplay;
		}
	}



	void Update() {
		SimulateEvents();
	}
}
