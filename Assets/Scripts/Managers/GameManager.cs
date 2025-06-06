using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
	using UnityEditor.Compilation;
#endif



// ━

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

				LabelField("Setup", EditorStyles.boldLabel);
				StartDirectly = Toggle("Start Directly", StartDirectly);
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

	[SerializeField] bool m_StartDirectly;

	GameState m_GameState;

	readonly List<EventGraphSO> m_ActiveGraphs = new();
	readonly List<BaseEvent   > m_ActiveEvents = new();
	readonly List<float       > m_EventElapsed = new();

	[SerializeField] int m_Gem;



	// Properties

	static bool StartDirectly {
		get => Instance.m_StartDirectly;
		set {
			if (Instance.m_StartDirectly != value) {
				Instance.m_StartDirectly = value;
				#if UNITY_EDITOR
					if (value && !EditorApplication.isPlayingOrWillChangePlaymode) {
						CompilationPipeline.RequestScriptCompilation();
					}
				#endif
			}
		}
	}

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

	static List<EventGraphSO> ActiveGraphs => Instance.m_ActiveGraphs;
	static List<BaseEvent   > ActiveEvents => Instance.m_ActiveEvents;
	static List<float       > EventElapsed => Instance.m_EventElapsed;



	public static int Gem {
		get => Instance.m_Gem;
		private set => Instance.m_Gem = value;
	}



	// Methods

	public static void PlayEvent(EventGraphSO graph) {
		graph.OnEventBegin.Invoke();
		ActiveGraphs.Add(graph);
		ActiveEvents.Add(graph.Entry);
		EventElapsed.Add(-1f);
	}

	static void SimulateEvents() {
		int i = 0;
		while (i < ActiveEvents.Count) {
			while (true) {
				if (ActiveEvents[i] == null) {
					ActiveGraphs[i].OnEventEnd.Invoke();
					ActiveGraphs.RemoveAt(i);
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
				if (ActiveEvents[i].Update() == false) {
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



	public static void CollectGem(int amount) {
		Gem += amount;
		UIManager.ShowGemCollectMessage("야호! {" + amount + "}마음 보석을 획득했어!");
	}



	// Lifecycle

	void Start() {
		var startDirectly = false;
		//#if UNITY_EDITOR
			startDirectly = StartDirectly;
		//#endif
		if (startDirectly) {
			GameState = GameState.Gameplay;
			UIManager.Initialize();
			UIManager.ShowGame();
		} else {
			GameState = GameState.Paused;
			UIManager.Initialize();
			//UIManager.ShowTitle();
		}
	}

	void Update() {
		SimulateEvents();
	}
}
