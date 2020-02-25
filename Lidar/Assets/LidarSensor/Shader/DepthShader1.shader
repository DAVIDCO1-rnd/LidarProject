Shader "LidarSensor/Depth1"
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
                float2 texcoord : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata_base v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

			float visualizeDepth(float depthVal)
			{
				float visualizeDepthVal = depthVal / 32.0f;
				return visualizeDepthVal;
			}

            fixed4 frag (v2f i) : SV_Target
            {
                float depthVal = GetDepth32(i.texcoord);
				float visualizeDepthVal = visualizeDepth(depthVal);
				return visualizeDepthVal;
            }
            ENDCG
        }
    }
}
