using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Core.Utils
{

    public class UiBoxBlurRenderPass : AUiBlurRenderPass
    {
        private readonly UiBoxBlurConfig config;

        private readonly float boxSizeHalf;
        private readonly Material material;

        private RenderTexture tmpTexture1;
        private RenderTexture tmpTexture2;

        public UiBoxBlurRenderPass(UiBlurFeature feature, UiBoxBlurConfig config) : base(feature)
        {
            this.config = config;
            boxSizeHalf = BurstFns.BoxesForGaussFirst(config.radius, 3);
            // material = new Material(Shader.Find("Hidden/UiBlur/BoxBlur"));
            material = new Material(StaticAssets.Get<Shader>("UiBlur.BoxBlur.shader"));
            profilingSampler = new ProfilingSampler(nameof(UiGaussianBlurRenderPass));
        }

        protected override void DoCameraSetup(CommandBuffer cmd, ref RenderingData renderingData,
            RenderTexture src, int width, int height)
        {
            int tmpWidth;
            int tmpHeight;
            if (config.scaling)
            {
                var scale = config.quality * 2.0f / boxSizeHalf;
                if (scale > 1) scale = 1;
                tmpWidth = Mathf.CeilToInt(width * scale);
                tmpHeight = Mathf.CeilToInt(height * scale);
            }
            else
            {
                tmpWidth = Mathf.CeilToInt(width * config.quality / 16f);
                tmpHeight = Mathf.CeilToInt(height * config.quality / 16f);
            }
            tmpTexture1 =
                RenderTexture.GetTemporary(tmpWidth, tmpHeight, 0, src.format, RenderTextureReadWrite.Default);
            tmpTexture2 =
                RenderTexture.GetTemporary(tmpWidth, tmpHeight, 0, src.format, RenderTextureReadWrite.Default);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            RenderTexture.ReleaseTemporary(tmpTexture1);
            RenderTexture.ReleaseTemporary(tmpTexture2);
        }

        protected override void DoExecute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, profilingSampler))
            {
                cmd.SetGlobalInt(QualityId, config.quality);
                cmd.SetGlobalFloat(RadiusID, boxSizeHalf);
                if (config.scaling) cmd.EnableKeyword(material, new LocalKeyword(material.shader, "scaling"));
                else cmd.DisableKeyword(material, new LocalKeyword(material.shader, "scaling"));

                CopyTexture(cmd, cameraColorTarget, tmpTexture1, material, 0);
                CopyTexture(cmd, tmpTexture1, tmpTexture2, material, 0);
                CopyTexture(cmd, tmpTexture2, OutputTarget, material, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        private static readonly int RadiusID = Shader.PropertyToID("_radius");
        private static readonly int QualityId = Shader.PropertyToID("_quality");

        [BurstCompile]
        private static class BurstFns
        {
            private static void BoxesForGauss(float sigma, int n, out float m, out float wl, out float wu)
            {
                var wIdeal = math.sqrt(12 * sigma * sigma / n + 1);
                wl = math.floor(wIdeal);
                if (wl % 2 == 0) wl--;
                wu = wl + 2;

                var mIdeal = (12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
                m = math.round(mIdeal);
            }

            [BurstCompile]
            public static float BoxesForGaussFirst(float sigma, int n)
            {
                BoxesForGauss(sigma, n, out var m, out var wl, out var wu);
                return 0 < m ? wl : wu;
            }
        }
    }

}
