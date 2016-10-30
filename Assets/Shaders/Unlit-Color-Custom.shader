// Unlit shader. Simplest possible colored shader.
// - no lighting
// - no lightmap support
// - no texture

Shader "Custom/Color" {
Properties {
	_OverDrawColor ("Main Color", Color) = (1,1,1,1)
}

SubShader {
	Tags { "RenderType"="Transparent" }
	LOD 100
	
	ZTest Always
	ZWrite Off
	Blend One One

	Pass {  
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				UNITY_FOG_COORDS(0)
			};

			fixed4 _OverDrawColor;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag(v2f i) : COLOR
			{
				fixed4 col = _OverDrawColor;
				//UNITY_APPLY_FOG(i.fogCoord, col);
				UNITY_OPAQUE_ALPHA(col.a);
				return col;
			}
		ENDCG
	}
}

}
