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
// Actor | Set Emotion
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Actor/Set Emotion")]
public class SetEmotionEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
	public class SetEmotionEventNode : BaseEventNode {
		SetEmotionEvent I => target as SetEmotionEvent;

		public SetEmotionEventNode() : base() {
			mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultNodeWidth;
		}

		public override void ConstructData() {
			var instance = new ObjectField() { value = I.instance };
			var emotion = new EnumField(Emotion.None) { value = I.emotion };
			instance.RegisterValueChangedCallback(evt => I.instance = evt.newValue as GameObject);
			emotion.RegisterValueChangedCallback(evt => I.emotion = (Emotion)evt.newValue);
			mainContainer.Add(instance);
			mainContainer.Add(emotion);
		}
	}
	#endif



	// Fields

	public GameObject instance;
	public Emotion emotion;



	// Methods

	public override void End() {
		if (instance && instance.TryGetComponent(out Actor actor)) actor.Emotion = emotion;
	}

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is SetEmotionEvent setEmotion) {
			instance = setEmotion.instance;
			emotion = setEmotion.emotion;
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Actor | Look At
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Actor/Look At")]
public class LookAtEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
	public class LookAtEventNode : BaseEventNode {
		LookAtEvent I => target as LookAtEvent;

		public LookAtEventNode() : base() {
			mainContainer.style.minWidth = mainContainer.style.maxWidth = DefaultNodeWidth;
		}

		public override void ConstructData() {
			var instance = new ObjectField() { value = I.instance };
			var target = new ObjectField() { value = I.target };
			instance.RegisterValueChangedCallback(evt => I.instance = evt.newValue as GameObject);
			target.RegisterValueChangedCallback(evt => I.target = evt.newValue as GameObject);
			mainContainer.Add(instance);
			mainContainer.Add(target);
		}
	}
	#endif



	// Fields

	public GameObject instance;
	public GameObject target;



	// Methods

	public override void End() {
		if (instance && instance.TryGetComponent(out Actor actor)) actor.LookAt(target);
	}

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is LookAtEvent lookAt) {
			instance = lookAt.instance;
			target = lookAt.target;
		}
	}
}



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Actor | Calculate Path
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[NodeMenu("Actor/Calculate Path")]
public class CalculatePathEvent : BaseEvent {

	// Node

	#if UNITY_EDITOR
	public class CalculatePathEventNode : BaseEventNode {
		CalculatePathEvent I => target as CalculatePathEvent;

		public CalculatePathEventNode() : base() {
			mainContainer.style.minWidth = mainContainer.style.maxWidth = ExtendedNodeWidth;
		}

		public override void ConstructData() {
			var instance = new ObjectField("Actor") { value = I.instance };
			var target = new ObjectField("Target") { value = I.target };
			var threshold = new FloatField("Threshold") { value = I.threshold };
			instance.RegisterValueChangedCallback(evt => I.instance = evt.newValue as GameObject);
			target.RegisterValueChangedCallback(evt => I.target = evt.newValue as GameObject);
			threshold.RegisterValueChangedCallback(evt => I.threshold = evt.newValue);
			mainContainer.Add(instance);
			mainContainer.Add(target);
			mainContainer.Add(threshold);
		}
	}
	#endif



	// Fields

	public GameObject instance;
	public GameObject target;
	public float threshold;



	// Methods

	public override void Start() {
		if (instance && instance.TryGetComponent(out Actor actor)) {
			actor.CalculatePath(target, threshold);
		}
	}

	public override bool Update() {
		if (instance && instance.TryGetComponent(out Actor actor)) {
			return actor.PathPoints.Count == 0;
		} else return true;
	}

	public override void CopyFrom(BaseEvent data) {
		base.CopyFrom(data);
		if (data is CalculatePathEvent calculatePath) {
			instance = calculatePath.instance;
			target = calculatePath.target;
			threshold = calculatePath.threshold;
		}
	}
}
