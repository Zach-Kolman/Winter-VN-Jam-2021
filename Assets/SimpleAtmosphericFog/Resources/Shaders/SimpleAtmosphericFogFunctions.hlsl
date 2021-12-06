#ifndef URPSimpleAtmosphericFogFunctions
#define URPSimpleAtmosphericFogFunctions

//////////////////////////// Core Functions ////////////////////////////

float4 _Remap(float4 In, float2 InMinMax, float2 OutMinMax){
    return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
}

float HeightFog(float height, float baseHeight, float maxHeight, float2 power){
    float _in = clamp(height, baseHeight, baseHeight + maxHeight);
    float remaped = 1 - _Remap(_in, float2(baseHeight, baseHeight + maxHeight), float2(0,1));

    return pow(saturate(remaped), power);
}

float ExponetialFog(float3 worldPos, float3 camPos, float density, float power){
    float3 distance = length(worldPos - camPos);

    float fa = 1 - exp(-(distance * density));
    return pow(saturate(fa), power);
}

float DirectionalInscattering(float3 rayDir, float3 sunDir, float power, float intesity){
    float amout = max(dot(rayDir, sunDir) * intesity, 0.0);
    return pow(saturate(amout), power);
}

//////////////// Data ///////////////////

float _FogDensity;
float _FogPower;
float _SkyAlpha;
float4 _FogColor;

float _UseGradientFog;
float _UseDepthAsGradientinterpolation;
float _GradientFogPower;
float _GradientFogScale;

float4 _FogColorMiddle;
float4 _FogColorFar;

float _UseHeight;
float _BaseHeight;
float _MaxHeight;
float _HeightPower;

float _SecondBaseHeight;
float _SecondMaxHeight;
float _SecondHeightPower;

float _SkyBoxBaseHeight;
float _SkyBoxMaxHeight;
float _SkyBoxHeightPower;
float _SkyBoxHeightFill;

float _DirectionalIntesity;
float _UseMainLightColor;
float4 _DirectionalColor;
float _DirectionalPower;

struct FogInputs {
    float3 worldPos;
    float3 worldCameraPos;
    float depth01;
    float3 sunDir;
    float4 sunColor;
};

struct FogOutput {
    float3 rayDir;
    float linear01Depth;
    float skyMask;
    float exponetialFog;
    float finalFog;
    float skyboxFog;

    float4 fogColor;

    float4 directionalColor;
    float directionalInscattering;
};


//////////////////////////// Fog Calcs ////////////////////////////

FogOutput CalcFog(FogInputs inputs){
    FogOutput _out;
    _out.rayDir = normalize(inputs.worldPos - inputs.worldCameraPos);
    _out.linear01Depth = inputs.depth01;
    _out.skyMask = _out.linear01Depth > 0.97 ? 1 : 0;

    _out.exponetialFog = ExponetialFog(inputs.worldPos, inputs.worldCameraPos, _FogDensity, _FogPower);
    
    float heightFog = HeightFog(inputs.worldPos.y, _BaseHeight, _MaxHeight, _HeightPower);
    float secondHeightFog = HeightFog(inputs.worldPos.y, _SecondBaseHeight, _SecondMaxHeight, _SecondHeightPower);
    float finalHeightFog = saturate(heightFog + secondHeightFog);

    _out.finalFog = _out.exponetialFog;
    _out.skyboxFog = _out.exponetialFog;

    if(_UseHeight > 0){
        _out.finalFog = lerp(0, _out.exponetialFog, finalHeightFog);
        _out.skyboxFog = HeightFog(inputs.worldPos.y, _SkyBoxBaseHeight, _SkyBoxMaxHeight, _SkyBoxHeightPower);
        _out.skyboxFog = lerp(_out.exponetialFog, _out.skyboxFog, _SkyBoxHeightFill);
    }

    _out.fogColor = _FogColor;
    _out.directionalColor  = float4(0,0,0,0);
    _out.directionalInscattering = 0;

    return _out;
}

float4 ApplyFogColor(FogOutput fogOut, float4 sceneColor){
    float4 finalFogColor = lerp(fogOut.fogColor, fogOut.directionalColor, fogOut.directionalInscattering);
    
    float4 sceneFog = lerp(sceneColor, finalFogColor, fogOut.finalFog * finalFogColor.a);
    
    //float4 skyFog = lerp(sceneFog, sceneColor, settings.SkyAlpha);
    float4 skyFog = lerp(sceneColor, finalFogColor, fogOut.skyboxFog * finalFogColor.a);
    skyFog = lerp(skyFog, sceneColor, _SkyAlpha);

    return lerp(sceneFog, skyFog, fogOut.skyMask);
}

float4 ApplyFogColor2(FogOutput fogOut, float4 sceneColor){
    float4 finalFogColor = lerp(fogOut.fogColor, fogOut.directionalColor, fogOut.directionalInscattering);
    return lerp(sceneColor, finalFogColor, fogOut.finalFog * finalFogColor.a);
}

void CalcDirectionalInscattering(inout FogOutput _fogOutput, inout FogInputs _fogInput){
    _fogOutput.directionalColor = _UseMainLightColor > 0 ?  _fogInput.sunColor : _DirectionalColor;
    _fogOutput.directionalInscattering = DirectionalInscattering(_fogOutput.rayDir, _fogInput.sunDir, _DirectionalPower, _DirectionalIntesity);
}
#endif