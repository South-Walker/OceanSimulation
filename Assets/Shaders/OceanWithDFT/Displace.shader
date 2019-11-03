Shader "Hidden/Displace"
{
	Properties
	{
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Common.cginc"
			sampler2D _Htilde;
			int _Len;
			float _Q;
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            float4 frag (v2f i) : SV_Target
            {
				float2 htilde = tex2D(_Htilde, i.uv).xy;
				float2 uv = i.uv * _Len;
					
				float2 k = GetK(uv, _Len);
				float klen = length(k);
				if (klen < MIN)
					return float4(0, 0, 0, 0);
				float kx = k.x / klen;
				float ky = k.y / klen;
				float2 dx = kx * iMultiply(-htilde) * _Q;
				float2 dy = ky * iMultiply(-htilde) * _Q;
				return float4(dx, dy);
            }
            ENDCG
        }
    }
}
