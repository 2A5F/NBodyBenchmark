using Latios.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Core
{

    [BurstCompile]
    public partial struct BodySystem : ISystem
    {
        private EntityQuery query;
        private ComponentTypeHandle<Body> bodyType;
        private ComponentTypeHandle<BodyLastPos> bodyLastPosType;
        private ComponentTypeHandle<BodyVelocity> bodyVelocityType;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Init>();
            bodyType = state.GetComponentTypeHandle<Body>();
            bodyLastPosType = state.GetComponentTypeHandle<BodyLastPos>();
            bodyVelocityType = state.GetComponentTypeHandle<BodyVelocity>();
            query = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<Body>()
                .WithAll<BodyLastPos>()
                .Build(ref state);
            state.RequireForUpdate(query);
            state.RequireForUpdate(
                new EntityQueryBuilder(Allocator.Temp).WithAll<Init>().WithAll<Inited>().Build(ref state)
            );
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            if (Time.timeScale <= 0) return;
            var delta = math.min(Time.deltaTime, 1);
            var init = SystemAPI.GetSingleton<Init>();

            bodyType.Update(ref state);
            bodyLastPosType.Update(ref state);
            bodyVelocityType.Update(ref state);

            using var chunks = query.ToArchetypeChunkArray(Allocator.TempJob);

            using var indexes = new NativeList<int2>(init.count, Allocator.TempJob);
            new BodyInitIndexesJob() { indexes = indexes.AsParallelWriter(), chunks = chunks }
                .Schedule(chunks.Length, 1).Complete();

            var velocity_update_job = new BodyVelocityUpdateJob
            {
                delta = delta,
                speedLimit = init.speedLimit,
                indexes = indexes,
                chunks = chunks,
                bodyType = bodyType,
                bodyLastPosType = bodyLastPosType,
                bodyVelocityType = bodyVelocityType,
            };
            velocity_update_job.Schedule(indexes.Length, 1).Complete();

            new BodyPosUpdateJob { delta = delta }.ScheduleParallel();
            state.Dependency.Complete();
        }
    }

    [BurstCompile]
    public struct BodyInitIndexesJob : IJobParallelFor
    {
        public NativeList<int2>.ParallelWriter indexes;
        [ReadOnly]
        public NativeArray<ArchetypeChunk> chunks;

        public void Execute(int index)
        {
            var chunk = chunks[index];
            for (var i = 0; i < chunk.Count; i++)
            {
                indexes.AddNoResize(new int2(index, i));
            }
        }
    }

    [BurstCompile]
    public unsafe struct BodyVelocityUpdateJob : IJobParallelFor
    {
        public float delta;
        public float speedLimit;
        [ReadOnly]
        public NativeList<int2> indexes;
        [NativeDisableParallelForRestriction]
        public NativeArray<ArchetypeChunk> chunks;
        [ReadOnly]
        public ComponentTypeHandle<Body> bodyType;
        [ReadOnly]
        public ComponentTypeHandle<BodyLastPos> bodyLastPosType;
        public ComponentTypeHandle<BodyVelocity> bodyVelocityType;

        public void Execute(int index)
        {
            var selfChunk = chunks[indexes[index].x];
            var selfIndex = indexes[index].y;
            ref readonly var selfDef =
                ref ((Body*)selfChunk.GetComponentDataPtrRO(ref bodyType))[selfIndex];
            var selfLastPos =
                ((BodyLastPos*)selfChunk.GetComponentDataPtrRO(ref bodyLastPosType))[selfIndex].lastPos;
            ref var selfVelocity =
                ref ((BodyVelocity*)selfChunk.GetComponentDataPtrRW(ref bodyVelocityType))[selfIndex];

            for (var i = 0; i < indexes.Length; i++)
            {
                if (i == index) continue;
                var idx = indexes[i];
                var chunk = chunks[idx.x];
                var bodies = (Body*)chunk.GetComponentDataPtrRO(ref bodyType);
                var lastPoses = (BodyLastPos*)chunk.GetComponentDataPtrRO(ref bodyLastPosType);

                var size = bodies[idx.y].size;
                var weight = bodies[idx.y].weight;
                var lastPos = lastPoses[idx.y].lastPos;

                var distance = math.distancesq(lastPos, selfLastPos);
                if (distance < math.pow(math.max(size, selfDef.size), 2)) continue;
                var force = weight / distance;
                var direct = math.normalize(lastPos - selfLastPos);
                var velocity = direct * force * delta;

                selfVelocity.velocity += velocity;

                // see Vector3.ClampMagnitude()
                var len_sq = math.lengthsq(selfVelocity.velocity);
                if (len_sq > speedLimit * speedLimit)
                {
                    selfVelocity.velocity = selfVelocity.velocity / math.sqrt(len_sq) * speedLimit;
                }
            }
        }
    }

    [BurstCompile]
    public partial struct BodyPosUpdateJob : IJobEntity
    {
        public float delta;

        private void Execute(
            ref WorldTransform wt, ref BodyLastPos lastPos, in BodyVelocity velocity)
        {
            lastPos.lastPos = wt.position;
            var np = wt.position + velocity.velocity * delta;
            wt.worldTransform.position = np;
        }
    }

}
