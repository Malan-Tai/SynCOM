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
            fixed4 _MainTex_TexelSize;
            int _OutlineSize;
            fixed4 _OutlineColor;

            fixed4 frag(v2f_img input) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, input.uv);

                // Simple sobel filter for the alpha channel.
                float2 d = _MainTex_TexelSize.xy * _OutlineSize;
                fixed alpha = 0;

                for (float i = -d.x; i < d.x; i += _MainTex_TexelSize.x)
                {
                    for (float j = -d.y; i * i + j * j < 4 * d.x * d.y; j += _MainTex_TexelSize.y)
                    {
                        alpha = max(alpha, tex2D(_MainTex, input.uv + float2(i, j)).a);
                    }
                }

                /*half a1 = tex2D(_MainTex, i.uv + d * float2(-1, -1)).a;
                half a2 = tex2D(_MainTex, i.uv + d * float2(0, -1)).a;
                half a3 = tex2D(_MainTex, i.uv + d * float2(+1, -1)).a;

                half a4 = tex2D(_MainTex, i.uv + d * float2(-1,  0)).a;
                half a6 = tex2D(_MainTex, i.uv + d * float2(+1,  0)).a;

                half a7 = tex2D(_MainTex, i.uv + d * float2(-1, +1)).a;
                half a8 = tex2D(_MainTex, i.uv + d * float2(0, +1)).a;
                half a9 = tex2D(_MainTex, i.uv + d * float2(+1, +1)).a;

                float gx = -a1 - a2 * 2 - a3 + a7 + a8 * 2 + a9;
                float gy = -a1 - a4 * 2 - a7 + a3 + a6 * 2 + a9;

                float w = sqrt(gx * gx + gy * gy) / 4;*/

                return fixed4(_OutlineColor.rgb, alpha);
            }

            ENDCG
        }
    }
}