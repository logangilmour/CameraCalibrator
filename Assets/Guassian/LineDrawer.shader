Shader "Unlit/LineDrawer"
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
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
		

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float mag : TEXCOORD0;
			};

			RWStructuredBuffer<float3> lines : register(u1); 
			
			v2f vert (uint id : SV_VertexID, uint instanceId : SV_InstanceId)
			{
				v2f o;
				float theta = lines[instanceId].x;
				float dist = lines[instanceId].y;
				float2 norm = float2(cos(theta),sin(theta));
				float2 dir = float2(norm.y,-norm.x);
				float2 pnt = norm*dist;
				float ln = id*2.0-1;
				o.vertex = float4(pnt+ln*dir,0,1);
				o.mag = lines[instanceId].z;

				//o.vertex = float4(ln,ln,0,1);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture

				return fixed4(1,1,1,1)*i.mag/10;
			}
			ENDCG
		}
	}
}
