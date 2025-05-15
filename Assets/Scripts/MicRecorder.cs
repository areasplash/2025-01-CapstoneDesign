using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Microphone Recorder
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI/Mic Recorder")]
[RequireComponent(typeof(Image), typeof(Button))]
public class MicRecorder : MonoBehaviour {

	// Constants

	const float AudioClipLength = 10f;
	const float ServerTimeout   =  4f;



	// Fields

	[Header("Resources")]
	[SerializeField] Sprite m_SpriteStart;
	[SerializeField] Sprite m_SpriteStop;
	[SerializeField] Sprite m_SpriteWait0;
	[SerializeField] Sprite m_SpriteWait1;
	[SerializeField] Sprite m_SpriteWait2;
	[SerializeField] Sprite m_SpriteWait3;

	Image  m_Image;
	Button m_Button;

	[Header("Components")]
	[SerializeField] MicSelector m_MicSelector;
	[SerializeField] TextMeshProUGUI m_Text;

	string    m_Mic;
	AudioClip m_Clip;



	// Properties

	Sprite SpriteStart => m_SpriteStart;
	Sprite SpriteStop  => m_SpriteStop;
	Sprite SpriteWait0 => m_SpriteWait0;
	Sprite SpriteWait1 => m_SpriteWait1;
	Sprite SpriteWait2 => m_SpriteWait2;
	Sprite SpriteWait3 => m_SpriteWait3;

	public Image  Image  => m_Image  || TryGetComponent(out m_Image ) ? m_Image  : null;
	public Button Button => m_Button || TryGetComponent(out m_Button) ? m_Button : null;
	public Sprite Sprite {
		get => Image.sprite;
		set => Image.sprite = value;
	}

	MicSelector MicSelector => m_MicSelector;
	string Text {
		get => m_Text.text;
		set => m_Text.text = value;
	}

	string Mic {
		get => m_Mic;
		set => m_Mic = value;
	}
	AudioClip Clip {
		get => m_Clip;
		set => m_Clip = value;
	}



	// Methods

	public void StartRecord() {
		if (Sprite != SpriteStart) return;
		Sprite = SpriteStop;
		Button.onClick.RemoveListener(StartRecord);
		Button.onClick.   AddListener( StopRecord);
		Mic = MicSelector.GetSelected();
		Clip = Microphone.Start(Mic, false, (int)AudioClipLength, 44100);
		if (!Clip) Text = $"Recording failed : {Mic} Unavailable";
		else _ = StopRecordAsync();
	}

	async Task StopRecordAsync() {
		float startpoint = Time.realtimeSinceStartup;
		while (Microphone.IsRecording(Mic)) {
			float elapsed = Time.realtimeSinceStartup - startpoint;
			Text = (elapsed % 3f) switch {
				< 1.0f => $"(Recording.)  ",
				< 2.0f => $"(Recording..) ",
				< 3.0f => $"(Recording...)",
				_ => $"(Recording)",
			};
			if ((int)(AudioClipLength - 0.1f) < elapsed) {
				StopRecord();
				break;
			}
			await Task.Yield();
		}
	}



	public void StopRecord() {
		if (Sprite != SpriteStop) return;
		Sprite = SpriteStart;
		Button.onClick.RemoveListener( StopRecord);
		Button.onClick.   AddListener(StartRecord);
		Microphone.End(Mic);
		_ = ConvertDataAsync();
	}

	async Task ConvertDataAsync() {
		Text = $"(Converting...)";
		Button.enabled = false;

		/* Transmit clip to server */

		float startpoint = Time.realtimeSinceStartup;
		while (true) {
			float elapsed = Time.realtimeSinceStartup - startpoint;
			Sprite = (elapsed % 1f) switch {
				< 0.1f => SpriteWait1,
				< 0.2f => SpriteWait2,
				< 0.3f => SpriteWait3,
				_ => SpriteWait0,
			};
			await Task.Yield();

			/* Receive text from server if arrived */

			if (ServerTimeout < elapsed) {
				Text = $"Conversion Failed : Server timeout";
				break;
			}
		}
		Sprite = SpriteStart;
		Button.enabled = true;
	}



	// Lifecycle

	void Start() {
		Sprite = SpriteStart;
		Button.onClick.AddListener(StartRecord);
		Text = $"";
	}
}
