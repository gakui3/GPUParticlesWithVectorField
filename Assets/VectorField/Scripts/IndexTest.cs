using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndexTest : MonoBehaviour
{
	[SerializeField] Vector4 GridSize;
	
	// Update is called once per frame
	void Update ()
	{
		var p = transform.position;
		p = new Vector3 (Mathf.Floor (p.x), Mathf.Floor (p.y), Mathf.Floor (p.z));

		var index = ((p.z * GridSize.x * GridSize.y) + (p.y * GridSize.x) + p.x) / GridSize.w;
		Debug.Log (index);

		Debug.Log (Mathf.Floor (-0.5f));
	}
}
