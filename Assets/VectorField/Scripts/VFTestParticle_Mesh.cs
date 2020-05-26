using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Collections.Generic;

public class VFTestParticle_Mesh : MonoBehaviour
{
	[SerializeField] int particleCount;
	[SerializeField] Shader renderShader;
	[SerializeField] ComputeShader particleUpdater;
	[SerializeField] VectorField3D vectorfield;
	[SerializeField] Color col;
	[SerializeField] Texture2D particleTex;

	[SerializeField] Mesh[] particleMesh;
	[SerializeField] List<int> particleContaner;
	[SerializeField] Material[] mat;
	Color[] colors;

	ComputeBuffer[] particlesBuffer;
	ComputeBuffer vfBuffer;

	int kernel;

	struct Particle
	{
		public Vector3 position;
		public Vector3 velocity;
		public Color color;
	}

	// Use this for initialization
	void Start ()
	{
		particleContaner = new List<int> ();

		var v = Mathf.FloorToInt (particleCount / 64000f);
		if (v == 0) {
			particleContaner.Add (particleCount);
		} else {
			for (int i = 0; i < v; i++) {
				particleContaner.Add (64000);
			}
			particleContaner.Add (particleCount % 64000);
		}

		initParticleBuffer ();
		initMesh ();
	}

	// Update is called once per frame
	void Update ()
	{
		updateParticleBuffer ();
		particleRender ();
	}

	void particleRender ()
	{
		if (mat.Length == 0) {
			mat = new Material[particleContaner.Count];
			for (int i = 0; i < particleContaner.Count; i++) {
				mat [i] = new Material (renderShader);
			}
		}

		//draw meshでparticleを描画する場合、半透明は無理？
		//全部いっしょに描画されてしまう　一つのmeshで描画すると、vertexid順に描画される
		//つまり、結局深度でsort必要じゃん！
		for (int i = 0; i < particleContaner.Count; i++) {
			mat [i].SetPass (0);
			mat [i].SetBuffer ("ParticlesBuffer", particlesBuffer [i]);
			mat [i].SetTexture ("ParticleTexture", particleTex);
			Graphics.DrawMesh (particleMesh [i], Matrix4x4.identity, mat [i], 0);
		}
	}

	void initParticleBuffer ()
	{
		particlesBuffer = new ComputeBuffer[particleContaner.Count];

		for (int k = 0; k < particleContaner.Count; k++) {
			
			particlesBuffer [k] = new ComputeBuffer (particleContaner [k], Marshal.SizeOf (typeof(Particle)));
			Particle[] particles = new Particle[particleContaner [k]];

			for (int i = 0; i < particleContaner [k]; i++) {
				Particle _particle = new Particle ();
				var _pos = Random.insideUnitSphere * 10f;
				_particle.color = Color.white;
				_particle.position = _pos;
				_particle.velocity = Random.insideUnitSphere;
				particles [i] = _particle;
			}

			particlesBuffer [k].SetData (particles);
		}
	}

	void initMesh ()
	{
		particleMesh = new Mesh[particleContaner.Count];
		for (int k = 0; k < particleContaner.Count; k++) {
			int vertexCount = particleContaner [k];
			Vector3[] vertices = new Vector3[vertexCount];

			int[] indices = new int[vertexCount];

			for (int i = 0; i < vertexCount; i++) {
				vertices [i] = Random.insideUnitSphere;
				//			vertices [i] = Vector3.zero;
				indices [i] = i;
			}

			particleMesh [k] = new Mesh ();
			particleMesh [k].vertices = vertices;
			particleMesh [k].SetIndices (indices, MeshTopology.Points, 0);
			particleMesh [k].RecalculateBounds ();
		}
	}

	void updateParticleBuffer ()
	{
		vfBuffer = vectorfield.getVFBuffer ();
		var p = transform.position;

		for (int i = 0; i < particleContaner.Count; i++) {

			//update
			kernel = particleUpdater.FindKernel ("Update");
			particleUpdater.SetBuffer (kernel, "ParticlesBuffer", particlesBuffer[i]);
			particleUpdater.SetBuffer (kernel, "VFBuffer", vfBuffer);
			particleUpdater.SetFloat ("dTime", Time.deltaTime);
			particleUpdater.SetVector ("Pivot", new Vector4 (p.x, p.y, p.z, 1));
			particleUpdater.Dispatch (kernel, particleContaner [i] / 32 + 1, 1, 1);
		}
	}
		
	void OnDisable ()
	{
		for (int i = 0; i < particleContaner.Count; i++) {
			particlesBuffer [i].Release ();
		}
	}
}
