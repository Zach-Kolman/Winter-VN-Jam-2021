Shader "OG/_SimpleAtmosphericFog" {
    Properties{
        [HideInInspector] _MainTex ("Base Map", 2D) = "white" {}
    }

    SubShader{
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        //ZTest Always ZWrite Off Cull Off

        Pass{
            HLSLPROGRAM
            #pragma vertex Vert 
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "SimpleAtmosphericFogFunctions.hlsl"

            struct Attributes{
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings{
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 pos           : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            //CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            //CBUFFER_END

            float4x4 _InverseView;

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

    
            float SampleDepth(float2 uv){
                float d = 0;
            #if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
                d = SAMPLE_TEXTURE2D_ARRAY(_CameraDepthTexture, sampler_CameraDepthTexture, uv, unity_StereoEyeIndex).r;
            #else
                d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, uv);
            #endif      
                return d;
            }

            float3 GetWorldPos(float3 pos, float2 uv){
                float3 a = (pos * float3(2,2,2)) - float3(1,1,1);
                float4 ndc = mul(unity_CameraInvProjection, float4(a.x, a.y, 1, 1)) * LinearEyeDepth(SampleDepth(uv), _ZBufferParams);
                float4 worldPos = mul(_InverseView, float4(ndc.x, ndc.y, ndc.z, 1));
                
                return worldPos;
            }

            FogInputs GetFogInputs(Varyings IN){
                Light light = GetMainLight();

                FogInputs _out;
                _out.worldPos = GetWorldPos(IN.pos, IN.uv); 
                _out.worldCameraPos = _WorldSpaceCameraPos;
                _out.depth01 = Linear01Depth(SampleDepth(IN.uv), _ZBufferParams);
                _out.sunDir = light.direction;
                _out.sunColor = half4(light.color,1);
                return _out;
            }

            TEXTURE2D(_FogGradientTex);
            SAMPLER(sampler_FogGradientTex);

            inline float4 _FogGradient(float t){
                return SAMPLE_TEXTURE2D(_FogGradientTex, sampler_FogGradientTex, float2(t, 0));
            }

            void CalcFogColor(inout FogOutput _fogOutput){
                if(_UseGradientFog){
                    if(_UseDepthAsGradientinterpolation > 0){
                        float gradientFogT = saturate(pow(_fogOutput.linear01Depth, _GradientFogPower) * _GradientFogScale);
                        _fogOutput.fogColor = _FogGradient(gradientFogT);
                    } else {
                        float gradientFogT = saturate(pow(_fogOutput.exponetialFog, _GradientFogPower) * _GradientFogScale);
                        _fogOutput.fogColor = _FogGradient(gradientFogT);
                    }
                }
            }
            
            Varyings Vert(Attributes IN){
                Varyings OUT = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.pos = IN.positionOS.xyz;
                //OUT.uv = IN.uv;
                return OUT;
            }
            
            float4 Frag(Varyings IN) : SV_Target{
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                half4 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                FogInputs _fogInput = GetFogInputs(IN);

                FogOutput _fogOutput = CalcFog(_fogInput);
                CalcFogColor(_fogOutput);
                CalcDirectionalInscattering(_fogOutput, _fogInput);
                return ApplyFogColor(_fogOutput, sceneColor);
            }
            ENDHLSL
        }
    }
}
