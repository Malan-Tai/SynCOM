Shader "Custom/SpriteWallCutout"
{
	Properties
	{
		_Color("Tint", Color) = (0, 0, 0, 1)
		_MainTex("Texture", 2D) = "white" {}
		_CutoutColor("Cutout color", Color) = (0, 0, 0, 1)
		_CutoutTex("Cutout texture", 2D) = "white" {}
		_HighlightSpeed("Speed of highlight oscillation", Float) = 1.0
		_MinHighlightAlpha("Minimum alpha of the highlight", Float) = 0.2
		_MaxHighlightAlpha("Maximum alpha of the highlight", Float) = 0.8
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Cull off
			ZWrite off
			ZTest Always

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			sampler2D _CutoutTex;
			float4 _MainTex_ST;
			float4 _CutoutTex_ST;

			fixed4 _Color;
			fixed4 _CutoutColor;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv1 : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv1 : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv1 = TRANSFORM_TEX(v.uv1, _MainTex);
				o.uv2 = TRANSFORM_TEX(mul(unity_ObjectToWorld, v.vertex).xy, _CutoutTex);
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET
			{
				fixed4 cutoutCol = _CutoutColor;
				cutoutCol.a *= tex2D(_MainTex, i.uv1).w * tex2D(_CutoutTex, i.uv2).w;
				return cutoutCol;
			}

			ENDCG
		}

		Pass
		{
			ZWrite off
			Cull off

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _Color;
			fixed4 _HighlightColor;
			int _Highlight;
			float _HighlightSpeed;
			float _MinHighlightAlpha;
			float _MaxHighlightAlpha;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				col *= _Color * i.color;
				float highlightAlpha =
					_Highlight * (1 - floor(1 - col.a)) *
					(_MinHighlightAlpha + (_MaxHighlightAlpha - _MinHighlightAlpha) * 0.5 * (1 + sin(_HighlightSpeed * _Time.y)));
				col = col + highlightAlpha * _HighlightColor;
				return fixed4(col.rgb, min(1, col.a));
			}

			ENDCG
		}
	}
}