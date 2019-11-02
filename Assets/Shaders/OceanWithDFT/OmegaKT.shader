Shader "Hidden/OmegaKT"
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

			float _DeltaT;
			int _Len;
			sampler2D _LastT;
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
				//float last = tex2D(_LastT, i.uv).r;
				float2 uv = float2(i.uv.x - 0.5,i.uv.y - 0.5);
				uv *= _Len;
				float2 k = GetK(uv, _Len);
				float omega = sqrt(G * length(k));
				float omegakt = _Time.y * omega;//+last;
				omegakt = fmod(omegakt, 2 * PI);
				return float4(omegakt, 0, 0, 0);
            }
            ENDCG
        }
    }
}
