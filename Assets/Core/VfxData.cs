using Core.Gpu;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.VFX;

namespace Core
{
    
    public static class VfxData
    {
        public static VisualEffect vfx;
        public static ComputeShader bodyShader;
        public static int BodyShaderVelocity;
        public static int BodyShaderUpdate;
        
        public static void Init(VisualEffect vfx , ComputeShader bodyShader)
        {
            VfxData.vfx = vfx;
            VfxData.bodyShader = bodyShader;
            BodyShaderVelocity = bodyShader.FindKernel("velocity");
            BodyShaderUpdate = bodyShader.FindKernel("update");
        }
        
        public static NativeArray<VFXBodyData> buffer;
        public static GraphicsBuffer graphicsBuffer;
        
        public static void ReSet(ref Init init)
        {
            Stop();
            init.vfxBodyData = new NativeArray<VFXBodyData>(init.count, Allocator.TempJob);
            graphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, init.count,
                UnsafeUtility.SizeOf<VFXBodyData>());
            vfx.SetGraphicsBuffer(SBodyBuffer, graphicsBuffer);
        }
        
        private static readonly int SBodyBuffer = Shader.PropertyToID("BodyBuffer");
        private static readonly int SStart = Shader.PropertyToID("Start");
        
        public static void Start()
        {
            vfx.enabled = true;
            vfx.Play();
        }
        
        public static void Stop()
        {
            vfx.Stop();
            vfx.enabled = false;
            graphicsBuffer?.Dispose();
        }
    }
    
}
