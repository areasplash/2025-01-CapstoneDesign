using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Minsu
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class Minsu : Actor {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(Minsu))]
	class MinsuEditor : EditorExtensions {
		Minsu I => target as Minsu;
		public override void OnInspectorGUI() {
			Begin("Minsu");

			LabelField("Animator", EditorStyles.boldLabel);
			I.BodyAnimator    = ObjectField("Body Animator",    I.BodyAnimator);
			I.EmotionAnimator = ObjectField("Emotion Animator", I.EmotionAnimator);
			Space();
			LabelField("Physics", EditorStyles.boldLabel);
			I.Speed = FloatField("Speed", I.Speed);
			Space();
			LabelField("Group Event", EditorStyles.boldLabel);
			I.Event = ObjectField("Event", I.Event);
			I.EventProbability = Slider("Event Probability", I.EventProbability, float.Epsilon, 1f);
			I.EventInterval = FloatField("Event Interval", I.EventInterval);
			I.SearchRange   = FloatField("Search Range",   I.SearchRange);
			Space();

			End();
		}
	}
	#endif



	// Fields

	[SerializeField] GameObject m_Event;
	[SerializeField] float m_EventProbability = 0.2f;
	[SerializeField] float m_EventInterval = 1f;
	[SerializeField] float m_SearchRange = 8f;
	float m_Timer;
	Actor m_Target;



	// Properties

	public GameObject Event {
		get => m_Event;
		set => m_Event = value;
	}
	public float EventProbability {
		get => m_EventProbability;
		set => m_EventProbability = value;
	}
	public float EventInterval {
		get => m_EventInterval;
		set => m_EventInterval = value;
	}
	public float SearchRange {
		get => m_SearchRange;
		set => m_SearchRange = value;
	}
	float Timer {
		get => m_Timer;
		set => m_Timer = value;
	}
	Actor Target {
		get => m_Target;
		set => m_Target = value;
	}



	// Methods

	void Start() => Event.gameObject.SetActive(false);

	protected override void Simulate() {
		Timer -= Time.deltaTime;
		if (!Target) {
			if (Timer <= 0f) {
				Timer = EventInterval;
				if (Random.value < EventProbability) {
					float distance = SearchRange;
					foreach (var actor in Actors) {
						if (actor == this || actor is Player || !actor.IsSimulated) continue;
						float s = GetDistance(actor.gameObject);
						if (s < distance) {
							distance = s;
							Target = actor;
						}
					}
					if (Target) {
						Target.IsSimulated = false;
						CalculatePath(Target.gameObject);
						Target.CalculatePath(gameObject);
						Timer = 10f;
					}
				}
			}
		} else {
			float s = GetDistance(Target.gameObject);
			if (4f < s && Timer <= 0f) {
				Target.IsSimulated = true;
				Target = null;
			}
			if (s < 4f && 0f <= Timer) {
				ClearPath();
				LookAt(Target.gameObject);
				Target.ClearPath();
				Target.LookAt(gameObject);
				Emotion = Target.Emotion = Emotion.Thinking;
				Timer = 0f;
				Event.gameObject.SetActive(true);
				var a = transform.position;
				var b = Target.transform.position;
				Event.transform.position = Vector3.Lerp(a, b, 0.5f);
			}
			if (s < 4f && Timer < -10f) {
				Target.IsSimulated = true;
				Emotion = Target.Emotion = Emotion.None;
				Target = null;
				Timer = EventInterval;
				Event.gameObject.SetActive(false);
			}

		}
	}
}
