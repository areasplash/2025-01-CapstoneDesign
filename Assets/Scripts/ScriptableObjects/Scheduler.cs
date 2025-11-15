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
		float day = Mathf.Floor(time);
		float timeOfDay = (time - day) * 24f;

		foreach (var behavior in Behaviors) {
			if (behavior.isTimeFixed) {
				if (timeOfDay >= behavior.minStartTime && timeOfDay < behavior.maxStartTime) {
					if (actor.BehaviorName != behavior.name) {
						actor.BehaviorName = behavior.name;
						actor.BehaviorStartTime = time;
						float durationInHours = UnityEngine.Random.Range(behavior.minDuration, behavior.maxDuration);
						actor.BehaviorDuration = durationInHours / 24f;
					}
					
					Vector3 position = actor.transform.position;
					if (behavior.isLocationBased) {
						Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized;
						float randomRange = UnityEngine.Random.Range(0f, behavior.locationRange);
						position = behavior.location + randomDirection * randomRange;
					}
					return position;
				}
			}
		}

		if (!string.IsNullOrEmpty(actor.BehaviorName)) {
			var currentBehavior = Behaviors.Find(b => b.name == actor.BehaviorName);

			if (currentBehavior.isTimeFixed) {
				actor.BehaviorName = null;
			} else {
				float behaviorEndTime = actor.BehaviorStartTime + actor.BehaviorDuration;
				if (time < behaviorEndTime) {
					Vector3 position = actor.transform.position;
					if (currentBehavior.isLocationBased) {
						Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized;
						float randomRange = UnityEngine.Random.Range(0f, currentBehavior.locationRange);
						position = currentBehavior.location + randomDirection * randomRange;
					}
					return position;
				} else {
					actor.BehaviorName = null;
				}
			}
		}

		List<Behavior> randomBehaviors = new List<Behavior>();
		float totalWeight = 0f;
		foreach (var behavior in Behaviors) {
			if (!behavior.isTimeFixed) {
				randomBehaviors.Add(behavior);
				totalWeight += behavior.weight;
			}
		}

		if (randomBehaviors.Count == 0) {
			return actor.transform.position;
		}

		float randomRoll = UnityEngine.Random.Range(0f, totalWeight);
		Behavior selectedBehavior = randomBehaviors[randomBehaviors.Count - 1];

		foreach (var behavior in randomBehaviors) {
			if (randomRoll <= behavior.weight) {
				selectedBehavior = behavior;
				break;
			}
			randomRoll -= behavior.weight;
		}

		actor.BehaviorName = selectedBehavior.name;
		actor.BehaviorStartTime = time;
		float newDurationInHours = UnityEngine.Random.Range(selectedBehavior.minDuration, selectedBehavior.maxDuration);
		actor.BehaviorDuration = newDurationInHours / 24f;

		Vector3 newPosition = actor.transform.position;
		if (selectedBehavior.isLocationBased) {
			Vector3 randomDirection = UnityEngine.Random.insideUnitSphere.normalized;
			float randomRange = UnityEngine.Random.Range(0f, selectedBehavior.locationRange);
			newPosition = selectedBehavior.location + randomDirection * randomRange;
		}
		return newPosition;
	}
}
