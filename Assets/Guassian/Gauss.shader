Shader "Hidden/Gauss"
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
			float4  _MainTex_TexelSize;
			float2 _Direction;
			int _KernelSize;
			sampler2D _Kernel;
			fixed4 frag (v2f iv) : SV_Target
			{

				
				fixed4 col=fixed4(0,0,0,0);
				 for(int i=0; i<_KernelSize; i++){
				 	float lookupUV = (i+0.5)/_KernelSize;
				 	col+= tex2D(_MainTex, iv.uv+_MainTex_TexelSize.xy*_Direction*(lookupUV*2-1))*tex2D(_Kernel,float2(lookupUV,0.5)).r;
				 } 


				return col;
			}
			ENDCG
		}
	}
}
