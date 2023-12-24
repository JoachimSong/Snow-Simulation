Shader "Unlit/SnowRender"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }

        SubShader
    {
        Pass
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _DepthTex;
            sampler2D _NormalTex;
            fixed4 _LightColor0;

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                float depth = tex2D(_DepthTex, i.uv).r;
                if (depth > 0.8)
                    discard;
                float3 normal = normalize(tex2D(_NormalTex, i.uv).rgb * 2.0 - 1.0);

                float4 clipPos = float4(i.uv * 2 - 1, depth * 2 - 1, 1);
                float4 viewPos = mul(UNITY_MATRIX_I_V, clipPos);
                float3 worldPos = viewPos.xyz / viewPos.w;

                float3 lightDir = -_WorldSpaceLightPos0;
                float3 lightCol = normalize(float3(1, 1, 1));
                float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);
                float3 halfDir = normalize(lightDir + viewDir);

                float NdotL = max(dot(normal, lightDir) * 0.8, 0);
                float NdotH = max(dot(normal, halfDir) * 0.8, 0);

                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb + float3(0.2, 0.23, 0.25);
                float3 diffuse = lightCol * NdotL;
                float3 specular = lightCol * pow(NdotH, 16);

                float3 col = (ambient + diffuse + specular);

                return float4(col, 1);

                //return float4(depth, depth, depth, 1);

                //return float4(normal, 1);
            }
            ENDCG
        }
    }
        FallBack "Diffuse"
}