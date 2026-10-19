using System;
using LaboVR.Controls;
using UnityEngine;

namespace LaboVR.Vr
{
    /// <summary>
    /// Orientation de la tete (3DOF) a partir du gyroscope. Sans gyroscope : glisser le doigt.
    /// Aucune position n'est estimee : le gyroscope ne donne qu'une orientation.
    /// </summary>
    public class HeadTracker : MonoBehaviour
    {
        public bool GyroAvailable { get; private set; }
        public Quaternion Rotation { get; private set; }

        private Quaternion _recenter = Quaternion.identity;
        private Quaternion _raw = Quaternion.identity;
        private float _dragYaw;
        private float _dragPitch;

        private void Awake()
        {
            Rotation = Quaternion.identity;
            try
            {
                GyroAvailable = SystemInfo.supportsGyroscope;
                if (GyroAvailable)
                {
                    Input.gyro.enabled = true;
                }
            }
            catch (Exception e)
            {
                GyroAvailable = false;
                Debug.LogWarning("Gyroscope indisponible : " + e.Message);
            }
        }

        private void Update()
        {
            Quaternion raw = Quaternion.identity;
            if (GyroAvailable)
            {
                try
                {
                    raw = GyroToUnity(Input.gyro.attitude);
                }
                catch (Exception e)
                {
                    GyroAvailable = false;
                    Debug.LogWarning("Lecture du gyroscope impossible : " + e.Message);
                }
            }
            else
            {
                raw = ReadDragFallback();
            }

            _raw = raw;
            Quaternion full = _recenter * raw;
            Rotation = Quaternion.SlerpUnclamped(Quaternion.identity, full, VrSettings.Sensitivity);
        }

        /// <summary>Annule le lacet actuel : la direction regardee devient l'avant.</summary>
        public void Recenter()
        {
            float yaw = _raw.eulerAngles.y;
            _recenter = Quaternion.Euler(0f, -yaw, 0f);
        }

        /// <summary>
        /// Conversion standard du gyroscope vers Unity (camera tournee vers l'arriere du telephone),
        /// puis compensation de l'orientation de l'ecran et correction manuelle en quarts de tour.
        /// </summary>
        private static Quaternion GyroToUnity(Quaternion q)
        {
            Quaternion fromDevice = Quaternion.Euler(90f, 0f, 0f) * new Quaternion(-q.x, -q.y, q.z, q.w);
            float roll = ScreenRollDegrees() + 90f * VrSettings.RollQuarterTurns;
            return fromDevice * Quaternion.Euler(0f, 0f, roll);
        }

        private static float ScreenRollDegrees()
        {
            switch (Screen.orientation)
            {
                case ScreenOrientation.LandscapeLeft:
                    return -90f;
                case ScreenOrientation.LandscapeRight:
                    return 90f;
                case ScreenOrientation.PortraitUpsideDown:
                    return 180f;
                default:
                    return 0f;
            }
        }

        private Quaternion ReadDragFallback()
        {
            Touch t;
            if (SafeInput.TouchCount > 0 && SafeInput.TryGetTouch(0, out t) && t.phase == TouchPhase.Moved)
            {
                _dragYaw += t.deltaPosition.x * 0.2f;
                _dragPitch = Mathf.Clamp(_dragPitch - t.deltaPosition.y * 0.2f, -80f, 80f);
            }
            return Quaternion.Euler(_dragPitch, _dragYaw, 0f);
        }
    }
}
