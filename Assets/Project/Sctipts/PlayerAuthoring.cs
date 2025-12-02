using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UI;

namespace Project.Sctipts
{
    public struct CameraTarget : IComponentData
    {
        public UnityObjectRef<Transform> CameraTransform;
    }

    public struct InitCameraTargetTag : IComponentData
    {
        
    }

    public struct PlayerTag : IComponentData
    {
        
    }

    public struct PlayerAttackData : IComponentData
    {
        public Entity AttackPrefab;
        public float CoolDownTime;
        public float3 DetectionSize;
        public CollisionFilter CollisionFilter;
    }

    public struct PlayerCooldownExpirationTimestamp : IComponentData, IEnableableComponent
    {
        public double Value;
    }

    public struct GemCollectedCount : IComponentData
    {
        public int Value;
    }

    public struct PlayerLevel : IComponentData
    {
        public int Value;
    }

    public struct UpdateExpUIFlag : IComponentData, IEnableableComponent{}

    public struct UpdateGemUIFlag : IComponentData, IEnableableComponent { }

    public struct PlayerWorldUIPrefab : IComponentData
    {
        public UnityObjectRef<GameObject> Prefab;
    }

    public struct PlayerWorldUI : ICleanupComponentData
    {
        public UnityObjectRef<Transform> CanvasTransform;
        public UnityObjectRef<Slider> HealthBarSlider;
        public UnityObjectRef<Slider> ExpSlider;
    }

    public class PlayerAuthoring : MonoBehaviour
    {
        public GameObject AttackPrefab;
        public float CoolDownTime;
        public float DetectionSize;
        public GameObject UIPrefab;
        public int PlayerLevelUp;
        
        private class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PlayerTag>(entity);
                AddComponent<InitCameraTargetTag>(entity);
                AddComponent<CameraTarget>(entity);
                
                var enemyLayer = LayerMask.NameToLayer("Enemy");
                var enemyLayerMask = (uint)math.pow(2, enemyLayer);
                var attackCollisionFilter = new CollisionFilter
                {
                    BelongsTo = uint.MaxValue,
                    CollidesWith = enemyLayerMask
                };
                
                AddComponent(entity, new PlayerAttackData
                {
                    AttackPrefab = GetEntity(authoring.AttackPrefab, TransformUsageFlags.Dynamic),
                    CoolDownTime = authoring.CoolDownTime,
                    DetectionSize = new float3(authoring.DetectionSize),
                    CollisionFilter = attackCollisionFilter
                });
                AddComponent<PlayerCooldownExpirationTimestamp>(entity);
                AddComponent<GemCollectedCount>(entity);
                AddComponent<UpdateGemUIFlag>(entity);
                AddComponent(entity, new PlayerWorldUIPrefab
                {
                    Prefab = authoring.UIPrefab,
                });
                AddComponent(entity, new PlayerLevel
                {
                    Value = authoring.PlayerLevelUp
                });
                AddComponent<UpdateExpUIFlag>(entity);
                SetComponentEnabled<UpdateExpUIFlag>(entity, false);
            }
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CameraInitSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<InitCameraTargetTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (CameraTargetSingleton.Instance == null)
            {
                return; 
            }
            
            var cameraTargetTransform = CameraTargetSingleton.Instance.transform;
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            foreach (var (cameraTarget, entity) in SystemAPI.Query<RefRW<CameraTarget>>().WithAll<InitCameraTargetTag, 
                         PlayerTag>().WithEntityAccess())
            {
                cameraTarget.ValueRW.CameraTransform =  cameraTargetTransform;
                ecb.RemoveComponent<InitCameraTargetTag>(entity);
            }
            
            ecb.Playback(state.EntityManager);
        }
    }

    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial struct MoveCameraSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (transfrom, cameraTarget) in SystemAPI.Query<LocalToWorld, CameraTarget>().
                         WithAll<PlayerTag>().WithNone<InitCameraTargetTag>())
            {
                cameraTarget.CameraTransform.Value.position = transfrom.Position;
            }
        }
    }

    public partial class InputSystem : SystemBase
    {
        private PlayerInputSystem _inputSystem;
        
        protected override void OnCreate()
        {
            _inputSystem = new PlayerInputSystem();
            _inputSystem.Enable();
        }
        
        protected override void OnUpdate()
        {
            var currentInput = (float2)_inputSystem.Player.Move.ReadValue<Vector2>();
            foreach (var direction in SystemAPI.Query<RefRW<CharacterMoveDirection>>().WithAll<PlayerTag>())
            {
                direction.ValueRW.Value = currentInput;
            }
        }
    }
    
    public partial struct PlayerAttackSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }
        public void OnUpdate(ref SystemState state)
        {
            var elapsedTime = SystemAPI.Time.ElapsedTime;
            var ecbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            var physicsWorldSingelton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            
            foreach (var (expirationTimestamp, attackData, transform) in SystemAPI.Query<RefRW<PlayerCooldownExpirationTimestamp>, PlayerAttackData, LocalTransform>())
            {
                if (expirationTimestamp.ValueRO.Value > elapsedTime) continue;
                
                var spawnPostion = transform.Position;
                var minDetectPosition = spawnPostion - attackData.DetectionSize;
                var maxDetectPosition = spawnPostion + attackData.DetectionSize;

                var aabbInput = new OverlapAabbInput()
                {
                    Aabb = new Aabb()
                    {
                        Min = minDetectPosition,
                        Max = maxDetectPosition
                    },
                    
                    Filter = attackData.CollisionFilter
                };

                var overlapHits = new NativeList<int>(state.WorldUpdateAllocator);
                if (!physicsWorldSingelton.OverlapAabb(aabbInput, ref overlapHits))
                {
                    continue;
                }
                
                var maxDistanceSq = float.MaxValue;
                var closestEnemyPosition = float3.zero;
                
                foreach (var overlapHit in overlapHits)
                {
                    var curEnemyPosition = physicsWorldSingelton.Bodies[overlapHit].WorldFromBody.pos;
                    var distanceToPlayerSq = math.distance(spawnPostion.xy, curEnemyPosition.xy);
                    if (distanceToPlayerSq < maxDistanceSq)
                    {
                        maxDistanceSq = distanceToPlayerSq;
                        closestEnemyPosition = curEnemyPosition;
                    }
                }
                var vectorToClosestEnemy = closestEnemyPosition - spawnPostion;
                var angleToClosestEnemy = math.atan2(vectorToClosestEnemy.y, vectorToClosestEnemy.x);
                var spawnOrientation = quaternion.Euler(0, 0, angleToClosestEnemy);
                var newAttack = ecb.Instantiate(attackData.AttackPrefab);
                ecb.SetComponent(newAttack, LocalTransform.FromPositionRotation(spawnPostion, spawnOrientation));
                expirationTimestamp.ValueRW.Value = elapsedTime + attackData.CoolDownTime;
            }
        }
    }

    public partial struct UpdateGemUISystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (gemCount, shouldUpdateUI) in SystemAPI
                         .Query<GemCollectedCount, EnabledRefRW<UpdateGemUIFlag>>())
            {
                GameUiController.Instance.GemCounter(gemCount.Value);
                shouldUpdateUI.ValueRW = false;
            }
        }
    }

    public partial struct PlayerWorldUISystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            
            foreach (var (uiPrefab, entity) in SystemAPI
                         .Query<PlayerWorldUIPrefab>().WithNone<PlayerWorldUI>().WithEntityAccess())
            {
                var newWorldUI = Object.Instantiate(uiPrefab.Prefab.Value);
                var sliders = newWorldUI.GetComponentsInChildren<Slider>();

                ecb.AddComponent(entity, new PlayerWorldUI
                {
                    CanvasTransform = newWorldUI.transform,
                    HealthBarSlider = sliders[0],
                    ExpSlider = sliders[1],
                });
            }

            foreach (var (transform, worldUI, currentHitPoints, characterMaxHitPoints) in SystemAPI
                         .Query<LocalToWorld, PlayerWorldUI, CharacterCurrentHitPoints, CharacterMaxHitPoints>())
            {
                worldUI.CanvasTransform.Value.position = transform.Position;
                var healthValue = (float)currentHitPoints.Value / characterMaxHitPoints.Value;
                worldUI.HealthBarSlider.Value.value = healthValue;
            }
            
            const float gemsPerLevel = 10f;

            foreach (var (worldUI, gemCount, expFlag) in SystemAPI
                         .Query<PlayerWorldUI, GemCollectedCount, EnabledRefRW<UpdateExpUIFlag>>())
            {
                var exp01 = math.saturate(gemCount.Value / gemsPerLevel);
                worldUI.ExpSlider.Value.value = exp01;

                expFlag.ValueRW = false;
            }
            

            foreach (var (worldUI, entity) in SystemAPI.Query<PlayerWorldUI>().WithNone<LocalToWorld>().WithEntityAccess())
            {
                if (worldUI.CanvasTransform.Value != null)
                {
                    Object.Destroy(worldUI.CanvasTransform.Value.gameObject);
                }
                
                ecb.RemoveComponent<PlayerWorldUI>(entity);
            }
            
            ecb.Playback(state.EntityManager);
        }
    }
}