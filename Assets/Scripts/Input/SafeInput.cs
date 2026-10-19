using System;
using UnityEngine;

namespace LaboVR.Controls
{
    /// <summary>
    /// Acces tolerant a l'ancien Input Manager : si le projet bascule un jour sur le nouvel
    /// Input System, les appels echouent proprement au lieu de faire planter l'application.
    /// </summary>
    public static class SafeInput
    {
        public static bool Broken { get; private set; }

        public static int TouchCount
        {
            get
            {
                if (Broken) return 0;
                try
                {
                    return Input.touchCount;
                }
                catch (Exception e)
                {
                    Fail(e);
                    return 0;
                }
            }
        }

        public static bool TryGetTouch(int index, out Touch touch)
        {
            touch = default(Touch);
            if (Broken) return false;
            try
            {
                touch = Input.GetTouch(index);
                return true;
            }
            catch (Exception e)
            {
                Fail(e);
                return false;
            }
        }

        public static bool KeyDown(KeyCode key)
        {
            if (Broken) return false;
            try
            {
                return Input.GetKeyDown(key);
            }
            catch (Exception e)
            {
                Fail(e);
                return false;
            }
        }

        private static void Fail(Exception e)
        {
            Broken = true;
            Debug.LogWarning("Ancien systeme Input indisponible : " + e.Message);
        }
    }
}
