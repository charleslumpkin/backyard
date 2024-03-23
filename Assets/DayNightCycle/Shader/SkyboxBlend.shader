Shader "Skybox/SkyboxBlend"
{
    Properties
    {
        _Tint("Tint Color", Color) = (1, 1, 1, 1)
        [Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
        _Rotation("Rotation", Range(0, 360)) = 0
        _Blend("Blend", Range(0.0, 1.0)) = 0.5

        [NoScaleOffset] _FrontTex("Front [+Z]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _BackTex("Back [-Z]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _LeftTex("Left [+X]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _RightTex("Right [-X]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _UpTex("Up [+Y]   (HDR)", 2D) = "grey" {}
        [NoScaleOffset] _DownTex("Down [-Y]   (HDR)", 2D) = "grey" {}
    }

        SubShader
    {
        Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" }
        Cull Off ZWrite Off

        CGINCLUDE
        #include "UnityCG.cginc"

        half4 _Tint;
        half _Exposure;
        float _Rotation;
        float _Blend;

        float3 RotateAroundYInDegrees(float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 rotationMatrix = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(rotationMatrix, vertex.xz), vertex.y).xzy;
        }

        struct AppData
        {
            float4 vertex : POSITION;
            float2 texcoord1 : TEXCOORD0;
            float2 texcoord2 : TEXCOORD1;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct VertexOutput
        {
            float4 pos : POSITION;
            float2 texcoord01 : TEXCOORD0;
            float2 texcoord02 : TEXCOORD1;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        VertexOutput vert(AppData v)
        {
            VertexOutput o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
            o.pos = UnityObjectToClipPos(rotated);
            o.texcoord01 = v.texcoord1;
            o.texcoord02 = v.texcoord2;
            return o;
        }

        half4 SkyboxFragment(VertexOutput i, sampler2D smp, sampler2D smp2, half4 smpDecode)
        {
            half4 tex1 = tex2D(smp, i.texcoord01);
            half4 tex2 = tex2D(smp2, i.texcoord02);
            half4 blendedTex = lerp(tex1, tex2, _Blend);

            half3 c = DecodeHDR(blendedTex, smpDecode);
            c = c * _Tint.rgb * UNITY_COLORSPACE_GAMMA;
            c *= _Exposure;
            return half4(c, 1);
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _FrontTex;
            sampler2D _FrontTex2;
            half4 _FrontTex_HDR;
            half4 frag(VertexOutput i) : SV_Target { return SkyboxFragment(i, _FrontTex, _FrontTex2, _FrontTex_HDR); }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _BackTex;
            sampler2D _BackTex2;
            half4 _BackTex_HDR;
            half4 frag(VertexOutput i) : SV_Target { return SkyboxFragment(i, _BackTex, _BackTex2, _BackTex_HDR); }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _LeftTex;
            sampler2D _LeftTex2;
            half4 _LeftTex_HDR;
            half4 frag(VertexOutput i) : SV_Target { return SkyboxFragment(i, _LeftTex, _LeftTex2, _LeftTex_HDR); }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _RightTex;
            sampler2D _RightTex2;
            half4 _RightTex_HDR;
            half4 frag(VertexOutput i) : SV_Target { return SkyboxFragment(i, _RightTex, _RightTex2, _RightTex_HDR); }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _UpTex;
            sampler2D _UpTex2;
            half4 _UpTex_HDR;
            half4 frag(VertexOutput i) : SV_Target { return SkyboxFragment(i, _UpTex, _UpTex2, _UpTex_HDR); }
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            sampler2D _DownTex;
            sampler2D _DownTex2;
            half4 _DownTex_HDR;
            half4 frag(VertexOutput i) : SV_Target { return SkyboxFragment(i, _DownTex, _DownTex2, _DownTex_HDR); }
            ENDCG
        }
    }
}