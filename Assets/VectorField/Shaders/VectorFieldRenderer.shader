Shader "VectorField/VectorFieldRenderer"
{

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	CGINCLUDE		
	#include "UnityCG.cginc"
	#include "Assets/CgIncludes/Util.cginc"
	#include "Assets/CgIncludes/Noise.cginc"


	struct v2g
	{
		float2 texcoord : TEXCOORD0;
		float3 pos : TEXCOORD1;
		float3 vel : TEXCOORD2;
		float3 normal : NORMAL;
	};

	struct g2f
	{
		float4 pos : SV_POSITION;
		float4 col : COLOR;
	};

	struct VectorField
	{
		float3 position;
		float3 velocity;
		float4 GridSize;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	StructuredBuffer<VectorField> VFBuffer;
			
	v2g vert (appdata_base v, uint vid : SV_VertexID)
	{
		v2g o;
		float3 pos = VFBuffer[vid].position;
		o.pos = pos;
		o.vel = VFBuffer[vid].velocity;
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.normal = v.normal;
		return o;
	}

	[maxvertexcount(2)]
	void geom(point v2g p[1], inout LineStream<g2f> lineStream)
	{
		g2f o;
		o.pos = mul(UNITY_MATRIX_VP, float4(p[0].pos, 1));
		o.col = float4(p[0].vel, 0);
		lineStream.Append(o);

		o.pos = mul(UNITY_MATRIX_VP, float4(p[0].pos + p[0].vel, 1));
		o.col = float4(p[0].vel, 1);
		lineStream.Append(o);
	}

	float4 frag (g2f i) : SV_Target
	{
		return i.col;
	}
	ENDCG

	SubShader
	{
		Tags { "RenderType"="Transparetn" "Queue" = "Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			ENDCG
		}
	}
	Fallback "Diffuse"
}
