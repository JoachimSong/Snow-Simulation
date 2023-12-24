Shader "Unlit/BlendTexture" 
{
    Properties{
        _MainTex("Texture", 2D) = "white" {}
    }

        SubShader{
            Tags { "RenderType" = "Opaque" }
            LOD 100

            Pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                sampler2D _DepthTex;
                sampler2D _SnowTex;
                sampler2D _CameraDepthTexture;

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target {
                    //return tex2D(_DepthTex, i.uv);
                    //return tex2D(_SnowTex, i.uv);

                    float mainDepth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r);
                    float snowDepth = tex2D(_DepthTex, i.uv);

                    if (snowDepth > 0.9 || snowDepth> mainDepth)
                        return tex2D(_MainTex, i.uv);

                    return tex2D(_SnowTex, i.uv);
                }
                ENDCG
            }
    }
}