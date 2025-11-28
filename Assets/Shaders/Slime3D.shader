Shader "Procedural/Slime3D"
{
    Properties
    {
        _CoreColor ("Core Color", Color) = (0.4, 0.95, 1.0, 1)
        _EdgeColor ("Edge Color", Color) = (0.25, 0.7, 0.95, 1)
        _RimColor ("Rim Light Color", Color) = (0.6, 1.0, 1.0, 1)
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.5
        _Translucency ("Translucency", Range(0, 1)) = 0.6
        _Shininess ("Shininess", Range(0, 1)) = 0.85
        _SpecularPower ("Specular Power", Range(1, 128)) = 32
        
        // Animation controls
        _SquishAmount ("Squish Amount", Range(0, 1)) = 0
        _BounceOffset ("Bounce Offset", Range(-1, 1)) = 0
        
        // Eye properties
        _EyeColor ("Eye Color", Color) = (0.15, 0.25, 0.6, 1)
        _PupilColor ("Pupil Color", Color) = (0.05, 0.1, 0.3, 1)
        _EyeShine ("Eye Shine", Color) = (1, 1, 1, 1)
        _BlinkAmount ("Blink Amount", Range(0, 1)) = 0
        
        // Mouth properties
        _MouthColor ("Mouth Color", Color) = (0.2, 0.35, 0.65, 1)
        _MouthCurve ("Mouth Curve", Range(0, 2)) = 1.0
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
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
            
            float4 _CoreColor;
            float4 _EdgeColor;
            float4 _RimColor;
            float _RimPower;
            float _Translucency;
            float _Shininess;
            float _SpecularPower;
            
            float _SquishAmount;
            float _BounceOffset;
            
            float4 _EyeColor;
            float4 _PupilColor;
            float4 _EyeShine;
            float _BlinkAmount;
            
            float4 _MouthColor;
            float _MouthCurve;
            
            // SDF primitives
            float sdSphere(float3 p, float r)
            {
                return length(p) - r;
            }
            
            float sdEllipsoid(float3 p, float3 r)
            {
                float k0 = length(p / r);
                float k1 = length(p / (r * r));
                return k0 * (k0 - 1.0) / k1;
            }
            
            float sdCircle(float2 p, float r)
            {
                return length(p) - r;
            }
            
            float sdArc(float2 p, float2 sc, float ra, float rb)
            {
                p.x = abs(p.x);
                return ((sc.y * p.x > sc.x * p.y) ? length(p - sc * ra) : abs(length(p) - ra)) - rb;
            }
            
            float opSmoothUnion(float d1, float d2, float k)
            {
                float h = saturate(0.5 + 0.5 * (d2 - d1) / k);
                return lerp(d2, d1, h) - k * h * (1.0 - h);
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
                
                // Apply animation deformations
                float squish = lerp(1.0, 0.7, _SquishAmount);
                float stretch = lerp(1.0, 1.3, _SquishAmount);
                p.y = (p.y - _BounceOffset) / squish;
                p.x = p.x / stretch;
                
                // === VOLUMETRIC 3D SLIME BODY ===
                // Simulate 3D sphere with depth using 2D distance and fake Z-depth
                float2 bodyCenter = float2(0, -0.05);
                float2 toCenter = p - bodyCenter;
                float distToCenter = length(toCenter);
                
                // Create spherical body with soft edges
                float bodyRadius = 0.85;
                float bodyDist = distToCenter - bodyRadius;
                
                // Calculate fake Z-depth for 3D volume effect
                float zDepth = sqrt(max(0.0, bodyRadius * bodyRadius - distToCenter * distToCenter));
                float normalizedDepth = zDepth / bodyRadius; // 0 at edges, 1 at center
                
                // Slime body mask with soft edges for volume
                float bodyMask = smoothstep(0.05, -0.05, bodyDist);
                
                // === 3D LIGHTING CALCULATION ===
                // Simulate surface normal based on sphere
                float3 normal = normalize(float3(toCenter.x, toCenter.y, zDepth));
                
                // Key light from top-left
                float3 lightDir = normalize(float3(-0.5, 0.8, 1.0));
                float NdotL = max(0.0, dot(normal, lightDir));
                
                // Rim light (edges catch light)
                float rimFactor = pow(1.0 - normalizedDepth, _RimPower);
                rimFactor *= smoothstep(0.0, 0.3, bodyMask);
                
                // Subsurface scattering (light through translucent body)
                float subsurface = pow(normalizedDepth, 0.6) * _Translucency;
                
                // Base color gradient - SOFTER and BRIGHTER for cuteness
                float3 bodyColor = lerp(_EdgeColor.rgb, _CoreColor.rgb, normalizedDepth * 0.8 + 0.2);
                
                // Apply lighting - brighter overall for adorable look
                bodyColor *= (0.75 + NdotL * 0.5); // Softer diffuse
                bodyColor += subsurface * _CoreColor.rgb * 0.5; // More glow
                bodyColor += _RimColor.rgb * rimFactor * 1.3; // Stronger rim
                
                // === SPECULAR HIGHLIGHTS (glossy cute look) ===
                float3 viewDir = float3(0, 0, 1);
                float3 halfDir = normalize(lightDir + viewDir);
                float spec = pow(max(0.0, dot(normal, halfDir)), _SpecularPower);
                spec *= normalizedDepth * _Shininess;
                bodyColor += spec * float3(1, 1, 1) * 2.0; // Brighter specular
                
                // Large soft highlight - positioned for cuteness
                float2 highlightPos = p - float2(-0.2, 0.4);
                float highlightDist = length(highlightPos);
                float highlight = smoothstep(0.55, 0.15, highlightDist) * normalizedDepth;
                bodyColor += highlight * float3(0.95, 1.0, 1.0) * 1.2;
                
                // === SOFT SHADOW AT BASE (ambient occlusion) ===
                float aoFactor = smoothstep(-0.7, -0.3, p.y) * smoothstep(0.8, 0.3, distToCenter);
                float3 shadowColor = _EdgeColor.rgb * 0.5;
                
                // === ADORABLE BIG EYES (kawaii style) ===
                float eyeSpacing = 0.28;
                float eyeY = 0.15;
                float eyeRadius = 0.22; // BIGGER for cuteness
                
                // Left eye
                float2 leftEyePos = p - float2(-eyeSpacing, eyeY);
                float leftEyeDist = length(leftEyePos);
                float leftEyeDepth = sqrt(max(0.0, eyeRadius * eyeRadius - leftEyeDist * leftEyeDist));
                float leftEyeNormDepth = leftEyeDepth / eyeRadius;
                
                float eyeScale = lerp(1.0, 0.15, _BlinkAmount);
                float leftEyeMask = smoothstep(0.02, -0.02, leftEyeDist - eyeRadius * eyeScale);
                
                // Right eye
                float2 rightEyePos = p - float2(eyeSpacing, eyeY);
                float rightEyeDist = length(rightEyePos);
                float rightEyeDepth = sqrt(max(0.0, eyeRadius * eyeRadius - rightEyeDist * rightEyeDist));
                float rightEyeNormDepth = rightEyeDepth / eyeRadius;
                float rightEyeMask = smoothstep(0.02, -0.02, rightEyeDist - eyeRadius * eyeScale);
                
                float eyeMask = max(leftEyeMask, rightEyeMask);
                
                // Eye color - bright and sparkly
                float eyeDepth = max(leftEyeNormDepth * leftEyeMask, rightEyeNormDepth * rightEyeMask);
                float3 eyeColor = lerp(_EyeColor.rgb * 0.7, _EyeColor.rgb * 1.2, eyeDepth);
                
                // Pupils - larger and rounder for cuteness
                float pupilRadius = 0.13;
                float leftPupilMask = smoothstep(0.01, -0.01, length(leftEyePos - float2(-0.02, -0.015)) - pupilRadius);
                float rightPupilMask = smoothstep(0.01, -0.01, length(rightEyePos - float2(-0.02, -0.015)) - pupilRadius);
                float pupilMask = max(leftPupilMask, rightPupilMask);
                
                // BIG sparkly shines (anime style)
                float leftShineMask = smoothstep(0.015, -0.015, length(leftEyePos - float2(-0.06, 0.06)) - 0.06);
                float rightShineMask = smoothstep(0.015, -0.015, length(rightEyePos - float2(-0.06, 0.06)) - 0.06);
                
                // Add secondary smaller shine
                float leftShine2Mask = smoothstep(0.01, -0.01, length(leftEyePos - float2(0.04, -0.03)) - 0.025);
                float rightShine2Mask = smoothstep(0.01, -0.01, length(rightEyePos - float2(0.04, -0.03)) - 0.025);
                
                float shineMask = max(max(leftShineMask, rightShineMask), max(leftShine2Mask, rightShine2Mask));
                
                // === CUTE SMILE (kawaii mouth) - FIXED POSITION ===
                float2 mouthPos = p - float2(0, -0.15); // Moved UP significantly
                float mouthAngle = _MouthCurve * 0.75;
                float2 mouthSC = float2(sin(mouthAngle), cos(mouthAngle));
                float mouthDist = sdArc(mouthPos, mouthSC, 0.22, 0.05); // Smaller and thinner
                float mouthMask = smoothstep(0.02, -0.02, mouthDist);
                
                // Ensure mouth stays below eyes
                float eyeSeparation = step(-0.05, p.y); // Only draw mouth below Y=-0.05
                mouthMask *= eyeSeparation;
                
                // Mouth color - softer and rounder
                float mouthDepth = smoothstep(0.05, -0.02, mouthDist);
                float3 mouthColor = lerp(_MouthColor.rgb * 1.1, _MouthColor.rgb * 0.6, mouthDepth * 0.5);
                
                // === GROUND SHADOW ===
                float2 shadowPos = p - float2(0, -0.92);
                float shadowDist = length(shadowPos / float2(1.2, 0.15));
                float shadow = smoothstep(1.0, 0.3, shadowDist);
                shadow = pow(shadow, 2.5) * 0.35;
                
                // === FINAL COMPOSITE ===
                float3 finalColor = float3(0, 0, 0);
                float finalAlpha = 0.0;
                
                // Shadow
                if (shadow > 0.01)
                {
                    finalColor = lerp(finalColor, shadowColor, shadow);
                    finalAlpha = max(finalAlpha, shadow * 0.4);
                }
                
                // Body with AO
                if (bodyMask > 0.01)
                {
                    float3 shadedBody = lerp(shadowColor, bodyColor, aoFactor);
                    finalColor = lerp(finalColor, shadedBody, bodyMask);
                    finalAlpha = max(finalAlpha, bodyMask * 0.96);
                }
                
                // Eyes
                if (eyeMask > 0.01)
                {
                    finalColor = lerp(finalColor, eyeColor, eyeMask);
                    finalAlpha = max(finalAlpha, eyeMask);
                }
                
                // Pupils
                if (pupilMask > 0.01)
                {
                    finalColor = lerp(finalColor, _PupilColor.rgb, pupilMask);
                }
                
                // Eye shine
                if (shineMask > 0.01)
                {
                    finalColor = lerp(finalColor, _EyeShine.rgb, shineMask);
                }
                
                // Mouth
                if (mouthMask > 0.01)
                {
                    finalColor = lerp(finalColor, mouthColor, mouthMask);
                }
                
                return fixed4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
}
