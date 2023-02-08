using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Core.Utils
{

    public abstract class AUiBlurRenderPass : ScriptableRenderPass
    {
        protected readonly UiBlurFeature feature;
        protected RTHandle cameraColorTarget;

        protected AUiBlurRenderPass(UiBlurFeature feature)
        {
            this.feature = feature;
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public void SetTarget(RTHandle colorHandle)
        {
            cameraColorTarget = colorHandle;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(cameraColorTarget);

            var src = (RenderTexture)cameraColorTarget;
            var width = src.width;
            var height = src.height;
            var renderTexture = feature.renderTexture;
            if (renderTexture.width != width || renderTexture.height != height)
            {
                if (renderTexture.IsCreated()) renderTexture.Release();
                renderTexture.width = width;
                renderTexture.height = height;
                renderTexture.format = src.format;
                renderTexture.Create();
            }
            else
            {
                if (!renderTexture.IsCreated()) renderTexture.Create();
            }

            DoCameraSetup(cmd, ref renderingData, src, width, height);
        }

        protected virtual void DoCameraSetup(CommandBuffer cmd, ref RenderingData renderingData, RenderTexture src,
            int width, int height) { }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            if (cameraData.camera.cameraType != CameraType.Game) return;

            DoExecute(context, ref renderingData);
        }

        protected abstract void DoExecute(ScriptableRenderContext context, ref RenderingData renderingData);

        protected RenderTargetIdentifier OutputTarget =>
            feature.outputToScreen ? cameraColorTarget : feature.renderTexture;

        protected static void CopyTexture(CommandBuffer cmd, RenderTexture source,
            RenderTargetIdentifier destination, Material material, int pass)
        {
            cmd.SetGlobalTexture(MainTextureID, source);
            cmd.SetRenderTarget(destination);
            DrawTriangle(cmd, material, pass);
        }

        protected static void DrawTriangle(CommandBuffer cmd, Material material, int shaderPass)
        {
            if (material == null)
            {
                Debug.LogError("material is null");
                return;
            }
            cmd.DrawProcedural(Matrix4x4.identity, material, shaderPass, MeshTopology.Triangles, 3, 1, SPropertyBlock);
        }

        public static readonly MaterialPropertyBlock SPropertyBlock = new();
        
        private static readonly int MainTextureID = Shader.PropertyToID("_MainTexture");
    }

}
