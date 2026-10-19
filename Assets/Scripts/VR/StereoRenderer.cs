using UnityEngine;

namespace LaboVR.Vr
{
    /// <summary>
    /// Rendu stereoscopique cote a cote : une camera par oeil dans une RenderTexture, puis
    /// recomposition a l'ecran avec une pre-distorsion en barillet qui compense les lentilles.
    /// </summary>
    public class StereoRenderer : MonoBehaviour
    {
        private const int CompositeLayer = 31;

        private Transform _head;
        private Camera _leftEye;
        private Camera _rightEye;
        private Camera _compositor;
        private RenderTexture _leftRt;
        private RenderTexture _rightRt;
        private Material _leftMat;
        private Material _rightMat;
        private int _builtW;
        private int _builtH;
        private float _builtScale = -1f;

        public bool Ready { get; private set; }
        public Transform Head { get { return _head; } }
        public int EyeWidth { get { return _leftRt != null ? _leftRt.width : 0; } }
        public int EyeHeight { get { return _leftRt != null ? _leftRt.height : 0; } }

        public void Build(Transform worldParent)
        {
            Shader shader = Resources.Load<Shader>("LensDistortion");
            if (shader == null)
            {
                Debug.LogError("Shader LensDistortion introuvable (Assets/Resources).");
                return;
            }

            GameObject headGo = new GameObject("Head");
            headGo.transform.SetParent(worldParent, false);
            headGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            headGo.AddComponent<AudioListener>();
            _head = headGo.transform;

            _leftEye = CreateEye("LeftEye");
            _rightEye = CreateEye("RightEye");

            _leftMat = new Material(shader);
            _rightMat = new Material(shader);
            BuildCompositor();

            EnsureTargets();
            ApplySettings();
            Ready = true;
        }

        private Camera CreateEye(string eyeName)
        {
            GameObject go = new GameObject(eyeName);
            go.transform.SetParent(_head, false);

            Camera cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.08f, 1f);
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 250f;
            cam.cullingMask = ~(1 << CompositeLayer);
            cam.allowHDR = false;
            cam.allowMSAA = false;
            cam.depth = 0f;
            return cam;
        }

        private void BuildCompositor()
        {
            GameObject go = new GameObject("Compositor");
            go.transform.SetParent(transform, false);

            _compositor = go.AddComponent<Camera>();
            _compositor.clearFlags = CameraClearFlags.SolidColor;
            _compositor.backgroundColor = Color.black;
            _compositor.cullingMask = 1 << CompositeLayer;
            _compositor.orthographic = true;
            _compositor.orthographicSize = 1f;
            _compositor.nearClipPlane = 0.1f;
            _compositor.farClipPlane = 10f;
            _compositor.allowHDR = false;
            _compositor.allowMSAA = false;
            _compositor.depth = 100f;

            CreateQuad(go.transform, "LeftQuad", -1f, 0f, _leftMat);
            CreateQuad(go.transform, "RightQuad", 0f, 1f, _rightMat);
        }

        private static void CreateQuad(Transform parent, string quadName, float x0, float x1, Material material)
        {
            GameObject go = new GameObject(quadName);
            go.layer = CompositeLayer;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 0f, 1f);

            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(x0, -1f, 0f),
                new Vector3(x1, -1f, 0f),
                new Vector3(x1, 1f, 0f),
                new Vector3(x0, 1f, 0f)
            };
            mesh.uv = new Vector2[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };
            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(1000f, 1000f, 1000f));

            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }

        private void LateUpdate()
        {
            if (!Ready) return;
            EnsureTargets();
            ApplySettings();
        }

        private void EnsureTargets()
        {
            int sw = Mathf.Max(2, Screen.width);
            int sh = Mathf.Max(2, Screen.height);
            float scale = VrSettings.RenderScale;
            bool same = _leftRt != null && sw == _builtW && sh == _builtH && Mathf.Approximately(scale, _builtScale);
            if (same) return;

            ReleaseTargets();

            int w = Mathf.Max(64, Mathf.RoundToInt(sw * 0.5f * scale));
            int h = Mathf.Max(64, Mathf.RoundToInt(sh * scale));
            _leftRt = NewTarget("EyeLeft", w, h);
            _rightRt = NewTarget("EyeRight", w, h);

            _leftEye.targetTexture = _leftRt;
            _rightEye.targetTexture = _rightRt;
            _leftMat.mainTexture = _leftRt;
            _rightMat.mainTexture = _rightRt;

            _builtW = sw;
            _builtH = sh;
            _builtScale = scale;
        }

        private static RenderTexture NewTarget(string targetName, int w, int h)
        {
            RenderTexture rt = new RenderTexture(w, h, 16, RenderTextureFormat.Default);
            rt.name = targetName;
            rt.filterMode = FilterMode.Bilinear;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.antiAliasing = 1;
            rt.Create();
            return rt;
        }

        private void ApplySettings()
        {
            float half = VrSettings.Ipd * 0.5f;
            _leftEye.transform.localPosition = new Vector3(-half, 0f, 0f);
            _rightEye.transform.localPosition = new Vector3(half, 0f, 0f);
            _leftEye.fieldOfView = VrSettings.Fov;
            _rightEye.fieldOfView = VrSettings.Fov;

            float aspect = (_leftRt != null && _leftRt.height > 0) ? (float)_leftRt.width / _leftRt.height : 1f;
            Vector4 k = new Vector4(VrSettings.K1, VrSettings.K2, 0f, 0f);

            _leftMat.SetVector("_K", k);
            _rightMat.SetVector("_K", k);
            _leftMat.SetFloat("_Aspect", aspect);
            _rightMat.SetFloat("_Aspect", aspect);
            _leftMat.SetVector("_Center", new Vector4(0.5f + VrSettings.LensShift, 0.5f, 0f, 0f));
            _rightMat.SetVector("_Center", new Vector4(0.5f - VrSettings.LensShift, 0.5f, 0f, 0f));
        }

        private void ReleaseTargets()
        {
            if (_leftEye != null) _leftEye.targetTexture = null;
            if (_rightEye != null) _rightEye.targetTexture = null;
            DestroyTarget(ref _leftRt);
            DestroyTarget(ref _rightRt);
        }

        private static void DestroyTarget(ref RenderTexture rt)
        {
            if (rt == null) return;
            rt.Release();
            Destroy(rt);
            rt = null;
        }

        private void OnDestroy()
        {
            ReleaseTargets();
            if (_leftMat != null) Destroy(_leftMat);
            if (_rightMat != null) Destroy(_rightMat);
        }
    }
}
