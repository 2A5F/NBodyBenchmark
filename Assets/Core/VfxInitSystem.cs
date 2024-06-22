using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Core
{
    
    [UpdateAfter(typeof(InitSystem))]
    public partial struct VfxInitSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Init>();
            state.RequireForUpdate(
                new EntityQueryBuilder(Allocator.Temp).WithAll<Init, VfxInited>().WithNone<Inited>().Build(ref state)
            );
        }
        
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            var initEntity = SystemAPI.GetSingletonEntity<Init>();
            var init = SystemAPI.GetComponentRW<Init>(initEntity);
            
            var vfxBodyData = init.ValueRO.vfxBodyData;
            init.ValueRW.vfxBodyData = default;
            
            VfxData.graphicsBuffer.SetData(vfxBodyData);
            VfxData.Start();
            
            vfxBodyData.Dispose();
            
            ecb.AddComponent<Inited>(initEntity);
            
            ecb.Playback(state.EntityManager);
        }
        
    }
    
}
