Shader "Hidden/Hough"
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
			float4 _Dest_TexelSize;

			float frag (v2f i) : SV_Target
			{
						float pi = 3.1459;

				fixed4 col = tex2D(_MainTex, i.uv);
				float ln = 0;
				float theta = i.uv.x*pi;
				float2 norm = float2(cos(theta),sin(theta)); // r = x cos 0 + y sin 0 // r - y sin 0 = x cos 0 // (r - y sin 0)/cos 0 = x
				float2 dir = float2(norm.y,-norm.x);
				float dist = i.uv.y*2-1;
				float2 pnt = norm*dist;

				float2 start = pnt-dir;
				for(int i=0; i<_MainTex_TexelSize.z; i++){
					float2 uv = (start+dir*2*i*_Dest_TexelSize.x+1)/2;
					ln+=tex2D(_MainTex,uv).r*(4.0/256.0);
				}
				return ln;
			}
			ENDCG
		}
	}
}
