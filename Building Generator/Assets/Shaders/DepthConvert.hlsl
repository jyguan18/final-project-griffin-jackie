// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
void DepthConvert_float(float InDepth, float near, float far, out float OutDepth)
{
    // OutDepth = DECODE_EYEDEPTH(InDepth);

    //x is (1-far/near), y is (far/near), z is (x/far) and w is (y/far).

    #if UNITY_REVERSED_Z != 1
    float x = -1.0 + far/near;
    float y = 1.0;
    #else
    float x = 1.0 - far/near;
    float y = far/near;
    #endif
    float z = x/far;
    float w = y/far;
    OutDepth = 1.0 / (z * InDepth + w);
    // if (InDepth > 0.99) {
    //     OutDepth = 1.0;
    // } else {
    //     OutDepth = 0.0;
    // }
    // OutDepth = InDepth;
}
