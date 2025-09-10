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
				message += "Audio Template is missing.\n";
				message += "Create an Audio Source as a child of this object and assign it here.";
				HelpBox(message, MessageType.Warning);
				Space();
			} else {
				LabelField("Audio Pool", $"{Audios.Count} / {Audios.Count + AudioPool.Count}");
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
			LabelField("Count", $"{ClipList.Count} / {AudioCount}");
			BeginDisabledGroup();
			int i = 0;
			int max = Mathf.Min(8, ClipList.Count);
			foreach (var audio in ClipList) {
				if (max < ++i) break;
				ObjectField(audio.ToString(), ClipData[(int)audio].AudioClip);
			}
			if (max < i) LabelField("...");
			EndDisabledGroup();
			Space();

			End();
		}
	}
	#endif



	// Constants

	const string MusicVolumeK = "MusicVolume";
	const float MusicVolumeD = 1f;

	const string SoundFXVolumeK = "SoundFXVolume";
	const float SoundFXVolumeD = 1f;



	static readonly int AudioCount = Enum.GetValues(typeof(Audio)).Length;

	enum State : byte {
		Unloaded,
		Loaded,
		Preloaded,
	}

	[Serializable]
	struct ClipEntry {
		public AudioClip AudioClip;
		public State State;
		public float LastPlayTime;
	}

	const int TimeSliceCount = 30;
	const float UnloadThreshold = 30f;



	// Fields

	[SerializeField] AudioMixer m_AudioMixer;
	[SerializeField] AudioMixerGroup m_MusicGroup;
	[SerializeField] AudioMixerGroup m_SoundFXGroup;
	float? m_MusicVolume;
	float? m_SoundFXVolume;

	[SerializeField] AudioSource m_AudioTemplate;
	Dictionary<uint, (AudioSource, Audio)> m_Audios = new();
	Stack<AudioSource> m_AudioPool = new();
	List<uint> m_IDBuffer = new();
	uint m_NextID;
	uint m_MusicID;

	[SerializeField] string m_SourcePath = "Assets/Audio";
	[SerializeField] List<Audio> m_ClipList = new();
	[SerializeField] ClipEntry[] m_ClipData = new ClipEntry[AudioCount];
	int m_SliceIndex;



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

	public static float MusicVolume {
		get => Instance.m_MusicVolume ??= PlayerPrefs.GetFloat(MusicVolumeK, MusicVolumeD);
		set {
			PlayerPrefs.SetFloat(MusicVolumeK, (Instance.m_MusicVolume = value).Value);
			AudioMixer?.SetFloat(MusicVolumeK, Mathf.Log10(Mathf.Max(0.00001f, value)) * 20f);
		}
	}
	public static float SoundFXVolume {
		get => Instance.m_SoundFXVolume ??= PlayerPrefs.GetFloat(SoundFXVolumeK, SoundFXVolumeD);
		set {
			PlayerPrefs.SetFloat(SoundFXVolumeK, (Instance.m_SoundFXVolume = value).Value);
			AudioMixer?.SetFloat(SoundFXVolumeK, Mathf.Log10(Mathf.Max(0.00001f, value)) * 20f);
		}
	}



	static AudioSource AudioTemplate {
		get => Instance.m_AudioTemplate;
		set => Instance.m_AudioTemplate = value;
	}
	static Dictionary<uint, (AudioSource, Audio)> Audios => Instance.m_Audios;
	static Stack<AudioSource> AudioPool => Instance.m_AudioPool;

	static List<uint> IDBuffer => Instance.m_IDBuffer;

	static uint NextID {
		get => Instance.m_NextID;
		set => Instance.m_NextID = value;
	}
	static uint MusicID {
		get => Instance.m_MusicID;
		set => Instance.m_MusicID = value;
	}



	static string SourcePath {
		get => Instance.m_SourcePath;
		set => Instance.m_SourcePath = value;
	}
	static List<Audio> ClipList => Instance.m_ClipList;
	static ClipEntry[] ClipData {
		get => Instance.m_ClipData;
		set => Instance.m_ClipData = value;
	}

	static int SliceIndex {
		get => Instance.m_SliceIndex;
		set => Instance.m_SliceIndex = value;
	}



	// Data Methods

	#if UNITY_EDITOR
	static void ClearAudioClips() {
		ClipList.Clear();
		ClipData = new ClipEntry[AudioCount];
	}

	static void LoadAudioClips() {
		ClearAudioClips();
		foreach (var clip in LoadAssets<AudioClip>(SourcePath)) {
			if (!Enum.TryParse(clip.name, out Audio audio)) continue;
			ClipList.Add(audio);
			ClipData[(int)audio] = new ClipEntry {
				AudioClip = clip,
				State = clip.preloadAudioData ? State.Preloaded : State.Unloaded,
			};
		}
		ClipList.TrimExcess();

		var message = string.Empty;
		for (int i = 0; i < AudioCount; i++) if (!ClipData[i].AudioClip)
			message += $"{(string.IsNullOrEmpty(message) ? string.Empty : ", ")}{(Audio)i}";
		if (!string.IsNullOrEmpty(message)) Debug.Log($"Missing audio clips:\n{message}");
	}

	static T[] LoadAssets<T>(string path) where T : UnityEngine.Object {
		var guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { path });
		var assets = new T[guids.Length];
		for (int i = 0; i < guids.Length; i++) {
			string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
			assets[i] = AssetDatabase.LoadAssetAtPath<T>(assetPath);
		}
		return assets;
	}
	#endif



	// Instance Methods

	static (uint, AudioSource) GetOrCreateInstance(Audio audio) {
		if (!AudioPool.TryPop(out var instance)) {
			instance = Instantiate(AudioTemplate);
		}
		ref var data = ref ClipData[(int)audio];
		if (data.AudioClip) {
			instance.clip = data.AudioClip;
			if (data.State == State.Unloaded) {
				data.State = State.Loaded;
				data.AudioClip.LoadAudioData();
			}
		}
		instance.gameObject.SetActive(true);
		while (++NextID == default || Audios.ContainsKey(NextID));
		Audios.Add(NextID, (instance, audio));
		return (NextID, instance);
	}

	static void UpdateInstances() {
		foreach (var (audioID, (instance, audio)) in Audios) {
			if (instance) {
				ref var data = ref ClipData[(int)audio];
				if (instance.isPlaying) data.LastPlayTime = Time.time;
				else {
					var state = data.AudioClip.loadState;
					if (state != AudioDataLoadState.Loading) IDBuffer.Add(audioID);
				}
			} else IDBuffer.Add(audioID);
		}
		if (0 < IDBuffer.Count) {
			foreach (var audioID in IDBuffer) RemoveInstance(audioID);
			IDBuffer.Clear();
		}
		int lastIndex = Mathf.Min(SliceIndex + TimeSliceCount, ClipList.Count);
		for (; SliceIndex < lastIndex; SliceIndex++) {
			ref var data = ref ClipData[(int)ClipList[SliceIndex]];
			if (data.State == State.Loaded && UnloadThreshold <= Time.time - data.LastPlayTime) {
				data.State = State.Unloaded;
				data.AudioClip.UnloadAudioData();
			}
		}
		if (SliceIndex == ClipList.Count) SliceIndex = 0;
	}

	static void RemoveInstance(uint audioID) {
		var instance = Audios[audioID].Item1;
		if (instance) {
			var transform_localPosition = AudioTemplate.transform.localPosition;
			if (instance.transform.localPosition != transform_localPosition) {
				instance.transform.localPosition = transform_localPosition;
			}
			var outputAudioMixerGroup = AudioTemplate.outputAudioMixerGroup;
			if (instance.outputAudioMixerGroup != outputAudioMixerGroup) {
				instance.outputAudioMixerGroup = outputAudioMixerGroup;
			}
			var loop = AudioTemplate.loop;
			if (instance.loop != loop) {
				instance.loop = loop;
			}
			var volume = AudioTemplate.volume;
			if (instance.volume != volume) {
				instance.volume = volume;
			}
			var spatialBlend = AudioTemplate.spatialBlend;
			if (instance.spatialBlend != spatialBlend) {
				instance.spatialBlend = spatialBlend;
			}
			var spread = AudioTemplate.spread;
			if (instance.spread != spread) {
				instance.spread = spread;
			}
			var minDistance = AudioTemplate.minDistance;
			if (instance.minDistance != minDistance) {
				instance.minDistance = minDistance;
			}
			var maxDistance = AudioTemplate.maxDistance;
			if (instance.maxDistance != maxDistance) {
				instance.maxDistance = maxDistance;
			}
			instance.gameObject.SetActive(false);
			AudioPool.Push(instance);
		}
		Audios.Remove(audioID);
	}



	// Audio Methods

	public static uint PlayMusic(Audio audio, float volume = 1f) {
		StopAudio(MusicID);
		var (audioID, instance) = GetOrCreateInstance(audio);
		instance.outputAudioMixerGroup = MusicGroup;
		instance.loop = true;
		instance.volume = volume;
		instance.Play();
		return MusicID = audioID;
	}

	public static uint PlaySoundFX(Audio audio, float volume = 1f) {
		var (audioID, instance) = GetOrCreateInstance(audio);
		instance.outputAudioMixerGroup = SoundFXGroup;
		instance.volume = volume;
		instance.Play();
		return audioID;
	}

	public static uint PlayPointSoundFX(
		Audio audio, Vector3 position, float volume = 1f, float spread = 0f,
		float minDistance = default, float maxDistance = default) {
		var (audioID, instance) = GetOrCreateInstance(audio);
		instance.transform.position = position;
		instance.outputAudioMixerGroup = SoundFXGroup;
		instance.volume = volume;
		instance.spatialBlend = 1f;
		instance.spread = spread;
		if (minDistance != default) instance.minDistance = minDistance;
		if (maxDistance != default) instance.maxDistance = maxDistance;
		instance.Play();
		return audioID;
	}

	public static uint PlayBlendSoundFX(
		Audio audio, Vector3 position, float volume = 1f, float spatialBlend = 0.5f) {
		var (audioID, instance) = GetOrCreateInstance(audio);
		instance.transform.position = position;
		instance.outputAudioMixerGroup = SoundFXGroup;
		instance.volume = volume;
		instance.spatialBlend = spatialBlend;
		instance.Play();
		return audioID;
	}

	public static void StopAudio(uint audioID) {
		if (Audios.TryGetValue(audioID, out var value)) {
			var instance = value.Item1;
			instance.Stop();
			RemoveInstance(audioID);
		}
	}

	public static void SetAudioPosition(uint audioID, Vector3 position) {
		if (Audios.TryGetValue(audioID, out var value)) {
			var instance = value.Item1;
			instance.transform.position = position;
		}
	}

	public static void SetAudioVolume(uint audioID, float volume) {
		if (Audios.TryGetValue(audioID, out var value)) {
			var instance = value.Item1;
			instance.volume = volume;
		}
	}



	// Lifecycle

	void Start() {
		_ = MusicVolume;
		_ = SoundFXVolume;
	}

	void LateUpdate() {
		UpdateInstances();
	}
}
