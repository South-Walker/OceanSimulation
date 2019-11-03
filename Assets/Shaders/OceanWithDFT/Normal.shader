Shader "Hidden/Normal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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

			#include "Common.cginc"
			sampler2D _Htilde;
			int _Len;
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

            fixed4 frag (v2f i) : SV_Target
            {
				float2 htilde = tex2D(_Htilde, i.uv).xy;
				float2 uv = i.uv * _Len;

				float2 k = GetK(uv, _Len);
				float kx = k.x;
				float ky = k.y;
				float2 nx = kx * iMultiply(-htilde);
				float2 ny = ky * iMultiply(-htilde);
				return float4(nx, ny);
            }
            ENDCG
        }
    }
}
