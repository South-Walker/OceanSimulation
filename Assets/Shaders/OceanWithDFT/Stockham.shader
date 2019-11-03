Shader "Hidden/Stockham"
{
	Properties
	{
	}

	SubShader
	{
		Pass
		{
			Cull Off
			ZWrite Off
			ZTest Off
			ColorMask RGBA

			CGPROGRAM

			#include "Common.cginc"

			sampler2D _Input;
			float _Len;
			float _SubLen;
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _HORIZONTAL _VERTICAL


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


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float4 frag(v2f i) : SV_TARGET
			{
				float index;

				#ifdef _HORIZONTAL
					index = i.uv.x * _Len - 0.5;
				#else
					index = i.uv.y * _Len - 0.5;
				#endif

				float evenIndex = floor(index / _SubLen) * (_SubLen * 0.5) + fmod(index, _SubLen * 0.5) + 0.5;

				#ifdef _HORIZONTAL
					float4 even = tex2D(_Input, float2((evenIndex), i.uv.y * _Len) / _Len);
					float4 odd  = tex2D(_Input, float2((evenIndex + _Len * 0.5), i.uv.y * _Len) / _Len);
				#else
					float4 even = tex2D(_Input, float2(i.uv.x * _Len, (evenIndex)) / _Len);
					float4 odd  = tex2D(_Input, float2(i.uv.x * _Len, (evenIndex + _Len * 0.5)) / _Len);
				#endif

					float twiddleV = -2 * PI * index / _SubLen;
				float2 twiddle = float2(cos(twiddleV), sin(twiddleV));
				float2 outputA = even.xy + ComplexNumberMultiply(twiddle, odd.xy);
				float2 outputB = even.zw + ComplexNumberMultiply(twiddle, odd.zw);

				return float4(outputA, outputB);
			}

			ENDCG
		}
	}
}
