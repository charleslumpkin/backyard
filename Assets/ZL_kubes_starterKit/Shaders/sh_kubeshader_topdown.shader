// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "ZerinLABS/sh_kubeshader_topdown"
{
    Properties
    {
		_ColorTop("Color Top", Color) = (1,0,0,1)
		_ColorMid("Color Mid", Color) = (0,1,0,1)
		_ColorBottom("Color Bottom", Color) = (0,0,1,1)
		_TransThresh("Transition threshold", Range(0.01,0.49)) = 0.25

		[NoScaleOffset]_MainTex ("PBR - Diffuse", 2D) = "white" {}
		_Mix("Color / Diffuse mix", Range(0,1)) = 0.5
		_Color("Color overlay (Multip)", Color) = (1,1,1,1)
		_UVshift("UV horizontal shift", Range(0,1)) = 0.0

        _Metallic ("PBR - Metallic", Range(0,1)) = 0.0
		_Glossiness("PBR - Smoothness", Range(0,1)) = 0.5
		[NoScaleOffset]_Normal ("PBR - Normal", 2D) = "bump" {}
		[HDR] _Emissive("PBR - Emissive)", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		uniform sampler2D _MainTex;
		uniform sampler2D _Normal;
		
		uniform fixed4 _ColorTop;
		uniform fixed4 _ColorMid;
		uniform fixed4 _ColorBottom;
		uniform half _TransThresh;

		uniform half _Mix;
		uniform fixed4 _Color;
		uniform half _UVshift;

		uniform half _Metallic;
		uniform half _Glossiness;
		uniform fixed4 _Emissive;

		/*
			float3 viewDir - contains view direction, for computing Parallax effects, rim lighting etc.
			float4 with COLOR semantic - contains interpolated per-vertex color.
			float4 screenPos - contains screen space position for reflection or screenspace effects. Note that this is not suitable for GrabPass; you need to compute custom UV yourself using ComputeGrabScreenPos function.
			float3 worldPos - contains world space position.
			float3 worldRefl - contains world reflection vector if surface shader does not write to o.Normal. See Reflect-Diffuse shader for example.
			float3 worldNormal - contains world normal vector if surface shader does not write to o.Normal.
			float3 worldRefl; INTERNAL_DATA - contains world reflection vector if surface shader writes to o.Normal. To get the reflection vector based on per-pixel normal map
			, use WorldReflectionVector (IN, o.Normal). See Reflect-Bumped shader for example.
			float3 worldNormal; INTERNAL_DATA - contains world normal vector if surface shader writes to o.Normal. To get the normal vector based on per-pixel normal map, use WorldNormalVector (IN, o.Normal).
		*/

        struct Input
        {
            float2 uv_MainTex;
			float3 worldNormal; INTERNAL_DATA //<<---- needed to get the "worldNormal"
        };
		        
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
		
		/*void vert(inout appdata_full v, out Input data)
		{
			UNITY_INITIALIZE_OUTPUT(Input, v);
			data.wNormal = v.Normal;// mul((float3x3)unity_ObjectToWorld, v.normal));
		}*/

        void surf (Input IN, inout SurfaceOutputStandard OUT)
        {
			fixed3 NM = UnpackNormal(tex2D(_Normal, IN.uv_MainTex));
			OUT.Normal = NM;
			fixed3 wNormal = WorldNormalVector(IN, OUT.Normal);

			fixed3 vertVect = fixed3(0.0, 1.0, 0.0);
			float dotProd = dot(wNormal, vertVect);

			fixed3 colTopBottom = lerp(_ColorBottom, _ColorTop, step(0, dotProd));
			
			float absDotProd = abs(dotProd);

			float mask = smoothstep(0.5 - _TransThresh, _TransThresh + 0.5, absDotProd);

			fixed3 col = lerp(_ColorMid, colTopBottom, mask);

			fixed4 DF = tex2D (_MainTex, IN.uv_MainTex + half2(_UVshift,0.0));
			
			DF.rgb = lerp(col, DF.rgb, _Mix) * _Color;

			OUT.Albedo = DF.rgb;
			OUT.Metallic = _Metallic;
			OUT.Smoothness = _Glossiness;
			OUT.Alpha = DF.a;
			OUT.Emission = _Emissive.rgb; 
        }
        ENDCG
    }
    FallBack "Diffuse"
}
