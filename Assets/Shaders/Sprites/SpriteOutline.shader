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
                // Simple sobel filter for the alpha channel.
                float2 d = _MainTex_TexelSize.xy * _OutlineSize;
                // map uvs to add pixels on the sides to draw the outline : uv * (max - min) + min 
                float2 uv = input.uv * ((1 + d) + d) - d;
                fixed alpha = 0;

                for (float i = -d.x; i <= d.x; i += _MainTex_TexelSize.x)
                {
                    for (float j = -d.y; j <= d.y; j += _MainTex_TexelSize.y)
                    {
                        alpha = int(i * i + j * j <= 4 * d.x * d.y) * max(alpha, tex2D(_MainTex, uv + float2(i, j)).a);
                    }
                }

                return fixed4(_OutlineColor.rgb, alpha);
            }

            ENDCG
        }
    }
}