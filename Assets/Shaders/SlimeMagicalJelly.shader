Shader "Procedural/SlimeMagicalJelly"
{
    Properties
    {
        // MAGICAL SLIME - Vibrant purple-pink gradient (max visual dopamine)
        _CoreColor ("Core Color", Color) = (0.616, 0.482, 1.0, 0.65)     // #9D7BFF - TRANSPARENT purple
        _EdgeColor ("Edge Color", Color) = (0.416, 0.22, 1.0, 0.72)      // #6A38FF - Semi-transparent glow
        _RimColor ("Rim Light Color", Color) = (0.9, 0.65, 1.0, 1)       // Strong purple-pink rim
        _InnerGlowColor ("Inner Glow Color", Color) = (1.0, 0.75, 1.0, 1) // Bright pink-white inner light
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 2.2                   // Stronger rim
        _FresnelPower ("Fresnel Power", Range(0.5, 5.0)) = 1.6           // Glass-like
        _Translucency ("Translucency", Range(0, 1)) = 0.8                // Transparent jelly
        _Shininess ("Shininess", Range(0, 1)) = 0.96                     // Very shiny jelly
        _SpecularPower ("Specular Power", Range(1, 128)) = 65            // Sharp highlights
        _InnerGlowStrength ("Inner Glow Strength", Range(0, 3)) = 3.0    // STRONG magical glow
        
        // CUTENESS FEATURES
        _BlushColor ("Blush Cheeks Color", Color) = (1.0, 0.4, 0.65, 0.75) // Bright pink blush
        _BlushSize ("Blush Size", Range(0, 0.3)) = 0.14
        _HighlightIntensity ("Surface Highlight", Range(0, 2)) = 1.4     // Glossy shine
        
        // Shape variation - gentle baby-like movement
        _WobbleAmount ("Wobble Amount", Range(0, 0.3)) = 0.018
        _WobbleSpeed ("Wobble Speed", Range(0, 5)) = 2.0
        
        // Animation controls
        _SquishAmount ("Squish Amount", Range(0, 1)) = 0
        _BounceOffset ("Bounce Offset", Range(-1, 1)) = 0
        _BreathingPulse ("Breathing Pulse", Range(0.95, 1.05)) = 1.0     // Scale pulse for alive feeling
        _BottomSquish ("Bottom Squish", Range(0, 0.5)) = 0.15
        
        // Eye properties - LARGER for baby schema
        _EyeColor ("Eye Color", Color) = (0.08, 0.15, 0.35, 1)
        _PupilColor ("Pupil Color", Color) = (0.01, 0.03, 0.15, 1)
        _EyeShine ("Eye Shine", Color) = (1, 1, 1, 1)
        _BlinkAmount ("Blink Amount", Range(0, 1)) = 0
        _EyeEmotiveness ("Eye Emotiveness", Range(0.5, 1.5)) = 1.0
        
        // NEW: Eye Expression Controls
        _EyeOffsetX ("Eye Gaze X", Range(-0.3, 0.3)) = 0
        _EyeOffsetY ("Eye Gaze Y", Range(-0.3, 0.3)) = 0
        _EyeRotation ("Eye Rotation", Range(-30, 30)) = 0
        _EyeSquintAmount ("Eye Squint", Range(0, 1)) = 0
        _PupilScale ("Pupil Scale", Range(0.3, 1.5)) = 1.0
        
        // NEW: Eyebrow Controls
        _EyebrowHeight ("Eyebrow Height", Range(-0.15, 0.15)) = 0
        _EyebrowAngle ("Eyebrow Angle", Range(-30, 30)) = 0
        _EyebrowVisible ("Eyebrow Visible", Range(0, 1)) = 0
        
        // NEW: Mouth Controls
        _MouthVisible ("Mouth Visible", Range(0, 1)) = 0
        _MouthCurve ("Mouth Curve", Range(-1, 1)) = 0
        
        // NEW: Tears and Effects
        _TearAmount ("Tear Amount", Range(0, 1)) = 0
        _BlushPulseSpeed ("Blush Pulse Speed", Range(0, 5)) = 0
        
        // NEW: Body Deformation
        _TopSquish ("Top Squish", Range(0, 0.5)) = 0
        _AsymmetryAmount ("Asymmetry", Range(0, 0.5)) = 0
        _LeanAngle ("Lean Angle", Range(-20, 20)) = 0
        
        // NEW: Effects
        _ColorShift ("Color Shift Hue", Range(-180, 180)) = 0
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.15
        
        // Magical Sparkle Particles
        _ParticleCount ("Particle Count", Range(0, 20)) = 12            // More sparkles
        _ParticleSpeed ("Particle Speed", Range(0, 2)) = 0.6
        _ParticleGlow ("Particle Glow", Range(0, 3)) = 2.0              // Brighter sparkles
        _ParticleTwinkle ("Particle Twinkle", Range(0, 2)) = 1.5        // Flash animation
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
            float4 _InnerGlowColor;
            float _RimPower;
            float _FresnelPower;
            float _Translucency;
            float _Shininess;
            float _SpecularPower;
            float _InnerGlowStrength;
            
            // NEW: Eye expression uniforms
            float _EyeOffsetX;
            float _EyeOffsetY;
            float _EyeRotation;
            float _EyeSquintAmount;
            float _PupilScale;
            float _EyebrowHeight;
            float _EyebrowAngle;
            float _EyebrowVisible;
            float _MouthVisible;
            float _MouthCurve;
            float _TearAmount;
            float _BlushPulseSpeed;
            
            // NEW: Body deformation uniforms
            float _TopSquish;
            float _AsymmetryAmount;
            float _LeanAngle;
            
            // NEW: Effect uniforms
            float _ColorShift;
            float _ShadowIntensity;
            
            float _WobbleAmount;
            float _WobbleSpeed;
            float _BottomSquish;
            
            float _SquishAmount;
            float _BounceOffset;
            
            float4 _EyeColor;
            float4 _PupilColor;
            float4 _EyeShine;
            float _BlinkAmount;
            float _EyeEmotiveness;
            
            float4 _BlushColor;
            float _BlushSize;
            float _HighlightIntensity;
            float _BreathingPulse;
            
            float _ParticleCount;
            float _ParticleSpeed;
            float _ParticleGlow;
            float _ParticleTwinkle;
            
            // Hash function for pseudo-random
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }
            
            float sdCircle(float2 p, float r)
            {
                return length(p) - r;
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
                
                // Apply breathing pulse (alive feeling)
                p /= _BreathingPulse;
                
                float time = _Time.y;
                
                // === IRREGULAR SHAPE (organic wobble) ===
                float wobbleAngle = atan2(p.y, p.x);
                float wobble1 = sin(wobbleAngle * 3.0 + time * _WobbleSpeed) * _WobbleAmount;
                float wobble2 = sin(wobbleAngle * 5.0 - time * _WobbleSpeed * 0.7) * _WobbleAmount * 0.5;
                float totalWobble = wobble1 + wobble2;
                
                // Apply animation deformations (multiply to compress/expand)
                float squish = lerp(1.0, 0.7, _SquishAmount);
                float stretch = lerp(1.0, 1.3, _SquishAmount);
                p.y = (p.y - _BounceOffset * 0.3) * squish;  // Multiply to compress Y, limit bounce offset
                p.x = p.x * stretch;  // Multiply to expand X
                
                // Bottom squish (sitting flat)
                float bottomFactor = smoothstep(-0.8, -0.5, p.y);
                p.y -= (1.0 - bottomFactor) * _BottomSquish;
                
                // === VOLUMETRIC 3D JELLY BODY ===
                float2 bodyCenter = float2(0, -0.08);
                float2 toCenter = p - bodyCenter;
                float distToCenter = length(toCenter);
                
                // Apply wobble to radius (75% base leaves 25% margin for breathing)
                float bodyRadius = 0.75 + totalWobble;
                float bodyDist = distToCenter - bodyRadius;
                
                // Calculate fake Z-depth for 3D volume
                float zDepth = sqrt(max(0.0, bodyRadius * bodyRadius - distToCenter * distToCenter));
                float normalizedDepth = zDepth / bodyRadius;
                
                // Soft body mask (tighter to prevent clipping)
                float bodyMask = smoothstep(0.04, -0.04, bodyDist);
                
                // === MAGICAL INNER GLOW (center to edge) ===
                float innerGlow = pow(normalizedDepth, 1.5) * _InnerGlowStrength;
                float3 glowColor = _InnerGlowColor.rgb * innerGlow;
                
                // === 3D LIGHTING ===
                float3 normal = normalize(float3(toCenter.x, toCenter.y, zDepth));
                float3 lightDir = normalize(float3(-0.5, 0.8, 1.0));
                float NdotL = max(0.0, dot(normal, lightDir));
                
                // FRESNEL EFFECT (glass-like edge glow)
                float fresnel = pow(1.0 - normalizedDepth, _FresnelPower);
                
                // Rim light (enhanced with fresnel)
                float rimFactor = pow(1.0 - normalizedDepth, _RimPower) * 1.5;
                rimFactor *= smoothstep(0.0, 0.3, bodyMask);
                
                // Subsurface scattering
                float subsurface = pow(normalizedDepth, 0.6) * _Translucency;
                
                // Base color gradient
                float3 bodyColor = lerp(_EdgeColor.rgb, _CoreColor.rgb, normalizedDepth * 0.8 + 0.2);
                
                // Apply lighting
                bodyColor *= (0.95 + NdotL * 0.2);
                bodyColor += subsurface * _CoreColor.rgb * 0.3;
                bodyColor += glowColor * 0.7;
                bodyColor += _RimColor.rgb * (rimFactor + fresnel * 0.6) * 1.0;
                
                // === PREMIUM SPECULAR (glossy wet) ===
                float3 viewDir = float3(0, 0, 1);
                float3 halfDir = normalize(lightDir + viewDir);
                float spec = pow(max(0.0, dot(normal, halfDir)), _SpecularPower);
                spec *= normalizedDepth * _Shininess;
                bodyColor += spec * float3(1, 1, 1) * 2.5;
                
                // === MICRO PARTICLES (magical sparkles inside) ===
                float particles = 0.0;
                for(float idx = 0.0; idx < _ParticleCount; idx += 1.0)
                {
                    float2 seed = float2(idx * 12.9898, idx * 78.233);
                    float angle = hash(seed) * 6.28318;
                    float radius = hash(seed + 1.0) * 0.6;
                    float floatSpeed = hash(seed + 2.0) * 0.5 + 0.5;
                    
                    float yOffset = frac(time * _ParticleSpeed * floatSpeed) * 1.8 - 0.9;
                    float2 particlePos = float2(cos(angle) * radius, yOffset);
                    
                    float particleDist = length(p - particlePos);
                    float particleSize = 0.02 + hash(seed + 3.0) * 0.02;
                    float particle = smoothstep(particleSize, 0.0, particleDist);
                    
                    // Only show particles inside body
                    particle *= smoothstep(0.05, -0.1, bodyDist);
                    particles += particle;
                }
                bodyColor += particles * _ParticleGlow * float3(1, 1, 0.9);
                
                // === CUTE DEW DROPS (water droplets) ===
                float dewDrops = 0.0;
                for(float d = 0.0; d < 3.0; d += 1.0)
                {
                    float2 dewSeed = float2(d * 45.67, d * 89.12);
                    float dewAngle = hash(dewSeed) * 6.28318;
                    float dewR = 0.6 + hash(dewSeed + 1.0) * 0.2;
                    float dewSlide = frac(time * 0.3 + hash(dewSeed + 2.0)) * 0.4 - 0.2;
                    
                    float2 dewPos = float2(cos(dewAngle) * dewR, sin(dewAngle) * dewR + dewSlide);
                    float dewDist = length(p - dewPos);
                    float dew = smoothstep(0.025, 0.01, dewDist);
                    
                    // Only on surface
                    dew *= smoothstep(-0.05, 0.05, bodyDist) * smoothstep(0.15, 0.05, bodyDist);
                    dewDrops += dew;
                }
                bodyColor += dewDrops * float3(0.9, 1.0, 1.0) * 2.0;
                
                // === SOFT SHADOW AT BASE ===
                float aoFactor = smoothstep(-0.7, -0.3, p.y) * smoothstep(0.8, 0.3, distToCenter);
                float3 shadowColor = _EdgeColor.rgb * 0.75;
                
                // === IRRESISTIBLE BIG EYES (Baby Schema - Kindchenschema) ===
                // DECLARE FIRST before using in bodyColor
                float eyeSpacing = 0.25;
                float eyeY = 0.10;
                float eyeRadius = 0.19 * _EyeEmotiveness;
                
                // EYE DEPTH - Dark shadow behind eyes for contrast
                float2 leftEyeCenter = float2(-eyeSpacing, eyeY);
                float2 rightEyeCenter = float2(eyeSpacing, eyeY);
                float leftEyeShadow = smoothstep(eyeRadius + 0.08, eyeRadius - 0.02, length(p - leftEyeCenter));
                float rightEyeShadow = smoothstep(eyeRadius + 0.08, eyeRadius - 0.02, length(p - rightEyeCenter));
                float eyeShadowMask = max(leftEyeShadow, rightEyeShadow);
                
                // Eyelid soft glow
                float leftEyeGlow = smoothstep(eyeRadius + 0.12, eyeRadius + 0.04, length(p - leftEyeCenter));
                float rightEyeGlow = smoothstep(eyeRadius + 0.12, eyeRadius + 0.04, length(p - rightEyeCenter));
                float eyelidGlow = max(leftEyeGlow, rightEyeGlow) * 0.3;
                
                // === CUTE BLUSH CHEEKS ===
                float2 leftBlushPos = float2(-0.35, -0.05);
                float2 rightBlushPos = float2(0.35, -0.05);
                float leftBlush = smoothstep(_BlushSize, 0.0, length(p - leftBlushPos));
                float rightBlush = smoothstep(_BlushSize, 0.0, length(p - rightBlushPos));
                float blushMask = max(leftBlush, rightBlush);
                float3 blushColor = _BlushColor.rgb * pow(blushMask, 0.6);
                
                // === SURFACE HIGHLIGHT (glossy jelly shine) ===
                float2 highlightPos = p - float2(-0.1, 0.3);
                float highlightDist = length(highlightPos / float2(0.6, 0.3));
                float highlight = smoothstep(1.0, 0.3, highlightDist) * _HighlightIntensity;
                highlight *= smoothstep(-0.2, 0.5, p.y); // Only on top half
                
                // NOW apply all effects to body color
                // Apply eye depth shadows to body
                bodyColor = lerp(bodyColor * 0.4, bodyColor, 1.0 - eyeShadowMask * 0.6);
                
                // Add eyelid glow
                bodyColor += eyelidGlow * float3(1.0, 0.8, 1.0);
                
                // Add blush to body
                bodyColor = lerp(bodyColor, bodyColor + blushColor, blushMask * 0.8);
                
                // Add surface highlight shine
                bodyColor += highlight * float3(1, 1, 1) * normalizedDepth;
                
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
                float eyeDepth = max(leftEyeNormDepth * leftEyeMask, rightEyeNormDepth * rightEyeMask);
                float3 eyeColor = lerp(_EyeColor.rgb * 0.7, _EyeColor.rgb * 1.2, eyeDepth);
                
                // Pupils - larger for innocence
                float pupilRadius = 0.105 * _PupilScale;
                
                // Apply gaze offset for eye tracking
                float2 gazeOffset = float2(_EyeOffsetX, _EyeOffsetY);
                float2 pupilBasePos = float2(-0.025, -0.01);  // Base looking slightly left-down
                
                float leftPupilMask = smoothstep(0.012, -0.012, length(leftEyePos - (pupilBasePos + gazeOffset)) - pupilRadius);
                float rightPupilMask = smoothstep(0.012, -0.012, length(rightEyePos - (pupilBasePos + gazeOffset)) - pupilRadius);
                float pupilMask = max(leftPupilMask, rightPupilMask);
                
                // Add WETNESS effect for emotional depth (glass marble eyes)
                float eyeWetness = pow(eyeDepth, 0.7) * 0.8;
                eyeColor += eyeWetness * float3(0.85, 0.9, 1.0);
                
                // TWINKLE animation - makes eyes feel alive and aware
                float twinkle = 1.0 + sin(time * 2.8 + hash(float2(1.5, 2.3)) * 6.28) * 0.18;
                
                // Larger, more prominent shines - FIXED: Now move with pupils!
                float2 shine1Offset = float2(-0.055, 0.058) + gazeOffset;
                float2 shine2Offset = float2(0.038, -0.032) + gazeOffset;
                float leftShineMask = smoothstep(0.015, -0.015, length(leftEyePos - shine1Offset) - 0.055);
                float rightShineMask = smoothstep(0.015, -0.015, length(rightEyePos - shine1Offset) - 0.055);
                float leftShine2Mask = smoothstep(0.01, -0.01, length(leftEyePos - shine2Offset) - 0.022);
                float rightShine2Mask = smoothstep(0.01, -0.01, length(rightEyePos - shine2Offset) - 0.022);
                float shineMask = max(max(leftShineMask, rightShineMask), max(leftShine2Mask, rightShine2Mask));
                shineMask *= twinkle * 1.3;
                
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
                
                // Body with transparency
                if (bodyMask > 0.01)
                {
                    float3 shadedBody = lerp(shadowColor, bodyColor, aoFactor);
                    finalColor = lerp(finalColor, shadedBody, bodyMask);
                    // Transparent alpha (center more transparent for jelly look)
                    float alphaFactor = lerp(_EdgeColor.a, _CoreColor.a, normalizedDepth * 0.7 + 0.2);
                    finalAlpha = max(finalAlpha, bodyMask * alphaFactor);
                }
                
                // Eyes (opaque)
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
                
                return fixed4(finalColor, finalAlpha);
            }
            ENDCG
        }
    }
}
