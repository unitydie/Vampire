using Unity.Entities;
using Unity.Physics;

namespace Project.Sctipts
{
    public struct BuffsSystemFlag : IComponentData, IEnableableComponent
    {
            
    }

    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateBefore(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial struct BuffsSystem : ISystem
    {
        
    }

    public struct DamageUpJobSystem : ITriggerEventsJobBase
    {
        public void Execute(TriggerEvent triggerEvent)
        {
            throw new System.NotImplementedException();
        }
    }
}