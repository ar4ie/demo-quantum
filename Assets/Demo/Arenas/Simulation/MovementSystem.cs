using Photon.Deterministic;
using Quantum;
using UnityEngine.Scripting;

namespace Demo.Arenas
{
    /// <summary>
    /// Controls player movement.
    /// </summary>
    [Preserve]
    public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>
    {
        public struct Filter
        {
            public EntityRef    Entity;
            public PlayerLink*  PlayerLink;
            public Transform3D* Transform;
            public Movement*    Movement;
            public KCC*         KCC;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (filter.Transform->Position.Y < -15)
            {
                frame.Signals.PlayerFell(filter.Entity);
                return;
            }

            var input = frame.GetPlayerInput(filter.PlayerLink->PlayerRef);
            var movement = filter.Movement;
            var kcc = filter.KCC;

            var moveDirection = FPQuaternion.Euler(0, input->LookRotation.Y, 0) * new FPVector3(input->MoveDirection.X, 0, input->MoveDirection.Y);
            if (moveDirection != default)
            {
                var currentRotation = kcc->Data.TransformRotation;
                var targetRotation = FPQuaternion.LookRotation(moveDirection);
                var nextRotation = FPQuaternion.Lerp(currentRotation, targetRotation, movement->LookRotationSpeed * frame.DeltaTime);
                kcc->SetLookRotation(nextRotation);
            }
            kcc->SetInputDirection(moveDirection);

            if (input->Jump.WasPressed && kcc->IsGrounded)
            {
                kcc->Jump(FPVector3.Up * filter.Movement->JumpForce);
            }
        }
    }
}
