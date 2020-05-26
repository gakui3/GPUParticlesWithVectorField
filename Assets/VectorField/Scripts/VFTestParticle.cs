using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class VFTestParticle : MonoBehaviour
{

	struct Particle
	{
		public Vector3 position;
		public Vector3 velocity;
		public Color color;
	}

	[SerializeField] ComputeShader updater;
	[SerializeField] Shader renderer;
	[SerializeField] VectorField3D vectorfield;

	[SerializeField] int particleCount;
	[SerializeField] Color color;

	[SerializeField] Texture2D particleTex;

	ComputeBuffer particlesBuffer;

	ComputeBuffer vfBuffer;
	Particle[] particlesArray;
	Material mat;
	string str;

	int kernel;
	int step;
	int stepno;
	int offset;
	int stage;
	int logsize;
	int max;
	int Count;

	void Start ()
	{
		BufferInit ();

		logsize = (int)Mathf.Log (particleCount, 2);
		max = logsize * (logsize + 1) / 2;

		Debug.Log (max - 1);

		BufferSort ();
	}

	void Update ()
	{
		updateParticles ();

		BufferSort ();
	}

	void OnRenderObject ()
	{
		if (mat == null) {
			mat = new Material (renderer);
		}
			
		//render particle
		mat.SetPass (0);
		mat.SetBuffer ("ParticlesBuffer", particlesBuffer);
		mat.SetTexture ("ParticleTexture", particleTex);
		Graphics.DrawProcedural (MeshTopology.Points, particleCount);
	}

	void BufferInit ()
	{
		particlesBuffer = new ComputeBuffer (particleCount, Marshal.SizeOf (typeof(Particle)));
		particlesArray = new Particle[particleCount];

		for (int i = 0; i < particleCount; i++) {
			Particle _particle = new Particle ();
			_particle.position = Random.insideUnitSphere * 50;
			_particle.velocity = Random.insideUnitSphere;
			_particle.color = color;
			particlesArray [i] = _particle;
		}
		particlesBuffer.SetData (particlesArray);
	}

//	float time = 0;
	void BufferSort(){

		var _cp = Camera.main.transform.position;
		var cp = new Vector4 (_cp.x, _cp.y, _cp.z, 1);

		float[] vMatrix = new float[16];
		var _v = Camera.main.worldToCameraMatrix;

		//set world2camera matrix to vMatrix
		vMatrix [0] = _v.m00;
		vMatrix [1] = _v.m01;
		vMatrix [2] = _v.m02;
		vMatrix [3] = _v.m03;
		vMatrix [4] = _v.m10;
		vMatrix [5] = _v.m11;
		vMatrix [6] = _v.m12;
		vMatrix [7] = _v.m13;
		vMatrix [8] = _v.m20;
		vMatrix [9] = _v.m21;
		vMatrix [10] = _v.m22;
		vMatrix [11] = _v.m23;
		vMatrix [12] = _v.m30;
		vMatrix [13] = _v.m31;
		vMatrix [14] = _v.m32;
		vMatrix [15] = _v.m33;

		kernel = updater.FindKernel ("BitonicSort");

//		time += Time.deltaTime;
//		if (time < 0.1f)
//			return;
		
		for (int i = 0; i < max; i++) {
			
			step = Count;
			int rank = 0;

			for (rank = 0; rank < step; rank++) {
				step -= rank + 1;
			}

			//2,4,8,16,32,64,128...
			stepno = 1 << (rank + 1);
//			Debug.Log ("stepno = " + stepno);

			//1, 2,1, 4,2,1, 8,4,2,1, 16,8...
			offset = 1 << (rank - step);
//			Debug.Log ("offset = " + offset);

			//2, 4,2, 8,4,2, 16,8,4,2, 32,16...
			stage = 2 * offset;
//			Debug.Log ("stage = " + stage);

			updater.SetBuffer (kernel, "ParticlesBuffer", particlesBuffer);
			updater.SetInt ("stepno", stepno);
			updater.SetInt ("offset", offset);
			updater.SetInt ("stage", stage);
			//updater.SetVector ("CameraPos", cp);
			//updater.SetFloats ("vMatrix", vMatrix);
			updater.Dispatch (kernel, (int)Mathf.Sqrt(particleCount) / 8 , (int)Mathf.Sqrt(particleCount) / 8, 1);

			if (Count < max) {
				Count += 1;
			}
		}

		Count = 0;
		step = 0;
		stepno = 0;
		offset = 0;
		stage = 0;
	}

	void updateParticles ()
	{
		vfBuffer = vectorfield.getVFBuffer ();
		var p = transform.position;

		//update
		kernel = updater.FindKernel ("Update");
		updater.SetBuffer (kernel, "ParticlesBuffer", particlesBuffer);
		updater.SetBuffer (kernel, "VFBuffer", vfBuffer);
		updater.SetFloat ("dTime", Time.deltaTime);
		updater.SetVector ("Pivot", new Vector4 (p.x, p.y, p.z, 1));
		updater.Dispatch (kernel, particleCount / 32, 1, 1);

//		str = " ";
//		particlesBuffer.GetData (particlesArray);
//		for (int i = 0; i < particlesArray.Length; i++) {
//			str += particlesArray [i].position + " ";
//		}
//		Debug.Log (str);
	}

	void OnDisable ()
	{
		particlesBuffer.Release ();
		vfBuffer.Release ();
	}
}