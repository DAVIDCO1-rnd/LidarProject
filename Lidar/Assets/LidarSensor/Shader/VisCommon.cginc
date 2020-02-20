sampler2D _MainTex;

float4 GetPixel(float2 texcoord) {
    return tex2D(_MainTex,texcoord);
}