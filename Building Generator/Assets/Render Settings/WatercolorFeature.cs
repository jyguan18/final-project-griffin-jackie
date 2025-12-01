using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WatercolorFeature : ScriptableRendererFeature
{
    class WatercolorPass : ScriptableRenderPass
    {
        public Material material;
        private RenderTargetHandle tempTexture;

        public WatercolorPass(Material mat)
        {
            material = mat;
            tempTexture.Init("_TempWatercolorRT");
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            cmd.GetTemporaryRT(tempTexture.id, desc);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("WatercolorPass");

            var source = renderingData.cameraData.renderer.cameraColorTarget;

            // Set the texture explicitly for the shader
            cmd.SetGlobalTexture("_MainTex", source);

            cmd.SetGlobalTexture("_DepthTex", renderingData.cameraData.renderer.cameraDepthTarget);

            // Blit with the material
            cmd.Blit(source, tempTexture.Identifier(), material, 0);
            cmd.Blit(tempTexture.Identifier(), source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    } // WatercolorPass class ends here

    [System.Serializable]
    public class Settings
    {
        public Material watercolorMaterial;
    }

    public Settings settings = new Settings();
    WatercolorPass pass;

    public override void Create()
    {
        pass = new WatercolorPass(settings.watercolorMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}