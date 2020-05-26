using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class VectorField3D : MonoBehaviour
{
	#region variable

	struct VectorField
	{
		public Vector3 position;
		public Vector3 velocity;
		public Vector4 GridSize;
	}

	[System.Serializable]
	public class VFMotion
	{
		
		public ComputeShader vf;
		[SerializeField, Range (0.01f, 2f)] public float scale = 1f;
		[SerializeField, Range (0.01f, 0.5f)] public float strength = 0.01f;
		public ComputeBuffer buffer;
		public int dispachCnt;

		public void CulcVF ()
		{
			vf.SetBuffer (0, "_VectorField", buffer);
			vf.SetFloat ("_dTime", Time.time);
			vf.SetFloat ("_Strength", strength);
			vf.SetFloat ("_Scale", scale);
			vf.Dispatch (0, dispachCnt / 8 + 1, 1, 1);
		}
	}

	[Header ("Shaders")]
	[SerializeField] Shader VectorFieldRenderer;
	[SerializeField] ComputeShader initVFBuffer;

	[Header ("VFs")]
	public List<VFMotion> vfMotions;

	[Header ("Values")]
	[SerializeField] Vector3 GridIndex;
	[SerializeField, Range (1, 10)] int SellSize = 1;

	[SerializeField] bool visible = true;

	ComputeBuffer vfBuffer;
	VectorField[] vfArray;
	Material vfRenderMat;
	Vector4 gridSize;
	int sumIndexCount;
	Vector3 tp;

	#endregion

	void Start ()
	{
		sumIndexCount = (int)(GridIndex.x * GridIndex.y * GridIndex.z);
		vfBuffer = new ComputeBuffer (sumIndexCount, Marshal.SizeOf (typeof(VectorField)));
		vfArray = new VectorField[sumIndexCount];
		gridSize = new Vector4 (GridIndex.x, GridIndex.y, GridIndex.z, SellSize);

		//3次元のindexを1次元に変換
		for (int i = 0; i < GridIndex.x; i++) {
			for (int k = 0; k < GridIndex.y; k++) {
				for (int m = 0; m < GridIndex.z; m++) {
//					var index = i + (k * (int)GridIndex.x) + (m * (int)GridIndex.y * (int)GridIndex.x);
					var index = (int)((m * GridIndex.x * GridIndex.y) + (k * GridIndex.x) + i);
					vfArray [index].position = new Vector3 ((float)i - ((GridIndex.x - 1) / 2), (float)k - ((GridIndex.y - 1) / 2), (float)m - ((GridIndex.z - 1) / 2)) * SellSize;
					vfArray [index].velocity = new Vector3 (0, 0, 0);
					vfArray [index].GridSize = gridSize;
				}
			}
		}
		vfBuffer.SetData (vfArray);
	}

	void Update ()
	{
		int dispachCnt = (int)(GridIndex.x * GridIndex.y * GridIndex.z);

		initVFBuffer.SetBuffer (0, "VFBuffer", vfBuffer);
		initVFBuffer.Dispatch (0, dispachCnt / 8 + 1, 1, 1);

		foreach (var v in vfMotions) {
			v.buffer = vfBuffer;
			v.dispachCnt = dispachCnt;
			v.CulcVF ();
		}
			
		DrawGrid ();
	}

	void OnRenderObject ()
	{
		if (!visible) {
			return;
		}

		if (vfRenderMat == null) {
			vfRenderMat = new Material (VectorFieldRenderer);
		}

		if (Camera.current.name == "Main Camera")
			return;

		vfRenderMat.SetPass (0);
		vfRenderMat.SetInt ("_VectorCount", sumIndexCount);
		vfRenderMat.SetBuffer ("VFBuffer", vfBuffer);
//		vfRenderMat.SetFloat ("_DebugLineScale", scale);
		Graphics.DrawProcedural (MeshTopology.Points, sumIndexCount);
	}

	void OnDisable ()
	{
		vfBuffer.Release ();
	}

	void DrawGrid ()
	{
		var color = Color.red;

		var x = gridSize.x * SellSize;
		var y = gridSize.y * SellSize;
		var z = gridSize.z * SellSize;

		var _x = -1 * x * 0.5f;
		var _y = -1 * y * 0.5f;
		var _z = -1 * z * 0.5f;

		x = x * 0.5f;
		y = y * 0.5f;
		z = z * 0.5f;

		Debug.DrawLine (new Vector3 (_x, _y, _z), new Vector3 (_x, y, _z), color);
		Debug.DrawLine (new Vector3 (x, _y, _z), new Vector3 (x, y, _z), color);
		Debug.DrawLine (new Vector3 (x, _y, z), new Vector3 (x, y, z), color);
		Debug.DrawLine (new Vector3 (_x, _y, z), new Vector3 (_x, y, z), color);

		Debug.DrawLine (new Vector3 (_x, _y, _z), new Vector3 (x, _y, _z), color);
		Debug.DrawLine (new Vector3 (_x, y, _z), new Vector3 (x, y, _z), color);
		Debug.DrawLine (new Vector3 (_x, _y, z), new Vector3 (x, _y, z), color);
		Debug.DrawLine (new Vector3 (_x, y, z), new Vector3 (x, y, z), color);

		Debug.DrawLine (new Vector3 (_x, _y, _z), new Vector3 (_x, _y, z), color);
		Debug.DrawLine (new Vector3 (x, _y, _z), new Vector3 (x, _y, z), color);
		Debug.DrawLine (new Vector3 (x, y, _z), new Vector3 (x, y, z), color);
		Debug.DrawLine (new Vector3 (_x, y, _z), new Vector3 (_x, y, z), color);
	}

	public ComputeBuffer getVFBuffer ()
	{
		return vfBuffer;
	}
}
