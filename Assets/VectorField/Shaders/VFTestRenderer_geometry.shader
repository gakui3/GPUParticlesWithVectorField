Shader "VectorField/VFTestRenderer_geometry"
{

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	CGINCLUDE		
	#include "UnityCG.cginc"


	struct v2g
	{
		float2 texcoord : TEXCOORD0;
		float4 pos : POSITION;
		float3 normal : NORMAL;
		float4 color : TEXCOORD1;
	};

	struct g2f
	{
		float2 texcoord : TEXCOORD0;
		float4 pos : POSITION;
		float3 normal : NORMAL;
	};

	struct Particle
	{
		float3 position;
		float3 velocity;
		float4 color;
	};

	sampler2D _MainTex;
	float4 _MainTex_ST;
	StructuredBuffer<Particle> ParticlesBuffer;
	sampler2D ParticleTexture;
			
	v2g vert (appdata_base v, uint vid : SV_VertexID)
	{
		v2g o;
		float4 wpos = float4(ParticlesBuffer[vid].position, 1);
		o.pos = wpos;
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.normal = v.normal;
		o.color = ParticlesBuffer[vid].color;
		return o;
	}

	[maxvertexcount(4)]
	void geom(point v2g p[1], inout TriangleStream<g2f> triStream)
	{
		g2f o;
		float _Size = 10;
		float _MinSizeFactor = 0.001;
		float4 pos = float4(0, 0, 0, 0);

		float3 up = UNITY_MATRIX_IT_MV[1].xyz;
		float3 right = -UNITY_MATRIX_IT_MV[0].xyz;
		float dist = length(ObjSpaceViewDir(p[0].pos));

		float halfS = 0.5f * (_Size + (dist * _MinSizeFactor));

		pos = p[0].pos;
		pos = float4(pos + halfS * right - halfS * up, 1.0f);
		o.pos = mul(UNITY_MATRIX_VP, pos);
		o.texcoord = float2(1, 0);
		o.normal = p[0].normal;
		triStream.Append(o);

		pos = p[0].pos;
		pos = float4(pos + halfS * right + halfS * up, 1.0f);
		o.pos = mul(UNITY_MATRIX_VP, pos);
		o.texcoord = float2(1, 1);
		o.normal = p[0].normal;
		triStream.Append(o);

		pos = p[0].pos;
		pos = float4(pos - halfS * right - halfS * up, 1.0f);
		o.pos = mul(UNITY_MATRIX_VP, pos);
		o.texcoord = float2(0, 0);
		o.normal = p[0].normal;
		triStream.Append(o);

		pos = p[0].pos;
		pos = float4(pos - halfS * right + halfS * up, 1.0f);
		o.pos = mul(UNITY_MATRIX_VP, pos);
		o.texcoord = float2(0, 1);
		o.normal = p[0].normal;
		triStream.Append(o);
	}
			
	float4 frag (g2f i) : SV_Target
	{
		float4 c = tex2D(ParticleTexture, i.texcoord);
		return c;
	}
	ENDCG

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" }

		//sortするよりもzwrite offにしたほうがいいかも
//		zwrite off

		Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
		//Blend One OneMinusSrcAlpha // Premultiplied transparency
		//Blend One One // Additive
		//Blend OneMinusDstColor One // Soft Additive
		//Blend DstColor Zero // Multiplicative
		//Blend DstColor SrcColor // 2x Multiplicative

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			ENDCG
		}
	}
	Fallback "Diffuse"
}
