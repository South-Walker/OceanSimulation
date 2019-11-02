Shader "Hidden/Htilde"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			sampler2D _Init;
			sampler2D _OmegaKT;
            #include "Common.cginc"

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
				float2 h0 = tex2D(_Init, i.uv).xy;
				float2 h0conj = tex2D(_Init, i.uv).zw;
				float omegakt = tex2D(_OmegaKT, i.uv).x;
				float isin = sin(omegakt);
				float rcos = cos(omegakt);
				float2 c1 = float2(rcos, isin);
				float2 c2 = float2(rcos, -isin);
				float2 res = ComplexNumberMultiply(h0, c1) +
					ComplexNumberMultiply(h0conj, c2);
				return float4(res, res);
            }
            ENDCG
        }
    }
}
