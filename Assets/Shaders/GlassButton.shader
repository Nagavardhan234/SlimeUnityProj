Shader "Procedural/GlassButton"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlassColor ("Glass Color", Color) = (0.7, 0.6, 0.9, 0.3)
        _BorderColor ("Border Color", Color) = (0.8, 0.7, 1.0, 0.8)
        _InnerGlow ("Inner Glow", Range(0, 2)) = 0.5
        _OuterGlow ("Outer Glow", Range(0, 2)) = 0.3
        _IconColor ("Icon Color", Color) = (1, 1, 1, 1)
        _IconType ("Icon Type", Float) = 0  // 0=Apple, 1=Z, 2=Hand
        _BorderRadius ("Border Radius", Range(0, 0.5)) = 0.25
        _BorderWidth ("Border Width", Range(0, 0.1)) = 0.02
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
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
            
            float4 _GlassColor;
            float4 _BorderColor;
            float _InnerGlow;
            float _OuterGlow;
            float4 _IconColor;
            float _IconType;
            float _BorderRadius;
            float _BorderWidth;
            
            // SDF primitives
            float sdCircle(float2 p, float r)
            {
                return length(p) - r;
            }
            
            float sdRoundedBox(float2 p, float2 b, float r)
            {
                float2 q = abs(p) - b + r;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
            }
            
            float sdSegment(float2 p, float2 a, float2 b)
            {
                float2 pa = p - a;
                float2 ba = b - a;
                float h = saturate(dot(pa, ba) / dot(ba, ba));
                return length(pa - ba * h);
            }
            
            float sdBox(float2 p, float2 b)
            {
                float2 d = abs(p) - b;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }
            
            float opUnion(float d1, float d2)
            {
                return min(d1, d2);
            }
            
            float opSubtraction(float d1, float d2)
            {
                return max(-d1, d2);
            }
            
            float opIntersection(float d1, float d2)
            {
                return max(d1, d2);
            }
            
            // Apple icon SDF
            float sdApple(float2 p)
            {
                // Scale and adjust
                p *= 2.2;
                p.y -= 0.1;
                
                // Apple body - two circles
                float2 p1 = p - float2(-0.15, 0.0);
                float2 p2 = p - float2(0.15, 0.0);
                float body = min(sdCircle(p1, 0.55), sdCircle(p2, 0.55));
                
                // Top indent
                float2 topP = p - float2(0, -0.45);
                float indent = sdCircle(topP, 0.2);
                body = max(body, -indent);
                
                // Leaf on top
                float2 leafP = p - float2(0.05, -0.65);
                leafP = float2(leafP.x * 0.5, leafP.y);
                float leaf = sdCircle(leafP, 0.18);
                
                // Stem
                float2 stemP = p - float2(0, -0.55);
                float stem = sdBox(stemP, float2(0.05, 0.15));
                
                float icon = min(body, min(leaf, stem));
                return icon;
            }
            
            // Letter Z icon SDF
            float sdLetterZ(float2 p)
            {
                p *= 2.5;
                float thickness = 0.18;
                
                // Top horizontal
                float2 topP = p - float2(0, -0.5);
                float top = sdBox(topP, float2(0.5, thickness));
                
                // Diagonal
                float2 diagP = p;
                float2 a = float2(-0.4, -0.35);
                float2 b = float2(0.4, 0.35);
                float diag = sdSegment(diagP, a, b) - thickness;
                
                // Bottom horizontal
                float2 botP = p - float2(0, 0.5);
                float bot = sdBox(botP, float2(0.5, thickness));
                
                float icon = min(top, min(diag, bot));
                return icon;
            }
            
            // Hand icon SDF
            float sdHand(float2 p)
            {
                p *= 2.0;
                p.y -= 0.1;
                
                float hand = 1000.0;
                
                // Palm
                float2 palmP = p - float2(0, 0.25);
                float palm = sdRoundedBox(palmP, float2(0.35, 0.3), 0.1);
                hand = palm;
                
                // Thumb
                float2 thumbP = p - float2(-0.4, 0.1);
                float thumb = sdRoundedBox(thumbP, float2(0.12, 0.25), 0.1);
                hand = min(hand, thumb);
                
                // Fingers
                for (int i = 0; i < 4; i++)
                {
                    float xOffset = -0.25 + float(i) * 0.17;
                    float yOffset = -0.15 - float(i % 2) * 0.08;
                    float2 fingerP = p - float2(xOffset, yOffset);
                    float finger = sdRoundedBox(fingerP, float2(0.09, 0.35), 0.08);
                    hand = min(hand, finger);
                }
                
                return hand;
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
                float2 p = (uv - 0.5) * 2.0;
                
                // Rounded rectangle button
                float buttonDist = sdRoundedBox(p, float2(0.85, 0.85), _BorderRadius);
                
                // Button mask
                float buttonMask = smoothstep(0.02, -0.02, buttonDist);
                
                // Border
                float borderDist = abs(buttonDist + _BorderWidth * 0.5) - _BorderWidth * 0.5;
                float border = smoothstep(0.01, -0.01, borderDist) * buttonMask;
                
                // Inner glow
                float innerGlow = exp(-max(buttonDist, 0.0) * 8.0) * _InnerGlow;
                innerGlow *= buttonMask;
                
                // Outer glow
                float outerGlow = exp(-max(-buttonDist, 0.0) * 4.0) * _OuterGlow;
                
                // Glass color with gradient
                float gradientMix = smoothstep(-1.0, 1.0, p.y);
                float3 glassColor = lerp(_GlassColor.rgb * 1.1, _GlassColor.rgb * 0.9, gradientMix);
                
                // Icon
                float iconDist = 1000.0;
                if (_IconType < 0.5)
                {
                    iconDist = sdApple(p);
                }
                else if (_IconType < 1.5)
                {
                    iconDist = sdLetterZ(p);
                }
                else
                {
                    iconDist = sdHand(p);
                }
                
                float iconMask = smoothstep(0.02, -0.02, iconDist);
                iconMask *= buttonMask;
                
                // Icon glow
                float iconGlow = exp(-max(iconDist, 0.0) * 12.0) * 0.3;
                iconGlow *= buttonMask;
                
                // Composite
                float3 finalColor = float3(0, 0, 0);
                float finalAlpha = 0.0;
                
                // Outer glow
                finalColor += _BorderColor.rgb * outerGlow;
                finalAlpha += outerGlow * 0.4;
                
                // Button glass
                if (buttonMask > 0.01)
                {
                    finalColor = lerp(finalColor, glassColor, buttonMask * _GlassColor.a);
                    finalAlpha = max(finalAlpha, buttonMask * _GlassColor.a);
                }
                
                // Inner glow
                finalColor += _GlassColor.rgb * innerGlow * 0.5;
                
                // Border
                if (border > 0.01)
                {
                    finalColor = lerp(finalColor, _BorderColor.rgb, border * _BorderColor.a);
                    finalAlpha = max(finalAlpha, border * _BorderColor.a);
                }
                
                // Icon
                if (iconMask > 0.01)
                {
                    finalColor = lerp(finalColor, _IconColor.rgb, iconMask * _IconColor.a);
                    finalAlpha = max(finalAlpha, iconMask);
                }
                
                // Icon glow
                finalColor += _IconColor.rgb * iconGlow;
                
                // Subtle frosted effect
                float noise = frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
                finalColor += (noise - 0.5) * 0.02 * buttonMask;
                
                return fixed4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
}
