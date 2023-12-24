Shader "Unlit/Blur"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	CGINCLUDE
	#include "UnityCG.cginc"

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

	sampler2D _MainTex;
	float4 _MainTex_ST;
	float4 _MainTex_TexelSize;
	float4 _BlurRadius;
	float _BilaterFilterFactor;
	sampler2D _CameraDepthNormalsTexture;
	sampler2D _CameraDepthTexture;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	half CompareColor(fixed4 col1, fixed4 col2)
	{
		float l1 = LinearRgbToLuminance(col1.rgb);
		float l2 = LinearRgbToLuminance(col2.rgb);
		return lerp(0, _BilaterFilterFactor, 1.0 - abs(l1 - l2));
	}

	half CompareNormal(float3 normal1, float3 normal2)
	{
		return lerp(0, _BilaterFilterFactor, dot(normal1, normal2));
	}

	float3 GetNormal(float2 uv)
	{
		return DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, uv));
	}

	half CompareDepth(float depth1, float depth2)
	{
		return lerp(0, _BilaterFilterFactor, 1 - abs(depth1 - depth2));
	}

	float GetDepth(float2 uv)
	{
		float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
		float linearDepth = Linear01Depth(depth);
		return linearDepth;
	}

	fixed4 frag_gaussian(v2f i) : SV_Target
	{
		float2 delta = _MainTex_TexelSize.xy * _BlurRadius.xy;
		fixed4 col = 0.37004405286 * tex2D(_MainTex, i.uv);
		col += 0.31718061674 * tex2D(_MainTex, i.uv - delta);
		col += 0.31718061674 * tex2D(_MainTex, i.uv + delta);
		col += 0.19823788546 * tex2D(_MainTex, i.uv - 2.0 * delta);
		col += 0.19823788546 * tex2D(_MainTex, i.uv + 2.0 * delta);
		col += 0.11453744493 * tex2D(_MainTex, i.uv - 3.0 * delta);
		col += 0.11453744493 * tex2D(_MainTex, i.uv + 3.0 * delta);

		col /= 0.37004405286 + 0.31718061674 + 0.31718061674 + 0.19823788546 + 0.19823788546 + 0.11453744493 + 0.11453744493;
		return fixed4(col.rgb, 1.0);
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
		
	fixed4 frag_bilateralnormal(v2f i) : SV_Target
	{
		float2 delta = _MainTex_TexelSize.xy * _BlurRadius.xy;

		float2 uv = i.uv;
		float2 uv0a = i.uv - delta;
		float2 uv0b = i.uv + delta;
		float2 uv1a = i.uv - 2.0 * delta;
		float2 uv1b = i.uv + 2.0 * delta;
		float2 uv2a = i.uv - 3.0 * delta;
		float2 uv2b = i.uv + 3.0 * delta;

		float3 normal = GetNormal(uv);
		float3 normal0a = GetNormal(uv0a);
		float3 normal0b = GetNormal(uv0b);
		float3 normal1a = GetNormal(uv1a);
		float3 normal1b = GetNormal(uv1b);
		float3 normal2a = GetNormal(uv2a);
		float3 normal2b = GetNormal(uv2b);

		fixed4 col = tex2D(_MainTex, uv);
		fixed4 col0a = tex2D(_MainTex, uv0a);
		fixed4 col0b = tex2D(_MainTex, uv0b);
		fixed4 col1a = tex2D(_MainTex, uv1a);
		fixed4 col1b = tex2D(_MainTex, uv1b);
		fixed4 col2a = tex2D(_MainTex, uv2a);
		fixed4 col2b = tex2D(_MainTex, uv2b);

		half w = 0.37004405286;
		half w0a = CompareNormal(normal, normal0a) * 0.31718061674;
		half w0b = CompareNormal(normal, normal0b) * 0.31718061674;
		half w1a = CompareNormal(normal, normal1a) * 0.19823788546;
		half w1b = CompareNormal(normal, normal1b) * 0.19823788546;
		half w2a = CompareNormal(normal, normal2a) * 0.11453744493;
		half w2b = CompareNormal(normal, normal2b) * 0.11453744493;

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
		//return fixed4(normal,1.0);
		//return fixed4(CompareNormal(normal, normal2b), CompareNormal(normal, normal2b), CompareNormal(normal, normal2b), 1.0);
	}

	fixed4 frag_bilateraldepth(v2f i) : SV_Target
	{
		float2 delta = _MainTex_TexelSize.xy * _BlurRadius.xy;

		float2 uv = i.uv;
		float2 uv0a = i.uv - delta;
		float2 uv0b = i.uv + delta;
		float2 uv1a = i.uv - 2.0 * delta;
		float2 uv1b = i.uv + 2.0 * delta;
		float2 uv2a = i.uv - 3.0 * delta;
		float2 uv2b = i.uv + 3.0 * delta;

		float depth = GetDepth(uv);
		float depth0a = GetDepth(uv0a);
		float depth0b = GetDepth(uv0b);
		float depth1a = GetDepth(uv1a);
		float depth1b = GetDepth(uv1b);
		float depth2a = GetDepth(uv2a);
		float depth2b = GetDepth(uv2b);

		fixed4 col = tex2D(_MainTex, uv);
		fixed4 col0a = tex2D(_MainTex, uv0a);
		fixed4 col0b = tex2D(_MainTex, uv0b);
		fixed4 col1a = tex2D(_MainTex, uv1a);
		fixed4 col1b = tex2D(_MainTex, uv1b);
		fixed4 col2a = tex2D(_MainTex, uv2a);
		fixed4 col2b = tex2D(_MainTex, uv2b);

		half w = 0.37004405286;
		half w0a = CompareDepth(depth, depth0a) * 0.31718061674;
		half w0b = CompareDepth(depth, depth0b) * 0.31718061674;
		half w1a = CompareDepth(depth, depth1a) * 0.19823788546;
		half w1b = CompareDepth(depth, depth1b) * 0.19823788546;
		half w2a = CompareDepth(depth, depth2a) * 0.11453744493;
		half w2b = CompareDepth(depth, depth2b) * 0.11453744493;

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
		//return fixed4(depth, depth, depth,1.0);
		//return fixed4(CompareDepth(depth, depth0a), CompareDepth(depth, depth0a), CompareDepth(depth, depth0a), 1.0);
	}

	ENDCG

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		//Pass 0 Gaussian Blur
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_gaussian


			ENDCG
		}

		//Pass 1 BilateralFiter Blur Color
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_bilateralcolor


			ENDCG
		}

		//Pass 2 BilateralFiter Blur Normal
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_bilateralnormal


			ENDCG
		}

		//Pass 3 BilateralFiter Blur Depth
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag_bilateraldepth


			ENDCG
		}
	}
}