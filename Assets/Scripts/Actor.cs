using UnityEngine;
using System.Collections.Generic;



// ━

public enum State {
	Idle,
	Moving,
}

public enum Emotion {
	None,
	Empty,
	Thinking,
	Embarrassed,
	Surprised,
	Smiling,
	Crying,
	Moved,
};



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Actor
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Actor : MonoBehaviour {

	// Fields

	[SerializeField] Animator m_BodyAnimator;
	[SerializeField] Animator m_EmotionAnimator;
	State m_State;
	Emotion m_Emotion;
	List<SpriteRenderer> m_Renderers;

	Rigidbody2D m_Body;
	Vector2 m_MoveVector;
	[SerializeField] float m_Speed = 4f;



	// Properties

	protected Animator BodyAnimator {
		get => m_BodyAnimator;
		set => m_BodyAnimator = value;
	}
	protected Animator EmotionAnimator {
		get => m_EmotionAnimator;
		set => m_EmotionAnimator = value;
	}

	public State State {
		get => m_State;
		set {
			if (m_State != value) {
				m_State = value;
				BodyAnimator.Play(value.ToString());
			}
		}
	}
	public Emotion Emotion {
		get => m_Emotion;
		set {
			if (m_Emotion != value) {
				m_Emotion = value;
				EmotionAnimator.Play(value.ToString());
			}
		}
	}

	protected List<SpriteRenderer> Renderers {
		get {
			if (m_Renderers == null) {
				m_Renderers = new();
				var stack = new Stack<Transform>();
				stack.Push(transform);
				while (0 < stack.Count) {
					var body = stack.Pop();
					if (body.TryGetComponent(out SpriteRenderer renderer)) m_Renderers.Add(renderer);
					for (int i = 0; i < body.childCount; i++) stack.Push(body.GetChild(i));
				}
			}
			return m_Renderers;
		}
	}
	protected bool FlipX {
		get => Renderers[0].flipX;
		set => Renderers.ForEach(renderer => renderer.flipX = value);
	}



	protected Rigidbody2D Body => m_Body || TryGetComponent(out m_Body) ? m_Body : null;

	public Vector2 Position {
		get => Body.position;
		set => Body.position = value;
	}
	protected Vector2 MoveVector {
		get => m_MoveVector;
		set => m_MoveVector = value;
	}
	public float Speed {
		get => m_Speed;
		protected set => m_Speed = value;
	}



	// Methods

	protected virtual void Simulate() { }
	protected virtual void Act() { }
	protected virtual void Draw() { }



	public void LookAt(GameObject target) {
		if (target == null) return;
		var direction = target.transform.position - transform.position;
		FlipX = direction.x < 0f;
	}



	// Lifecycle

	void Update() {
		switch (GameManager.GameState) {
			case GameState.Gameplay:
				Simulate();
				Act();
				Draw();
				break;
			case GameState.Cutscene:
				Act();
				Draw();
				break;
			case GameState.Paused:
				Draw();
				break;
		}
	}
}
