using Unity.Entities;
using UnityEngine;

namespace Core
{

    public class InitPrefabAuthoring : MonoBehaviour
    {
        public GameObject bodyPrefab;
        
        private class InitBaker : Baker<InitPrefabAuthoring>
        {
            public override void Bake(InitPrefabAuthoring authoring)
            {
                AddComponent(new InitPrefab
                {
                    bodyPrefab = GetEntity(authoring.bodyPrefab),
                });
            }
        }
    }

}
