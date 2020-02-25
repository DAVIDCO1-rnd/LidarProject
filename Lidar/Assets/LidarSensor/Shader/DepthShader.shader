Shader "LidarSensor/Depth"
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
            #include "DepthCommon.cginc"

            struct v2f
            {
				float3 worldPos : TEXCOORD0;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }

			float visualizeDepth(float depthVal)
			{
				float visualizeDepthVal = depthVal * 255.0f;
				return visualizeDepthVal;
			}

            fixed4 frag (v2f i) : SV_Target
            {
				float distFromCamera = length(i.worldPos);
				float visualizeDepthVal = visualizeDepth(distFromCamera);
				return visualizeDepthVal;
            }
            ENDCG
        }
    }
}
