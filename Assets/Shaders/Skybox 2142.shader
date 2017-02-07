Shader "Custom/Sky2142" {
	Properties
	{
		_AlphaCutoff("Alpha Cutoff", Range(-1, 1)) = 0.5
		_Mag("Magnitude", Float) = 1
		[HideInInspector] _Altitude("Altitude", Float) = 0 //input scaled aircraft altitude to drop horizon as it ascends

		_FrontTexture("Front Texture", 2D) = "white"
		_BackTexture("Back Texture", 2D) = "black"
		_AlphaMask("Alpha Mask", 2D) = "white"
	}

		SubShader
	{
		Tags{ "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
		Cull Off ZWrite Off

		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
				//float2 texcoord : TEXCOORD0;
				//float2 texoffset : TEXCOORD1;
			};

			uniform float4 _FrontTexture_ST;

			static const float PI = 3.1415927;
			static const float TWO_PI_RECIP = 1.0 / (2 * 3.1415927);
			sampler2D _FrontTexture;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				/*
				if (v.uv.x < 0)
				{
					v.uv.x = 1 - v.uv.x;
				}
				*/
				//o.uv = TRANSFORM_TEX(v, _MainTex);
				//v.texcoord.x = (atan2(v.texcoord.x, v.texcoord.z) + 3.14159) / 6.28318 + _FrontTexture_ST.z;
				//o.texcoord = v.texcoord.xy;
				//o.texoffset.x = v.texcoord.x * _FrontTexture_ST.x + _FrontTexture_ST.z;
				o.texcoord = v.texcoord;

				o.texcoord.y = v.texcoord.y * _FrontTexture_ST.y + _FrontTexture_ST.w;
				//o.uv = v.uv *_FrontTexture_ST.xy + _FrontTexture_ST.zw;
				return o;
			}

			sampler2D _BackTexture;
			sampler2D _AlphaMask;
			float _AlphaCutoff;
			float _Mag;
			float _Altitude;

			float4 frag(v2f i) : SV_Target
			{
				i.texcoord.x = (atan2(i.texcoord.x, i.texcoord.z) + PI) * TWO_PI_RECIP + _FrontTexture_ST.z;
				
				float alpha = tex2D(_AlphaMask, i.texcoord).a;
				float2 normalMap = tex2D(_AlphaMask, i.texcoord).xy;
				normalMap = (normalMap * 2 - 1) * _Mag;

				float4 color = tex2D(_FrontTexture, i.texcoord + normalMap);
				float4 backColor = tex2D(_BackTexture, i.texcoord);

				//color.a = clamp((color.a - _AlphaCutoff), 0, 1);
				color.a = clamp((alpha - _AlphaCutoff), 0, 1);
				color = color * color.a + backColor * (1 - color.a);
				float fade = clamp((i.texcoord.y + _Altitude -  _FrontTexture_ST.w) / _FrontTexture_ST.y, 0, 1);
				color = color * fade;
				
//				fade = clamp(((-i.texcoord.y + 1) - _FrontTexture_ST.w) / _FrontTexture_ST.y, 0, 1);
	//			color = color * fade;

				/*if (i.texcoord.x < 0.5)
				{
					//i.uv.x = 1 - i.uv.x;
				//	color = float4(0, 0, 0, 0);
				}*/
				/*
				if (i.uv.y < -0.5)
				{
					color = float4(0, 0, 0, 0);
				}
				*/
				return color;
			}
			ENDCG
		}
	}
}