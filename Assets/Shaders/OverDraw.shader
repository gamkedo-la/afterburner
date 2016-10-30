Shader "Custom/Over Draw"
{
	//Properties
	//{
	//	_OverDrawColor("Over Draw Color", Color) = (1,1,1,1)
	//}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
		}
		//Tags{ "RenderType" = "Transparent" }

		ZTest Always
		ZWrite Off
		Blend One One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}

			half4 _OverDrawColor;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = _OverDrawColor;
				UNITY_OPAQUE_ALPHA(col.a);
				return col;
			}
			ENDCG
		}
	}
}
