using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OG {
    public class SimpleAtmosphericFogRenderFeature : ScriptableRendererFeature {
        class CustomRenderPass : ScriptableRenderPass{
            
            static readonly int _FogDensity = Shader.PropertyToID("_FogDensity");
            static readonly int _FogPower = Shader.PropertyToID("_FogPower");
            static readonly int _SkyAlpha = Shader.PropertyToID("_SkyAlpha");
            static readonly int _FogColor = Shader.PropertyToID("_FogColor");

            static readonly int _FogGradientTex = Shader.PropertyToID("_FogGradientTex");
            static readonly int _UseGradientFog = Shader.PropertyToID("_UseGradientFog");
            static readonly int _UseDepthAsGradientinterpolation = Shader.PropertyToID("_UseDepthAsGradientinterpolation");
            static readonly int _GradientFogPower = Shader.PropertyToID("_GradientFogPower");
            static readonly int _GradientFogScale = Shader.PropertyToID("_GradientFogScale");

            static readonly int _UseHeight = Shader.PropertyToID("_UseHeight");
            static readonly int _BaseHeight = Shader.PropertyToID("_BaseHeight");
            static readonly int _MaxHeight = Shader.PropertyToID("_MaxHeight");
            static readonly int _HeightPower = Shader.PropertyToID("_HeightPower");
            
            static readonly int _SecondBaseHeight = Shader.PropertyToID("_SecondBaseHeight");
            static readonly int _SecondMaxHeight = Shader.PropertyToID("_SecondMaxHeight");
            static readonly int _SecondHeightPower = Shader.PropertyToID("_SecondHeightPower");

            static readonly int _SkyBoxBaseHeight = Shader.PropertyToID("_SkyBoxBaseHeight");
            static readonly int _SkyBoxMaxHeight = Shader.PropertyToID("_SkyBoxMaxHeight");
            static readonly int _SkyBoxHeightPower = Shader.PropertyToID("_SkyBoxHeightPower");
            static readonly int _SkyBoxHeightFill = Shader.PropertyToID("_SkyBoxHeightFill");

            static readonly int _DirectionalIntesity = Shader.PropertyToID("_DirectionalIntesity");
            static readonly int _UseMainLightColor = Shader.PropertyToID("_UseMainLightColor");
            static readonly int _DirectionalColor = Shader.PropertyToID("_DirectionalColor");
            static readonly int _DirectionalPower = Shader.PropertyToID("_DirectionalPower");

            public RenderTargetIdentifier source;

            RenderTargetHandle tempTexture;
            Material material;
            Texture2D fogGradient = null;

            public static Texture2D CreateGradentTex (Gradient grad, int width = 32, int height = 1) {
                    var gradTex = new Texture2D(width, height, TextureFormat.ARGB32, false);
                    gradTex.filterMode = FilterMode.Bilinear;
                    gradTex.wrapMode =  TextureWrapMode.Clamp;
                    float inv = 1f / (width - 1);
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            var t = x * inv;
                            Color col = grad.Evaluate(t);
                            gradTex.SetPixel(x, y, col);
                        }
                    }
                    gradTex.Apply();
                    return gradTex; 
            }

            public static void UpdateGradientTex (Texture2D tex, Gradient grad, int width = 32, int height = 1) {
                    var gradTex = tex;
                    float inv = 1f / (width - 1);
                    for (int y = 0; y < height; y++) {
                        for (int x = 0; x < width; x++) {
                            var t = x * inv;
                            Color col = grad.Evaluate(t);
                            gradTex.SetPixel(x, y, col);
                        }
                    }
                    gradTex.Apply();
            }

            public CustomRenderPass(bool useShaderGraphVersion = false){
                Shader shader = Shader.Find("OG/_SimpleAtmosphericFog");
                if(useShaderGraphVersion){
                    Shader s = Shader.Find("OG/SimpleAtmosphericFog");
                    if(s != null)
                        shader = s;
                }

                if(shader == null) return;
                material = CoreUtils.CreateEngineMaterial(shader);
            }

            public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor){
                cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData){
                if (!renderingData.cameraData.postProcessEnabled) return;
                var stack = VolumeManager.instance.stack;
                var simpleAtmosphericFog = stack.GetComponent<SimpleAtmosphericFog>();


                if (simpleAtmosphericFog == null) { return; }
                if (simpleAtmosphericFog.IsActive() == false) { return; }

                if (material == null) return;
                if(renderingData.cameraData.cameraType != CameraType.Game && renderingData.cameraData.cameraType != CameraType.SceneView) return;

                CommandBuffer cmd = CommandBufferPool.Get();
                cmd.Clear();


                if(simpleAtmosphericFog.useGradientFog.value){
                    if(fogGradient == null){
                        fogGradient = CreateGradentTex(simpleAtmosphericFog.gradientFog.value);
                        Shader.SetGlobalTexture(_FogGradientTex, fogGradient);
                    } else {
                        if(simpleAtmosphericFog.realtimeUpdateGradientFog.value)
                            UpdateGradientTex(fogGradient, simpleAtmosphericFog.gradientFog.value);

                        Shader.SetGlobalTexture(_FogGradientTex, fogGradient);
                    }
                }


                //Shader.SetGlobalMatrix("_InverseView", renderingData.cameraData.camera.cameraToWorldMatrix);
                Shader.SetGlobalMatrix("_InverseView", renderingData.cameraData.camera.cameraToWorldMatrix);
                
                Shader.SetGlobalFloat(_FogDensity, simpleAtmosphericFog.fogDensity.value);
                Shader.SetGlobalFloat(_FogPower, simpleAtmosphericFog.fogPower.value);
                Shader.SetGlobalFloat(_SkyAlpha, simpleAtmosphericFog.skyAlpha.value);
                Shader.SetGlobalColor(_FogColor, simpleAtmosphericFog.fogColor.value);

                Shader.SetGlobalFloat(_UseGradientFog, simpleAtmosphericFog.useGradientFog.value ? 1 : 0);
                Shader.SetGlobalFloat(_UseDepthAsGradientinterpolation, simpleAtmosphericFog.useDepthAsGradientInterpolation.value ? 1 : 0);
                Shader.SetGlobalFloat(_GradientFogPower, simpleAtmosphericFog.gradientFogPower.value);
                Shader.SetGlobalFloat(_GradientFogScale, simpleAtmosphericFog.gradientFogScale.value);

                Shader.SetGlobalFloat(_UseHeight, simpleAtmosphericFog.useHeightFog.value ? 1 : 0);
                Shader.SetGlobalFloat(_BaseHeight, simpleAtmosphericFog.baseHeight.value);
                Shader.SetGlobalFloat(_MaxHeight, simpleAtmosphericFog.maxHeight.value);
                Shader.SetGlobalFloat(_HeightPower, simpleAtmosphericFog.heightPower.value);

                Shader.SetGlobalFloat(_SecondBaseHeight, simpleAtmosphericFog.secondFogBaseHeight.value);
                Shader.SetGlobalFloat(_SecondMaxHeight, simpleAtmosphericFog.secondMaxHeight.value);
                Shader.SetGlobalFloat(_SecondHeightPower, simpleAtmosphericFog.secondFogHeightPower.value);

                Shader.SetGlobalFloat(_SkyBoxBaseHeight, simpleAtmosphericFog.skyboxFogBaseHeight.value);
                Shader.SetGlobalFloat(_SkyBoxMaxHeight, simpleAtmosphericFog.skyboxMaxHeight.value);
                Shader.SetGlobalFloat(_SkyBoxHeightPower, simpleAtmosphericFog.skyboxFogHeightPower.value);
                Shader.SetGlobalFloat(_SkyBoxHeightFill, simpleAtmosphericFog.skyboxHeightFill.value);

                Shader.SetGlobalFloat(_DirectionalIntesity, simpleAtmosphericFog.directionalIntesity.value);
                Shader.SetGlobalInt(_UseMainLightColor, simpleAtmosphericFog.useMainLightColor.value ? 1 : 0);
                Shader.SetGlobalColor(_DirectionalColor, simpleAtmosphericFog.directionalColor.value);
                Shader.SetGlobalFloat(_DirectionalPower, simpleAtmosphericFog.directionalPower.value);

                //

                cmd.Blit(source, tempTexture.Identifier(), material, 0);
                cmd.Blit(tempTexture.Identifier(), source);

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd){}
        }
        //

        [Tooltip("Shader Graph version is deprecated.")]
        public bool useShaderGraphVersion = false;
        [Tooltip("Change the render other to apply the for effect. Example: can use to make transparent materials render in front of the fog.")]
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        CustomRenderPass m_ScriptablePass;

        public override void Create(){
            m_ScriptablePass = new CustomRenderPass(useShaderGraphVersion);
            m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData){
            m_ScriptablePass.source = renderer.cameraColorTarget;
            m_ScriptablePass.renderPassEvent = renderPassEvent;

            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}
