Shader "Custom/URP_ORM_PBR"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map (RGB)", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Range(0, 2)) = 1.0
        
        [NoScaleOffset] _MaskMap("ORM Map (R:AO, G:Rough, B:Metal)", 2D) = "white" {}
        _MetallicScale("Metallic Scale", Range(0, 1)) = 1.0
        _RoughnessScale("Roughness Scale", Range(0, 1)) = 1.0
        _OcclusionScale("Occlusion Scale", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "Queue" = "Geometry"
        }
        
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            
            // 유니티 셰이더 키워드 정의
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float4 tangentOS    : TANGENT;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float2 uv           : TEXCOORD1;
                float4 shadowCoord  : TEXCOORD3; // 매크로 대신 수동으로 float4 지정
                half3 normalWS      : TEXCOORD4;
                half4 tangentWS     : TEXCOORD5; 
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _BumpScale;
                half _MetallicScale;
                half _RoughnessScale;
                half _OcclusionScale;
            CBUFFER_END

            TEXTURE2D(_BaseMap);        SAMPLER(sampler_BaseMap);
            TEXTURE2D(_BumpMap);        SAMPLER(sampler_BumpMap);
            TEXTURE2D(_MaskMap);        SAMPLER(sampler_MaskMap);

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                
                output.normalWS = normalInput.normalWS;
                output.tangentWS = half4(normalInput.tangentWS, input.tangentOS.w);

                // 유니티 표준 방식으로 그림자 좌표 변환 및 대입
                output.shadowCoord = TransformWorldToShadowCoord(vertexInput.positionWS);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. 텍스처 샘플링
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                half4 orm = SAMPLE_TEXTURE2D(_MaskMap, sampler_MaskMap, input.uv);
                half4 packedNormal = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv);

                // 2. ORM 데이터 추출 및 변환
                half occlusion = lerp(1.0, orm.r, _OcclusionScale);
                half roughness = orm.g * _RoughnessScale;
                half metallic = orm.b * _MetallicScale;
                half smoothness = 1.0 - roughness; 

                // 3. 법선 벡터(Normal) 계산
                half3 normalTS = UnpackNormalScale(packedNormal, _BumpScale);
                half3 sgn = input.tangentWS.w * GetOddNegativeScale();
                half3 bitangentWS = cross(input.normalWS, input.tangentWS.xyz) * sgn;
                half3 normalWS = TransformTangentToWorld(normalTS, half3x3(input.tangentWS.xyz, bitangentWS, input.normalWS));
                normalWS = normalize(normalWS);

                // 4. 유니티 PBR 입력 구조체 채우기
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = SafeNormalize(GetCameraPositionWS() - input.positionWS);
                inputData.shadowCoord = input.shadowCoord; // 직접 대입
                inputData.bakedGI = SampleSH(normalWS);

                // 5. 최종 라이팅 연산
                half4 finalColor = UniversalFragmentPBR(
                    inputData, 
                    baseColor.rgb, 
                    metallic, 
                    half3(0, 0, 0), 
                    smoothness, 
                    occlusion, 
                    half3(0, 0, 0), 
                    baseColor.a
                );

                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}
