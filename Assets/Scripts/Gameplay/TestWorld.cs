using UnityEngine;

namespace LaboVR.Gameplay
{
    /// <summary>
    /// Petit decor de test genere par code : sol a damier, piliers et cubes proches,
    /// pour verifier la stereo (relief), l'echelle et l'orientation du gyroscope.
    /// </summary>
    public static class TestWorld
    {
        public static GameObject Build()
        {
            GameObject root = new GameObject("TestWorld");

            Shader shader = Resources.Load<Shader>("UnlitShaded");
            if (shader == null)
            {
                Debug.LogError("Shader UnlitShaded introuvable (Assets/Resources).");
                return root;
            }

            Texture2D checker = MakeChecker(new Color(0.24f, 0.26f, 0.30f), new Color(0.13f, 0.14f, 0.17f));
            Material floorMat = NewMaterial(shader, Color.white, checker);
            floorMat.mainTextureScale = new Vector2(60f, 60f);
            AddPrimitive(root.transform, PrimitiveType.Plane, "Floor", Vector3.zero, new Vector3(12f, 1f, 12f), floorMat);

            Material white = NewMaterial(shader, new Color(0.85f, 0.87f, 0.90f), Texture2D.whiteTexture);
            Material orange = NewMaterial(shader, new Color(0.95f, 0.45f, 0.12f), Texture2D.whiteTexture);
            Material blue = NewMaterial(shader, new Color(0.20f, 0.45f, 0.95f), Texture2D.whiteTexture);
            Material[] palette = new Material[] { white, orange, blue };

            Random.InitState(7);
            for (int i = 0; i < 28; i++)
            {
                float angle = (i / 28f) * Mathf.PI * 2f + Random.Range(-0.08f, 0.08f);
                float dist = Random.Range(6f, 16f);
                float height = Random.Range(2.5f, 7f);
                float width = Random.Range(0.6f, 1.4f);
                bool round = (i % 2 == 0);

                Vector3 pos = new Vector3(Mathf.Sin(angle) * dist, height * 0.5f, Mathf.Cos(angle) * dist);
                Vector3 scale = round ? new Vector3(width, height * 0.5f, width) : new Vector3(width, height, width);
                PrimitiveType type = round ? PrimitiveType.Cylinder : PrimitiveType.Cube;
                AddPrimitive(root.transform, type, "Pillar" + i, pos, scale, palette[i % palette.Length]);
            }

            // Reperes proches pour juger le relief : devant l'utilisateur apres recentrage.
            AddPrimitive(root.transform, PrimitiveType.Cube, "Near1", new Vector3(-1.2f, 1.5f, 2.5f), Vector3.one * 0.5f, orange);
            AddPrimitive(root.transform, PrimitiveType.Cube, "Near2", new Vector3(0.6f, 1.2f, 4.0f), Vector3.one * 0.8f, blue);
            AddPrimitive(root.transform, PrimitiveType.Cube, "Near3", new Vector3(2.0f, 1.8f, 6.0f), Vector3.one * 1.2f, white);

            return root;
        }

        private static GameObject AddPrimitive(Transform parent, PrimitiveType type, string objectName,
            Vector3 position, Vector3 scale, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = objectName;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;

            Collider col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            Renderer r = go.GetComponent<Renderer>();
            r.sharedMaterial = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            return go;
        }

        private static Material NewMaterial(Shader shader, Color color, Texture texture)
        {
            Material m = new Material(shader);
            m.SetColor("_Color", color);
            m.mainTexture = texture;
            return m;
        }

        private static Texture2D MakeChecker(Color a, Color b)
        {
            const int size = 64;
            const int cell = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color[] px = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    px[y * size + x] = (((x / cell) + (y / cell)) % 2 == 0) ? a : b;
                }
            }
            tex.SetPixels(px);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Trilinear;
            tex.Apply(true);
            return tex;
        }
    }
}
