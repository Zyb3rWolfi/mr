Shader "Custom/ToonCartoonShader"
{
    Properties
    {
        // The main image texture mapping (Section 4)
        _MainTex ("Character/Object Texture", 2D) = "white" {}
        
        // Custom control parameters for the anime style
        _LightPos ("Light Position", Vector) = (0, 10, 0, 1)
        _AmbientColor ("Shadow / Ambient Color", Color) = (0.2, 0.2, 0.4, 1.0)
        _BrightColorTint ("Bright Light Tint", Color) = (1.0, 1.0, 1.0, 1.0)
        
        // This threshold controls where the bright side cuts off into the dark shadow side
        _ToonThreshold ("Toon Lighting Threshold", Range(0, 1)) = 0.3
    }
    SubShader
    {
        // Configured for immediate URP framework compatibility (Section 1)
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;  
                float2 uv : TEXCOORD0;     
                float3 normal : NORMAL;     
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;       
                float2 uv : TEXCOORD0;             
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : EXTRA_TEXCOORD;
            };

            // Main constant buffer structure keeping our asset SRP Batcher optimized (Section 1)
            CBUFFER_START(UnityPerMaterial)
                float4 _LightPos;       
                float4 _AmbientColor;
                float4 _BrightColorTint;
                float _ToonThreshold;
                float4 _MainTex_ST;     
            CBUFFER_END

            sampler2D _MainTex;

            v2f vert (appdata v)
            {
                v2f o;
                
                // Project vertices into viewable clip matrix space (Section 1)
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                
                // Apply our structural UV scaling trick to destroy noise artifacts (Section 4)
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw; 

                // Pass the world normal and vertex position down to compute pixel lighting (Section 5)
                o.worldNormal = TransformObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o; 
            }

            half4 frag (v2f i) : SV_Target
            {
                // 1. Sample our base texture pixels (Section 4)
                float4 texColor = tex2D(_MainTex, i.uv);

                // 2. Normalize spatial vectors (Section 5)
                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_LightPos.xyz - i.worldPos);

                // 3. Calculate standard diffuse light intensity (Lambertian) (Section 5)
                float diffuseIntensity = dot(normal, lightDir);

                // 4. THE TOON SHADING TRICK:
                // Instead of letting diffuseIntensity slide smoothly from -1 to 1,
                // we check if it is above our custom slider threshold.
                // If it is, we snap it to 1.0 (fully bright). If it's below, we snap it to 0.0 (shadow).
                float toonLightStep = step(_ToonThreshold, diffuseIntensity);

                // 5. Interpolate our colors based on our sharp step
                // This blends cleanly between our dark shadow accent and bright highlight tint
                float4 lightingColor = lerp(_AmbientColor, _BrightColorTint, toonLightStep);

                // 6. Combine our final graphic texture with the sharp cell shaded lighting layers
                return texColor * lightingColor;
            }
            ENDHLSL
        }
    }
}