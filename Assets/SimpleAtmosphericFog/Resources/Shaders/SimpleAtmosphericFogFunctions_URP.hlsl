#ifndef URPSimpleAtmosphericFogFunctions_URP
#define URPSimpleAtmosphericFogFunctions_URP

#include "SimpleAtmosphericFogFunctions.hlsl"

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

void URPApplyFog_float(float4 sceneColor, float3 worldPos, float depth01, out float4 Out){
    #if SHADERGRAPH_PREVIEW
    Out = float4(1,1,1,1);
    #else
    Light light = GetMainLight();

    FogInputs _fogInput;
    _fogInput.worldPos = worldPos;
    _fogInput.worldCameraPos = _WorldSpaceCameraPos;
    _fogInput.depth01 = depth01;
    _fogInput.sunDir = light.direction;
    _fogInput.sunColor = half4(light.color,1);

    FogOutput _fogOutput = CalcFog(_fogInput);
    CalcFogColor(_fogOutput);
    CalcDirectionalInscattering(_fogOutput, _fogInput);
    Out = ApplyFogColor2(_fogOutput, sceneColor);
    #endif
}
#endif