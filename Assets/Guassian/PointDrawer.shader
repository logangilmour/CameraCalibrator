Shader "Unlit/PointDrawer"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
		

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float col : TEXCOORD0;
			};

			StructuredBuffer<float3> _Points;
			
			v2f vert (uint id : SV_VertexID, uint instanceId : SV_InstanceId)
			{
				v2f o;

				o.vertex = float4(_Points[instanceId].xy+(float2(id,id)*2-1)*0.01,0,1);
				o.col=_Points[instanceId].z;
				//o.vertex = float4(ln,ln,0,1);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture

				return fixed4(i.col%9/9.0+i.col<0,i.col/9/9.0+i.col<0,i.col<0,1);
			}
			ENDCG
		}
	}
}
