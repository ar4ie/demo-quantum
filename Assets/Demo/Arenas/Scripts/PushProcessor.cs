using Photon.Deterministic;
using Quantum;
using UnityEngine;

namespace Demo.Arenas
{
    public unsafe class PushProcessor : KCCProcessor, IAfterMoveStep
    {
        public FP Force = 1; 
        
        public void AfterMoveStep(KCCContext context, KCCProcessorInfo processorInfo, KCCOverlapInfo overlapInfo)
        {
            foreach (var hit in overlapInfo.ColliderHits)
            {
                if (context.Frame.Unsafe.TryGetPointer<PhysicsBody3D>(hit.PhysicsHit.Entity, out var body))
                {
                    var transform = context.Frame.Unsafe.GetPointer<Transform3D>(hit.PhysicsHit.Entity);
                    body->AddForce(Force * context.Frame.DeltaTime * -hit.PhysicsHit.Normal, relativePoint: hit.PhysicsHit.Point - transform->Position);
                }
                else if (context.Frame.Unsafe.TryGetPointer<KCC>(hit.PhysicsHit.Entity, out var kcc))
                {
                    kcc->AddExternalForce(Force * context.Frame.DeltaTime * -hit.PhysicsHit.Normal);
                }
            }
        }
    }
}