Shader "Starfield"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StarDensity ("Star Density", Range(1, 100)) = 30.0
        _TwinkleSpeed ("Twinkle Speed", Range(0, 10)) = 3.0
        _BGColor ("Background Color", Color) = (0.02, 0.02, 0.05, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            float _StarDensity;
            float _TwinkleSpeed;
            fixed4 _BGColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // 简单的伪随机函数
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float StarLayer(float2 uv)
            {
                float2 gv = frac(uv) - 0.5; // 网格坐标
                float2 id = floor(uv);      // 网格ID
                
                float n = Hash21(id); // 为每个格子生成随机数
                
                // 只有随机数大于 0.9 的格子才渲染星星
                float size = frac(n * 123.45);
                float d = length(gv - (float2(n, frac(n * 10.0)) - 0.5) * 0.8);
                
                // 星星闪烁逻辑
                float twinkle = sin(_Time.y * _TwinkleSpeed + n * 6.28) * 0.5 + 0.5;
                
                // 绘制圆形星星，并让边缘模糊
                float star = smoothstep(0.1 * size, 0.05 * size, d);
                return star * twinkle;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv * _StarDensity;
                
                // 叠加三层星星，产生远近深度感
                float stars = 0;
                stars += StarLayer(uv);
                stars += StarLayer(uv * 1.5 + float2(50, 50)) * 0.5; // 远处的星星
                stars += StarLayer(uv * 0.5 - float2(100, 100)) * 1.2; // 近处的亮星

                fixed4 col = _BGColor + stars;
                return col;
            }
            ENDCG
        }
    }
}