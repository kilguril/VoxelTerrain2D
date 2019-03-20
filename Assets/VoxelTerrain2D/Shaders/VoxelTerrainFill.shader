Shader "VoxelTerrain2D/Pixel Lit Terrain Fill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "IgnoreProjector" = "True" "Queue"="Geometry" }
        Blend Off

		LOD 100
		Cull Off
		Fog { Mode Off }

        Pass
        {
			Tags { LightMode = Vertex }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float2 uv      : TEXCOORD0;
				half3  viewpos : TEXCOORD1;
				UNITY_FOG_COORDS(2)

                float4 position : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			// Shamelessly taken from Ferr2Ds lighting model
			float3 GetLight(int i, float3 aViewPos) {
				half3  toLight = unity_LightPosition[i].xyz - aViewPos * unity_LightPosition[i].w;
				half   distSq = dot(toLight, toLight);
				half   atten = 1.0 / ((distSq * unity_LightAtten[i].z) + 1.0);

				// this prevents areas outside the radius from getting lit, with a bit of a gradient to prevent it from being harsh
				float cutoff = saturate((unity_LightAtten[i].w - distSq) / unity_LightAtten[i].w);

				return unity_LightColor[i].rgb * atten * cutoff;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv       = TRANSFORM_TEX( mul(unity_ObjectToWorld, v.vertex).xy, _MainTex);

				o.viewpos  = UnityObjectToViewPos( v.vertex ).xyz;

				UNITY_TRANSFER_FOG(o, o.position);
                return o;
            }

            fixed4 frag (v2f inp) : SV_Target
            {
                fixed4 col   = tex2D(_MainTex, inp.uv);
				fixed3 light = UNITY_LIGHTMODEL_AMBIENT;

				for (int i = 0; i < 8; i++) {
					light += GetLight(i, inp.viewpos);
				}

				col.rgb *= light;

				UNITY_APPLY_FOG(inp.fogCoord, col );
                return col;
            }
            ENDCG
        }
    }
	Fallback "Unlit/Texture"
}
