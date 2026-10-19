using UnityEngine;

namespace LaboVR.Vr
{
    /// <summary>Reglages persistants du mode VR (enregistres avec PlayerPrefs).</summary>
    public static class VrSettings
    {
        private const string Prefix = "labovr.";

        /// <summary>Ecart entre les deux cameras virtuelles, en metres.</summary>
        public static float Ipd = 0.064f;
        /// <summary>Champ de vision vertical de chaque oeil, en degres.</summary>
        public static float Fov = 90f;
        /// <summary>Distorsion en barillet, terme en r^2.</summary>
        public static float K1 = 0.22f;
        /// <summary>Distorsion en barillet, terme en r^4.</summary>
        public static float K2 = 0.12f;
        /// <summary>Decalage des centres optiques : positif = vers le milieu, negatif = vers l'exterieur.</summary>
        public static float LensShift = 0f;
        /// <summary>Resolution de rendu par oeil (1 = pleine resolution).</summary>
        public static float RenderScale = 0.8f;
        /// <summary>Multiplicateur de la rotation de la tete (1 = naturel).</summary>
        public static float Sensitivity = 1f;
        /// <summary>Correction d'orientation du gyroscope, en quarts de tour.</summary>
        public static int RollQuarterTurns = 0;

        public static void ResetToDefaults()
        {
            Ipd = 0.064f;
            Fov = 90f;
            K1 = 0.22f;
            K2 = 0.12f;
            LensShift = 0f;
            RenderScale = 0.8f;
            Sensitivity = 1f;
            RollQuarterTurns = 0;
        }

        public static void Clamp()
        {
            Ipd = Mathf.Clamp(Ipd, 0.05f, 0.08f);
            Fov = Mathf.Clamp(Fov, 60f, 120f);
            K1 = Mathf.Clamp(K1, 0f, 1.2f);
            K2 = Mathf.Clamp(K2, 0f, 0.8f);
            LensShift = Mathf.Clamp(LensShift, -0.2f, 0.2f);
            RenderScale = Mathf.Clamp(RenderScale, 0.4f, 1f);
            Sensitivity = Mathf.Clamp(Sensitivity, 0.5f, 1.5f);
            RollQuarterTurns = ((RollQuarterTurns % 4) + 4) % 4;
        }

        public static void Load()
        {
            Ipd = PlayerPrefs.GetFloat(Prefix + "ipd", Ipd);
            Fov = PlayerPrefs.GetFloat(Prefix + "fov", Fov);
            K1 = PlayerPrefs.GetFloat(Prefix + "k1", K1);
            K2 = PlayerPrefs.GetFloat(Prefix + "k2", K2);
            LensShift = PlayerPrefs.GetFloat(Prefix + "lens", LensShift);
            RenderScale = PlayerPrefs.GetFloat(Prefix + "scale", RenderScale);
            Sensitivity = PlayerPrefs.GetFloat(Prefix + "sens", Sensitivity);
            RollQuarterTurns = PlayerPrefs.GetInt(Prefix + "roll", RollQuarterTurns);
            Clamp();
        }

        public static void Save()
        {
            Clamp();
            PlayerPrefs.SetFloat(Prefix + "ipd", Ipd);
            PlayerPrefs.SetFloat(Prefix + "fov", Fov);
            PlayerPrefs.SetFloat(Prefix + "k1", K1);
            PlayerPrefs.SetFloat(Prefix + "k2", K2);
            PlayerPrefs.SetFloat(Prefix + "lens", LensShift);
            PlayerPrefs.SetFloat(Prefix + "scale", RenderScale);
            PlayerPrefs.SetFloat(Prefix + "sens", Sensitivity);
            PlayerPrefs.SetInt(Prefix + "roll", RollQuarterTurns);
            PlayerPrefs.Save();
        }
    }
}
