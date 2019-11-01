Shader "Hidden/Initial"
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
			float _A;
			int _Len;
			float2 _Wind;
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

            sampler2D _MainTex;

			fixed4 frag(v2f i) : SV_Target
			{
				float2 uv = float2(i.uv.x - 0.5,i.uv.y - 0.5);
				uv *= _Len;
				float phi = Phillips(uv, _A, _Len, _Wind);
				float2 ht0 = Htilde0(uv, phi);
				float2 ht0conj = Conj(ht0);
				return fixed4(ht0, ht0conj);
            }
            ENDCG
        }
    }
}
