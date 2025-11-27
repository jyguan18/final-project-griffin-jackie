Shader "Custom/TerrainShader"
{
    Properties
    {
        _TerrainGradient ("Terrain Gradient", 2D) = "white" {}
        _MinTerrainHeight ("Min Height", Float) = 0
        _MaxTerrainHeight ("Max Height", Float) = 40
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        sampler2D _TerrainGradient;
        float _MinTerrainHeight;
        float _MaxTerrainHeight;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 worldPosY = IN.worldPos.y;

            float heightValue = saturate((worldPosY - _MinTerrainHeight) / (_MaxTerrainHeight - _MinTerrainHeight));

            o.Albedo = tex2D(_TerrainGradient, float2(0, heightValue));
        }
        ENDCG
    }
    FallBack "Diffuse"
}