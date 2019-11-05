Shader "Hidden/WhiteCap"
{
    Properties
    {
	}
		SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			sampler2D _Displace;
			float4 _Displace_TexelSize;
			float _Q;
			float _Threshold;
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

			float4 frag(v2f i) : SV_Target
			{
				float2 up = i.uv + float2(0,-_Displace_TexelSize.y);
				float2 down = i.uv + float2(0, _Displace_TexelSize.y);
				float2 right = i.uv + float2(_Displace_TexelSize.x, 0);
				float2 left = i.uv + float2(-_Displace_TexelSize.x, 0);
				float jxx = tex2D(_Displace, right).x - tex2D(_Displace, left).x;
				float jyy = tex2D(_Displace, down).z - tex2D(_Displace, up).z;
				float jxy = tex2D(_Displace, down).x - tex2D(_Displace, up).x;
				float j = (1 + jxx * _Q) * (1 + jyy * _Q) - _Q * _Q * jxy * jxy;

				return float4(_Threshold - 2 * j, 0, 0, 0);
            }
            ENDCG
        }
    }
}
