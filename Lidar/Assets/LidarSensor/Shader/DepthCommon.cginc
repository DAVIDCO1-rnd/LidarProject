#define MAX_DEPTH_16BIT (65536.0/1000.0)

sampler2D _CameraDepthTexture;

float GetDepth16(float2 texcoord) {
    float depth = tex2D(_CameraDepthTexture, texcoord);
    return LinearEyeDepth(depth)/MAX_DEPTH_16BIT;
}

float GetDepth32(float2 texcoord) {
    float depth = tex2D(_CameraDepthTexture, texcoord);
    return LinearEyeDepth(depth);
}