using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Core.Utils
{

    public class UiGaussianBlurRenderPass : AUiBlurRenderPass
    {
        private readonly UIGaussianBlurConfig config;

        private readonly Material material;
        private float _width_mod;

        private RenderTexture tmpTexture1;
        private RenderTexture tmpTexture2;

        public UiGaussianBlurRenderPass(UiBlurFeature feature, UIGaussianBlurConfig config) : base(feature)
        {
            this.config = config;
            // material = new Material(Shader.Find("Hidden/UiBlur/GaussianBlur"));
            material = new Material(StaticAssets.Get<Shader>("UiBlur.GaussianBlur.shader"));
            profilingSampler = new ProfilingSampler(nameof(UiGaussianBlurRenderPass));
        }

        protected override void DoCameraSetup(CommandBuffer cmd, ref RenderingData renderingData,
            RenderTexture src, int width, int height)
        {
            _width_mod = 1.0f / (1.0f * (1 << config.downSamplingTimes));

            var tmp_width = width >> config.downSamplingTimes;
            var tmp_height = height >> config.downSamplingTimes;

            tmpTexture1 = RenderTexture.GetTemporary(tmp_width, tmp_height, 0, src.format);
            tmpTexture1.filterMode = FilterMode.Bilinear;
            tmpTexture2 = RenderTexture.GetTemporary(tmp_width, tmp_height, 0, src.format);
            tmpTexture2.filterMode = FilterMode.Bilinear;
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
                // 降采样
                CopyTexture(cmd, cameraColorTarget, tmpTexture1, material, 0);

                // 模糊
                for (var i = 0; i < config.iterations; i++)
                {
                    cmd.SetGlobalFloat(SpreadID, config.spread * _width_mod + i);

                    // 模糊 x
                    CopyTexture(cmd, tmpTexture1, tmpTexture2, material, 1);
                    // 模糊 y
                    CopyTexture(cmd, tmpTexture2, tmpTexture1, material, 2);
                }

                // 复制输出
                CopyTexture(cmd, tmpTexture1, OutputTarget, material, 3);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        private static readonly int SpreadID = Shader.PropertyToID("_spread");
    }

}
