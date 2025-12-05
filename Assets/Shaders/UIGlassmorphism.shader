Shader "UI/Glassmorphism"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,0.7)
        _BlurAmount ("Blur Amount", Range(0, 10)) = 5
        _BorderGlow ("Border Glow", Color) = (1,1,1,0.3)
        _BorderWidth ("Border Width", Range(0, 10)) = 1.5
        
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
            Name "Glassmorphism"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _BlurAmount;
            fixed4 _BorderGlow;
            float _BorderWidth;
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }
            
            // Simple blur approximation using multiple texture samples
            fixed4 ApplyBlur(sampler2D tex, float2 uv, float blurAmount)
            {
                fixed4 color = fixed4(0, 0, 0, 0);
                float total = 0.0;
                
                // 9-tap blur kernel
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 offset = float2(x, y) * blurAmount * 0.002;
                        float weight = 1.0 - (abs(x) + abs(y)) * 0.15;
                        color += tex2D(tex, uv + offset) * weight;
                        total += weight;
                    }
                }
                
                return color / total;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample texture with blur effect
                fixed4 color = ApplyBlur(_MainTex, IN.texcoord, _BlurAmount);
                
                // Apply glass tint
                color *= IN.color;
                
                // Calculate border glow (edge detection)
                float2 edgeDist = min(IN.texcoord, 1.0 - IN.texcoord);
                float edge = min(edgeDist.x, edgeDist.y);
                float borderMask = smoothstep(0.0, _BorderWidth * 0.01, edge);
                
                // Apply border glow (brighter at edges)
                color.rgb = lerp(_BorderGlow.rgb, color.rgb, borderMask);
                color.a = lerp(_BorderGlow.a + color.a, color.a, borderMask);
                
                // Clip for rect mask
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                // Premultiply alpha
                color.rgb *= color.a;
                
                return color;
            }
            ENDCG
        }
    }
}
