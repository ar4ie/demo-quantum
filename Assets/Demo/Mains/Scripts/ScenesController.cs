using System;
using Cysharp.Threading.Tasks;
using Demo.Arenas;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demo.Mains
{
    public class ScenesController : MonoBehaviour
    {
        public static ScenesController Instance;

        public GameObject LoadingScreen;

        private Arena _arena;

        void Awake()
        {
            if (Instance != null)
                throw new Exception("Duplicate ScenesController");
            Instance = this;
        }

        public async UniTask OpenArenaAsync(string arenaName)
        {
            try
            {
                LoadingScreen.SetActive(true);

                await CloseArenaAsync();

                var index = SceneUtility.GetBuildIndexByScenePath(arenaName);
                await SceneManager.LoadSceneAsync(index, LoadSceneMode.Additive);
                var scene = SceneManager.GetSceneByBuildIndex(index);
                if (!TryGetRootComponent(scene, out _arena))
                    throw new Exception("Arena not found");
                await _arena.StartGame();
            }
            finally
            {
                LoadingScreen.SetActive(false);
            }
        }

        public async UniTask CloseArenaAsync()
        {
            if (_arena == null)
                return;
            await _arena.StopGame();
            await SceneManager.UnloadSceneAsync(_arena.gameObject.scene);
            _arena = null;
        }

        private static bool TryGetRootComponent<T>(Scene scene, out T component)
        {
            foreach (var obj in scene.GetRootGameObjects())
            {
                if (obj.TryGetComponent(out component))
                    return true;
            }
            component = default;
            return false;
        }
    }
}