using UnityEngine;

#if UNITY_EDITOR
	using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// UI Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/UI Manager")]
public class UIManager : MonoSingleton<UIManager> {

	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(UIManager))]
		class UIManagerEditor : EditorExtensions {
			UIManager I => target as UIManager;
			public override void OnInspectorGUI() {
				Begin("UI Manager");
				
				End();
			}
		}
	#endif



	// Fields



	// Properties



	// Methods

}
