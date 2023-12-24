Shader "Unlit/SnowIce"
{
    Properties
    {
        _SnowColor("Snow Color", Color) = (1,1,1,1)
        _IceColor("Ice Color", Color) = (0.277,0.386,0.468,0.2)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Thickness("Thickness", Range(-1,1)) = 0.5
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Opaque" }
            ZWrite off
            Lighting off

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard fullforwardshadows alpha
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup
            #pragma target 3.0

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            StructuredBuffer<float3> positions;
            StructuredBuffer<float3> sizes;
            StructuredBuffer<float4> states;
        #endif

            struct Input
            {
                float2 uv_MainTex;
            };

            sampler2D _MainTex;
            half _Glossiness;
            half _Metallic;
            fixed4 _SnowColor;
            fixed4 _IceColor;

            void setup()
            {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                float3 size = sizes[unity_InstanceID] * (1 - states[unity_InstanceID].y);
                unity_ObjectToWorld._11_21_31_41 = float4(size.x, 0, 0, 0);
                unity_ObjectToWorld._12_22_32_42 = float4(0, size.y, 0, 0);
                unity_ObjectToWorld._13_23_33_43 = float4(0, 0, size.z, 0);
                unity_ObjectToWorld._14_24_34_44 = float4(positions[unity_InstanceID], 1);
                unity_WorldToObject = unity_ObjectToWorld;
                unity_WorldToObject._14_24_34 *= -1;
                unity_WorldToObject._11_22_33 = 1.0f / unity_WorldToObject._11_22_33;
            #endif
            }

            void surf(Input IN, inout SurfaceOutputStandard o)
            {
                fixed4 c = fixed4(1, 1, 1, 1);
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                float iceProportion = states[unity_InstanceID].z / (states[unity_InstanceID].x + states[unity_InstanceID].z);
                c = tex2D(_MainTex, IN.uv_MainTex) * lerp(_SnowColor, _IceColor, iceProportion);
            #endif    
                o.Albedo = c.rgb;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = c.a;
            }
            ENDCG
        }
        FallBack "Diffuse"
}