using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Core
{

    [BurstCompile]
    public partial struct InitSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate(
                new EntityQueryBuilder(Allocator.Temp).WithAll<Init>().WithNone<Inited>().Build(ref state)
            );
            state.RequireForUpdate<InitPrefab>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var prefab = SystemAPI.GetSingleton<InitPrefab>();
            var initEntity = SystemAPI.GetSingletonEntity<Init>();
            var init = SystemAPI.GetComponent<Init>(initEntity);
            
            using var ecb = new EntityCommandBuffer(Allocator.TempJob);

            var seed = init.useSeed ? init.seed : (uint)(Time.unscaledTimeAsDouble * 1000);
            
            var job = new InitJob
            {
                seed = seed,
                ecb = ecb.AsParallelWriter(),
                prefab = prefab,
                init = init,
            };
            var handle = job.Schedule(init.count, 1);
            handle.Complete();

            ecb.AddComponent<Inited>(initEntity);
            
            ecb.Playback(state.EntityManager);
        }
    }

    [BurstCompile]
    public struct InitJob : IJobParallelFor
    {
        public uint seed;
        public EntityCommandBuffer.ParallelWriter ecb;
        public InitPrefab prefab;
        public Init init;

        public void Execute(int index)
        {
            var rand = Random.CreateFromIndex(seed + (uint)index);

            var pos = rand.NextFloat3(-init.spaceSize, init.spaceSize);
            var weight = rand.NextFloat(init.minWeight, init.maxWeight);
            var color = rand.NextFloat3(0.5f, 2.5f);
            var velocity = math.normalize(rand.NextFloat3(-1, 1)) * rand.NextFloat(init.minVelocity, init.maxVelocity);

            var scale = math.pow(weight, 0.5f);

            var body = ecb.Instantiate(0, prefab.bodyPrefab);
            ecb.SetComponent(1, body, new LocalTransform
            {
                Position = pos,
                Scale = scale,
                Rotation = default,
            });
            ecb.SetComponent(1, body, new MaterialColor
            {
                Value = new float4(color, 1),
            });
            ecb.AddComponent(2, body, new Body
            {
                weight = weight,
                size = scale,
            });
            ecb.AddComponent(2, body, new BodyLastPos
            {
                lastPos = pos,
            });
            ecb.AddComponent(2, body, new BodyVelocity
            {
                velocity = velocity,
            });
        }
    }

}
