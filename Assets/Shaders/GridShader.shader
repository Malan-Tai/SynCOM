Shader "Custom/GridLines"
{
	Properties
	{
		_GridColor("Grid color", Color) = (1, 1, 1, 1)
		_LineWidth("Lines width", Range(0, .3)) = .01
		_MouseMinRange("Grid min display range near mouse", Float) = 0.75
		_MouseMaxRange("Grid max display range near mouse", Float) = 1.5
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		ZWrite Off
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		LOD 200

		Pass
		{
			CGPROGRAM

			#pragma vertex vert alpha
			#pragma fragment frag alpha

			#include "UnityCG.cginc"

			float _CellSize;
			fixed4 _GridColor;
			float _LineWidth;
			int _DisplayAllGrid;
			float2 _MouseCoord;
			float _MouseMinRange;
			float _MouseMaxRange;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				// xy position of the vertex in worldspace.
				o.uv = mul(unity_ObjectToWorld, v.vertex).xz;

				return o;
			}

			float DrawGrid(float2 uv, float cellSize, float aa)
			{
				float aaThresh = aa;
				float aaMin = aa * 0.1;

				float2 gUV = uv / cellSize + aaThresh;
				gUV = frac(gUV);
				gUV -= aaThresh;
				gUV = smoothstep(aaThresh, aaMin, abs(gUV));

				return max(gUV.x, gUV.y);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float alpha = DrawGrid(i.uv, _CellSize, _LineWidth);
				alpha *= max(_DisplayAllGrid, 1 - saturate((distance(i.uv, _MouseCoord) - _MouseMinRange) / (_MouseMaxRange - _MouseMinRange)));

				return fixed4(_GridColor.xyz, alpha);
			}
			ENDCG
		}
	}
}