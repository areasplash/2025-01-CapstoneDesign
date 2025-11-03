using UnityEngine;
using System;
using System.Collections.Generic;
using Random = Unity.Mathematics.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif



[Serializable]
public struct Behavior {
	public string name;

	public bool isTimeFixed;
	public float minStartTime;
	public float maxStartTime;
	public float weight;
	public float minDuration;
	public float maxDuration;

	public bool isLocationBased;
	public Vector3 location;
	public float locationRange;
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Scheduler
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[CreateAssetMenu(fileName = "Scheduler", menuName = "Scriptable Objects/Scheduler")]
public class Scheduler : ScriptableObject {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(Scheduler))]
	class SchedulerEditor : EditorExtensions {
		Scheduler I => target as Scheduler;
		public override void OnInspectorGUI() {
			Begin("Scheduler");

			HelpBox("All time values are in 24-hour format (0.0 to 24.0).", MessageType.Info);
			for (int i = 0; i < I.Behaviors.Count; i++) {
				var behavior = I.Behaviors[i];
				LabelField($"  {behavior.name}", EditorStyles.boldLabel);

				BeginVertical(EditorStyles.helpBox);
				BeginHorizontal();
				behavior.name = TextField("Name", behavior.name);
				if (Button("-", GUILayout.Width(18f))) {
					I.Behaviors.RemoveAt(i);
					break;
				}
				EndHorizontal();
				Space();

				BeginHorizontal();
				PrefixLabel("Is Time Fixed");
				behavior.isTimeFixed = ToggleLeft(behavior.isTimeFixed switch {
					true  => "Time Fixed",
					false => "Random",
				}, behavior.isTimeFixed);
				EndHorizontal();
				if (behavior.isTimeFixed) {
					BeginHorizontal();
					PrefixLabel("Start Time (Min / Max)");
					behavior.minStartTime = FloatField(behavior.minStartTime);
					behavior.maxStartTime = FloatField(behavior.maxStartTime);
					behavior.maxStartTime = Mathf.Max(behavior.minStartTime, behavior.maxStartTime);
					EndHorizontal();
				} else {
					behavior.weight = Slider("Weight", behavior.weight, 0f, 1f);
				}
				BeginHorizontal();
				PrefixLabel("Duration (Min / Max)");
				behavior.minDuration = FloatField(behavior.minDuration);
				behavior.maxDuration = FloatField(behavior.maxDuration);
				behavior.maxDuration = Mathf.Max(behavior.minDuration, behavior.maxDuration);
				EndHorizontal();
				Space();

				BeginHorizontal();
				PrefixLabel("Is Location Based");
				behavior.isLocationBased = ToggleLeft(behavior.isLocationBased switch {
					true  => "Location Based",
					false => "Location Free",
				}, behavior.isLocationBased);
				EndHorizontal();
				if (behavior.isLocationBased) {
					behavior.location = Vector3Field("Location", behavior.location);
					BeginHorizontal();
					PrefixLabel("Location Range");
					behavior.locationRange = FloatField(behavior.locationRange);
					EndHorizontal();
				}
				I.Behaviors[i] = behavior;
				EndVertical();
			}
			if (Button("Add Behavior")) {
				I.Behaviors.Add(new Behavior() {
					name = "New Behavior",
					isTimeFixed = true,
					weight = 0.5f,
					isLocationBased = true,
				});
			}

			End();
		}
	}
	#endif



	// Fields

	[SerializeField] List<Behavior> m_Behaviors = new();



	// Properties

	List<Behavior> Behaviors {
		get => m_Behaviors;
	}



    // Methods

	public Vector3 GetNextBehavior(Actor actor, float time) {
		uint day = (uint)Mathf.FloorToInt(time);
		uint seed = (uint)actor.GetInstanceID() ^ (day * 997);
		float timeOfDay = (time - day) * 24f;
		float totalWeight = 0f;

		foreach (var behavior in Behaviors) if (behavior.isTimeFixed) {
			uint behaviorSeed = seed ^ (uint)behavior.name.GetHashCode();
			var random = new Random(behaviorSeed);
			float startTime = random.NextFloat(behavior.minStartTime, behavior.maxStartTime);
			float duration = random.NextFloat(behavior.minDuration, behavior.maxDuration);
			if (timeOfDay >= startTime && timeOfDay < startTime + duration) {
				if (actor.BehaviorName != behavior.name) {
					actor.BehaviorName = behavior.name;
					actor.BehaviorStartTime = time;
				}
				var position = actor.transform.position;
				if (behavior.isLocationBased) {
					var direction = (Vector3)random.NextFloat3Direction();
					float range = random.NextFloat(0f, behavior.locationRange);
					position = behavior.location + direction * range;
				}
				return position;
			}
		} else totalWeight += behavior.weight;

		if (!string.IsNullOrEmpty(actor.BehaviorName)) {
			var behavior = Behaviors.Find(b => b.name == actor.BehaviorName);
			if (behavior.isTimeFixed) {
				actor.BehaviorName = null;
			} else {
				uint startDay = (uint)Mathf.FloorToInt(actor.BehaviorStartTime);
				uint startSeed = (uint)actor.GetInstanceID() ^ (startDay * 997);
				uint behaviorSeed = startSeed ^ (uint)behavior.name.GetHashCode();
				var random = new Random(behaviorSeed);
				float startTime = actor.BehaviorStartTime;
				float duration = random.NextFloat(behavior.minDuration, behavior.maxDuration);
				if (time < startTime + duration) {
					var position = actor.transform.position;
					if (behavior.isLocationBased) {
						var direction = (Vector3)random.NextFloat3Direction();
						float range = random.NextFloat(0f, behavior.locationRange);
						position = behavior.location + direction * range;
					}
					return position;
				} else {
					actor.BehaviorName = null;
				}
			}
		} {
			var behavior = (Behavior)default;
			var temp = new Random(seed ^ (uint)(time * 100000));
			float value = temp.NextFloat(0f, totalWeight);
			foreach (var behav1or in Behaviors) if (!behav1or.isTimeFixed) {
				if ((value -= behav1or.weight) <= 0f) {
					behavior = behav1or;
					break;
				}
			}
			actor.BehaviorName = behavior.name;
			actor.BehaviorStartTime = time;
			uint behaviorSeed = seed ^ (uint)behavior.name.GetHashCode();
			var random = new Random(behaviorSeed);
			var position = actor.transform.position;
			if (behavior.isLocationBased) {
				var direction = (Vector3)random.NextFloat3Direction();
				float range = random.NextFloat(0f, behavior.locationRange);
				position = behavior.location + direction * range;
			}
			return position;
		}
	}
}
