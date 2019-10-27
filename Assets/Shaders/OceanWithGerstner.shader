Shader "Hidden/OceanWithGerstner"
{
	Properties
	{
		_Diffuse("Diffuse", Color) = (1, 1, 1, 1)
		_Specular("Specular", Color) = (1, 1, 1, 1)
		_Gloss("Gloss", Range(8.0, 256)) = 20
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Pass
		{
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"

			fixed4 _Diffuse;
			fixed4 _Specular;
			float _Gloss;

			float _Dx[4];
			float _Dy[4];
			//steepness=A
			float _Steepness[4];
			//Frequency=W
			float _Frequency[4];
			float _Speed[4];
			float _SubColors[3];
			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
			};
			float4 D;
			float temp;
			float sint, cost, wa;
			float agree;
			v2f vert(a2v v)
			{
				v2f o;
				for (int i = 0; i < 4; i++)
				{
					D = float4(_Dx[i], 0, _Dy[i], 1.0);

					agree = _Frequency[i] * dot(D, v.vertex) + _Speed[i] * _Time.y;
					sint = sin(agree);
					cost = cos(agree);
					v.vertex.y += _Steepness[i] * sint;

					wa = _Frequency[i] * _Steepness[i];
					sint *= wa;
					cost *= wa;
					v.normal.x += -D.x * cost;
					v.normal.z += -D.y * cost;
				}

				v.normal.y += 1;

				o.pos = UnityObjectToClipPos(v.vertex);

				o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
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

				return fixed4(ambient + diffuse + specular, 1.0);
		}
		ENDCG
	}
	}
}
