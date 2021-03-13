Shader "Custom/GrassTest"
{
Properties
    {
        _TriangleOffset ("Triangle Offset", Float) = 0.1
        _Color ("Color", Color) = (1,1,1,1)
        _GradientMap("Gradient map", 2D) = "white" {}

        //Position and dimensions
        _GrassHeight("Grass height", float) = 0
        _GrassWidth("Grass width", Range(0.0, 1.0)) = 1.0
        _PositionRandomness("Position randomness", float) = 0

        //Grass blades
        _GrassBlades("Grass blades per triangle", Range(0, 15)) = 1
        _MinimunGrassBlades("Minimum grass blades per triangle", Range(0, 15)) = 1
        _MaxCameraDistance("Max camera distance", float) = 10
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
            #pragma target 4.0

            #pragma multi_compile_instancing
 
            #include "UnityCG.cginc"

            sampler2D _GradientMap;
            float _GrassHeight;
            float _GrassWidth;
            float _PositionRandomness;
 
            float _GrassBlades;
            float _MinimunGrassBlades;
            float _MaxCameraDistance;

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
 
            struct v2g
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
 
            struct g2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float random (float2 st) {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            g2f getGrassVertex(float4 position, float2 uv, fixed4 color) {
                g2f o;

                // set all values in the g2f o to 0.0
                UNITY_INITIALIZE_OUTPUT(g2f, o);

                o.vertex = UnityObjectToClipPos(position);
                o.uv = uv;
                o.color = color;
                return o;
            }
 
            v2g vert(appdata v)
            {
                v2g o;
 
                // set all values in the v2g o to 0.0
                UNITY_INITIALIZE_OUTPUT(v2g, o);
 
                // setup the instanced id to be accessed
                UNITY_SETUP_INSTANCE_ID(v);
 
                // copy instance id in the appdata v to the v2g o
                UNITY_TRANSFER_INSTANCE_ID(v, o);
 
                o.vertex = v.vertex;
                return o;
            }
 
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _TriangleOffset)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)
 
            [maxvertexcount(48)]
            void geom(triangle v2g IN[3] : SV_POSITION, inout TriangleStream<g2f> tristream)
            {
                g2f o;
 
                // set all values in the g2f o to 0.0
                UNITY_INITIALIZE_OUTPUT(g2f, o);
 
                // setup the instanced id to be accessed
                UNITY_SETUP_INSTANCE_ID(IN[0]);
 
                // copy instance id in the v2f IN[0] to the g2f o
                UNITY_TRANSFER_INSTANCE_ID(IN[0], o);
                
                // calculate face normal (the face for blades to stand on)
                float3 normal = normalize(cross(IN[1].vertex - IN[0].vertex, IN[2].vertex - IN[0].vertex));

                // determine blade number
                // int grassBlades = ceil(lerp(_GrassBlades, _MinimunGrassBlades, saturate(distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, IN[0].vertex)) / _MaxCameraDistance)));
                int grassBlades = _GrassBlades;

                for (unsigned int i = 0; i < grassBlades; i++) {
                    float r1 = random(mul(unity_ObjectToWorld, IN[0].vertex).xz * (i+1));
                    float r2 = random(mul(unity_ObjectToWorld, IN[1].vertex).xz * (i+1));

                    // random barycentric coodinate inside the triangle face, use it as midpoint
                    float4 midpoint = (1 - sqrt(r1)) * IN[0].vertex + (sqrt(r1) * (1 - r2)) * IN[1].vertex + (sqrt(r1) * r2) * IN[2].vertex;

                    // map it to [-1, 1]
                    r1 = r1 * 2.0 - 1.0;
                    r2 = r2 * 2.0 - 1.0;

                    // going two opposite directions from the midpoint, get 2 points for the blade to stand on (use random triangle vertices)
                    float4 bladePoint1 = midpoint + _GrassWidth * normalize(IN[i % 3].vertex - midpoint);
                    float4 bladePoint2 = midpoint + _GrassWidth * normalize(IN[i % 3].vertex - midpoint);


                    // use uniform height for now
                    float heightFactor = 1.0 * _GrassHeight;

                    // append bladePoint1 to triangle stream
                    tristream.Append(getGrassVertex(bladePoint1, float2(0, 0), fixed4(0, 0, 0, 1)));

                    // calculate top point of that blade (add in wind factor later)
                    float4 bladeTopPoint = midpoint + float4(normal, 0.0) * heightFactor + float4(r1, 0.0, r2, 0.0) * _PositionRandomness;
                    tristream.Append(getGrassVertex(bladeTopPoint, float2(0.5, 1), fixed4(1, 0.8, 1, 1)));

                    // append bladePoint2 to triangle stream
                    tristream.Append(getGrassVertex(bladePoint2, float2(1, 0), fixed4(0, 0, 0, 1)));
                }

                // no need to draw the base mesh

                // end current primitive strip
                tristream.RestartStrip();

            }
 
            fixed4 frag (g2f i) : SV_Target
            {
                // setup instance id to be accessed
                UNITY_SETUP_INSTANCE_ID(i);

                fixed4 gradientMapColor = tex2D(_GradientMap, float2(i.color.x, 0.0));
                // access instanced property
                fixed4 col = UNITY_ACCESS_INSTANCED_PROP(Props, _Color * (gradientMapColor + i.color.g));
 
                return col;
            }
            ENDCG
        }

    }
}