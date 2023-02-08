using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Core
{

    [BurstCompile]
    public partial struct StopSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Stop>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            using var ecb = new EntityCommandBuffer(Allocator.TempJob);

            foreach (var (_, stopEntity) in SystemAPI.Query<RefRO<Stop>>().WithEntityAccess())
            {
                foreach (var (_, initEntity) in SystemAPI.Query<RefRO<Init>>().WithEntityAccess())
                {
                    ecb.DestroyEntity(initEntity);
                }
                new RemoveBodiesJob { ecb = ecb.AsParallelWriter() }.ScheduleParallel();
                state.Dependency.Complete();
                ecb.DestroyEntity(stopEntity);
            }

            var first_re_init = true;
            foreach (var (re, e) in SystemAPI.Query<RefRO<ReInit>>().WithEntityAccess())
            {
                if (first_re_init)
                {
                    first_re_init = false;
                    var ie = ecb.CreateEntity();
                    ecb.AddComponent(ie, re.ValueRO.init);
                }
                ecb.DestroyEntity(e);
            }

            ecb.Playback(state.EntityManager);
        }
    }

    [BurstCompile]
    [WithAll(typeof(Body))]
    public partial struct RemoveBodiesJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;

        private void Execute(Entity e)
        {
            ecb.DestroyEntity(0, e);
        }
    }

}
