Shader "Custom/Grass"
{
    Properties {
		[Header(Shading)]
        _TopColor("Top Color", Color) = (1,1,1,1)
		_BottomColor("Bottom Color", Color) = (1,1,1,1)
		_TranslucentGain("Translucent Gain", Range(0,1)) = 0.5

		// grass blade face bend rotation
		_BendRotationRandom("Bend Rotation Bend", Range(0, 1)) = 0.2

		// grass blade width / height
		_BladeWidth("Blade Width", Float) = 0.05
		_BladeWidth("Blade Width Random", Float) = 0.02
		_BladeHeight("Blade Height", Float) = 0.5
		_BladeHeightRandom("Blade Height Random", Float) = 0.3

		// control subdivision amount
		_TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1

		// grass blade curve
		_BladeForward("Blade Forward Amount", Float) = 0.38
		_BladeCurve("Blade Curvature Amount", Range(1, 4)) = 2
    }

	CGINCLUDE
	#define BLADE_SEGMENTS 3 // for grass blade curvature (divide into 3 segments)
	#include "UnityCG.cginc"
	#include "Autolight.cginc"
	#include "Assets/Shaders/CustomTessellation.cginc" // to subdivide an input surface into more primitives (triangles)

	// Simple noise function, sourced from http://answers.unity.com/answers/624136/view.html
	// Extended discussion on this function can be found at the following link:
	// https://forum.unity.com/threads/am-i-over-complicating-this-random-function.454887/#post-2949326
	// Returns a number in the 0...1 range.
	float rand(float3 co) {
		return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
	}

	// Construct a rotation matrix that rotates around the provided axis, sourced from:
	// https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
	float3x3 AngleAxis3x3(float angle, float3 axis) {
		float c, s;
		sincos(angle, s, c);

		float t = 1 - c;
		float x = axis.x;
		float y = axis.y;
		float z = axis.z;

		return float3x3(
			t * x * x + c, t * x * y - s * z, t * x * z + s * y,
			t * x * y + s * z, t * y * y + c, t * y * z - s * x,
			t * x * z - s * y, t * y * z + s * x, t * z * z + c
		);
	}


	// ----- Vertex structures (already in CustomTessellation.cginc) ---- //


	// ---- blade face bend rotation ---- //
	float _BendRotationRandom;

	// ---- blade width / height ---- //
	float _BladeWidth;
	float _BladeWidthRandom;
	float _BladeHeight;
	float _BladeHeightRandom;

	// ---- blade face curvature ---- //
	float _BladeForward;
	float _BladeCurve;



	// ---- Geometry Shader structures ---- //
	struct geometryOutput {
		float4 pos : SV_POSITION;
		
		// for color sampling (provide frag shader with uv)
		float2 uv : TEXCOORD0;
	};

	geometryOutput VertexOutput(float3 pos, float2 uv) {
		geometryOutput output;
		output.pos = UnityObjectToClipPos(pos);
		output.uv = uv;
		return output;
	}

	// ---- Grass generation utitily function ---- //
	geometryOutput GenerateGrassVertex(float3 vertexPosition, float width, float height, float forwardCurveOffset, float2 uv, float3x3 transformMatrix) {
		float3 tangentPosition = float3(width, forwardCurveOffset, height);

		float3 localPosition = vertexPosition + mul(transformMatrix, tangentPosition);
		return VertexOutput(localPosition, uv);
	}


	// ---- Geomety shader ---- //
	[maxvertexcount(2 * BLADE_SEGMENTS + 1)] // based on how many segments we have on one single grass blade
	void geo(triangle vertexOutput IN[3], inout TriangleStream<geometryOutput> triangleStream) {
		geometryOutput output;


		float3 pos = IN[0].vertex;

		// get the 3 vectors for tangent space: normal, tangent, biTangent
		float3 vNormal = IN[0].normal;
		float4 vTangent = IN[0].tangent;

		// unity stores the direction of bitangent in tangent's w coordinate (??how does that work??)
		float3 vBiTangent = cross(vNormal, vTangent) * vTangent.w;

		// transform tangent to local space
		float3x3 tangentToLocal = float3x3(
			vTangent.x, vBiTangent.x, vNormal.x,
			vTangent.y, vBiTangent.y, vNormal.y,
			vTangent.z, vBiTangent.z, vNormal.z
		);

		// create random facing of the grass blades
		// multiply rand (0 to 1) by two-pi to get the complete range of rotation angle, use pos as random seed
		float3x3 bladeFaceRotationMatrix = AngleAxis3x3(rand(pos) * UNITY_TWO_PI, float3(0, 0, 1));
		

		// create random blade face bend
		// multiply UNITY_PI by 0.5, to get random degree range 0 - 90 degrees
		// swizzle pos vector to get a unique seed
		float3x3 bendRotationMatrix = AngleAxis3x3(rand(pos.xxy) * _BendRotationRandom * UNITY_PI * 0.5, float3(-1, 0, 0));

		// calculate transform matrix (we need a second independent one cuz we wanna fix the blades to the ground)
		float3x3 transformationMatrix = mul(mul(tangentToLocal, bladeFaceRotationMatrix), bendRotationMatrix);
		float3x3 transformationMatrixFacing = mul(tangentToLocal, bladeFaceRotationMatrix);

		// generate random blade widths / heights
		float width = (rand(pos.xzy) * 2 - 1) * _BladeWidthRandom + _BladeWidth;
		float height = (rand(pos.zxx) * 2 - 1) * _BladeHeightRandom + _BladeHeight;

		// generate random blade curve / forward
		float bladeForward = rand(pos.xyx) * _BladeForward;
		

		// generate blade segments
		for (int i = 0; i < BLADE_SEGMENTS; i++) {
			// t ranges from 0 ... 1, representing how far we are along the blade
			float t = i / (float)BLADE_SEGMENTS;

			// from the root to the tip of the blade, calculate segment width / height
			float segmentHeight = height * t;
			float segmentWidth = width * (1 - t);

			// generate random segment forward (make it non-linear)
			float segmentForward = pow(t, _BladeCurve) * bladeForward;

			// transform matrix on the root should be the 'Facing' one
			float3x3 transformMatrix = (i == 0) ? transformationMatrixFacing : transformationMatrix;

			// insert vertices
			// ensure each vertex is in clip space, or else it will override the result and bringing it to screen space
			// also align them with the input points normal (using tangent space)
			triangleStream.Append(GenerateGrassVertex(pos, segmentWidth, segmentHeight, segmentForward, float2(0, t), transformMatrix));
			triangleStream.Append(GenerateGrassVertex(pos, -segmentWidth, segmentHeight, segmentForward, float2(1, t), transformMatrix));
		}
		
		// insert the vertex at the tip of the blade
		triangleStream.Append(GenerateGrassVertex(pos, 0, height, bladeForward, float2(0.5, 1), transformationMatrix));
	}


	ENDCG

    SubShader {
		Cull Off

        Pass {
			Tags {
				"RenderType" = "Opaque"
				"LightMode" = "ForwardBase"
			}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma geometry geo
			// ---- tessellation stage ---- //
			#pragma hull hull
			#pragma domain domain
			// ---------------------------- //
			#pragma multi_compile_instancing
			#pragma target 4.6
            
			#include "Lighting.cginc"

			float4 _TopColor;
			float4 _BottomColor;
			float _TranslucentGain;

			// goal is to allow artist to input define bottom / top color, and interpolate between them
			float4 frag (geometryOutput geoOut, fixed facing : VFACE) : SV_Target {	
				
				return lerp(_BottomColor, _TopColor, geoOut.uv.y);
            }
            ENDCG
        }
    }
}