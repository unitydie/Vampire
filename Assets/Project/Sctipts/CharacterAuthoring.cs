using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace Project.Sctipts
{
    public struct InitializeCharacterFlag : IComponentData, IEnableableComponent
    {
        
    }
    
    public struct CharacterMoveDirection : IComponentData
    {
        public float2 Value;
    }

    public struct CharacterMaxHitPoints : IComponentData
    {
        public int Value;
    }

    public struct CharacterCurrentHitPoints : IComponentData
    {
        public int Value;
    }

    public struct DamageThisFrame : IBufferElementData
    {
        public int Value;
    }

    public struct CharacterMoveSpeed : IComponentData
    {
        public float Value;
    }

    public class CharacterAuthoring : MonoBehaviour
    {
        public float MoveSpeed;
        public int HitPoints;
        
        private class Baker : Baker<CharacterAuthoring>
        {
            public override void Bake(CharacterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<InitializeCharacterFlag>(entity);
                AddComponent<CharacterMoveDirection>(entity);
                AddComponent(entity, new CharacterMoveSpeed
                {
                    Value = authoring.MoveSpeed
                });
                
                AddComponent(entity, new CharacterMaxHitPoints
                {
                    Value = authoring.HitPoints
                });
                AddComponent(entity, new CharacterCurrentHitPoints
                {
                    Value = authoring.HitPoints
                });
                
                AddBuffer<DamageThisFrame>(entity);
                AddComponent<DestroyEntityFlag>(entity);
                SetComponentEnabled<DestroyEntityFlag>(entity, false);
            }
        }
    }
        
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CharacterInitSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (mass, shouldInit) in SystemAPI.Query<RefRW<PhysicsMass>, EnabledRefRW<InitializeCharacterFlag>>())
            {
                mass.ValueRW.InverseInertia = float3.zero;
                shouldInit.ValueRW = false;
            }
        }
    }

    public partial struct CharacterMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (velocity, direction, speed) in SystemAPI.Query<RefRW<PhysicsVelocity>, CharacterMoveDirection, CharacterMoveSpeed>())
            {
                var moveStep2d = direction.Value * speed.Value;
                velocity.ValueRW.Linear = new float3(moveStep2d, 0f);
            }
        }
    }

    public partial struct ProcessDamageThisFrame : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (hitPoints, damageThisFrames, entity) in SystemAPI.Query<RefRW<CharacterCurrentHitPoints>, 
                         DynamicBuffer<DamageThisFrame>>().WithPresent<DestroyEntityFlag>().WithEntityAccess())
            {
                if (damageThisFrames.IsEmpty) continue;
                
                foreach (var damage in damageThisFrames)
                {
                    hitPoints.ValueRW.Value -= damage.Value;
                }
                
                damageThisFrames.Clear();

                if (hitPoints.ValueRO.Value <= 0)
                {
                    SystemAPI.SetComponentEnabled<DestroyEntityFlag>(entity, true);
                }
            }
        }
    }
}