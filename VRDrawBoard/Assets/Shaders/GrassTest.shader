Shader "Custom/GrassTest"
{
Properties
    {
        _TriangleOffset ("Triangle Offset", Float) = 0.1
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
 
            #pragma multi_compile_instancing
 
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
 
            struct v2g
            {
                float4 worldPos : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
 
            struct g2f
            {
                float4 pos : SV_POSITION;
                float3 barycentric : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
 
            v2g vert (appdata v)
            {
                v2g o;
 
                // set all values in the v2g o to 0.0
                UNITY_INITIALIZE_OUTPUT(v2g, o);
 
                // setup the instanced id to be accessed
                UNITY_SETUP_INSTANCE_ID(v);
 
                // copy instance id in the appdata v to the v2g o
                UNITY_TRANSFER_INSTANCE_ID(v, o);
 
                o.worldPos = UnityObjectToClipPos(v.vertex);
                return o;
            }
 
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _TriangleOffset)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)
 
            [maxvertexcount(3)]
            void geom(triangle v2g IN[3] : SV_POSITION, inout TriangleStream<g2f> tristream)
            {
                g2f o;
 
                // set all values in the g2f o to 0.0
                UNITY_INITIALIZE_OUTPUT(g2f, o);
 
                // setup the instanced id to be accessed
                UNITY_SETUP_INSTANCE_ID(IN[0]);
 
                // copy instance id in the v2f IN[0] to the g2f o
                UNITY_TRANSFER_INSTANCE_ID(IN[0], o);
 
                // access instanced property
                // float triOffset = UNITY_ACCESS_INSTANCED_PROP(Props, _TriangleOffset);
 
                // explode triangles by each triangle's actual surface normal
                // float3 triNormal = normalize(cross(IN[0].worldPos.xyz - IN[1].worldPos.xyz, IN[0].worldPos.xyz - IN[2].worldPos.xyz)) * triOffset;
 

                o.pos = UnityObjectToClipPos(float4(0.5, 0, 0, 1));
                tristream.Append(o);
 
                o.pos = UnityObjectToClipPos(float4(-0.5, 0, 0, 1));
                tristream.Append(o);
 
                o.pos = UnityObjectToClipPos(float4(0, 1, 0, 1));
                tristream.Append(o);
            }
 
            fixed4 frag (g2f i) : SV_Target
            {
                // setup instance id to be accessed
                UNITY_SETUP_INSTANCE_ID(i);
 
                // access instanced property
                fixed4 col = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
 
                return col;
            }
            ENDCG
        }
    }
}
