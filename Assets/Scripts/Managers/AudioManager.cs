using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif



// Audio Clips

public enum Audio {
	None,
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Audio Manager
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("Manager/Audio Manager")]
public sealed class AudioManager : MonoSingleton<AudioManager> {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(AudioManager))]
	class AudioManagerEditor : EditorExtensions {
		AudioManager I => target as AudioManager;
		public override void OnInspectorGUI() {
			Begin("Audio Manager");
			I.TrySetInstance();

			LabelField("Audio Mixer", EditorStyles.boldLabel);
			AudioMixer   = ObjectField("Audio Mixer",   AudioMixer);
			MusicGroup   = ObjectField("Music Group",   MusicGroup);
			SoundFXGroup = ObjectField("SoundFX Group", SoundFXGroup);
			Space();

			LabelField("Audio Instance", EditorStyles.boldLabel);
			AudioTemplate = ObjectField("Audio Template", AudioTemplate);
			if (AudioTemplate == null) {
				var message = string.Empty;
				message += "Audio Template is required.\n";
				message += "Create an Audio Source as a child of this object and assign it here.";
				HelpBox(message, MessageType.Warning);
				Space();
			} else {
				LabelField("Audio Pool", $"{Audios.Count} / {Audios.Count + Pooled.Count}");
				Space();
			}

			LabelField("Audio Clip", EditorStyles.boldLabel);
			SourcePath = TextField("Source Path", SourcePath);
			BeginHorizontal();
			PrefixLabel("Load Audio Clips");
			if (Button("Clear")) ClearAudioClips();
			if (Button("Load")) LoadAudioClips();
			EndHorizontal();
			Space();

			LabelField("Audio Clip Data", EditorStyles.boldLabel);
			LabelField("Count", $"{List.Count} / {AudioLength}");
			BeginDisabledGroup();
			int i = 0;
			int max = Mathf.Min(8, List.Count);
			foreach (var audio in List) {
				if (max < ++i) break;
				ObjectField(audio.ToString(), Data[(int)audio].AudioClip);
			}
			if (max < i) LabelField("...");
			EndDisabledGroup();
			Space();

			End();
		}
	}
	#endif



	// Constants

	const string MusicKey = "Music";
	const float MusicDefault = 1f;

	const string SoundFXKey = "SoundFX";
	const float SoundFXDefault = 1f;



	static readonly int AudioLength = Enum.GetValues(typeof(Audio)).Length;

	enum State : byte {
		Unloaded,
		Loaded,
		Preloaded,
	}

	[Serializable]
	struct DataSet {
		public AudioClip AudioClip;
		public State State;
		public float LastPlayed;
	}

	const int TimeSliceCount = 10;
	const float Lifetime = 30f;



	// Fields

	[SerializeField] AudioMixer m_AudioMixer;
	[SerializeField] AudioMixerGroup m_MusicGroup;
	[SerializeField] AudioMixerGroup m_SoundFXGroup;
	float? m_Music;
	float? m_SoundFX;

	[SerializeField] AudioSource m_AudioTemplate;
	Dictionary<uint, (AudioSource, Audio)> m_Audios = new();
	Stack<AudioSource> m_Pooled = new();
	uint m_ID;
	List<uint> m_IDs = new();

	[SerializeField] string m_SourcePath = "Assets/Audio";
	[SerializeField] List<Audio> m_SourceList = new();
	[SerializeField] DataSet[] m_SourceData = new DataSet[AudioLength];
	int m_Index;



	// Properties

	static AudioMixer AudioMixer {
		get => Instance.m_AudioMixer;
		set => Instance.m_AudioMixer = value;
	}
	static AudioMixerGroup MusicGroup {
		get => Instance.m_MusicGroup;
		set => Instance.m_MusicGroup = value;
	}
	static AudioMixerGroup SoundFXGroup {
		get => Instance.m_SoundFXGroup;
		set => Instance.m_SoundFXGroup = value;
	}

	public static float Music {
		get => Instance.m_Music ??= PlayerPrefs.GetFloat(MusicKey, MusicDefault);
		set {
			PlayerPrefs.SetFloat(MusicKey, (Instance.m_Music = value).Value);
			AudioMixer?.SetFloat(MusicKey, Mathf.Log10(Mathf.Max(0.00001f, Music)) * 20f);
		}
	}
	public static float SoundFX {
		get => Instance.m_SoundFX ??= PlayerPrefs.GetFloat(SoundFXKey, SoundFXDefault);
		set {
			PlayerPrefs.SetFloat(SoundFXKey, (Instance.m_SoundFX = value).Value);
			AudioMixer?.SetFloat(SoundFXKey, Mathf.Log10(Mathf.Max(0.00001f, SoundFX)) * 20f);
		}
	}



	static AudioSource AudioTemplate {
		get => Instance.m_AudioTemplate;
		set => Instance.m_AudioTemplate = value;
	}
	static Dictionary<uint, (AudioSource, Audio)> Audios => Instance.m_Audios;
	static Stack<AudioSource> Pooled => Instance.m_Pooled;

	static uint ID {
		get => Instance.m_ID;
		set => Instance.m_ID = value;
	}
	static List<uint> IDs => Instance.m_IDs;



	static string SourcePath {
		get => Instance.m_SourcePath;
		set => Instance.m_SourcePath = value;
	}
	static List<Audio> List => Instance.m_SourceList;
	static DataSet[] Data => Instance.m_SourceData;

	static int Index {
		get => Instance.m_Index;
		set => Instance.m_Index = value;
	}



	// Data Methods

	#if UNITY_EDITOR
	static void ClearAudioClips() {
		List.Clear();
		Data.Initialize();
	}

	static void LoadAudioClips() {
		ClearAudioClips();
		foreach (var clip in LoadAssets<AudioClip>(SourcePath)) {
			if (!Enum.TryParse(clip.name, out Audio audio)) continue;
			List.Add(audio);
			Data[(int)audio] = new DataSet {
				AudioClip = clip,
				State = clip.preloadAudioData ? State.Preloaded : State.Unloaded,
			};
		}
		List.TrimExcess();

		var message = string.Empty;
		for (int i = 0; i < AudioLength; i++) if (!Data[i].AudioClip) message += $"{(Audio)i}, ";
		if (!string.IsNullOrEmpty(message)) Debug.Log($"Missing audio clips: {message}");
	}

	public static T[] LoadAssets<T>(string path) where T : UnityEngine.Object {
		var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { path });
		var assets = new T[guids.Length];
		for (int i = 0; i < guids.Length; i++) {
			string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
			assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
		}
		return assets;
	}
	#endif



	// Object Pool Methods

	static (uint, AudioSource) GetOrCreateInstance(Audio audio) {
		if (!Pooled.TryPop(out var instance)) {
			instance = Instantiate(AudioTemplate, Instance.transform);
		}
		ref var data = ref Data[(int)audio];
		if (data.AudioClip) {
			instance.clip = data.AudioClip;
			if (data.State == State.Unloaded) {
				data.State = State.Loaded;
				data.AudioClip.LoadAudioData();
			}
		}
		instance.gameObject.SetActive(true);
		while (++ID == default || Audios.ContainsKey(ID));
		Audios.Add(ID, (instance, audio));
		return (ID, instance);
	}

	static void ReleaseInstance(uint id) {
		var instance = Audios[id].Item1;
		if (instance) {
			instance.gameObject.SetActive(false);
			instance.transform.localPosition = default;
			instance.outputAudioMixerGroup = AudioTemplate.outputAudioMixerGroup;
			instance.loop = AudioTemplate.loop;
			instance.volume = AudioTemplate.volume;
			instance.spatialBlend = AudioTemplate.spatialBlend;
			instance.spread = AudioTemplate.spread;
			instance.minDistance = AudioTemplate.minDistance;
			instance.maxDistance = AudioTemplate.maxDistance;
			Pooled.Push(instance);
		}
		Audios.Remove(id);
	}

	static void UpdateInstances() {
		foreach (var (id, (instance, audio)) in Audios) {
			ref var data = ref Data[(int)audio];
			if (instance.isPlaying) data.LastPlayed = Time.time;
			else if (data.AudioClip.loadState != AudioDataLoadState.Loading) IDs.Add(id);
		}
		foreach (var id in IDs) ReleaseInstance(id);
		IDs.Clear();

		int maxIndex = Mathf.Min(Index + TimeSliceCount, List.Count);
		for (; Index < maxIndex; Index++) {
			ref var data = ref Data[(int)List[Index]];
			if (data.State == State.Loaded && Lifetime <= Time.time - data.LastPlayed) {
				data.State = State.Unloaded;
				data.AudioClip.UnloadAudioData();
			}
		}
		if (Index == List.Count) Index = 0;
	}



	// Audio Methods

	public static uint PlayMusic(Audio audio, float volume = 1f) {
		var (id, instance) = GetOrCreateInstance(audio);
		instance.outputAudioMixerGroup = MusicGroup;
		instance.loop = true;
		instance.volume = volume;
		instance.Play();
		return id;
	}

	public static uint PlaySoundFX(Audio audio, float volume = 1f) {
		var (id, instance) = GetOrCreateInstance(audio);
		instance.outputAudioMixerGroup = SoundFXGroup;
		instance.volume = volume;
		instance.Play();
		return id;
	}

	public static uint PlayPointSoundFX(Audio audio, Vector3 position, float volume = 1f,
		float spread = 0f, float minDistance = default, float maxDistance = default) {
		var (id, instance) = GetOrCreateInstance(audio);
		instance.transform.position = position;
		instance.outputAudioMixerGroup = SoundFXGroup;
		instance.volume = volume;
		instance.spatialBlend = 1f;
		instance.spread = spread;
		if (minDistance != default) instance.minDistance = minDistance;
		if (maxDistance != default) instance.maxDistance = maxDistance;
		instance.Play();
		return id;
	}

	public static uint PlayBlendSoundFX(Audio audio, Vector3 position, float volume = 1f,
		float spatialBlend = 0.5f) {
		var (id, instance) = GetOrCreateInstance(audio);
		instance.transform.position = position;
		instance.outputAudioMixerGroup = SoundFXGroup;
		instance.volume = volume;
		instance.spatialBlend = spatialBlend;
		instance.Play();
		return id;
	}

	public static void StopAudio(uint id) {
		if (Audios.TryGetValue(id, out var value)) {
			value.Item1.Stop();
			ReleaseInstance(id);
		}
	}

	public static void SetAudioPosition(uint id, Vector3 position) {
		if (Audios.TryGetValue(id, out var value)) value.Item1.transform.position = position;
	}



	// Lifecycle

	void Start() {
		_ = Music;
		_ = SoundFX;
	}

	void Update() {
		UpdateInstances();
	}
}
