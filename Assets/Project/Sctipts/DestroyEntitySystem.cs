using Unity.Entities;
using Unity.Transforms;

namespace Project.Sctipts
{
    public struct DestroyEntityFlag : IComponentData, IEnableableComponent
    {
        
    }
   
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial struct DestroyEntitySystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var endEcbSystem = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();
            var endEcb = endEcbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            var beginEcbSystem = SystemAPI.GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
            var beginEcb = beginEcbSystem.CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (_, entity) in SystemAPI.Query<DestroyEntityFlag>().WithEntityAccess())
            {
                // тут можно добавить если игрок умер, то вызываем GameOver с монобеха
                if (SystemAPI.HasComponent<GemPrefab>(entity))
                { 
                    var gemPrefab = SystemAPI.GetComponent<GemPrefab>(entity).Value;
                   var newGem = beginEcb.Instantiate(gemPrefab);
                   
                   var spawnPosition = SystemAPI.GetComponent<LocalToWorld>(entity).Position;
                   beginEcb.SetComponent(newGem, LocalTransform.FromPosition(spawnPosition));
                }
                endEcb.DestroyEntity(entity);
            }
        }
    }
}