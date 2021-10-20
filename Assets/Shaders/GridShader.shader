// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/GridShader"
{
	Properties
	{
		_CellSize("Cell size", Float) = 1
		_GridColor("Color", Color) = (1, 1, 1, 1)
		_LineWidth("Line width", Range(0, .3)) = .01
		_Alpha("Alpha", Range(0, 1)) = 1
	}

	SubShader
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			CGPROGRAM

			#pragma vertex vert alpha
			#pragma fragment frag alpha

			#include "UnityCG.cginc"

			float _CellSize;
			fixed4 _GridColor;
			float _LineWidth;
			float _Grid2Size;
			float _Grid3Size;
			float _Alpha;

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

				float2 fl = floor(gUV);
				gUV = frac(gUV);
				gUV -= aaThresh;
				gUV = smoothstep(aaThresh, aaMin, abs(gUV));
				float d = max(gUV.x, gUV.y);

				return d;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed alpha = DrawGrid(i.uv, _CellSize, _LineWidth);
				return float4(_GridColor.xyz, alpha);
			}
			ENDCG
		}
	}
}
