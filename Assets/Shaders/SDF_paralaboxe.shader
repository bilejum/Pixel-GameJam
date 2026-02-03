Shader "Custom/RaymarchingBox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // --- 核心数学函数转换 ---

            float sdBox(float3 p, float3 b)
            {
                float3 q = abs(p) - b;
                return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
            }

            float3 opRep(float3 p, float3 c, float3 l)
            {
                return p - c * clamp(round(p / c), -l, l);
            }

            float map(float3 p)
            {
                p.z += 5.0;
                p = opRep(p, float3(4.0, 4.0, 4.0), float3(1.0, 1.0, 1.0));
                return sdBox(p, float3(1.0, 1.0, 1.0));
            }

            float3 calcNormal(float3 p)
            {
                const float h = 1e-5;
                const float2 k = float2(1, -1);
                return normalize(
                    k.xyy * map(p + k.xyy * h) +
                    k.yyx * map(p + k.yyx * h) +
                    k.yxy * map(p + k.yxy * h) +
                    k.xxx * map(p + k.xxx * h)
                );
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float aspectRatio = _ScreenParams.x / _ScreenParams.y;

                // 模拟原始代码中的非 CineShader 摄像机设置
                float3 rayOri = float3((uv - 0.5) * float2(aspectRatio, 1.0) * 6.0, 3.0);
                float3 rayDir = float3(0.0, 0.0, -1.0);
                float maxDepth = 10.0;
                
                float depth = 0.0;
                float3 p = rayOri;
                
                // Raymarching 循环
                [loop]
                for(int j = 0; j < 64; j++) {
                    p = rayOri + rayDir * depth;
                    float dist = map(p);
                    depth += dist;
                    if (dist < 1e-4 || depth > maxDepth) break;
                }
                
                depth = min(maxDepth, depth);
                float3 n = calcNormal(p);
                
                // 光照计算
                float b = max(0.0, dot(n, float3(0.577, 0.577, 0.577)));
                
                // 颜色计算 (Unity 中 _Time.y 是秒)
                float3 col = (0.5 + 0.5 * cos((b + _Time.y * 3.0) + uv.xyx * 2.0 + float3(0,2,4))) * (0.85 + b * 0.35);
                col *= exp(-depth / maxDepth);

                return float4(col, 1.0);
            }
            ENDCG
        }
    }
}