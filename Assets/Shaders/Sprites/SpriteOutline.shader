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

                for (float i = -d.x; i <= d.x; i += _MainTex_TexelSize.x)
                {
                    for (float j = -d.y; j <= d.y; j += _MainTex_TexelSize.y)
                    {
                        alpha = int(i * i + j * j <= 4 * d.x * d.y) * max(alpha, tex2D(_MainTex, input.uv + float2(i, j)).a);
                    }
                }

                return fixed4(_OutlineColor.rgb, alpha);
            }

            ENDCG
        }
    }
}