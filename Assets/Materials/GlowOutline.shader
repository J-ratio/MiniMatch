Shader "Custom/AlphaEdgeOutline"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,0,1)
        _OutlineSize ("Outline Size", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _OutlineColor;
            float _OutlineSize;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float alpha = tex2D(_MainTex, uv).a;
                float outline = 0.0;
                float2 offset = float2(_OutlineSize/_ScreenParams.x, _OutlineSize/_ScreenParams.y);

                // Sample 8 directions
                for (int x = -1; x <= 1; x++)
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    float a = tex2D(_MainTex, uv + float2(x, y) * offset).a;
                    outline = max(outline, a);
                }

                float4 col = tex2D(_MainTex, uv);
                // Only draw outline where neighbor alpha > 0 and current alpha == 0
                float edge = step(0.01, outline) * step(alpha, 0.01);
                float4 outlineCol = _OutlineColor * edge;
                outlineCol.a *= _OutlineColor.a;

                return col + outlineCol;
            }
            ENDCG
        }
    }
}
