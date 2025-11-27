Shader "Custom/SlimeGlow"
{
    Properties
    {
        _Color ("Main Color", Color) = (0.5, 1, 0.8, 1)
        _MainTex ("Base Texture", 2D) = "white" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0.9
        _Metallic ("Metallic", Range(0,1)) = 0.1
        
        [Header(Emission Glow)]
        _EmissionColor ("Emission Color", Color) = (0.5, 1, 0.8, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 2
        
        [Header(Fresnel Rim Light)]
        _FresnelColor ("Fresnel Color", Color) = (1, 1, 1, 1)
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 3
        _FresnelIntensity ("Fresnel Intensity", Range(0, 5)) = 1.5
        
        [Header(Subsurface Scattering)]
        _SSSColor ("SSS Color", Color) = (1, 0.9, 0.7, 1)
        _SSSPower ("SSS Power", Range(0.1, 10)) = 4
        _SSSIntensity ("SSS Intensity", Range(0, 3)) = 0.8
        
        [Header(Interior Detail)]
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 3
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.15
        _NoiseSpeed ("Noise Speed", Range(0, 2)) = 0.3
        
        [Header(Transparency)]
        _Alpha ("Alpha", Range(0, 1)) = 0.85
        _RimAlpha ("Rim Alpha Boost", Range(0, 0.3)) = 0.12
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0
        
        sampler2D _MainTex;
        
        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            float3 worldNormal;
            float3 worldPos;
        };
        
        half4 _Color;
        half _Smoothness;
        half _Metallic;
        half4 _EmissionColor;
        half _EmissionIntensity;
        half4 _FresnelColor;
        half _FresnelPower;
        half _FresnelIntensity;
        half4 _SSSColor;
        half _SSSPower;
        half _SSSIntensity;
        half _NoiseScale;
        half _NoiseIntensity;
        half _NoiseSpeed;
        half _Alpha;
        half _RimAlpha;
        
        // Simple 3D noise function for interior detail
        float noise3D(float3 p)
        {
            return frac(sin(dot(p, float3(12.9898, 78.233, 45.164))) * 43758.5453);
        }
        
        float smoothNoise(float3 p)
        {
            float3 i = floor(p);
            float3 f = frac(p);
            f = f * f * (3.0 - 2.0 * f); // Smoothstep
            
            float n = noise3D(i);
            return lerp(n, noise3D(i + float3(1, 1, 1)), f.x);
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // FORCE: Use _Color directly, ignore texture (texture defaults to white causing issues)
            fixed4 c = _Color;
            
            // Interior detail using 3D noise
            float3 noisePos = IN.worldPos * _NoiseScale + _Time.y * _NoiseSpeed;
            float noiseValue = smoothNoise(noisePos);
            c.rgb += noiseValue * _NoiseIntensity * _Color.rgb; // Modulate noise by color
            
            o.Albedo = c.rgb;
            
            // Metallic and smoothness for jelly-like appearance
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            
            // Fresnel rim lighting
            float3 normalizedViewDir = normalize(IN.viewDir);
            float fresnel = 1.0 - saturate(dot(normalizedViewDir, IN.worldNormal));
            fresnel = pow(fresnel, _FresnelPower);
            
            // Subsurface scattering approximation
            // Simulates light passing through the slime
            float3 lightDir = _WorldSpaceLightPos0.xyz;
            float backlight = saturate(dot(normalizedViewDir, -lightDir));
            float thickness = 1.0 - fresnel; // Thicker areas let less light through
            float sss = pow(backlight * thickness, _SSSPower) * _SSSIntensity;
            
            // Combine emission effects
            half3 emission = _EmissionColor.rgb * _EmissionIntensity;
            emission += _FresnelColor.rgb * fresnel * _FresnelIntensity;
            emission += _SSSColor.rgb * sss;
            
            o.Emission = emission;
            
            // Alpha with Fresnel boost for soft translucent edges
            o.Alpha = saturate(_Alpha + (fresnel * _RimAlpha));
        }
        ENDCG
    }
    
    FallBack "Standard"
}
