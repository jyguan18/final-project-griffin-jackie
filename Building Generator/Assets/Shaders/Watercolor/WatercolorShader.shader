Shader "Hidden/WatercolorShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        // Add properties for your paper texture, edge color, etc.
        _PaperTex ("Paper Texture", 2D) = "white" {}
    }
    SubShader
    {
        // Tag it as a post-processing effect
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100

        // PASS 0: Edge Detection/Base Color
        Pass
        {
            Name "EdgeDetectionPass"
            ZTest Always Cull Off ZWrite Off
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            sampler2D _MainTex;
            // URP provides access to depth/normals via these names
            sampler2D _CameraDepthTexture; 
            sampler2D _CameraNormalsTexture;
            sampler2D _PaperTex;

            v2f vert (appdata v)
            {
                v2f o;
                // Standard blit vertex shader
                o.vertex = float4(v.vertex.xy * 2 - 1, 0, 1);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                // **THIS IS WHERE YOU IMPLEMENT THE BLOG LOGIC**
                // 1. Get Depth/Normals for edge detection
                // float depth = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, i.uv, 0);
                // float3 normal = SAMPLE_TEXTURE2D(_CameraNormalsTexture, sampler_CameraNormalsTexture, i.uv).xyz;

                // 2. Implement Sobel operator using neighboring samples
                // 3. Apply color bleed logic
                
                // Placeholder code:
                float4 color = tex2D(_MainTex, i.uv);
                float4 paperColor = tex2D(_PaperTex, i.uv * 10); // Tile the paper texture

                // Simple multiplication blend as a starting point
                return color * paperColor * 1.5; 
            }
            ENDHLSL
        }
        // You would add more passes (PASS 1 for color bleed, PASS 2 for texture, etc.)
    }
}