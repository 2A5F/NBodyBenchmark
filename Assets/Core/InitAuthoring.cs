using Unity.Entities;
using UnityEngine;

namespace Core
{

    public class InitAuthoring : MonoBehaviour
    {
        [Min(0)]
        public int count = 10;
        [Min(0)]
        public float speedLimit = 299_792_458;
        [Min(0)]
        public int spaceSize = 100;
        [Min(0)]
        public float minWeight = 1;
        [Min(0)]
        public float maxWeight = 10;
        [Min(0)]
        public float minVelocity;
        [Min(0)]
        public float maxVelocity = 30;

        private class InitBaker : Baker<InitAuthoring>
        {
            public override void Bake(InitAuthoring authoring)
            {
                var e = GetEntity(TransformUsageFlags.None);
                AddComponent(e, new Init
                {
                    useSeed = false,
                    count = authoring.count,
                    speedLimit = authoring.speedLimit,
                    spaceSize = authoring.spaceSize,
                    minWeight = authoring.minWeight,
                    maxWeight = authoring.maxWeight,
                    minVelocity = authoring.minVelocity,
                    maxVelocity = authoring.maxVelocity,
                });
            }
        }
    }

}
