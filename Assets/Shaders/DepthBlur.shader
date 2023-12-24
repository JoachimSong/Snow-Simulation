Shader "Unlit/DepthBlur"
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
        float4 _MainTex_TexelSize;
        sampler2D _CameraDepthTexture;
        sampler2D _CameraDepthNormalsTexture;
        float _BilaterFilterFactor;
        float4 _BlurRadius;


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

        fixed4 frag_depth(v2f i) : SV_Target {
            float depth = Linear01Depth(tex2D(_CameraDepthTexture, i.uv).r);
            return fixed4(depth, depth, depth, 1.0);
        }

        half CompareColor(fixed4 col1, fixed4 col2) {
            float l1 = LinearRgbToLuminance(col1.rgb);
            float l2 = LinearRgbToLuminance(col2.rgb);
            return smoothstep(_BilaterFilterFactor, 1.0, 1.0 - abs(l1 - l2));
        }

        fixed4 frag_bilateralcolor(v2f i) : SV_Target
        {
            float2 delta = _MainTex_TexelSize.xy * _BlurRadius.xy;
            fixed4 col = tex2D(_MainTex, i.uv);
            fixed4 col0a = tex2D(_MainTex, i.uv - delta);
            fixed4 col0b = tex2D(_MainTex, i.uv + delta);
            fixed4 col1a = tex2D(_MainTex, i.uv - 2.0 * delta);
            fixed4 col1b = tex2D(_MainTex, i.uv + 2.0 * delta);
            fixed4 col2a = tex2D(_MainTex, i.uv - 3.0 * delta);
            fixed4 col2b = tex2D(_MainTex, i.uv + 3.0 * delta);

            half w = 0.37004405286;
            half w0a = CompareColor(col, col0a) * 0.31718061674;
            half w0b = CompareColor(col, col0b) * 0.31718061674;
            half w1a = CompareColor(col, col1a) * 0.19823788546;
            half w1b = CompareColor(col, col1b) * 0.19823788546;
            half w2a = CompareColor(col, col2a) * 0.11453744493;
            half w2b = CompareColor(col, col2b) * 0.11453744493;

            half3 result;
            result = w * col.rgb;
            result += w0a * col0a.rgb;
            result += w0b * col0b.rgb;
            result += w1a * col1a.rgb;
            result += w1b * col1b.rgb;
            result += w2a * col2a.rgb;
            result += w2b * col2b.rgb;

            result /= w + w0a + w0b + w1a + w1b + w2a + w2b;
            return fixed4(result, 1.0);
            //return fixed4(CompareColor(col, col2b), CompareColor(col, col2b), CompareColor(col, col2b), 1.0);
        }


        fixed4 frag_DepthToNormal(v2f i) : SV_Target
        {
            float depth = tex2D(_MainTex, i.uv).r;
            if (depth > 0.8)
                return float4(0, 0, 0, 1.0);
            float2 texelSize = _MainTex_TexelSize.xy;
            float2 offset = float2(texelSize.x, 0);
            float leftDepth = tex2D(_MainTex, i.uv - offset).r;
            float rightDepth = tex2D(_MainTex, i.uv + offset).r;

            offset = float2(0, texelSize.y);
            float bottomDepth = tex2D(_MainTex, i.uv - offset).r;
            float topDepth = tex2D(_MainTex, i.uv + offset).r;

            float dx1 = depth-leftDepth;
            float dx2 = rightDepth - depth;
            float mindx = dx1;
            if (abs(dx1) > abs(dx2)) {
                mindx = dx2;
            }

            float dy1 = depth - bottomDepth;
            float dy2 = topDepth - depth;
            float mindy = dy1;
            if (abs(dy1) > abs(dy2)) {
                mindy = dy2;
            }

            float3 dx = float3(texelSize.x, 0.0, mindx);
            float3 dy = float3(0.0, texelSize.y, mindy);
            float3 normal = normalize(cross(dx, dy));

            //// 根据法线强度进行调整
            //normal *= _NormalStrength;

            // 将法线从[-1, 1]映射到[0, 1]
            normal = 0.5 * (normal + 1.0);

            return float4(normal, 1.0);
        }
        ENDCG

        //Pass 0 Depth
        Pass {
            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag_depth  

            ENDCG
        }

        //Pass 1 Blur
        Pass{
            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag_bilateralcolor

            ENDCG
        }

        //Pass 2 Depth To Normal
        Pass{
            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag_DepthToNormal

            ENDCG
        }
    }
        FallBack Off
}
