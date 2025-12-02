using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;


namespace Project.Sctipts
{
    public struct GemTag : IComponentData{}
    
    public class GemAuthoring : MonoBehaviour
    {
        private class Baker : Baker<GemAuthoring>
        {
            public override void Bake(GemAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<GemTag>(entity);
                AddComponent<DestroyEntityFlag>(entity);
                SetComponentEnabled<DestroyEntityFlag>(entity, false);
            }
        }
    }

    public partial struct CollectGemSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SimulationSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var newCollectedJob = new CollectGemJob
            {
                GemTagLookup = SystemAPI.GetComponentLookup<GemTag>(true),
                GemsCollectedCountLookup = SystemAPI.GetComponentLookup<GemCollectedCount>(),
                DestroyEntityFlagLookup = SystemAPI.GetComponentLookup<DestroyEntityFlag>(),
                UpdateGemUIFlagLookup = SystemAPI.GetComponentLookup<UpdateGemUIFlag>(),
                PlayerLevelLookup = SystemAPI.GetComponentLookup<PlayerLevel>(),
                UpdateExpUIFlagLookup = SystemAPI.GetComponentLookup<UpdateExpUIFlag>()
            };
            
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            state.Dependency = newCollectedJob.Schedule(simulationSingleton, state.Dependency);
        }
    }

    [BurstCompile] 
    public struct CollectGemJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<GemTag> GemTagLookup;
        public ComponentLookup<GemCollectedCount> GemsCollectedCountLookup;
        public ComponentLookup<DestroyEntityFlag> DestroyEntityFlagLookup;
        public ComponentLookup<UpdateGemUIFlag> UpdateGemUIFlagLookup;
        public ComponentLookup<UpdateExpUIFlag> UpdateExpUIFlagLookup;
        public ComponentLookup<PlayerLevel> PlayerLevelLookup;
        
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity gemEntity;
            Entity playerEntity;
            
            if (GemTagLookup.HasComponent(triggerEvent.EntityA) && GemsCollectedCountLookup.HasComponent(triggerEvent.EntityB))
            {
                gemEntity = triggerEvent.EntityA;
                playerEntity =  triggerEvent.EntityB;
            }
            else if (GemTagLookup.HasComponent(triggerEvent.EntityB) && GemsCollectedCountLookup.HasComponent(triggerEvent.EntityA))
            {
                gemEntity = triggerEvent.EntityB;
                playerEntity =  triggerEvent.EntityA;
            }
            else
            {
                return;
            }
            
            var gemsCollected = GemsCollectedCountLookup[playerEntity];
            gemsCollected.Value += 1;
            GemsCollectedCountLookup[playerEntity] = gemsCollected;
            
            var playerLevel = PlayerLevelLookup[playerEntity];
            playerLevel.Value += 1;                
            PlayerLevelLookup[playerEntity] = playerLevel;
            
            UpdateGemUIFlagLookup.SetComponentEnabled(playerEntity, true);
            UpdateExpUIFlagLookup.SetComponentEnabled(playerEntity, true);
            
            DestroyEntityFlagLookup.SetComponentEnabled(gemEntity, true);
        }
    }
}