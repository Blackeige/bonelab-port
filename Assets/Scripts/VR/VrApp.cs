using System.Text;
using LaboVR.Controls;
using LaboVR.Gameplay;
using LaboVR.Ui;
using UnityEngine;

namespace LaboVR.Vr
{
    /// <summary>Chef d'orchestre de la v0.1 : decor de test, suivi de tete, rendu stereo, menu.</summary>
    public class VrApp : MonoBehaviour
    {
        private const float LongPressSeconds = 1.2f;
        private const float TapMaxSeconds = 0.6f;

        private HeadTracker _tracker;
        private StereoRenderer _stereo;
        private DiagnosticsHud _hud;
        private readonly StringBuilder _sb = new StringBuilder(256);

        private float _fps = 60f;
        private float _fpsMin = 60f;
        private float _windowMin = 999f;
        private float _windowTime;
        private bool _autoRecentered;

        private bool _touching;
        private bool _longFired;
        private float _touchStart;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            VrSettings.Load();

            GameObject world = TestWorld.Build();
            world.transform.SetParent(transform, false);

            _tracker = gameObject.AddComponent<HeadTracker>();
            _stereo = gameObject.AddComponent<StereoRenderer>();
            _stereo.Build(transform);

            _hud = gameObject.AddComponent<DiagnosticsHud>();
            _hud.OnRecenter = Recenter;
            _hud.OnQuit = QuitApp;

            RefreshInfo();
        }

        private void Update()
        {
            float dt = Mathf.Max(0.0001f, Time.unscaledDeltaTime);
            float instant = 1f / dt;
            _fps = Mathf.Lerp(_fps, instant, 0.05f);
            _windowMin = Mathf.Min(_windowMin, instant);
            _windowTime += dt;
            if (_windowTime >= 1f)
            {
                _fpsMin = _windowMin;
                _windowMin = 999f;
                _windowTime = 0f;
                RefreshInfo();
            }

            if (!_autoRecentered && Time.unscaledTime > 1f)
            {
                _autoRecentered = true;
                _tracker.Recenter();
            }

            HandleTouch();

            if (SafeInput.KeyDown(KeyCode.Escape))
            {
                _hud.MenuOpen = !_hud.MenuOpen;
            }
        }

        private void LateUpdate()
        {
            if (_stereo != null && _stereo.Head != null && _tracker != null)
            {
                _stereo.Head.localRotation = _tracker.Rotation;
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) VrSettings.Save();
        }

        private void Recenter()
        {
            _tracker.Recenter();
            _hud.Pulse();
        }

        private void QuitApp()
        {
            Application.Quit();
        }

        /// <summary>Toucher bref = recentrer ; appui long = ouvrir le menu.</summary>
        private void HandleTouch()
        {
            if (_hud.MenuOpen)
            {
                _touching = false;
                return;
            }

            if (SafeInput.TouchCount == 0)
            {
                _touching = false;
                return;
            }

            Touch t;
            if (!SafeInput.TryGetTouch(0, out t)) return;

            if (t.phase == TouchPhase.Began)
            {
                _touching = true;
                _longFired = false;
                _touchStart = Time.unscaledTime;
            }
            else if (_touching && !_longFired && Time.unscaledTime - _touchStart >= LongPressSeconds)
            {
                _longFired = true;
                _hud.MenuOpen = true;
            }
            else if (_touching && t.phase == TouchPhase.Ended)
            {
                if (!_longFired && Time.unscaledTime - _touchStart <= TapMaxSeconds)
                {
                    Recenter();
                }
                _touching = false;
            }
        }

        private void RefreshInfo()
        {
            _sb.Length = 0;
            _sb.Append("FPS ").Append(_fps.ToString("F0")).Append(" (min ").Append(_fpsMin.ToString("F0")).Append(")");
            _hud.HudLine = _sb.ToString();

            _sb.Append("\nGyroscope : ").Append(_tracker.GyroAvailable ? "oui" : "non");
            _sb.Append("\nÉcran : ").Append(Screen.width).Append("x").Append(Screen.height);
            _sb.Append("  ").Append(Screen.orientation.ToString());
            _sb.Append("\nRendu par œil : ").Append(_stereo.EyeWidth).Append("x").Append(_stereo.EyeHeight);
            _sb.Append("\nAppareil : ").Append(SystemInfo.deviceModel);
            _sb.Append("\nGPU : ").Append(SystemInfo.graphicsDeviceName);
            _sb.Append("\nAPI : ").Append(SystemInfo.graphicsDeviceType.ToString());
            _sb.Append("\nVersion : ").Append(Application.version);
            _hud.Info = _sb.ToString();
        }
    }
}
