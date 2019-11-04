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
			sampler2D _Height;
			float4 _Height_TexelSize;
			sampler2D _Displace;
			float4 _Displace_TexelSize;
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
			float3 Sample(float2 uv)
			{
				float2 dxz = tex2D(_Displace, uv).xz;
				float h = tex2D(_Height, uv).r;
				return float3(dxz.x, h, dxz.y);
			}

			float4 frag(v2f i) : SV_Target
			{
				float2 center = i.uv;
				float2 up = i.uv + float2(0, -_Height_TexelSize.y);
				float2 down = i.uv + float2(0, _Height_TexelSize.y);
				float2 left = i.uv + float2(-_Height_TexelSize.x, 0);
				float2 right = i.uv + float2(_Height_TexelSize.x, 0);
				float3 x = float3(2, 0, 0) + Sample(right) - Sample(left);
				float3 z = float3(0, 0, 2) + Sample(down) - Sample(up);
				return float4(normalize(cross(z, x)), 1.0);
            }
            ENDCG
        }
    }
}
