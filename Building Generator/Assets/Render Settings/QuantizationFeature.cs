using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class QuantizationFeature : ScriptableRendererFeature
{
    class QuantizationPass : ScriptableRenderPass
    {
        public Material material;

        private RenderTargetIdentifier cameraColorTarget;
        private RenderTargetHandle tempTexture;

        public QuantizationPass(Material mat)
        {
            material = mat;
            tempTexture.Init("_QuantizationRT");
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
#if UNITY_2022_2_OR_NEWER
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
#else
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
#endif

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            cmd.GetTemporaryRT(tempTexture.id, desc);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("QuantizationPass");

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
        public Material quantizationMaterial;
    }

    public Settings settings = new Settings();
    QuantizationPass pass;

    public override void Create()
    {
        pass = new QuantizationPass(settings.quantizationMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}