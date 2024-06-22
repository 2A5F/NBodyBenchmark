using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Core.Utils
{
    
    public class DualKawaseBlurFeature : ScriptableRendererFeature
    {
        public RenderTexture renderTexture;
        
        [Space]
        [Min(0)]
        public float radius = 5;
        [Range(1, 10)]
        public int quality = 4;
        [Range(1, 10)]
        public float scale = 2;
        
        private DualKawaseBlurPass _pass;
        
        public override void Create()
        {
            _pass = new DualKawaseBlurPass(this);
            _pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }
        
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType != CameraType.Game) return;
            
            renderer.EnqueuePass(_pass);
        }
    }
    
}
