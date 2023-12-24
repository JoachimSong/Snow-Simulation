Shader "Unlit/Water"
{
	Properties
	{
		_AbsorptionScale("Absorption Scale", Range(0.01, 10)) = 1.5
		_AbsorptionCoff("Absorption Coff", Vector) = (0.45, 0.029, 0.018)
	}
		SubShader
	{
		Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100
		
		GrabPass { "BackGroundTexture" }
		cull front
		ztest always
		blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			#define NUM_SAMPLES 64

			float _AbsorptionScale;
			float3 _AbsorptionCoff;
			sampler2D BackGroundTexture;
			sampler2D _CameraDepthTexture;

			float3 volumePosition, volumeSize;
			sampler3D volumeTexture;
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD0;
				float4 grabPos : TEXCOORD1;
				float2 uv : TEXCOORD2;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.grabPos = ComputeGrabScreenPos(o.pos);
				o.uv = v.texcoord.xy;
				return o;
			}

			struct Ray
			{
				float3 origin;
				float3 dir;
			};

			struct AABB
			{
				float3 Min;
				float3 Max;
			};

			//find intersection points of a ray with a box
			bool IntersectBox(Ray r, AABB aabb, out float t0, out float t1)
			{
				float3 invR = 1.0 / r.dir;
				float3 tbot = invR * (aabb.Min - r.origin);
				float3 ttop = invR * (aabb.Max - r.origin);
				float3 tmin = min(ttop, tbot);
				float3 tmax = max(ttop, tbot);
				float2 t = max(tmin.xx, tmin.yz);
				t0 = max(t.x, t.y);
				t = min(tmax.xx, tmax.yz);
				t1 = min(t.x, t.y);
				return t0 <= t1;
			}
			float3 convertTextureSpace(float3 worldPos)
			{
				return (worldPos - volumePosition + 0.5 * volumeSize) / volumeSize;
			}
			float3 convertWorldSpace(float3 texturePos)
			{
				return(texturePos * volumeSize - 0.5 * volumeSize + volumePosition);
			}
			fixed4 frag(v2f i) : SV_Target
			{
				float3 pos = _WorldSpaceCameraPos;
				float3 grab = tex2Dproj(BackGroundTexture, i.grabPos).rgb;

				Ray r;
				r.origin = pos;
				r.dir = normalize(i.worldPos - pos);

				AABB aabb;
				aabb.Min = float3(-0.5,-0.5,-0.5) * volumeSize + volumePosition;
				aabb.Max = float3(0.5,0.5,0.5) * volumeSize + volumePosition;

				//figure out where ray from eye hit front of cube
				float tnear, tfar;
				IntersectBox(r, aabb, tnear, tfar);

				//if eye is in cube then start ray at eye
				if (tnear < 0.0) tnear = 0.0;

				float3 rayStart = r.origin + r.dir * tnear;
				float3 rayStop = r.origin + r.dir * tfar;

				//convert to texture space
				rayStart = convertTextureSpace(rayStart);
				rayStop = convertTextureSpace(rayStop);

				float3 start = rayStart;
				float dist = distance(rayStop, rayStart);
				float stepSize = dist / float(NUM_SAMPLES);
				float3 ds = normalize(rayStop - rayStart) * stepSize;

				float2 screenUV = i.pos.xy / i.pos.w;
				/*float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
				depth = LinearEyeDepth(depth);*/

				//accumulate density though volume along ray
				float density = 0;
				for (int j = 0; j < NUM_SAMPLES; j++, start += ds)
				{
					//if (distance(pos, convertWorldSpace(start)) > depth)
					//	break;
					density += tex3D(volumeTexture, start).x;
				}
				density = max(density - 5, 0);

				//density = depth;
				float3 col = grab * exp(-_AbsorptionCoff * density * _AbsorptionScale / NUM_SAMPLES * 64);

				return float4(col, 0.6);
			}
			ENDCG
		}
	}
}

