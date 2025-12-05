Shader "UI/GradientButton"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _TopColor ("Top Color", Color) = (1,1,1,1)
        _BottomColor ("Bottom Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,1,1,0.5)
        _GlowIntensity ("Glow Intensity", Range(0, 2)) = 0.5
        
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
            Name "GradientButton"
            
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
            fixed4 _TopColor;
            fixed4 _BottomColor;
            fixed4 _GlowColor;
            float _GlowIntensity;
            float4 _ClipRect;
            
            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color;
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target
            {
                // Sample texture (for rounded corners)
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                
                // Vertical gradient
                fixed4 gradientColor = lerp(_BottomColor, _TopColor, IN.texcoord.y);
                
                // Apply glow at edges (radial)
                float2 center = IN.texcoord - 0.5;
                float dist = length(center);
                float glow = smoothstep(0.5, 0.3, dist) * _GlowIntensity;
                gradientColor.rgb += _GlowColor.rgb * glow;
                
                // Combine
                fixed4 color = gradientColor * texColor * IN.color;
                
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
