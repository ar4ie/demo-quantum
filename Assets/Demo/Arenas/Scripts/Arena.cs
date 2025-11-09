using System.Threading;
using Cysharp.Threading.Tasks;
using Photon.Deterministic;
using Photon.Realtime;
using Quantum;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demo.Arenas
{
	/// <summary>
	/// Controls arena lifecycle.
	/// </summary>
	public class Arena : MonoBehaviour
	{
		public RuntimeConfig RuntimeConfig;
		public RuntimePlayer RuntimePlayer;

		private RealtimeClient _client;
		private CancellationTokenSource _cancellationTokenSource;

		private void Start()
		{
			// local testing
			if (gameObject.scene == SceneManager.GetActiveScene())
			{
				StartGame(mode: DeterministicGameMode.Local).Forget();
			}
		}

		public async UniTask StartGame(DeterministicGameMode mode = DeterministicGameMode.Multiplayer)
		{
			try
			{
				Debug.Log("Starting game");
				
				if (_client != null)
				{
					await StopGame();
				}

				_cancellationTokenSource = new CancellationTokenSource();
				var cancellationToken = _cancellationTokenSource.Token;

				if (mode == DeterministicGameMode.Multiplayer)
				{
					var matchmakingArguments = new MatchmakingArguments
					{
						PhotonSettings = new AppSettings(PhotonServerSettings.Global.AppSettings),
						RoomName = gameObject.scene.name,
						PluginName = "QuantumPlugin",
						MaxPlayers = Quantum.Input.MAX_COUNT,
					};
					if (matchmakingArguments.AsyncConfig == null)
					{
						matchmakingArguments.AsyncConfig = AsyncConfig.Global;
						matchmakingArguments.AsyncConfig.CancellationToken = cancellationToken;
					}

					_client = await MatchmakingExtensions.ConnectToRoomAsync(matchmakingArguments);
					_client.CallbackMessage.Listen<Arena, OnDisconnectedMsg>(this, OnDisconnected);
				}

				var sessionRunnerArguments = new SessionRunner.Arguments
				{
					RunnerFactory = QuantumRunnerUnityFactory.DefaultFactory,
					GameParameters = QuantumRunnerUnityFactory.CreateGameParameters,
					ClientId = _client?.UserId,
					RuntimeConfig = RuntimeConfig,
					SessionConfig = QuantumDeterministicSessionConfigAsset.DefaultConfig,
					GameMode = mode,
					PlayerCount = Quantum.Input.MAX_COUNT,
					Communicator = _client != null ? new QuantumNetworkCommunicator(_client) : null,
					CancellationToken = cancellationToken,
				};
				var runner = (QuantumRunner)await SessionRunner.StartAsync(sessionRunnerArguments);

				runner.Game.AddPlayer(0, RuntimePlayer);
				while (runner.Game.GetLocalPlayers().Count == 0)
					await UniTask.NextFrame();
				
				Debug.Log("Started game");
			}
			finally
			{
				_cancellationTokenSource = null;
			}
		}

		public async UniTask StopGame()
		{
			Debug.Log("Stopping game");
			
			if (_cancellationTokenSource != null)
			{
				_cancellationTokenSource.Cancel();
				while (_cancellationTokenSource != null)
					await UniTask.NextFrame();
			}

			if (_client != null)
			{
				_client.CallbackMessage.UnlistenAll(this);
				_client = null;
			}

			await QuantumRunner.ShutdownAllAsync();
			
			Debug.Log("Stopped game");
		}

		private void OnDisconnected(OnDisconnectedMsg message)
		{
			Debug.LogWarning($"Disconnected: {message.cause}");
		}
	}
}