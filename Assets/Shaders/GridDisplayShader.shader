Shader "Custom/GridDisplay"
{
	Properties
	{
		[NoScaleOffset] _MoveBlobTileset("Move blob tileset", 2D) = "white" {}
		_MoveBlobColor("Move color", Color) = (1, 1, 1, 1)

		[NoScaleOffset] _AttackBlobTileset("Attack blob tileset", 2D) = "white" {}
		_AttackBlobColor("Attack color", Color) = (1, 1, 1, 1)

		_GridColor("Grid color", Color) = (1, 1, 1, 1)
		_LineWidth("Grid lines width", Range(0, .3)) = .01
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 200

		Pass
		{
			CGPROGRAM

			#pragma vertex vert alpha
			#pragma fragment frag alpha

			#include "UnityCG.cginc"

			// All zones (multiple zones types can't be rendered at same time)
			int _GridWidthInTiles;
			int _GridHeightInTiles;
			int _ReachableCoordsCount;
			int2 _ReachableCoords[500];
			int _BlobIndices[500];

			// Move zone
			sampler2D _MoveBlobTileset;
			fixed4 _MoveBlobColor;
			bool _RenderMoveZone;

			// Attack zone
			sampler2D _AttackBlobTileset;
			fixed4 _AttackBlobColor;
			bool _RenderAttackZone;


			struct appdata
			{
				float2 uv : TEXCOORD;
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float2 uv : TEXCOORD;
				float4 vertex : SV_POSITION;
			};


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;

				return o;
			}

			int2 FindBlobCoordFromBlobIndex(int blobIndex)
			{
				switch (blobIndex)
				{
				case 0:		return int2(0, 0);
				case 4:		return int2(1, 0);
				case 71:	return int2(2, 0);
				case 193:	return int2(3, 0);
				case 7:		return int2(4, 0);
				case 199:	return int2(5, 0);
				case 197:	return int2(6, 0);
				case 64:	return int2(7, 0);
				case 5:		return int2(0, 1);
				case 69:	return int2(1, 1);
				case 93:	return int2(2, 1);
				case 119:	return int2(3, 1);
				case 223:	return int2(4, 1);
				//case 256:	return int2(5, 1); // Not possible
				case 245:	return int2(6, 1);
				case 65:	return int2(7, 1);
				case 23:	return int2(0, 2);
				case 213:	return int2(1, 2);
				case 81:	return int2(2, 2);
				case 31:	return int2(3, 2);
				case 253:	return int2(4, 2);
				case 125:	return int2(5, 2);
				case 113:	return int2(6, 2);
				case 16:	return int2(7, 2);
				case 29:	return int2(0, 3);
				case 117:	return int2(1, 3);
				case 85:	return int2(2, 3);
				case 95:	return int2(3, 3);
				case 247:	return int2(4, 3);
				case 215:	return int2(5, 3);
				case 209:	return int2(6, 3);
				case 1:		return int2(7, 3);
				case 21:	return int2(0, 4);
				case 84:	return int2(1, 4);
				case 87:	return int2(2, 4);
				case 221:	return int2(3, 4);
				case 127:	return int2(4, 4);
				case 255:	return int2(5, 4);
				case 241:	return int2(6, 4);
				case 17:	return int2(7, 4);
				case 20:	return int2(0, 5);
				case 68:	return int2(1, 5);
				case 92:	return int2(2, 5);
				case 112:	return int2(3, 5);
				case 28:	return int2(4, 5);
				case 124:	return int2(5, 5);
				case 116:	return int2(6, 5);
				case 80:	return int2(7, 5);
				default:	return int2(-1, -1);
				}
			}

			fixed4 frag(v2f i) : SV_Target
			{
				if (!_RenderMoveZone && !_RenderAttackZone)
				{
					discard;
				}

				int2 uvGridCoord = int2(floor(i.uv.x * _GridWidthInTiles), floor(i.uv.y * _GridHeightInTiles));

				int2 blobCoord = int2(-1, -1);
				for (int j = 0; j < _ReachableCoordsCount; j++)
				{
					if (all(uvGridCoord == _ReachableCoords[j]))
					{
						blobCoord = FindBlobCoordFromBlobIndex(_BlobIndices[j]);
						break;
					}
				}

				if (blobCoord.x == -1)
				{
					discard;
				}

				float2 worldUv = float2(i.uv.x * _GridWidthInTiles, i.uv.y * _GridHeightInTiles);
				float2 outUv = (blobCoord + frac(worldUv)) / float2(8.0, 6.0);
				if (_RenderAttackZone)
				{
					fixed4 col = tex2D(_AttackBlobTileset, outUv);
					return _AttackBlobColor * col;
				}
				else if (_RenderMoveZone)
				{
					fixed4 col = tex2D(_MoveBlobTileset, outUv);
					return _MoveBlobColor * col;
				}
				// else Not possible

				return fixed4(1, 1, 1, 0);
			}

			ENDCG
		}

		Pass
		{
			CGPROGRAM

			#pragma vertex vert alpha
			#pragma fragment frag alpha

			#include "UnityCG.cginc"

			float _CellSize;
			fixed4 _GridColor;
			float _LineWidth;
			bool _RenderGridLines;

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
				if (!_RenderGridLines)
				{
					discard;
				}
				
				fixed alpha = DrawGrid(i.uv, _CellSize, _LineWidth);
				return fixed4(_GridColor.xyz, alpha);
			}
			ENDCG
		}
	}
}
