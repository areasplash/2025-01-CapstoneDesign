using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
	using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Player
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

public class Player : Actor {

	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(Player))]
		class PlayerEditor : EditorExtensions {
			Player I => target as Player;
			public override void OnInspectorGUI() {
				Begin("Player");

				LabelField("Animator", EditorStyles.boldLabel);
				I.BodyAnimator    = ObjectField("Body Animator",    I.BodyAnimator);
				I.EmotionAnimator = ObjectField("Emotion Animator", I.EmotionAnimator);
				Space();
				LabelField("Physics", EditorStyles.boldLabel);
				I.Speed = FloatField("Speed", I.Speed);
				Space();

				End();
			}
		}
	#endif



	// Properties

	public bool IsInteractable => 0 < list.Count;



	// Methods

	readonly List<GameObject> list = new();
	void FixedUpdate() => list.Clear();
	void OnTriggerStay2D(Collider2D other) => list.Add(other.gameObject);

	public IInteractable GetInteractable() {
		if (list.Count == 0) return null;
		float closest = float.MaxValue;
		var closestObject = default(GameObject);
		foreach (var gameObject in list) {
			if (gameObject == null) continue;
			float xdelta = transform.position.x - gameObject.transform.position.x;
			float ydelta = transform.position.y - gameObject.transform.position.y;
			float distancesq = xdelta * xdelta + ydelta * ydelta;
			if (distancesq < closest) {
				closest = distancesq;
				closestObject = gameObject;
			}
		}
		return closestObject.TryGetComponent(out IInteractable interactable) ? interactable : null;
	}



	protected override void Simulate() {
		MoveVector = InputManager.MoveDirection;
		if (InputManager.GetKeyDown(KeyAction.Interact)) GetInteractable()?.Interact(gameObject);
	}

	protected override void Act() {
		State = MoveVector == default ? State.Idle : State.Moving;
		FlipX = MoveVector.x < 0f;
		Body.linearVelocity = GameManager.GridMultiplier * MoveVector * Speed;
		MoveVector = default;
	}
}
