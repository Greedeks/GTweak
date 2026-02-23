#define PI 3.141592653f
#define value 1.0f

float4 main(float2 uv : TEXCOORD) : COLOR {
    uv = 2 * uv - 1;
    float saturation = length(uv);
    float hue = 3 * (PI - atan2(uv.y, -uv.x)) / PI;
    float chroma = value * saturation;
    float second = chroma * (1 - abs(hue % 2.0 - 1));
    float m = value - chroma;
    float3 rgb;
    if (hue < 1)
        rgb = float3(chroma, second, 0);
    else if (hue < 2)
        rgb = float3(second, chroma, 0);
    else if (hue < 3)
        rgb = float3(0, chroma, second);
    else if (hue < 4)
        rgb = float3(0, second, chroma);
    else if (hue < 5)
        rgb = float3(second, 0, chroma);
    else
        rgb = float3(chroma, 0, second);
    return float4(rgb + m, saturation < 1);
}