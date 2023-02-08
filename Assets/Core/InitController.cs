using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using Random = Unity.Mathematics.Random;

namespace Core
{

    public class InitController : MonoBehaviour
    {
        public Transform cameraStart;
        public Transform mainCamera;
        
        public UIDocument document;

        [Header("默认参数")]
        [Min(0)]
        public int count = 100;
        [Min(0)]
        public float speedLimit = 299_792_458;
        [Min(0)]
        public int spaceSize = 1000;
        [Min(0)]
        public float minWeight = 10;
        [Min(0)]
        public float maxWeight = 100;
        [Min(0)]
        public float minVelocity;
        [Min(0)]
        public float maxVelocity = 10;

        private VisualElement root;
        private Button startBtn;
        private Button seedStartBtn;
        private Button stopBtn;
        private Button reGenSeedBtn;
        private Button reSetSpeedLimitBtn;
        private TextField seedInput;
        private IntegerField countInput;
        private FloatField speedLimitInput;
        private FloatField spaceSizeInput;
        private FloatField minWeightInput;
        private FloatField maxWeightInput;
        private FloatField minVelocityInput;
        private FloatField maxVelocityInput;

        private Init init;

        private void Start()
        {
            root = document.rootVisualElement;
            startBtn = root.Q<Button>("start-btn");
            seedStartBtn = root.Q<Button>("seed-start-btn");
            stopBtn = root.Q<Button>("stop-btn");
            reGenSeedBtn = root.Q<Button>("re-gen-seed-btn");
            reSetSpeedLimitBtn = root.Q<Button>("re-set-speed-limit-btn");
            seedInput = root.Q<TextField>("seed-input");
            countInput = root.Q<IntegerField>("count-input");
            speedLimitInput = root.Q<FloatField>("speed-limit-input");
            spaceSizeInput = root.Q<FloatField>("space-size-input");
            minWeightInput = root.Q<FloatField>("min-weight-input");
            maxWeightInput = root.Q<FloatField>("max-weight-input");
            minVelocityInput = root.Q<FloatField>("min-velocity-input");
            maxVelocityInput = root.Q<FloatField>("max-velocity-input");

            startBtn.RegisterCallback<ClickEvent>(OnClickStartBtn);
            seedStartBtn.RegisterCallback<ClickEvent>(OnClickSeedStartBtn);
            stopBtn.RegisterCallback<ClickEvent>(OnClickStopBtn);
            reGenSeedBtn.RegisterCallback<ClickEvent>(OnClickReGenSeedBtn);
            reSetSpeedLimitBtn.RegisterCallback<ClickEvent>(OnClickReSetSpeedLimitBtn);

            seedInput.RegisterCallback<ChangeEvent<string>>(OnSeedChange);
            countInput.RegisterCallback<ChangeEvent<int>>(OnCountChange);
            speedLimitInput.RegisterCallback<ChangeEvent<float>>(OnSpeedLimitChange);
            spaceSizeInput.RegisterCallback<ChangeEvent<float>>(OnSpaceSizeChange);
            minWeightInput.RegisterCallback<ChangeEvent<float>>(OnMinWeightChange);
            maxWeightInput.RegisterCallback<ChangeEvent<float>>(OnMaxWeightChange);
            minVelocityInput.RegisterCallback<ChangeEvent<float>>(OnMinVelocityChange);
            maxVelocityInput.RegisterCallback<ChangeEvent<float>>(OnMaxVelocityChange);

            init = new Init
            {
                useSeed = true,
                count = count,
                speedLimit = speedLimit,
                spaceSize = spaceSize,
                minWeight = minWeight,
                maxWeight = maxWeight,
                minVelocity = minVelocity,
                maxVelocity = maxVelocity,
            };

            UpdateInputs();
            ReGenSeed();
            ReInit();
        }

        private void OnClickStartBtn(ClickEvent e)
        {
            ReGenSeed();
            ReInit();
        }

        private void OnClickSeedStartBtn(ClickEvent e)
        {
            ReInit();
        }

        private void OnClickStopBtn(ClickEvent e)
        {
            Stop();
        }

        private void OnClickReGenSeedBtn(ClickEvent e)
        {
            ReGenSeed();
        }

        private void OnClickReSetSpeedLimitBtn(ClickEvent e)
        {
            init.speedLimit = 299_792_458;
            UpdateInputs();
        }

        private void OnSeedChange(ChangeEvent<string> e)
        {
            init.seed = (uint)e.newValue.GetHashCode();
        }

        private void OnCountChange(ChangeEvent<int> e)
        {
            init.count = math.min(math.max(e.newValue, 0), 100_000);
            UpdateInputs();
        }

        private void OnSpeedLimitChange(ChangeEvent<float> e)
        {
            init.speedLimit = math.max(e.newValue, 0);
            UpdateInputs();
        }

        private void OnSpaceSizeChange(ChangeEvent<float> e)
        {
            init.spaceSize = math.max(e.newValue, 0);
            UpdateInputs();
        }

        private void OnMinWeightChange(ChangeEvent<float> e)
        {
            init.minWeight = math.max(e.newValue, 0.1f);
            UpdateInputs();
        }

        private void OnMaxWeightChange(ChangeEvent<float> e)
        {
            init.maxWeight = math.max(e.newValue, 0.1f);
            UpdateInputs();
        }

        private void OnMinVelocityChange(ChangeEvent<float> e)
        {
            init.minVelocity = math.max(e.newValue, 0f);
            UpdateInputs();
        }

        private void OnMaxVelocityChange(ChangeEvent<float> e)
        {
            init.maxVelocity = math.max(e.newValue, 0f);
            UpdateInputs();
        }

        private void UpdateInputs()
        {
            countInput.value = init.count;
            speedLimitInput.value = init.speedLimit;
            spaceSizeInput.value = init.spaceSize;
            minWeightInput.value = init.minWeight;
            maxWeightInput.value = init.maxWeight;
            minVelocityInput.value = init.minVelocity;
            maxVelocityInput.value = init.maxVelocity;
        }

        private void ReGenSeed()
        {
            var rand = new Random((uint)DateTimeOffset.Now.ToUnixTimeMilliseconds().GetHashCode());
            var seed = rand.NextUInt(uint.MaxValue).ToString();
            seedInput.value = seed;
            init.seed = (uint)seed.GetHashCode();
        }

        private void ReInit()
        {
            mainCamera.position = cameraStart.position;
            mainCamera.rotation = cameraStart.rotation;
            Hybrid.ReInit(init, World.DefaultGameObjectInjectionWorld.EntityManager);
        }

        private void Stop()
        {
            Hybrid.Stop(World.DefaultGameObjectInjectionWorld.EntityManager);
        }

        [BurstCompile]
        private static class Hybrid
        {
            [BurstCompile]
            public static void Stop(in EntityManager entityManager)
            {
                using var ecb = new EntityCommandBuffer(Allocator.TempJob);

                var e = ecb.CreateEntity();
                ecb.AddComponent<Stop>(e);

                ecb.Playback(entityManager);
            }

            [BurstCompile]
            public static void ReInit(in Init init, in EntityManager entityManager)
            {
                using var ecb = new EntityCommandBuffer(Allocator.TempJob);

                var e = ecb.CreateEntity();
                ecb.AddComponent<Stop>(e);

                e = ecb.CreateEntity();
                ecb.AddComponent(e, new ReInit { init = init });

                ecb.Playback(entityManager);
            }
        }
    }

}
