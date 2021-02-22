﻿Shader "Custom/NoiseBall"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
        _Smoothness("Smoothness", Range(0, 1)) = 0
        _Metallic("Metallic", Range(0, 1)) = 0
    }
    SubShader
    {

        Tags { "RenderType"="Opaque" }

        Cull Off

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows
        #pragma vertex vert addShadow
        #pragma instancing_options procedural:setup
        #pragma target 3.5

        #if SHADER_TARGET >= 35 && (defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_PSSL) || defined(SHADER_API_SWITCH) || defined(SHADER_API_VULKAN) || (defined(SHADER_API_METAL) && defined(UNITY_COMPILER_HLSLCC)))
            #define SUPPORT_STRUCTUREDBUFFER
        #endif

        #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) && defined(SUPPORT_STRUCTUREDBUFFER)
            #define ENABLE_INSTANCING
        #endif

        struct appdata
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 tangent : TANGENT;
            float4 texcoord1 : TEXCOORD1;
            float4 texcoord2 : TEXCOORD2;
            uint vid : SV_VertexID;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Input
        {
            float vface : VFACE;
        };

        struct geometryOutput {
            float4 pos : SV_POSITION;
        };

        half4 _Color;
        half _Smoothness;
        half _Metallic;

        float4x4 _LocalToWorld;
        float4x4 _WorldToLocal;


        #if defined(ENABLE_INSTANCING)

        StructuredBuffer<float4> _PositionBuffer;
        StructuredBuffer<float4> _NormalBuffer;

        #endif

        void vert(inout appdata v) {
            #if defined(ENABLE_INSTANCING)

            uint id = unity_InstanceID * 3 + v.vid;

            v.vertex.xyz = _PositionBuffer[id].xyz;
            v.normal = _NormalBuffer[id].xyz;

            #endif
        }

        void setup() {
            unity_ObjectToWorld = _LocalToWorld;
            unity_WorldToObject = _WorldToLocal;
        }
        
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Normal = float3(0, 0, IN.vface < 0 ? -1 : 1);
        }
        ENDCG


    }

    FallBack "Diffuse"
}