using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Project.Sctipts
{
    public struct BulletAuthoringData : IComponentData
    {
        public float MoveSpeed;
        public int AttackDamage;
    }
    
    public class BulletAuthoring : MonoBehaviour
    {
        public float MoveSpeed;
        public int AttackDamage;

        private class Baker : Baker<BulletAuthoring>
        {
            public override void Bake(BulletAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new BulletAuthoringData
                {
                    MoveSpeed = authoring.MoveSpeed,
                    AttackDamage = authoring.AttackDamage
                });
                AddComponent<DestroyEntityFlag>(entity);
                SetComponentEnabled<DestroyEntityFlag>(entity, false);
            }
        }
    }

    public partial struct MoveBulletSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (transfrom, data) in SystemAPI.Query<RefRW<LocalTransform>, BulletAuthoringData>())
            {
                transfrom.ValueRW.Position += transfrom.ValueRW.Right() * data.MoveSpeed * deltaTime;
            }
        }
    }

    [UpdateInGroup(typeof(PhysicsSystemGroup))]
    [UpdateAfter(typeof(PhysicsSimulationGroup))]
    [UpdateBefore(typeof(AfterPhysicsSystemGroup))]
    public partial struct BulletAttackSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var attackJob = new BulletAttackJob
            {
                BulletDataLookup = SystemAPI.GetComponentLookup<BulletAuthoringData>(true),
                EnemyLookup = SystemAPI.GetComponentLookup<EnemyTag>(true),
                DamageBufferLookup = SystemAPI.GetBufferLookup<DamageThisFrame>(),
                DestroyEntityFlagLookup =  SystemAPI.GetComponentLookup<DestroyEntityFlag>()
            };
            
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            state.Dependency = attackJob.Schedule(simulationSingleton, state.Dependency);
        }
    }

    public struct BulletAttackJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<BulletAuthoringData>  BulletDataLookup;
        [ReadOnly] public ComponentLookup<EnemyTag> EnemyLookup;
        public BufferLookup<DamageThisFrame> DamageBufferLookup;
        public ComponentLookup<DestroyEntityFlag> DestroyEntityFlagLookup;
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity bulletEntity;
            Entity enemyEntity;

            if (BulletDataLookup.HasComponent(triggerEvent.EntityA) && EnemyLookup.HasComponent(triggerEvent.EntityB))
            {
                bulletEntity = triggerEvent.EntityA;
                enemyEntity =  triggerEvent.EntityB;
            }
            else if (BulletDataLookup.HasComponent(triggerEvent.EntityB) && EnemyLookup.HasComponent(triggerEvent.EntityA))
            {
                bulletEntity = triggerEvent.EntityB;
                enemyEntity =  triggerEvent.EntityA;
            }
            else
            {
                return;
            }

            var attackDamage = BulletDataLookup[bulletEntity].AttackDamage;
            var enemyDamageBuffer = DamageBufferLookup[enemyEntity];
            enemyDamageBuffer.Add(new DamageThisFrame{Value =  attackDamage});
            
            DestroyEntityFlagLookup.SetComponentEnabled(bulletEntity, true);
        }
    }
}