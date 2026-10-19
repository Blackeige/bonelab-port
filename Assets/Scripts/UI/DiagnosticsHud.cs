using System;
using System.Collections.Generic;
using LaboVR.Vr;
using UnityEngine;

namespace LaboVR.Ui
{
    /// <summary>
    /// Interface 2D (IMGUI) : courte indication en VR, menu de reglages et diagnostic en mode plat.
    /// </summary>
    public class DiagnosticsHud : MonoBehaviour
    {
        public bool MenuOpen;
        public string Info = "";
        public string HudLine = "";
        public Action OnRecenter;
        public Action OnQuit;

        private readonly Queue<string> _log = new Queue<string>();
        private string _logText = "";
        private float _hudUntil;
        private GUIStyle _label;
        private GUIStyle _small;
        private GUIStyle _center;
        private GUIStyle _button;
        private GUIStyle _title;

        private void OnEnable()
        {
            Application.logMessageReceived += OnLog;
            Pulse();
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= OnLog;
            VrSettings.Save();
        }

        /// <summary>Affiche a nouveau l'indication courte pendant quelques secondes.</summary>
        public void Pulse()
        {
            _hudUntil = Time.unscaledTime + 8f;
        }

        private void OnLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Log) return;
            string prefix = (type == LogType.Warning) ? "! " : "ERREUR : ";
            string line = prefix + condition;
            if (line.Length > 110) line = line.Substring(0, 110);
            _log.Enqueue(line);
            while (_log.Count > 6) _log.Dequeue();
            _logText = string.Join("\n", _log.ToArray());
        }

        private void EnsureStyles()
        {
            if (_label != null) return;

            _label = new GUIStyle(GUI.skin.label);
            _label.fontSize = 22;
            _label.alignment = TextAnchor.MiddleLeft;
            _label.normal.textColor = Color.white;

            _small = new GUIStyle(GUI.skin.label);
            _small.fontSize = 18;
            _small.alignment = TextAnchor.UpperLeft;
            _small.wordWrap = true;
            _small.normal.textColor = new Color(0.85f, 0.95f, 1f);

            _center = new GUIStyle(GUI.skin.label);
            _center.fontSize = 20;
            _center.alignment = TextAnchor.UpperCenter;
            _center.wordWrap = true;
            _center.normal.textColor = new Color(0.9f, 1f, 0.9f);

            _button = new GUIStyle(GUI.skin.button);
            _button.fontSize = 22;

            _title = new GUIStyle(GUI.skin.label);
            _title.fontSize = 28;
            _title.fontStyle = FontStyle.Bold;
            _title.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            EnsureStyles();

            float s = Screen.height / 540f;
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(s, s, 1f));
            float vw = Screen.width / s;

            if (MenuOpen)
            {
                DrawMenu(vw);
            }
            else
            {
                DrawHud(vw);
            }

            GUI.matrix = previous;
        }

        private void DrawHud(float vw)
        {
            if (Time.unscaledTime > _hudUntil) return;

            string text = "Toucher : recentrer  |  Appui long : menu\n" + HudLine;
            GUI.Label(new Rect(vw * 0.25f - 260f, 440f, 520f, 90f), text, _center);
            GUI.Label(new Rect(vw * 0.75f - 260f, 440f, 520f, 90f), text, _center);
        }

        private void DrawMenu(float vw)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.82f);
            GUI.DrawTexture(new Rect(0f, 0f, vw, 540f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(20f, 8f, vw - 40f, 40f), "Labo VR - réglages (v0.1)", _title);

            float y = 56f;
            y = Row(20f, y, 580f, "IPD", (VrSettings.Ipd * 1000f).ToString("F0") + " mm",
                () => VrSettings.Ipd -= 0.001f, () => VrSettings.Ipd += 0.001f);
            y = Row(20f, y, 580f, "Champ de vision", VrSettings.Fov.ToString("F0") + " °",
                () => VrSettings.Fov -= 2f, () => VrSettings.Fov += 2f);
            y = Row(20f, y, 580f, "Distorsion k1", VrSettings.K1.ToString("F2"),
                () => VrSettings.K1 -= 0.02f, () => VrSettings.K1 += 0.02f);
            y = Row(20f, y, 580f, "Distorsion k2", VrSettings.K2.ToString("F2"),
                () => VrSettings.K2 -= 0.02f, () => VrSettings.K2 += 0.02f);
            y = Row(20f, y, 580f, "Décalage optique", VrSettings.LensShift.ToString("F2"),
                () => VrSettings.LensShift -= 0.01f, () => VrSettings.LensShift += 0.01f);
            y = Row(20f, y, 580f, "Résolution de rendu", (VrSettings.RenderScale * 100f).ToString("F0") + " %",
                () => VrSettings.RenderScale -= 0.05f, () => VrSettings.RenderScale += 0.05f);
            y = Row(20f, y, 580f, "Sensibilité gyroscope", VrSettings.Sensitivity.ToString("F2"),
                () => VrSettings.Sensitivity -= 0.05f, () => VrSettings.Sensitivity += 0.05f);
            y = Row(20f, y, 580f, "Rotation de l'image", (VrSettings.RollQuarterTurns * 90) + " °",
                () => VrSettings.RollQuarterTurns -= 1, () => VrSettings.RollQuarterTurns += 1);
            VrSettings.Clamp();

            GUI.Label(new Rect(20f, y + 4f, 580f, 540f - y - 8f), "Journal :\n" + _logText, _small);

            float x = 640f;
            float w = Mathf.Max(300f, vw - x - 20f);
            float by = 56f;

            if (GUI.Button(new Rect(x, by, w, 44f), "Recentrer la vue", _button))
            {
                if (OnRecenter != null) OnRecenter();
            }
            by += 52f;

            if (GUI.Button(new Rect(x, by, w, 44f), "Réinitialiser les réglages", _button))
            {
                VrSettings.ResetToDefaults();
            }
            by += 52f;

            if (GUI.Button(new Rect(x, by, w, 44f), "Quitter l'application", _button))
            {
                VrSettings.Save();
                if (OnQuit != null) OnQuit();
            }
            by += 52f;

            if (GUI.Button(new Rect(x, by, w, 56f), "FERMER LE MENU (retour VR)", _button))
            {
                MenuOpen = false;
                VrSettings.Save();
                Pulse();
            }
            by += 66f;

            GUI.Label(new Rect(x, by, w, 540f - by - 8f), Info, _small);
        }

        private float Row(float x, float y, float w, string title, string value, Action minus, Action plus)
        {
            GUI.Label(new Rect(x, y, w - 150f, 38f), title + " : " + value, _label);
            if (GUI.Button(new Rect(x + w - 140f, y, 64f, 38f), "-", _button)) minus();
            if (GUI.Button(new Rect(x + w - 70f, y, 64f, 38f), "+", _button)) plus();
            return y + 44f;
        }
    }
}
