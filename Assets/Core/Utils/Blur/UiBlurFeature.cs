using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Core.Utils
{

    public class UiBlurFeature : ScriptableRendererFeature
    {
        public RenderTexture renderTexture;

        [Space]
        public bool outputToScreen;

        [Space]
        public UiBlurConfig config;

        private AUiBlurRenderPass _pass;

        public override void Create()
        {
            _pass = config.method switch
            {
                UiBlurMethod.GaussianBlur => new UiGaussianBlurRenderPass(this, config.gaussianBlurConfig),
                UiBlurMethod.BoxBlur => new UiBoxBlurRenderPass(this, config.boxBlurConfig),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game) return;

            renderer.EnqueuePass(_pass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game) return;

            _pass.ConfigureInput(ScriptableRenderPassInput.Color);
            _pass.SetTarget(renderer.cameraColorTargetHandle);
        }
    }

}
