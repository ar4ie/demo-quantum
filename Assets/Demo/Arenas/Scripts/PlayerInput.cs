using Cysharp.Threading.Tasks;
using Demo.Mains;
using Photon.Deterministic;
using Quantum;
using UnityEngine;

namespace Demo.Arenas
{
	/// <summary>
	/// Collects player input.
	/// </summary>
	public class PlayerInput : QuantumEntityViewComponent
	{
		public Vector2 LookSensitivity = new (0.3f, 0.3f);
		public FPVector2 PitchClamp = new (-30, 70);
		public Vector2 LookRotation => _input.LookRotation.ToUnityVector2();
		private InputActions _inputActions;
		private Quantum.Input _input;

		public override void OnActivate(Frame frame)
		{
			var playerLink = GetPredictedQuantumComponent<PlayerLink>();
			if (!Game.PlayerIsLocal(playerLink.PlayerRef))
			{
				enabled = false;
				return;
			}

			QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
			QuantumEvent.Subscribe<EventResetLookRotation>(this, OnResetLookRotation);
			QuantumEvent.Subscribe<EventTeleportToArena>(this, OnTeleportToArena);

			_inputActions ??= new InputActions();
			_inputActions.Enable();
			SetCursorLocked(true);
		}

		public override void OnDeactivate()
		{
			_inputActions?.Disable();
			SetCursorLocked(false);
		}

		public override void OnUpdateView()
		{
			if (_inputActions.UI.Cancel.WasPerformedThisFrame())
			{
				SetCursorLocked(false);
			}
			if (_inputActions.UI.Click.WasPerformedThisFrame())
			{
				SetCursorLocked(true);
			}
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				_input.MoveDirection = default;
				return;
			}

			var lookValue = _inputActions.Player.Look.ReadValue<Vector2>() * LookSensitivity;
			var lookRotationDelta = new Vector2(-lookValue.y, lookValue.x);

			_input.LookRotation += lookRotationDelta.ToFPVector2();
			_input.LookRotation.X = FPMath.Clamp(_input.LookRotation.X, PitchClamp.X, PitchClamp.Y);
			_input.MoveDirection = _inputActions.Player.Move.ReadValue<Vector2>().ToFPVector2();
			_input.Jump = _inputActions.Player.Jump.IsPressed();
		}

		private void SetCursorLocked(bool locked)
		{
			Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
			Cursor.visible = !locked;
		}

		private void PollInput(CallbackPollInput callback)
		{
			callback.SetInput(_input, DeterministicInputFlags.Repeatable);
		}

		private void OnResetLookRotation(EventResetLookRotation callback)
		{
			_input.LookRotation = callback.LookRotation;
		}

		private void OnTeleportToArena(EventTeleportToArena callback)
		{
			ScenesController.Instance?.OpenArenaAsync(callback.ArenaName).Forget();
		}
	}
}
