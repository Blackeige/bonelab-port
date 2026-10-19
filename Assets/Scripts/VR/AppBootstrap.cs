using UnityEngine;

namespace LaboVR.Vr
{
    /// <summary>
    /// Point d'entree : cree toute l'application au lancement, quelle que soit la scene.
    /// </summary>
    public static class AppBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            GameObject root = new GameObject("LaboVR");
            Object.DontDestroyOnLoad(root);
            root.AddComponent<VrApp>();
        }
    }
}
