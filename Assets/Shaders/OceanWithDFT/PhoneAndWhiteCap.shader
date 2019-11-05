Shader "Hidden/PhoneAndWhiteCap"
{
	Properties
	{
		_Diffuse("Diffuse", Color) = (0, 1, 1, 1)
		_Specular("Specular", Color) = (0.3, 0.3, 0.3, 1)
		_Gloss("Gloss", Range(0, 256)) = 20
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
			#include "Lighting.cginc"

			fixed4 _Diffuse;
			fixed4 _Specular;
			float _Gloss;
			sampler2D _Height;
			sampler2D _Displace;
			sampler2D _MainTex;
			sampler2D _Normal;
			sampler2D _WhiteCap;
            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
            };
            v2f vert (appdata v)
            {
                v2f o;
				v.vertex.y += tex2Dlod(_Height, v.uv).r;
				float2 dxz = tex2Dlod(_Displace, v.uv).xz;
				v.vertex.x += dxz.x;
				v.vertex.z += dxz.y;
				o.vertex = UnityObjectToClipPos(v.vertex);

				o.worldNormal = mul(tex2Dlod(_Normal, v.uv),
					(float3x3)unity_WorldToObject);
                o.uv = v.uv.xy;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }



            fixed4 frag (v2f i) : SV_Target
            {
				// Get ambient term
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

				fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);

				// Compute diffuse term
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir));

				// Get the reflect direction in world space
				fixed3 reflectDir = normalize(reflect(-worldLightDir, worldNormal));
				// Get the view direction in world space
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				// Compute specular term
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(reflectDir, viewDir)), _Gloss);
				float whitecap = tex2D(_WhiteCap, i.uv).x;
				float3 wc = float3(whitecap, whitecap, whitecap);
				return fixed4(ambient + diffuse + specular + wc, 1.0);
            }
            ENDCG
        }
    }
}
