Shader "Custom/ParticleFun"
{
    Properties
    {
        _PointSize("Point size", Float) = 50.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            StructuredBuffer<float3> posBuffer;
            StructuredBuffer<float3> velBuffer;

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _PointSize;

            CBUFFER_END


            struct Attributes
            {
                float4 positionOS : POSITION;
                uint instanceID : SV_InstanceID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float size: PSIZE;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;


                // Color
                float vel = length(velBuffer[IN.instanceID]);
                OUT.color = half4(0.8*vel.x, vel.x*0.2, 1-vel, 1);

                // Position
                float3 pos = posBuffer[IN.instanceID];
                OUT.positionHCS = TransformWorldToHClip(pos);
                OUT.size = 10000;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return IN.color;
            }
            ENDHLSL
        }
    }
}