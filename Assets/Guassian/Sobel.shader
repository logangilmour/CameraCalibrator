Shader "Hidden/Sobel"
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
				float2 poff = float2(_MainTex_TexelSize.x,0);
				float2 soff = float2(0,_MainTex_TexelSize.y);
				float horizontal = tex2D(_MainTex, i.uv-poff+soff).r+tex2D(_MainTex,i.uv-poff).r*2+tex2D(_MainTex,i.uv-poff-soff).r
									-tex2D(_MainTex,i.uv+poff+soff).r-tex2D(_MainTex,i.uv+poff).r*2-tex2D(_MainTex,i.uv+poff-soff).r;


				poff = float2(0,_MainTex_TexelSize.y);
				soff = float2(_MainTex_TexelSize.x,0);
				float vertical = tex2D(_MainTex, i.uv-poff+soff).r+tex2D(_MainTex,i.uv-poff).r*2+tex2D(_MainTex,i.uv-poff-soff).r
									-tex2D(_MainTex,i.uv+poff+soff).r-tex2D(_MainTex,i.uv+poff).r*2-tex2D(_MainTex,i.uv+poff-soff).r;

				float strength = sqrt(pow(horizontal,2)+pow(vertical,2));
				float2 dir = normalize(float2((horizontal+1)/2,(vertical+1)/2));
				return half4(dir,strength,0);
			}
			ENDCG
		}
	}
}
