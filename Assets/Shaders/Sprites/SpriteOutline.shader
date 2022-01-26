Shader "Custom/SpriteOutline"
{
    Properties
    {
        _MainTex("-", 2D) = ""{}
        _OutlineSize("Outline size", Int) = 1
        _OutlineColor("Outline color", Color) = (1, 0, 0, 1)
    }

    Subshader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            ZTest Always
            Cull Off
            ZWrite Off
            Fog { Mode off }

            CGPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex vert_img
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            int _OutlineSize;
            half4 _OutlineColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Simple sobel filter for the alpha channel.
                float d = _MainTex_TexelSize.xy * _OutlineSize;

                half a1 = tex2D(_MainTex, i.uv + d * float2(-1, -1)).a;
                half a2 = tex2D(_MainTex, i.uv + d * float2(0, -1)).a;
                half a3 = tex2D(_MainTex, i.uv + d * float2(+1, -1)).a;

                half a4 = tex2D(_MainTex, i.uv + d * float2(-1,  0)).a;
                half a6 = tex2D(_MainTex, i.uv + d * float2(+1,  0)).a;

                half a7 = tex2D(_MainTex, i.uv + d * float2(-1, +1)).a;
                half a8 = tex2D(_MainTex, i.uv + d * float2(0, +1)).a;
                half a9 = tex2D(_MainTex, i.uv + d * float2(+1, +1)).a;

                float gx = -a1 - a2 * 2 - a3 + a7 + a8 * 2 + a9;
                float gy = -a1 - a4 * 2 - a7 + a3 + a6 * 2 + a9;

                float w = sqrt(gx * gx + gy * gy) / 4;

                return lerp(col, _OutlineColor, w);
            }

            ENDCG
        }
    }
}