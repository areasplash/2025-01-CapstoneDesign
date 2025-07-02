using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Inventory Canvas
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[AddComponentMenu("UI/Inventory Canvas")]
public class InventoryCanvas : BaseCanvas {

	// Editor

	#if UNITY_EDITOR
	[CustomEditor(typeof(InventoryCanvas))]
	class InventoryCanvasEditor : EditorExtensions {
		InventoryCanvas I => target as InventoryCanvas;
		public override void OnInspectorGUI() {
			Begin("Inventory Canvas");

			LabelField("Template", EditorStyles.boldLabel);
			I.PrefabTemplate = ObjectField("Prefab Template", I.PrefabTemplate);
			I.InstanceSpace = FloatField("Instance Space", I.InstanceSpace);
			Space();

			End();
		}
	}
	#endif



	// Constants

	enum Drag : byte {
		None,
		Item,
		Camera,
	}



	// Fields

	List<GameObject> m_Prefabs;
	Dictionary<GameObject, GameObject> m_InstancePrefabs = new();
	[SerializeField] GameObject m_PrefabTemplate;
	[SerializeField] float m_InstanceSpace = 2f;

	Vector2 m_PointPosition;
	GameObject m_PointedPrefab;
	Drag m_DragMode;



	// Properties

	List<GameObject> Prefabs {
		get => m_Prefabs;
		set => m_Prefabs = value;
	}
	Dictionary<GameObject, GameObject> InstancePrefabs => m_InstancePrefabs;

	GameObject PrefabTemplate {
		get => m_PrefabTemplate;
		set => m_PrefabTemplate = value;
	}
	float InstanceSpace {
		get => m_InstanceSpace;
		set => m_InstanceSpace = value;
	}



	Vector2 PointPosition {
		get => m_PointPosition;
		set => m_PointPosition = value;
	}
	GameObject PointedPrefab {
		get => m_PointedPrefab;
		set => m_PointedPrefab = value;
	}
	bool IsPointingUI => EventSystem.current.IsPointerOverGameObject();

	Drag DragMode {
		get => m_DragMode;
		set => m_DragMode = value;
	}
	bool IsDraggingItem   => DragMode == Drag.Item;
	bool IsDraggingCamera => DragMode == Drag.Camera;



	// Lifecycle

	void OnEnable() {
		var contentRect = PrefabTemplate.transform.parent;
		var width = (PrefabTemplate.transform as RectTransform).sizeDelta.x;
		var space = InstanceSpace;
		Prefabs ??= Resources.LoadAll<GameObject>("").ToList();
		foreach (var prefab in Prefabs) {
			if (5 <= prefab.name.Length && prefab.name[..5].Equals("Debug")) continue;
			//if (GameManager.Inventory.ContainsKey(prefab.name)) continue;
			// Continue if the prefab name is not existing in the inventory.

			var instance = Instantiate(PrefabTemplate, contentRect);
			var position = new Vector2(space + (width + space) * InstancePrefabs.Count, 0f);
			(instance.transform as RectTransform).anchoredPosition = position;
			if (instance.TryGetComponent(out Image image)) {
				if (prefab.TryGetComponent(out SpriteRenderer spriteRenderer)) {
					image.sprite = spriteRenderer.sprite;
				}
			}
			instance.name = prefab.name;
			instance.SetActive(true);
			InstancePrefabs.Add(instance, prefab);
		}
	}

	void OnDisable() {
		foreach (var instance in InstancePrefabs.Keys) Destroy(instance);
		InstancePrefabs.Clear();
	}



	void LateUpdate() {
		if (InputManager.GetKeyDown(KeyAction.Click)) {
			PointPosition = InputManager.PointPosition;
			if (IsPointingUI) {
				var eventData = new PointerEventData(EventSystem.current) { position = PointPosition };
				var results = new List<RaycastResult>();
				EventSystem.current.RaycastAll(eventData, results);
				bool match = true;
				match &= 0 < results.Count;
				match &= InstancePrefabs.TryGetValue(results[0].gameObject, out var prefab);
				if (match) {
					PointedPrefab = prefab;
					DragMode = Drag.Item;
				}
			} else DragMode = Drag.Camera;
		}
		if (IsDraggingItem) {
			var position = CameraManager.MainCamera.ScreenToWorldPoint(InputManager.PointPosition);
			position.z = 0f;
			if (InputManager.GetKeyUp(KeyAction.Click) && !IsPointingUI) {
				Instantiate(PointedPrefab, position, Quaternion.identity);
			}
		}
		if (IsDraggingCamera) {
			if (InputManager.GetKey(KeyAction.Click) && PointPosition != InputManager.PointPosition) {
				var prev = CameraManager.MainCamera.ScreenToWorldPoint(PointPosition);
				var next = CameraManager.MainCamera.ScreenToWorldPoint(InputManager.PointPosition);
				var deltaPosition = next - prev;
				CameraManager.Position -= deltaPosition;
				PointPosition = InputManager.PointPosition;
			}
		}
		if (!InputManager.GetKey(KeyAction.Click)) DragMode = Drag.None;
	}
}
