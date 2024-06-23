using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Core
{
    
    [UpdateAfter(typeof(VfxInitSystem))]
    public partial struct VfxBodySystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Init>();
            state.RequireForUpdate(
                new EntityQueryBuilder(Allocator.Temp).WithAll<Init, VfxInited, Inited>().Build(ref state)
            );
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var init = SystemAPI.GetSingleton<Init>();
            
            var shader = VfxData.bodyShader;
            var buffer = VfxData.graphicsBuffer;
            if (buffer == null || !shader) return;
            
            var velocity = VfxData.BodyShaderVelocity;
            var update = VfxData.BodyShaderUpdate;
            
            var groups = buffer.count / 1024;
            if (buffer.count % 1024 > 0) groups += 1;
            
            shader.SetFloat(Delta, math.min(Time.deltaTime, 1));
            shader.SetFloat(SpeedLimit, init.speedLimit);
            
            shader.SetBuffer(velocity, BodyBuffer, buffer);
            shader.Dispatch(velocity, groups, 1, 1);
            
            shader.SetBuffer(update, BodyBuffer, buffer);
            shader.Dispatch(update, groups, 1, 1);
        }
        
        private static readonly int BodyBuffer = Shader.PropertyToID("BodyBuffer");
        private static readonly int Delta = Shader.PropertyToID("delta");
        private static readonly int SpeedLimit = Shader.PropertyToID("speedLimit");
    }
    
}
