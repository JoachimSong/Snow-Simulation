Shader "Unlit/DepthNormalTexture" 
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex;
        float4 _MainTex_ST;
        sampler2D _CameraDepthTexture;
        sampler2D _CameraDepthNormalsTexture;

        struct v2f {
            float4 pos:SV_POSITION;
            half2 uv:TEXCOORD0;
        };


        v2f vert(appdata_img v) {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = v.texcoord;
            return o;
        }

        fixed4 frag_depth(v2f i) : SV_Target{
            float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
            float linearDepth = Linear01Depth(depth);
            //return fixed4(linearDepth, linearDepth, linearDepth, 1.0);
            depth = depth * 1;
            return fixed4(depth, depth, depth, 1.0);
        }

        fixed4 frag_normal(v2f i) : SV_Target{
            fixed3 normal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, i.uv));
            return fixed4(normal * 0.5 + 0.5, 1.0);
        }

        ENDCG

        //Pass 0 Depth Texture
        Pass {
            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag_depth  

            ENDCG
        }

        //Pass 1 Normal Texture
        Pass{
            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag_normal  

            ENDCG
        }


    }
        FallBack Off
}
