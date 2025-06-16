using UnityEngine;



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Base Canvas
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[RequireComponent(typeof(Canvas))]
public abstract class BaseCanvas : MonoBehaviour {

	// Methods

	public virtual void Show() {
		gameObject.SetActive(true);
	}

	public virtual void Hide(bool keepState = false) {
		gameObject.SetActive(false);
	}

	public virtual void Back() {
		UIManager.PopOverlay();
	}



	// Lifecycle

	protected virtual void Update() {
		if (UIManager.CurrentCanvas != this) return;
		if (InputManager.GetKeyUp(KeyAction.Cancel)) Back();
	}
}
