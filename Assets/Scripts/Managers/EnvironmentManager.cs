using UnityEngine;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Environment Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/Environment Manager")]
[RequireComponent(typeof(Light2D))]
public class EnvironmentManager : MonoSingleton<EnvironmentManager> {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(EnvironmentManager))]
	class EnvironmentManagerEditor : EditorExtensions {
		EnvironmentManager I => target as EnvironmentManager;
		public override void OnInspectorGUI() {
			Begin("Environment Manager");
			I.TrySetInstance();

			LabelField("Main Light", EditorStyles.boldLabel);
			BeginHorizontal();
			PrefixLabel("Day Color Intensity");
			DayColor = ColorField(DayColor);
			DayIntensity = EditorGUILayout.FloatField(DayIntensity, GUILayout.Width(60f));
			EndHorizontal();
			BeginHorizontal();
			PrefixLabel("Night Color Intensity");
			NightColor = ColorField(NightColor);
			NightIntensity = EditorGUILayout.FloatField(NightIntensity, GUILayout.Width(60f));
			EndHorizontal();
			DayLength = FloatField("Day Length",  DayLength);
			TimeOfDay = FloatField("Time of Day", TimeOfDay);
			IntentLevel++;
			Simulate = Toggle("Simulate", Simulate);
			IntentLevel--;
			Space();

			End();
		}
	}
	#endif



	// Fields

	[SerializeField] Color m_DayColor = new(1.0f, 1.0f, 1.0f, 0f);
	[SerializeField] Color m_NightColor = new(0.1f, 0.1f, 0.3f, 0f);
	[SerializeField] float m_DayIntensity = 1.0f;
	[SerializeField] float m_NightIntensity = 0.5f;

	Light2D m_MainLight;
	[SerializeField] float m_DayLength = 300f;
	[SerializeField] float m_TimeOfDay = 0.5f;
	[SerializeField] bool m_Simulate = true;



	// Properties

	static Light2D MainLight => Instance.m_MainLight ||
		Instance.TryGetComponent(out Instance.m_MainLight) ?
		Instance.m_MainLight : null;

	static float DayIntensity {
		get => Instance.m_DayIntensity;
		set => Instance.m_DayIntensity = value;
	}
	static float NightIntensity {
		get => Instance.m_NightIntensity;
		set => Instance.m_NightIntensity = value;
	}
	static Color DayColor {
		get => Instance.m_DayColor;
		set => Instance.m_DayColor = value;
	}
	static Color NightColor {
		get => Instance.m_NightColor;
		set => Instance.m_NightColor = value;
	}

	public static float DayLength {
		get => Instance.m_DayLength;
		set => Instance.m_DayLength = value;
	}
	public static float TimeOfDay {
		get => Instance.m_TimeOfDay;
		set {
			if (Instance.m_TimeOfDay != value) {
				Instance.m_TimeOfDay = value;
				float t = value % 1f;
				float blend = t switch {
					>= 0.3f and < 0.7f => 1f,
					>= 0.2f and < 0.3f => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, 0.3f, t)),
					>= 0.7f and < 0.8f => Mathf.SmoothStep(1f, 0f, Mathf.InverseLerp(0.7f, 0.8f, t)),
					_ => 0f,
				};
				MainLight.color = Color.Lerp(NightColor, DayColor, blend);
				MainLight.intensity = Mathf.Lerp(NightIntensity, DayIntensity, blend);
			}
		}
	}
	public static bool Simulate {
		get => Instance.m_Simulate;
		set => Instance.m_Simulate = value;
	}



	// Lifecycle

	void Update() {
		if (Simulate) TimeOfDay += Time.deltaTime / DayLength;
	}
}
