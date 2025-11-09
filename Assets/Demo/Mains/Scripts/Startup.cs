using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Demo.Mains
{
    public class Startup : MonoBehaviour
    {
        public string ArenaName;
        
        void Start()
        {
            Application.targetFrameRate = 60;
            ScenesController.Instance.OpenArenaAsync(ArenaName).Forget();
        }
    }
}