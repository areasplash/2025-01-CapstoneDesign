using UnityEngine;
using System.IO;

#if UNITY_EDITOR
	using UnityEditor;
#endif



// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Billboard Generator
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class BillboardGenerator : MonoBehaviour {

	// Editor

	#if UNITY_EDITOR
		[CustomEditor(typeof(BillboardGenerator))]
		class BillboardGeneratorSOEditor : EditorExtensions {
			BillboardGenerator I => target as BillboardGenerator;
			public override void OnInspectorGUI() {
				Begin("Billboard Generator");

				var status = PrefabUtility.GetPrefabInstanceStatus(I.gameObject);
				var disabled = status == PrefabInstanceStatus.Connected;
				if (disabled) HelpBox("This script is attached to a prefab instance.");
				BeginDisabledGroup(disabled);
				LabelField("Data", EditorStyles.boldLabel);
				I.Size   = Vector2Field("Size",   I.Size);
				I.Normal = Slider      ("Normal", I.Normal, -90f, 90f);
				I.Pivot  = EnumField   ("Pivot",  I.Pivot);
				BeginHorizontal();
				PrefixLabel("Generate Billboard");
				if (Button("Generate")) I.GenerateBillboard(I.MeshFilter);
				EndHorizontal();
				Space();
				EndDisabledGroup();

				End();
			}
		}
	#endif



	// Constants

	enum MeshPivot : byte {
		Center,
		Bottom,
	}



	// Fields

	[SerializeField] Vector2   m_Size   = new(1f, 1f);
	[SerializeField] float     m_Normal = 30f;
	[SerializeField] MeshPivot m_Pivot  = MeshPivot.Center;

	MeshFilter m_MeshFilter;



	// Properties

	Vector2 Size {
		get => m_Size;
		set => m_Size = value;
	}
	float Normal {
		get => m_Normal;
		set => m_Normal = value;
	}
	MeshPivot Pivot {
		get => m_Pivot;
		set => m_Pivot = value;
	}

	MeshFilter MeshFilter => m_MeshFilter || TryGetComponent(out m_MeshFilter) ? m_MeshFilter : null;



	// Methods

	#if UNITY_EDITOR
		public void GenerateBillboard(MeshFilter meshFilter) {
			var halfWidth  = Size.x * 0.5f;
			var halfHeight = Size.y * 0.5f;
			var x = 0f;
			var y = Pivot == MeshPivot.Bottom ? halfHeight : 0f;
			var radian = Mathf.Deg2Rad * Normal;
			var normal = new Vector3(0f, Mathf.Sin(radian), -Mathf.Cos(radian));

			var mesh = new Mesh() {
				name = "Billboard",
				vertices = new Vector3[] {
					new(x - halfWidth, y - halfHeight, 0f), new(x + halfWidth, y - halfHeight, 0f),
					new(x - halfWidth, y + halfHeight, 0f), new(x + halfWidth, y + halfHeight, 0f),
				},
				uv = new Vector2[] {
					new(0f, 0f), new(1f, 0f),
					new(0f, 1f), new(1f, 1f),
				},
				normals = new Vector3[] {
					normal, normal,
					normal, normal,
				},
				triangles = new int[] {
					0, 3, 1,
					3, 0, 2,
				},
			};
			mesh.RecalculateTangents();

			var parent = transform;
			while (parent.parent != null) parent = parent.parent;
			var path = $"Assets/Materials/Meshes/Billboard_{parent.name}.asset";
			Directory.CreateDirectory("Assets/Materials/Meshes");
			AssetDatabase.CreateAsset(mesh, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			meshFilter.sharedMesh = mesh;
		}
	#endif
}
