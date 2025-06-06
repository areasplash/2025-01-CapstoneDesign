using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif



// ━

public enum TriggerType {
	OnInteract,
	InRange,
};



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Event Trigger
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Component/Event Trigger")]
[RequireComponent(typeof(Collider2D))]
public sealed class EventTrigger : MonoBehaviour, IInteractable {

	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(EventTrigger))]
		class EventTriggerAuthoringEditor : EditorExtensions {
			EventTrigger I => target as EventTrigger;
			public override void OnInspectorGUI() {
				Begin("Event Trigger");

				LabelField("Event", EditorStyles.boldLabel);
				I.Event = ObjectField("Event Graph", I.Event);
				if (!I.Event) {
					BeginHorizontal();
					PrefixLabel(" ");
					if (Button("Create Event Graph")) {
						I.Event = CreateInstance<EventGraphSO>();
						I.Event.name = I.gameObject.name;
						I.Event.Open();
					}
					EndHorizontal();
				}
				Space();
				LabelField("Trigger", EditorStyles.boldLabel);
				I.InteractionType   = EnumField("Interaction Type",    I.InteractionType);
				I.TriggerType       = EnumField("Trigger Type",        I.TriggerType);
				I.PlayerOnly        = Toggle   ("Player Only",         I.PlayerOnly);
				I.UseIterationLimit = Toggle   ("Use Iteration Limit", I.UseIterationLimit);
				if (I.UseIterationLimit) I.Count = Mathf.Max(0, IntField("Count", I.Count));
				I.UseCooldown       = Toggle   ("Use Cooldown",        I.UseCooldown);
				if (I.UseCooldown) I.Cooldown = Mathf.Max(0, FloatField("Cooldown", I.Cooldown));
				Space();
				if (I.Event) {
					LabelField("Event Graph", EditorStyles.boldLabel);
					if (Button("Open Event Graph")) I.Event.Open();
					Space();
					var prop = serializedObject.FindProperty("m_Event");
					if (prop.objectReferenceValue) {
						var obj = new SerializedObject(prop.objectReferenceValue);
						EditorGUILayout.PropertyField(obj.FindProperty("m_OnEventBegin"));
						EditorGUILayout.PropertyField(obj.FindProperty("m_OnEventEnd"  ));
						obj.ApplyModifiedProperties();
					}
					Space();
				}
				End();
			}

			void OnSceneGUI() {
				if (I.Event == null) return;
				if (I.Event.Clone != null) {
					Tools.current = Tool.None;
					foreach (var data in I.Event.Clone.GetEvents()) data.DrawHandles();
				}
			}
		}

		void OnDrawGizmosSelected() {
			if (Event == null) return;
			Gizmos.color = Color.green;
			foreach (var data in Event.Entry.GetEvents()) data.DrawGizmos();

			if (Event.Clone == null) return;
			Gizmos.color = Color.white;
			foreach (var data in Event.Clone.GetEvents()) data.DrawGizmos();
		}
	#endif



	// Fields

	[SerializeField] EventGraphSO m_Event;
	[SerializeField] InteractionType m_InteractionType;
	[SerializeField] TriggerType     m_TriggerType;
	[SerializeField] bool  m_PlayerOnly = true;
	[SerializeField] bool  m_UseIterationLimit;
	[SerializeField] bool  m_UseCoolodwn;
	[SerializeField] int   m_Count;
	[SerializeField] float m_Cooldown;
	float m_Timer;



	// Properties

	public EventGraphSO Event {
		get => m_Event;
		set => m_Event = value;
	}
	public InteractionType InteractionType {
		get => m_InteractionType;
		set => m_InteractionType = value;
	}
	public TriggerType TriggerType {
		get => m_TriggerType;
		set => m_TriggerType = value;
	}
	public bool PlayerOnly {
		get => m_PlayerOnly;
		set => m_PlayerOnly = value;
	}
	public bool UseIterationLimit {
		get => m_UseIterationLimit;
		set => m_UseIterationLimit = value;
	}
	public bool UseCooldown {
		get => m_UseCoolodwn;
		set => m_UseCoolodwn = value;
	}
	public int Count {
		get => m_Count;
		set => m_Count = value;
	}
	public float Cooldown {
		get => m_Cooldown;
		set => m_Cooldown = value;
	}
	float Timer {
		get => m_Timer;
		set => m_Timer = value;
	}



	// Methods

	public void Interact(GameObject interactor) {
		if ((!UseIterationLimit || 0 < Count) && (!UseCooldown || Timer == 0f)) {
			GameManager.PlayEvent(Event);
			if (UseIterationLimit) Count--;
			if (UseCooldown) Timer = Cooldown;
		}
	}



	// Lifecycle

	void Update() {
		if (UseCooldown) Timer = Mathf.Max(0f, Timer - Time.deltaTime);
	}

	void OnTriggerStay2D(Collider2D other) {
		if (!PlayerOnly || other.TryGetComponent(out Player _)) switch (TriggerType) {
			case TriggerType.OnInteract:
				break;
			case TriggerType.InRange:
				Interact(other.gameObject);
				break;
		}
	}
}
