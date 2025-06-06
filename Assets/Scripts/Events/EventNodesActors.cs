using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.Linq;

#if UNITY_EDITOR
	using UnityEditor;
	using UnityEditor.UIElements;
	using UnityEditor.Experimental.GraphView;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Actor | Emotion
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Actor/Emotion")]
public class EmotionEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
		public class EmotionEventNode : BaseEventNode {
			EmotionEvent I => target as EmotionEvent;

			public EmotionEventNode() : base() {
				mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultSize.x;
			}

			public override void ConstructData() {
				var instance = new ObjectField()             { value = I.instance };
				var emotion  = new EnumField  (Emotion.None) { value = I.emotion  };
				instance.RegisterValueChangedCallback(evt => I.instance = evt.newValue as GameObject);
				emotion .RegisterValueChangedCallback(evt => I.emotion  = (Emotion)evt.newValue);
				mainContainer.Add(instance);
				mainContainer.Add(emotion);
			}
		}
	#endif



	// Fields

	public GameObject instance;
	public Emotion    emotion;



	// Methods

	public override void End() {
		if (instance && instance.TryGetComponent(out Actor actor)) actor.Emotion = emotion;
	}

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is EmotionEvent changeEmotion) {
			instance = changeEmotion.instance;
			emotion  = changeEmotion.emotion;
		}
	}
}
