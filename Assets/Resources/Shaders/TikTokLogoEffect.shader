Shader "UI/TikTokLogoEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Glitch Settings)]
        _Speed ("闪烁速度", Float) = 15.0
        _Amount ("偏移强度", Range(0, 0.05)) = 0.01
        _Frequency ("触发频率", Range(0, 1)) = 0.4
        
        // UI 必须属性（防止在 ScrollView 中显示异常）
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Speed;
            float _Amount;
            float _Frequency;

            // 简单噪声函数
            float hash(float n) { return frac(sin(n) * 43758.5453123); }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 1. 计算时间随机种子
                float timeStep = floor(_Time.y * _Speed);
                float isGlitch = step(1.0 - _Frequency, hash(timeStep));
                
                // 2. 计算偏移量 (利用 hash 产生随机正负偏移)
                float offset = (hash(timeStep + 0.1) - 0.5) * 2.0 * _Amount * isGlitch;
                
                // 3. RGB 通道分离采样
                // R通道向左偏，B通道向右偏，G不动
                fixed4 colR = tex2D(_MainTex, i.texcoord + float2(offset, 0));
                fixed4 colG = tex2D(_MainTex, i.texcoord);
                fixed4 colB = tex2D(_MainTex, i.texcoord - float2(offset, 0));

                // 4. 组合颜色 (抖音风格：红、绿、蓝分离)
                fixed4 finalCol;
                finalCol.r = colR.r;
                finalCol.g = colG.g;
                finalCol.b = colB.b;
                // 透明度取三个通道的最大值或平均值，防止偏移处变透明
                finalCol.a = max(colR.a, max(colG.a, colB.a));

                return finalCol * i.color;
            }
            ENDCG
        }
    }
}