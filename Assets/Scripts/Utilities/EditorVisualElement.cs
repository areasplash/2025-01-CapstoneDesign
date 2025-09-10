using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using System;

#if UNITY_EDITOR
using UnityEditor.UIElements;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Editor Visual Element
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

#if UNITY_EDITOR
public static class EditorVisualElement {

	// Constants

	public const float Node1U = 128f;
	public const float Node2U = 224f;
	public const float Node2ULabel = 56f;



	// Layout Methods

	public static Label Label(string text) {
		var label = new Label(text);
		label.style.marginLeft = 3f;
		label.style.marginRight = -1f;
		label.style.marginTop = 2f;
		label.style.width = Node2ULabel;
		return label;
	}

	public static Button Button(string text, Action onClick) {
		var button = new Button(onClick) { text = text };
		button.style.marginTop = button.style.marginBottom = 1f;
		button.style.marginLeft = button.style.marginRight = 3f;
		button.style.height = 18f;
		return button;
	}



	// Field Methods

	public static IntegerField IntField(
		int value, Action<int> onChanged) {
		var intField = new IntegerField() { value = value };
		intField.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		intField.style.width = Node1U - 8f;
		return intField;
	}
	public static IntegerField IntField(
		string label, int value, Action<int> onChanged) {
		var intField = new IntegerField(label) { value = value };
		intField.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		intField.style.width = Node2U - 8f;
		intField.labelElement.style.minWidth = Node2ULabel;
		intField.labelElement.style.maxWidth = Node2ULabel;
		return intField;
	}

	public static Toggle Toggle(
		bool value, Action<bool> onChanged) {
		var toggle = new Toggle() { value = value };
		toggle.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		return toggle;
	}
	public static Toggle Toggle(
		string label, bool value, Action<bool> onChanged) {
		var toggle = new Toggle(label) { value = value };
		toggle.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		return toggle;
	}

	public static EnumField EnumField<T>(
		T value, Action<T> onChanged) where T : Enum {
		var enumField = new EnumField(default(T)) { value = value };
		enumField.RegisterValueChangedCallback(callback => onChanged((T)callback.newValue));
		enumField.style.width = Node1U - 8f;
		return enumField;
	}
	public static EnumField EnumField<T>(
		string label, T value, Action<T> onChanged) where T : Enum {
		var enumField = new EnumField(label, default(T)) { value = value };
		enumField.RegisterValueChangedCallback(callback => onChanged((T)callback.newValue));
		enumField.style.width = Node2U - 8f;
		enumField.labelElement.style.minWidth = Node2ULabel;
		enumField.labelElement.style.maxWidth = Node2ULabel;
		return enumField;
	}
	public static VisualElement TextEnumField<T>(
		T value, Action<T> onChanged) where T : Enum {
		var root = new VisualElement();
		root.style.flexDirection = FlexDirection.Row;
		var child0 = new TextField() { value = value.ToString() };
		var child1 = new EnumField(default(T)) { value = value };
		child0.RegisterValueChangedCallback(callback => {
			var name = callback.newValue;
			if (Enum.TryParse(typeof(T), name, out var t)) {
				child1.value = (T)t;
				onChanged((T)t);
			}
		});
		child1.RegisterValueChangedCallback(callback => {
			child0.value = callback.newValue.ToString();
			onChanged((T)callback.newValue);
		});
		child0.style.minWidth = child0.style.maxWidth = (Node1U - 14f) * 0.5f;
		child1.style.minWidth = child1.style.maxWidth = (Node1U - 14f) * 0.5f;
		root.Add(child0);
		root.Add(child1);
		return root;
	}
	public static VisualElement TextEnumField<T>(
		string label, T value, Action<T> onChanged) where T : Enum {
		var root = new VisualElement();
		root.style.flexDirection = FlexDirection.Row;
		var child0 = Label(label);
		root.Add(child0);
		var child1 = new TextField() { value = value.ToString() };
		var child2 = new EnumField(default(T)) { value = value };
		child1.RegisterValueChangedCallback(callback => {
			var name = callback.newValue;
			if (Enum.TryParse(typeof(T), name, out var t)) {
				child2.value = (T)t;
				onChanged((T)t);
			}
		});
		child2.RegisterValueChangedCallback(callback => {
			child1.value = callback.newValue.ToString();
			onChanged((T)callback.newValue);
		});
		child1.style.minWidth = child1.style.maxWidth = (Node2U - Node2ULabel - 16f) * 0.5f;
		child2.style.minWidth = child2.style.maxWidth = (Node2U - Node2ULabel - 16f) * 0.5f;
		root.Add(child1);
		root.Add(child2);
		return root;
	}

	public static FloatField FloatField(
		float value, Action<float> onChanged) {
		var floatField = new FloatField() { value = value };
		floatField.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		floatField.style.width = Node1U - 8f;
		return floatField;
	}
	public static FloatField FloatField(
		string label, float value, Action<float> onChanged) {
		var floatField = new FloatField(label) { value = value };
		floatField.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		floatField.style.width = Node2U - 8f;
		floatField.labelElement.style.minWidth = Node2ULabel;
		floatField.labelElement.style.maxWidth = Node2ULabel;
		return floatField;
	}
	public static VisualElement Slider(
		float value, float min, float max, Action<float> onChanged) {
		var root = new VisualElement();
		root.style.flexDirection = FlexDirection.Row;
		var child0 = new Slider(min, max) { value = value };
		var child1 = new FloatField() { value = value };
		child0.RegisterValueChangedCallback(callback => child1.value = callback.newValue);
		child1.RegisterValueChangedCallback(callback => child0.value = callback.newValue);
		child0.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		child1.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		child0.style.minWidth = child0.style.maxWidth = Node1U - 14f - 35f;
		child1.style.minWidth = child1.style.maxWidth = 35f;
		root.Add(child0);
		root.Add(child1);
		return root;
	}
	public static VisualElement Slider(
		string label, float value, float min, float max, Action<float> onChanged) {
		var root = new VisualElement();
		root.style.flexDirection = FlexDirection.Row;
		var child0 = Label(label);
		root.Add(child0);
		var child1 = new Slider(min, max) { value = value };
		var child2 = new FloatField() { value = value };
		child1.RegisterValueChangedCallback(callback => child2.value = callback.newValue);
		child2.RegisterValueChangedCallback(callback => child1.value = callback.newValue);
		child1.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		child2.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		child1.style.minWidth = child1.style.maxWidth = Node2U - Node2ULabel - 16f - 35f;
		child2.style.minWidth = child2.style.maxWidth = 35f;
		root.Add(child1);
		root.Add(child2);
		return root;
	}

	public static Vector3Field Vector3Field(
		Vector3 value, Action<Vector3> onChanged) {
		var vector3Field = new Vector3Field() { value = value };
		vector3Field.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		vector3Field.style.width = Node1U - 8f;
		return vector3Field;
	}
	public static Vector3Field Vector3Field(
		string label, Vector3 value, Action<Vector3> onChanged) {
		var vector3Field = new Vector3Field(label) { value = value };
		vector3Field.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		vector3Field.style.width = Node2U - 8f;
		vector3Field.labelElement.style.minWidth = Node2ULabel;
		vector3Field.labelElement.style.maxWidth = Node2ULabel;
		vector3Field.ElementAt(1).ElementAt(0).style.width = 0f;
		vector3Field.ElementAt(1).ElementAt(1).style.width = 0f;
		vector3Field.ElementAt(1).ElementAt(2).style.width = 0f;
		return vector3Field;
	}

	public static ColorField ColorField(
		Color value, Action<Color> onChanged) {
		var colorField = new ColorField() { value = value };
		colorField.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		colorField.style.width = Node1U - 8f;
		return colorField;
	}
	public static ColorField ColorField(
		string label, Color value, Action<Color> onChanged) {
		var colorField = new ColorField(label) { value = value };
		colorField.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		colorField.style.width = Node2U - 8f;
		colorField.labelElement.style.minWidth = Node2ULabel;
		colorField.labelElement.style.maxWidth = Node2ULabel;
		return colorField;
	}

	public static TextField TextField(
		string value, Action<string> onChanged) {
		var textField = new TextField() { value = value };
		textField.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		textField.style.width = Node1U - 8f;
		return textField;
	}
	public static TextField TextField(
		string label, string value, Action<string> onChanged) {
		var textField = new TextField(label) { value = value };
		textField.RegisterValueChangedCallback(callback => onChanged(callback.newValue));
		textField.style.width = Node2U - 8f;
		textField.labelElement.style.minWidth = Node2ULabel;
		textField.labelElement.style.maxWidth = Node2ULabel;
		return textField;
	}

	public static ObjectField ObjectField<T>(
		T value, Action<T> onChanged) where T : Object {
		var objectField = new ObjectField() { value = value };
		objectField.RegisterValueChangedCallback(callback => onChanged(callback.newValue as T));
		objectField.style.width = Node1U - 8f;
		return objectField;
	}
	public static ObjectField ObjectField<T>(
		string label, T value, Action<T> onChanged) where T : Object {
		var objectField = new ObjectField(label) { value = value };
		objectField.RegisterValueChangedCallback(callback => onChanged(callback.newValue as T));
		objectField.style.width = Node2U - 8f;
		objectField.labelElement.style.minWidth = Node2ULabel;
		objectField.labelElement.style.maxWidth = Node2ULabel;
		return objectField;
	}
}
#endif
