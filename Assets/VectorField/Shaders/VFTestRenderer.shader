Shader "VectorField/VFTestRenderer"
{

	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	CGINCLUDE		
	#include "UnityCG.cginc"


	struct v2f
	{
		float2 texcoord : TEXCOORD0;
		float4 pos : POSITION;
		float3 normal : NORMAL;
		float4 color : TEXCOOD1;
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
			
	v2f vert (appdata_base v, uint vid : SV_VertexID)
	{
		v2f o;
		float4 wpos = float4(ParticlesBuffer[vid].position, 1);
		o.pos = mul(UNITY_MATRIX_VP, wpos);
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.normal = v.normal;
		o.color = ParticlesBuffer[vid].color;
		return o;
	}
			
	float4 frag (v2f i) : SV_Target
	{
		float4 c = float4(1, 1, 1, 0.5);
		return c;
	}
	ENDCG

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		LOD 100
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
			#pragma fragment frag
			ENDCG
		}
	}
	Fallback "Diffuse"
}
