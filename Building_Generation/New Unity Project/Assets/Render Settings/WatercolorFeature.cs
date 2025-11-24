using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WatercolorFeature : ScriptableRendererFeature
{
    class WatercolorPass : ScriptableRenderPass
    {
        public Material material;

        private RenderTargetIdentifier cameraColorTarget;
        private RenderTargetHandle tempTexture;

        public WatercolorPass(Material mat)
        {
            material = mat;
            tempTexture.Init("_TempWatercolorRT");
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Correct way to get camera color target
#if UNITY_2022_2_OR_NEWER
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
#else
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
#endif

            // Allocate temp RT with same descriptor as camera
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            cmd.GetTemporaryRT(tempTexture.id, desc);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("WatercolorPass");

            // Blit camera → temp
            Blit(cmd, cameraColorTarget, tempTexture.Identifier(), material, 0);

            // Blit temp → camera
            Blit(cmd, tempTexture.Identifier(), cameraColorTarget);


            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }

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
