using UnityEngine;

namespace Magicarium.Unity
{
    /// <summary>
    /// Bootstraps the Magicarium game on scene load using Unity's
    /// RuntimeInitializeOnLoadMethod attribute, so no MonoBehaviour
    /// needs to be placed in the scene manually.
    /// </summary>
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            // Prevent duplicate creation (e.g. if the scene already has a GameManager)
            if (Object.FindObjectOfType<GameManager>() != null) return;

            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>();
        }
    }
}
