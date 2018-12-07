Shader "Hidden/Maxima"
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
			
			sampler2D_float _MainTex;
			float4 _MainTex_TexelSize;

			AppendStructuredBuffer<float3> lines;



			float frag (v2f i) : SV_Target
			{
				float pi = 3.1459;

				float theta = i.uv.x*pi;
				float dist = i.uv.y*2-1;

				float3x3 vals;

				float ln = tex2D(_MainTex, i.uv).r;
				float3 n = float3(_MainTex_TexelSize.xy,0);
				float notmax = ln<tex2D(_MainTex,i.uv+n.xz).r || ln<tex2D(_MainTex,i.uv+n.zy).r || ln<tex2D(_MainTex,i.uv-n.xz).r || ln<tex2D(_MainTex,i.uv-n.zy).r
								|| ln<tex2D(_MainTex,i.uv+n.xy).r || ln<tex2D(_MainTex,i.uv-n.xy).r || ln<tex2D(_MainTex,i.uv+n.xz-n.zy).r || ln<tex2D(_MainTex,i.uv-n.xz+n.zy).r;

				if(ln>4 && !notmax){
					lines.Append(float3(theta,dist,ln));
				}
				return ln;
			}
			ENDCG
		}
	}
}
