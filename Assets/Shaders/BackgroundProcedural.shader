Shader "Procedural/BackgroundProcedural"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _TopColor ("Top Color", Color) = (0.6, 0.4, 0.9, 1)
        _BottomColor ("Bottom Color", Color) = (0.3, 0.5, 1, 1)
        _BubbleCount ("Bubble Count", Float) = 12
        _BubbleSpeed ("Bubble Speed", Float) = 0.3
        _BubbleGlow ("Bubble Glow", Range(0, 2)) = 0.8
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Background" 
            "RenderType"="Opaque" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
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
            
            float4 _TopColor;
            float4 _BottomColor;
            float _BubbleCount;
            float _BubbleSpeed;
            float _BubbleGlow;
            
            // Hash functions for pseudo-random
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }
            
            float hash13(float3 p3)
            {
                p3 = frac(p3 * 0.1031);
                p3 += dot(p3, p3.zyx + 31.32);
                return frac((p3.x + p3.y) * p3.z);
            }
            
            // SDF for circle
            float sdCircle(float2 p, float r)
            {
                return length(p) - r;
            }
            
            // Smooth minimum for soft blending
            float smin(float a, float b, float k)
            {
                float h = max(k - abs(a - b), 0.0) / k;
                return min(a, b) - h * h * k * 0.25;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 p = (uv - 0.5) * float2(aspect, 1.0) * 2.0;
                
                // Gradient background
                float gradientMix = smoothstep(0.0, 1.0, uv.y);
                float3 bgColor = lerp(_BottomColor.rgb, _TopColor.rgb, gradientMix);
                
                // Add subtle radial gradient
                float radialGrad = length(p) * 0.3;
                bgColor *= (1.0 - radialGrad * 0.2);
                
                // Floating bubbles
                float bubbleMask = 0.0;
                float bubbleGlow = 0.0;
                
                for (int j = 0; j < (int)_BubbleCount; j++)
                {
                    float seed = float(j) * 3.7854;
                    float2 offset = float2(hash(float2(seed, seed * 1.3)), hash(float2(seed * 2.1, seed)));
                    offset = offset * 2.0 - 1.0;
                    offset *= float2(aspect * 0.8, 0.8);
                    
                    // Animate position
                    float timeOffset = hash(float2(seed * 5.2, seed * 3.1)) * 6.28;
                    float t = _Time.y * _BubbleSpeed + timeOffset;
                    float2 motion = float2(sin(t * 0.5) * 0.1, cos(t * 0.7) * 0.15);
                    offset += motion;
                    
                    // Bubble size variation
                    float size = 0.08 + hash(float2(seed * 7.3, seed * 4.2)) * 0.15;
                    
                    float2 bubbleP = p - offset;
                    float dist = sdCircle(bubbleP, size);
                    
                    // Soft bubble with glow
                    float bubble = smoothstep(0.02, -0.005, dist);
                    bubbleMask += bubble;
                    
                    // Outer glow
                    float glow = exp(-dist * 8.0) * 0.3;
                    bubbleGlow += glow;
                    
                    // Inner highlight
                    float2 highlightOffset = float2(-0.02, 0.03) * size;
                    float highlight = smoothstep(0.015, 0.0, length(bubbleP - highlightOffset));
                    bubbleMask += highlight * 0.5;
                }
                
                // Composite bubbles
                float3 bubbleColor = _TopColor.rgb * 1.2;
                bgColor = lerp(bgColor, bubbleColor, saturate(bubbleMask) * 0.15);
                bgColor += bubbleGlow * _TopColor.rgb * _BubbleGlow;
                
                // Soft vignette
                float vignette = 1.0 - length(p) * 0.3;
                vignette = smoothstep(0.3, 1.0, vignette);
                bgColor *= vignette;
                
                return fixed4(bgColor, 1.0);
            }
            ENDCG
        }
    }
}
