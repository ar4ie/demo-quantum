using Quantum;
using UnityEngine;
using UnityEngine.Scripting;

namespace Demo.Arenas
{
    /// <summary>
    /// Manages player lifecycle and interaction.
    /// </summary>
    [Preserve]
    public unsafe class PlayerSystem : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlayerRemoved, ISignalPlayerFell, ISignalOnTriggerEnter3D
    {
        void ISignalOnPlayerAdded.OnPlayerAdded(Frame frame, PlayerRef playerRef, bool firstTime)
        {
            var runtimePlayer = frame.GetPlayerData(playerRef);
            var playerEntity = frame.Create(runtimePlayer.PlayerAvatar);
            frame.AddOrGet<PlayerLink>(playerEntity, out var playerLink);
            playerLink->PlayerRef = playerRef;
            RespawnPlayer(frame, playerEntity);
        }

        void ISignalOnPlayerRemoved.OnPlayerRemoved(Frame frame, PlayerRef playerRef)
        {
            foreach (var pair in frame.GetComponentIterator<PlayerLink>())
            {
                if (pair.Component.PlayerRef != playerRef)
                    continue;
                frame.Destroy(pair.Entity);
            }
        }

        void ISignalPlayerFell.PlayerFell(Frame frame, EntityRef entity)
        {
            RespawnPlayer(frame, entity);
        }

        private void RespawnPlayer(Frame frame, EntityRef entity)
        {
            var kcc = frame.Unsafe.GetPointer<KCC>(entity);
            var spawnPoint = frame.Unsafe.GetPointer<Transform3D>(frame.GetSingletonEntityRef<SpawnPoint>());
            kcc->Teleport(frame, spawnPoint->Position);
            kcc->SetLookRotation(spawnPoint->Rotation);
            kcc->Data.KinematicVelocity = default;
            var playerLink = frame.Unsafe.GetPointer<PlayerLink>(entity);
            frame.Events.ResetLookRotation(playerLink->PlayerRef, kcc->GetLookRotation());
        }

        void ISignalOnTriggerEnter3D.OnTriggerEnter3D(Frame frame, TriggerInfo3D info)
        {
            if (!frame.Unsafe.TryGetPointer<PlayerLink>(info.Other, out var playerLink))
                return;

            if (frame.Unsafe.TryGetPointer<Portal>(info.Entity, out var portal))
            {
                frame.Events.TeleportToArena(playerLink->PlayerRef, portal->ArenaName);
            }
        }
    }
}