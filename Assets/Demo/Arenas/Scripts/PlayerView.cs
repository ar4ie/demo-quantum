using Quantum;
using UnityEngine;

namespace Demo.Arenas
{
	/// <summary>
	/// Controls player animation and camera position.
	/// </summary>
	public class PlayerView : QuantumEntityViewComponent
	{
		private static readonly int AnimatorSpeed        = Animator.StringToHash("Speed");
		private static readonly int AnimatorGrounded     = Animator.StringToHash("Grounded");

		public PlayerInput Input;
		public Animator Animator;
		public Transform CameraPivot;
		public Transform CameraHandle;

		public override void OnUpdateView()
		{
			var kcc = GetPredictedQuantumComponent<KCC>();
			Animator.SetFloat(AnimatorSpeed, kcc.RealSpeed.AsFloat);
			Animator.SetBool(AnimatorGrounded, kcc.IsGrounded);
		}

		public override void OnLateUpdateView()
		{
			var playerLink = GetPredictedQuantumComponent<PlayerLink>();
			if (!Game.PlayerIsLocal(playerLink.PlayerRef))
				return;
			CameraPivot.rotation = Quaternion.Euler(Input.LookRotation);
			Camera.main.transform.SetPositionAndRotation(CameraHandle.position, CameraHandle.rotation);
		}
	}
}
