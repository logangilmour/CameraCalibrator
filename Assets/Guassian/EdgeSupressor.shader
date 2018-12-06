Shader "Hidden/EdgeSupressor"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				//return col.z;
				float2 off = col.xy*_MainTex_TexelSize.xy;

				fixed4 pos = tex2D(_MainTex,i.uv+off);
				fixed4 neg = tex2D(_MainTex,i.uv-off);
				if(pos.z>col.z || neg.z>col.z){
					col.z=0;
				}

				return col.z-0.2>0 && i.uv.x>_MainTex_TexelSize.x && i.uv.y>_MainTex_TexelSize.y && i.uv.x<1-_MainTex_TexelSize.x && i.uv.y<1-_MainTex_TexelSize.y;
			}
			ENDCG
		}
	}
}
