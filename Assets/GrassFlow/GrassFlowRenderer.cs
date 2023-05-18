using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using GrassFlow;
using System.Threading.Tasks;


[ExecuteInEditMode]
[AddComponentMenu("Rendering/GrassFlow")]
public class GrassFlowRenderer : MonoBehaviour {



    [Tooltip("Receive shadows on the grass. Can be expensive, especially with cascaded shadows on. (Requires the grass shader with depth pass to render properly)")]
    public bool receiveShadows = true;

    [Tooltip("Grass casts shadows. Fairly expensive option. (Also requires the grass shader with depth pass to render at all)")]
    [SerializeField] private bool _castShadows = false;
    [SerializeField] private ShadowCastingMode shadowMode;
    public bool castShadows {
        get { return _castShadows; }
        set {
            _castShadows = value;
            shadowMode = value ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }
    }

    [Tooltip("This setting only effects the editor. Most of the time you're going to want this on as it prevents visual popping as scripts are recompiled and such. " +
        "You can turn it off to get a more accurate view of game performance, though really it hardly makes any difference.")]
    public bool updateBuffers = true;

    [Tooltip("Enables the ability to paint grass color and parameters dynamically in both the editor and in game. If true it creates Rendertextures from supplied textures " +
        "that can be painted and saved.")]
    [SerializeField] private bool _enableMapPainting = false;
    public bool enableMapPainting {
        get { return _enableMapPainting; }
        set {
            _enableMapPainting = value;

            if (value) {
                MapSetup();
            } else {
                ReleaseDetailMapRTs();
            }

            UpdateShaders();
        }
    }



    [Tooltip("If true, an instance of the material will be created to render with. Important if you want to use the same material for multiple grasses but want them to have different textures etc.")]
    public bool useMaterialInstance = false;



    [Tooltip("Layer to render the grass on.")]
    public int renderLayer;

    [Tooltip("Amount to expand grass chunks on terrain, helps avoid artifacts on edges of chunks. Preferably set this as low as you can without it looking bad.")]
    public float terrainExpansion = 0.35f;


    [Tooltip("Enables the ability for grass to orient itself to the slope of the terrain and shade itself better. You can disable this to save on memory and a slight load time boost, " +
        "but it really isn't recommended to do so unless you really need to.")]
    public bool useTerrainNormalMap = true;

    [Tooltip("In my testing this made performance worse. Even though it feels like it should be faster with how indirect instancing works. Which is frustrating because ykno " +
        "I made the feature and then it's just worse somehow but well, here we are. You can try it out anyway and see if it's better for your situation. " +
        "IMPORTANT: You'll need to enable a shader keyword at the top of GrassFlow/Shaders/GrassStructsVars.cginc by uncommenting it for this to work properly.")]
    public bool useIndirectInstancing = false;

    [Tooltip("Does this really need a tooltip? Uhh, well chunk bounds are expanded automatically by blade height to avoid grass popping out when the bounds are culled at strange angles.")]
    [HideInInspector] public bool visualizeChunkBounds = false;

    [Tooltip("Dicards chunks that don't have ANY grass in them based on the parameter map density channel, " +
        "this will be significantly more performant if your terrain has large areas without grass." +
        "WARNING: enabling this removes the chunks completely, meaning that grass could not be dynamically added back in those chunks during runtime. " +
        "NOTE: Only applies during play mode to avoid conflicts with painting.")]
    public bool discardEmptyChunks = false;



    [Tooltip("Controls the LOD parameter of the grass. X = render distance. Y = density falloff sharpness (how quickly the amount of grass is reduced to zero). " +
        "Z = offset, basically a positive number prevents blades from popping out within this distance.")]
    [SerializeField] private Vector3 _lodParams = new Vector3(15, 1.1f, 0);
    public Vector3 lodParams {
        get { return _lodParams; }
        set {
            _lodParams = value;

            foreach (var gMesh in terrainMeshes) {
                if (gMesh.drawnMat) gMesh.drawnMat.SetVector("_LOD", value);
            }
        }
    }

    [SerializeField] private float maxRenderDistSqr = 150f * 150f;

    [Tooltip("Controls max render dist of the grass chunks. This value is mostly just used to quickly reject far away chunks for rendering.")]
    [SerializeField] private float _maxRenderDist = 150f;
    public float maxRenderDist {
        get { return _maxRenderDist; }
        set {
            _maxRenderDist = value;
            maxRenderDistSqr = value * value;
        }
    }

    [Tooltip("Don't enable this setting unless your source mesh has very NON-uniform density as it'll increase processing time and probably produce worse results. " +
        "This setting attempts to subdivide the mesh to make all triangles as close to the same size as it can, the original shape will be matched exactly. " +
        "Because this subdivides the mesh, you may want to decrease GrassPerTri to account for the increased density.")]
    public bool normalizeMeshDensity = false;


    [Tooltip("Enables a partially asynchronous multithreaded execution of the initial processing that can slightly reduce load times if you have a large mesh. " +
        "The downside of this is that the game might start before the grass is loaded.")]
    public bool asyncInitialization = false;

    //[Tooltip("Compress the terrain normal map to save on memory at the expense of a small increase in initial loading time.")]
    //public bool compressTerrainNormalMap = true;

    //old maintanence for making sure can update these old value if someone upgrades to this version
    [SerializeField] int _instanceCount = 30;
    [SerializeField] Mesh grassMesh;
    [SerializeField] Terrain terrainObject;
    [SerializeField] Transform terrainTransform;
    [SerializeField] Material grassMaterial;
    [SerializeField] Texture2D colorMap;
    [SerializeField] Texture2D paramMap;
    [SerializeField] Texture2D typeMap;

    [SerializeField] int grassPerTri = 4;
    [SerializeField] float normalizeMaxRatio = 12f;
    [SerializeField] GrassRenderType renderType;
    [SerializeField] int chunksX = 5;
    [SerializeField] int chunksY = 1;
    [SerializeField] int chunksZ = 5;

    public bool hasRequiredAssets {
        get {

            bool hasAssets = true;

            foreach (var gMesh in terrainMeshes) {
                hasAssets &= gMesh.hasRequiredAssets;
            }

            return hasAssets;
        }
    }

    [SerializeField] public List<GrassMesh> terrainMeshes;
    [HideInInspector] public int selectedIdx;



    public enum GrassRenderType { Terrain, Mesh }


    //Static Vars
    static ComputeShader gfComputeShader;
    static ComputeBuffer rippleBuffer;
    static ComputeBuffer counterBuffer;
    static ComputeBuffer forcesBuffer;
    static RippleData[] forcesArray;
    static GrassForce[] forceClassArray;
    static int forcesCount;
    static bool forcesDirty;
    static int updateRippleKernel;
    static int addRippleKernel;
    static int noiseKernel;
    static int normalKernel;
    static int heightKernel;
    static int emptyChunkKernel;
    static int ripDeltaTimeHash = Shader.PropertyToID("ripDeltaTime");


    static RenderTexture noise3DTexture;

    //static Shader paintShader;
    //static Material paintMat;
    //const int paintPass = 0;
    //const int splatPass = 1;
    static ComputeShader paintShader;
    static int paintKernel;
    static int splatKernel;

    public static HashSet<GrassFlowRenderer> instances = new HashSet<GrassFlowRenderer>();
    static bool runRipple = true;

    /// <summary>
    /// This is set to true as soon as a ripple is added and stays true unless manually set to false.
    /// When true it signals the ripple update shaders to run, it doesn't take long to run them and theres no easy generic way to know when all ripples are depleted without asking the gpu for the memory which would be slow.
    /// But you can manually set this if you know your ripples only last a certain amount of time or something.
    /// Realistically its not that important though.
    /// </summary>
    public static bool updateRipples = false;


    //-----------------------------------------------------------------------------------------
    //----------------------------------------ACTUAL CODE---------------------------------------
    //-----------------------------------------------------------------------------------------


    void Awake() {
        instances.Add(this);

        StartupInit();
    }


    void UnHookRender() {

#if !GRASSFLOW_SRP
        Camera.onPreCull -= Render;
#else
        RenderPipelineManager.beginCameraRendering -= Render;
#endif
    }

    void HookRender() {

        UnHookRender();

#if !GRASSFLOW_SRP
        Camera.onPreCull += Render;
#else
        RenderPipelineManager.beginCameraRendering += Render;
#endif
    }

#if UNITY_2019_2_OR_NEWER
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
    static void StaticDomain() {
        runRipple = true;
        updateRipples = false;
        instances = new HashSet<GrassFlowRenderer>();
        forcesCount = 0;

        if (noise3DTexture) {
            noise3DTexture.Release();
            noise3DTexture = null;
        }

        ReleaseBuffers();
    }

    public bool CheckRequiresChunks() {
        foreach (var gMesh in terrainMeshes) {
            if (gMesh.chunks == null) {
                initialized = false;
                return false;
            }
        }

        return true;
    }


    public void OnEnable() {
        UnHookRender();

        CheckRequiresChunks();

        if (hasRequiredAssets) {
            UnHookRender();
            HookRender();
        }

        UpdateTransform(true);

        //have to reset these on enable due to reasons related to what is described in OnDisable
        CheckIndirectInstancingArgs();
    }


    private void OnDisable() {
        UnHookRender();

        ReleaseBuffers();

        initialized = false;

        //unity throws a buncha warnings about the indirect args buffer being unallocated and disposed by the garbage collector when scripts are rebuilt if we dont do this
        //becuse of how unity's weird system of serialization works it just automatically unallocates the buffer on reload so we have to catch it here and do it manually
        //because for whatever reason youre not supposed to let garbage collection dispose of them automatically or itll complain
        ReleaseIndirectArgsBuffers();
    }


#if UNITY_EDITOR

    private void Reset() {

        CheckTerrainMeshes();
        GrassMesh firstMesh = terrainMeshes[0];

        firstMesh.terrainTransform = transform;
        firstMesh.terrainObject = GetComponent<Terrain>();

        MeshFilter meshF;
        if (meshF = GetComponent<MeshFilter>()) {
            firstMesh.grassMesh = meshF.sharedMesh;
            firstMesh.renderType = GrassRenderType.Mesh;
        }

    }


    //the validation function is mainly to regenerate certain things that are lost upon unity recompiling scripts
    //but also in some other situations like saving the scene
    private void OnValidate() {
        if (!isActiveAndEnabled || !hasRequiredAssets || StackTraceUtility.ExtractStackTrace().Contains("Inspector"))
            return;


        if (terrainMeshes == null) {
            Refresh();
        } else {
            GetResources(true);
            UpdateShaders();
            MapSetup();
        }


        if (!initialized && !initializing) {
            Init();
        } else {
            UnHookRender();
            if (isActiveAndEnabled) {
                HookRender();
            }
        }

    }


    private void OnDrawGizmos() {
        if (!visualizeChunkBounds) return;
        if (selectedIdx >= terrainMeshes.Count) return;

        var gMesh = terrainMeshes[selectedIdx];
        if (gMesh.chunks == null) return;

        //if (cameraCulls.Count == 0) return;
        //var chunks = cameraCulls.ElementAt(0).Value.culledChunks;

        //foreach (var chunk in chunks) {

        //    if (chunk.instancesToRender == 0) {
        //        Gizmos.color = Color.white;
        //    } else {
        //        float t = chunk.instancesToRender / (float)gMesh.instanceCount;
        //        Gizmos.color = Color.Lerp(Color.green, Color.red, t);
        //    }

        //    Gizmos.DrawWireCube(chunk.parentChunk.worldBounds.center, chunk.parentChunk.worldBounds.size);
        //}


        foreach (var chunk in gMesh.chunks) {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(chunk.worldBounds.center, chunk.worldBounds.size);
        }

    }

    //void OnDrawGizmosSelected() {
    //    if (selectedIdx >= terrainMeshes.Count) return;

    //    var gMesh = terrainMeshes[selectedIdx];
    //    if (gMesh.chunks == null) return;
    //    if (!gMesh.mesh) return;

    //    Gizmos.color = new Color(0.8f, 0.8f, 0.8f, 0.045f);
    //    var t = gMesh.terrainTransform;
    //    foreach (var chunk in gMesh.chunks) {
    //        Gizmos.DrawWireMesh(gMesh.mesh, chunk.submeshIdx, t.position, t.rotation, t.lossyScale);
    //    }
    //}

    //stuff for toggling the preprocessor definition
    public const string grassSRPDefine = "GRASSFLOW_SRP";

    static readonly UnityEditor.BuildTargetGroup[] definePlatforms = new UnityEditor.BuildTargetGroup[] {
        UnityEditor.BuildTargetGroup.Standalone,
        UnityEditor.BuildTargetGroup.XboxOne,
        UnityEditor.BuildTargetGroup.PS4,
        UnityEditor.BuildTargetGroup.Android,
        UnityEditor.BuildTargetGroup.iOS,
        UnityEditor.BuildTargetGroup.WebGL,
        UnityEditor.BuildTargetGroup.Switch,
    };


#if GRASSFLOW_SRP
    [UnityEditor.MenuItem("CONTEXT/GrassFlowRenderer/Disable URP Support (READ DOC)")]   
#else
    [UnityEditor.MenuItem("CONTEXT/GrassFlowRenderer/Enable URP Support (READ DOC)")]
#endif
    public static void ToggleSRP() {
        ToggleDefineSymbol(grassSRPDefine, definePlatforms);
    }

    static bool CheckForDefineSymbol(string symbolName) {
        return UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(definePlatforms[0]).Contains(symbolName);
    }

    static bool ToggleDefineSymbol(string symbolName, UnityEditor.BuildTargetGroup[] platforms) {
        bool enable = !CheckForDefineSymbol(symbolName);

        foreach (UnityEditor.BuildTargetGroup buildTarget in platforms) {
            string defines = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTarget);

            if (!defines.Contains(symbolName) && enable) {
                UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defines + ";" + symbolName);

            } else if (defines.Contains(symbolName) && !enable) {
                UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTarget, defines.Replace(symbolName, ""));
            }
        }

        return enable;
    }


#endif


    /// <summary>
    /// Releases current assets and reinitializes the grass.
    /// Warning: Will reset current map paint status. (If that is the intended effect, use RevertDetailMaps() instead)
    /// </summary>
    public async Task Refresh(bool isAsync = false) {
        if (!this) return;
        if (Application.isEditor) {
            initializing = false;
        }

        if (!Application.isPlaying) {
            SortGrassMeshes();
        }

        if (isActiveAndEnabled) {
            ReleaseAssets();

            await InitAsync(isAsync);
        }
    }

    public void RefreshMaterials() {
        foreach (var gMesh in terrainMeshes) {
            gMesh.GetResources(true);
        }
    }

    public delegate void GrassEvent();
    public GrassEvent OnInititialized;

    void Init() {
        InitAsync(false);
    }

    public void CheckTerrainMeshes() {
        if (terrainMeshes == null || terrainMeshes.Count == 0) {
            var gMesh = GetEmptyGrassMesh();

            //this takes the old serialized values and shoves them into a new gMesh for the refactored system
            //just kinda keeps it compatible when people update
            gMesh.instanceCount = _instanceCount;
            gMesh.grassMesh = grassMesh;
            gMesh.terrainObject = terrainObject;
            gMesh.terrainTransform = terrainTransform;
            gMesh.grassMaterial = grassMaterial;
            gMesh.colorMap = colorMap;
            gMesh.paramMap = paramMap;
            gMesh.typeMap = typeMap;

            gMesh.grassPerTri = grassPerTri;
            gMesh.normalizeMaxRatio = normalizeMaxRatio;
            gMesh.renderType = renderType;
            gMesh.chunksX = chunksX;
            gMesh.chunksY = chunksY;
            gMesh.chunksZ = chunksZ;


#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif

            terrainMeshes = new List<GrassMesh>() { gMesh };
        }
    }


    //-----------------------------------------------------------------------------------------
    //----------------------------------------INIT---------------------------------------
    //-----------------------------------------------------------------------------------------
    async void StartupInit() {
        CheckTerrainMeshes();

        if (hasRequiredAssets) {
            Task initTask = InitAsync(Application.isPlaying);

            while (!initTask.IsCompleted && !gameStarted) {
                await Task.Delay(10);
            }

            if (!asyncInitialization) processAsync = false;
        }
    }

    bool initialized = false;
    bool initializing = false;
    public static bool processAsync;
    async Task InitAsync(bool isAsync = true) {

        processAsync = isAsync;

        CheckTerrainMeshes();

        if (!hasRequiredAssets) {
            Debug.LogError("GrassFlow: Not all required assets assigned in the inspector!");
            return;
        }

        if (!isActiveAndEnabled) return;
        if (initializing) return;

        initializing = true;

        await GetWaitForCullingTask();

        try {

            CheckRippleBuffers();


            Camera mainCam = Camera.main;
            if (mainCam && !cameraCulls.ContainsKey(mainCam)) {
                await Task.Run(() => {
                    cullResults = new CameraCullResults(mainCam, terrainMeshes, this);
                    cameraCulls.Add(mainCam, cullResults);
                });
            }

            if (!this) {
                //check if the object is destroyed and make sure we dont weirdly hook into a destroyed renderer instance thingy idk its weird
                initializing = false;
                return;
            }

            GetResources(false);

            HookRender();


            IEnumerable<GrassMesh> gMeshesToLoad;
            if (mainCam) {
                //sort the meshes by distance so that visually the closest ones load in first if there are a lot
                Vector3 camPos = mainCam.transform.position;
                gMeshesToLoad = terrainMeshes.OrderByDescending(x => x.worldBounds.SqrDistance(camPos));
            } else {
                gMeshesToLoad = terrainMeshes;
            }

            foreach (var gMesh in gMeshesToLoad) {

                gMesh.GetResources();
                gMesh.MapSetup();

                await LoadChunksForMesh(gMesh);

                if (!this) {
                    initializing = false;
                    UnHookRender();
                    return;
                }
            }


            initialized = true;

        } catch (System.Exception ex) {
            Debug.LogException(ex);
        }

        //print("init: " + this);
        initializing = false;

        if (initialized) {
            OnInititialized?.Invoke();
        }
    }


    void CheckIndirectInstancingArgs() {
        if (useIndirectInstancing) {
            if (terrainMeshes != null) {
                foreach (var chunk in terrainMeshes) {
                    chunk.SetIndirectArgs();
                }
            }
        } else {
            ReleaseIndirectArgsBuffers();
        }
    }

    void ReleaseIndirectArgsBuffers() {
        if (terrainMeshes != null) {
            foreach (var chunk in terrainMeshes) {
                chunk.ReleaseIndirectArgsBuffers();
            }
        }
    }


    public GrassMesh GetEmptyGrassMesh() {

        GrassMesh gMesh = new GrassMesh() {
            owner = this,
        };

        return gMesh;
    }

    /// <summary>
    /// Use this to add grass meshes at runtime.
    /// </summary>
    public async Task<GrassMesh> AddMesh(Mesh addMesh, Transform transform, Material grassMat,
        Texture2D colorMap = null, Texture2D paramMap = null, Texture2D typeMap = null,
        int instanceCount = 30, int grassPerTri = 3, float normalizeMaxRatio = 12f,
        int chunksX = 5, int chunksY = 1, int chunksZ = 5, bool isAsync = true) {

        return await AddGrassMesh(addMesh, null, transform, grassMat, colorMap, paramMap, typeMap,
            instanceCount, grassPerTri, normalizeMaxRatio, chunksX, chunksY, chunksZ, isAsync);
    }

    /// <summary>
    /// Use this to add grass meshes at runtime.
    /// </summary>
    public async Task<GrassMesh> AddTerrain(Terrain addTerrain, Transform transform, Material grassMat,
        Texture2D colorMap = null, Texture2D paramMap = null, Texture2D typeMap = null,
        int instanceCount = 30, int grassPerTri = 3, float normalizeMaxRatio = 12f,
        int chunksX = 5, int chunksY = 1, int chunksZ = 5, bool isAsync = true) {

        return await AddGrassMesh(null, addTerrain, transform, grassMat, colorMap, paramMap, typeMap,
            instanceCount, grassPerTri, normalizeMaxRatio, chunksX, chunksY, chunksZ, isAsync);
    }

    async Task<GrassMesh> AddGrassMesh(Mesh addMesh, Terrain addTerrain, Transform transform, Material grassMat,
        Texture2D colorMap = null, Texture2D paramMap = null, Texture2D typeMap = null,
        int instanceCount = 30, int grassPerTri = 3, float normalizeMaxRatio = 12f,
        int chunksX = 5, int chunksY = 1, int chunksZ = 5, bool isAsync = true) {

        processAsync = isAsync;

        GrassMesh gMesh = GetEmptyGrassMesh();

        gMesh.instanceCount = instanceCount;
        gMesh.grassPerTri = grassPerTri;
        gMesh.normalizeMaxRatio = normalizeMaxRatio;

        gMesh.grassMesh = addMesh;
        gMesh.terrainObject = addTerrain;
        gMesh.grassMaterial = grassMat;
        gMesh.terrainTransform = transform;

        gMesh.colorMap = colorMap;
        gMesh.paramMap = paramMap;
        gMesh.typeMap = typeMap;

        gMesh.chunksX = chunksX;
        gMesh.chunksY = chunksY;
        gMesh.chunksZ = chunksZ;

        gMesh.renderType = addMesh ? GrassRenderType.Mesh : GrassRenderType.Terrain;

        gMesh.GetResources();
        gMesh.MapSetup();

        await LoadChunksForMesh(gMesh);

        terrainMeshes.Add(gMesh);

        return gMesh;
    }


    public void RemoveMesh(Mesh mesh) {

        if (terrainMeshes == null) return;
        for (int i = 0; i < terrainMeshes.Count; i++) {
            if (terrainMeshes[i].grassMesh == mesh) {
                RemoveGrassMesh(i);
                return;
            }
        }
    }

    public void RemoveTerrain(Terrain terrain) {

        if (terrainMeshes == null) return;
        for (int i = 0; i < terrainMeshes.Count; i++) {
            if (terrainMeshes[i].terrainObject == terrain) {
                RemoveGrassMesh(i);
                return;
            }
        }
    }

    public void RemoveGrassMesh(GrassMesh gMesh) {
        if (terrainMeshes == null) return;
        RemoveGrassMesh(terrainMeshes.IndexOf(gMesh));
    }

    public void RemoveGrassMesh(int idx) {
        if (terrainMeshes == null) return;
        if (idx >= 0) {
            //ClearCulledChunks();

            var mesh = terrainMeshes[idx];
            mesh.shouldDraw = false;
            terrainMeshes.RemoveAt(idx);
            mesh.Dispose();
        }
    }

    public async Task LoadChunksForMesh(GrassMesh gMesh, bool isAsync = false) {
        processAsync = isAsync;
        await LoadChunksForMesh(gMesh);
    }

    async Task LoadChunksForMesh(GrassMesh gMesh) {

        gMesh.chunks = null;

        float bHeight = gMesh.drawnMat.GetFloat("bladeHeight");

        if (gMesh.renderType == GrassRenderType.Mesh) {

            gMesh.chunks = await MeshChunker.ChunkMesh(gMesh, bHeight, normalizeMeshDensity);

        } else {

            gMesh.chunks = await MeshChunker.ChunkTerrain(gMesh, terrainExpansion, bHeight);

            SetGrassMeshTerrainData(gMesh);

            if (Application.isPlaying && discardEmptyChunks) DiscardUnusedChunks();

        }

        await gMesh.Update(processAsync);

        if (useIndirectInstancing) {
            gMesh.SetIndirectArgs();
        } else {
            gMesh.ReleaseIndirectArgsBuffers();
        }
    }

    public GrassMesh GetSelectedGrassMesh() {

        CheckTerrainMeshes();

        if (selectedIdx >= terrainMeshes?.Count) {
            selectedIdx = 0;
        }

        if (selectedIdx >= terrainMeshes?.Count) {
            return null;
        }

        return terrainMeshes[selectedIdx];
    }

    void SortGrassMeshes() {
        if (terrainMeshes == null) return;

#if UNITY_EDITOR
        //this is dumb but if i dont record this then if you try to undo itll break things
        UnityEditor.Undo.RecordObject(this, "GrassFlow");
#endif

        var prevSelMesh = GetSelectedGrassMesh();
        terrainMeshes.Sort((a, b) => a.name.CompareTo(b.name));
        selectedIdx = terrainMeshes.IndexOf(prevSelMesh);

#if UNITY_EDITOR
        UnityEditor.Undo.FlushUndoRecordObjects();
#endif
    }


    public void SetGrassMeshTerrainData(GrassMesh gMesh) {

        if (!gMesh.terrainHeightmap) {
            gMesh.terrainHeightmap = TextureCreator.GetTerrainHeightMap(gMesh.terrainObject, gfComputeShader, heightKernel, true);
        }


        if (useTerrainNormalMap && !gMesh.terrainNormalMap) {
            gMesh.terrainNormalMap = TextureCreator.GetTerrainNormalMap(gMesh.terrainObject, gfComputeShader, gMesh.terrainHeightmap, normalKernel);
        }
    }

    void DiscardUnusedChunks() {

        foreach (var gMesh in terrainMeshes) {

            Texture paramTex;
            if (!(paramTex = gMesh.paramMapRT)) paramTex = gMesh.paramMap;

            if (!gMesh.hasRequiredAssets || !paramTex
                || gMesh.renderType != GrassRenderType.Terrain) return;

            gfComputeShader.SetVector("chunkDims", new Vector4(gMesh.chunksX, gMesh.chunksZ));
            gfComputeShader.SetTexture(emptyChunkKernel, "paramMap", paramTex);

            var terrainChunks = gMesh.chunks;

            ComputeBuffer chunkResultsBuffer = new ComputeBuffer(terrainChunks.Length, sizeof(int));
            int[] chunkResults = new int[terrainChunks.Length];
            chunkResultsBuffer.SetData(chunkResults);
            gfComputeShader.SetBuffer(emptyChunkKernel, "chunkResults", chunkResultsBuffer);

            gfComputeShader.Dispatch(emptyChunkKernel, Mathf.CeilToInt(paramTex.width / paintThreads), Mathf.CeilToInt(paramTex.height / paintThreads), 1);

            chunkResultsBuffer.GetData(chunkResults);
            chunkResultsBuffer.Release();

            List<MeshChunker.MeshChunk> resultChunks = new List<MeshChunker.MeshChunk>();
            for (int i = 0; i < terrainChunks.Length; i++) {
                if (chunkResults[i] > 0) resultChunks.Add(terrainChunks[i]);
            }

            gMesh.chunks = resultChunks.ToArray();
        }
    }

    new void Destroy(Object obj) {
        if (Application.isPlaying) {
            Object.Destroy(obj);
        } else {
            DestroyImmediate(obj);
        }
    }



    void ReleaseAssets() {

        ReleaseDetailMapRTs();

        if (terrainMeshes != null) {
            foreach (var gMesh in terrainMeshes) {

                gMesh.ReleaseAssets();

                gMesh.ReleaseIndirectArgsBuffers();

                gMesh.Dispose();
            }
        }

        initialized = false;
    }

    void ReleaseDetailMapRTs() {
        foreach (var gMesh in terrainMeshes) gMesh.ReleaseDetailMapRTs();
    }

    /// <summary>
    /// Reverts unsaved paints to grass color and paramter maps.
    /// </summary>
    public void RevertDetailMaps() {
        foreach (var gMesh in terrainMeshes) gMesh.RefreshDetailMaps();
    }

    void MapSetup() {
        foreach (var gMesh in terrainMeshes) gMesh.MapSetup();
    }

    /// <summary>
    /// Updates the transformation matrices used to render grass.
    /// You should call this if the object the grass is attached to moves.
    /// </summary>
    public async void UpdateTransform(bool isAsync = false) {

        if (terrainMeshes == null) return;
        foreach (var gMesh in terrainMeshes) {
            if (!gMesh.terrainTransform) continue;
            await gMesh.UpdateTransform(isAsync);
        }
    }


    const int maxRipples = 128;
    const int maxForces = 64;

    void CheckRippleBuffers() {
        if (rippleBuffer == null) {
            rippleBuffer = new ComputeBuffer(maxRipples, Marshal.SizeOf(typeof(RippleData)));
        }
        if (forcesBuffer == null) {
            forcesBuffer = new ComputeBuffer(maxForces, Marshal.SizeOf(typeof(RippleData)));
        }
        if (forcesArray == null) {
            forcesArray = new RippleData[maxForces];
        }
        if (forceClassArray == null) {
            forceClassArray = new GrassForce[maxForces];
        }
        if (counterBuffer == null) {
            counterBuffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(Vector4)));
            counterBuffer.SetData(new Vector4[] { Vector4.zero });
        }
    }

    void GetResources(bool alsoGetMeshResources) {
        if (alsoGetMeshResources && terrainMeshes != null) {
            foreach (var gMesh in terrainMeshes) {
                gMesh.GetResources();
            }
        }

        if (!gfComputeShader) gfComputeShader = Resources.Load<ComputeShader>("GrassFlow/GrassFlowCompute");
        addRippleKernel = gfComputeShader.FindKernel("AddRipple");
        updateRippleKernel = gfComputeShader.FindKernel("UpdateRipples");
        noiseKernel = gfComputeShader.FindKernel("NoiseMain");
        normalKernel = gfComputeShader.FindKernel("NormalsMain");
        heightKernel = gfComputeShader.FindKernel("HeightmapMain");
        emptyChunkKernel = gfComputeShader.FindKernel("EmptyChunkDetect");

        if (!paintShader) paintShader = Resources.Load<ComputeShader>("GrassFlow/GrassFlowPainter");
        //if(!paintMat) paintMat = new Material(paintShader);
        paintKernel = paintShader.FindKernel("PaintKernel");
        splatKernel = paintShader.FindKernel("ApplySplatTex");


        if (!noise3DTexture) {
            noise3DTexture = Resources.Load<RenderTexture>("GrassFlow/GF3DNoise");
            noise3DTexture.Release();
            noise3DTexture.enableRandomWrite = true;
            noise3DTexture.Create();

            //compute 3d noise
            gfComputeShader.SetTexture(noiseKernel, "NoiseResult", noise3DTexture);
            gfComputeShader.Dispatch(noiseKernel, noise3DTexture.width / 8, noise3DTexture.height / 8, noise3DTexture.volumeDepth / 8);
        }
    }



    struct RippleData {
        internal Vector4 pos; // w = strength
        internal Vector4 drssParams;//xyzw = decay, radius, sharpness, speed 
    }


    private void Update() {
        UpdateRipples();

#if UNITY_EDITOR
        if (updateBuffers && hasRequiredAssets)
            UpdateShaders();

        CheckInspectorPaint();
#endif
    }


#if UNITY_EDITOR
    bool shouldPaint;
    System.Action paintAction;

    void CheckInspectorPaint() {
        if (shouldPaint && paintAction != null) {
            paintAction.Invoke();
            shouldPaint = false;
        }
    }

    //its really stupid that this has to exist but it do be that way
    //its explained why in GrassFlowInspector
    //used only for painting during scene gui
    void InspectorSetPaintAction(System.Action action) {
        paintAction = action;
        shouldPaint = true;
    }
#endif


    /// <summary>
    /// This basically sets all required variables and textures to the various shaders to make them run.
    /// You might need to call this after changing certain variables/textures to make them take effect.
    /// </summary>
    public void UpdateShaders() {
        if (terrainMeshes == null) return;
        foreach (var gMesh in terrainMeshes) {
            UpdateShader(gMesh);
        }
    }

    public void UpdateShader(GrassMesh gMesh) {

        Material grassMat = gMesh.drawnMat;

        if (!grassMat) return;

        if (rippleBuffer != null && counterBuffer != null) {
            grassMat.SetBuffer(rippleBufferID, rippleBuffer);
            grassMat.SetBuffer(rippleCountID, counterBuffer);

            try {
                gfComputeShader.SetBuffer(addRippleKernel, rippleBufferID, rippleBuffer);
                gfComputeShader.SetBuffer(updateRippleKernel, rippleBufferID, rippleBuffer);
                gfComputeShader.SetBuffer(addRippleKernel, rippleCountID, counterBuffer);
                gfComputeShader.SetBuffer(updateRippleKernel, rippleCountID, counterBuffer);
            } catch { }
        }

        if (forcesBuffer != null) {
            grassMat.SetBuffer(forcesBufferID, forcesBuffer);
        }

        if (noise3DTexture) {
            grassMat.SetTexture(_NoiseTexID, noise3DTexture);
        }

        gMesh.UpdateMaps(enableMapPainting);

        gMesh.UpdateTerrain();

        //a bit weird but saves having to do an extra division in the shader ¯\_(ツ)_/¯
        float numGrassAtlasTexes = grassMat.GetFloat(numTexturesID);
        grassMat.SetFloat(numTexturesPctUVID, 1.0f / numGrassAtlasTexes);
    }



    //----------------------------------
    //MAIN RENDER FUNCTION--------------
    //----------------------------------

    CameraCullResults cullResults;

#if !GRASSFLOW_SRP
    void Render(Camera cam) {
#else
    void Render(ScriptableRenderContext context, Camera cam) {
#endif


        if ((cam.cullingMask & (1 << renderLayer)) == 0) {
            //don't even bother if the cameras cullingmask doesn't contain the renderlayer
            return;
        }



        if (!cameraCulls.TryGetValue(cam, out cullResults)) {
            cullResults = new CameraCullResults(cam, terrainMeshes, this);
            cameraCulls.Add(cam, cullResults);
        }

#if UNITY_EDITOR
        if (!this || !isActiveAndEnabled) {
            UnHookRender();
            return;
        }
        //these arent really as much of an issue in a built game
#if UNITY_2018_3_OR_NEWER
        //make sure not to render grass in prefab stage unless its part of the prefab
        if(UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null
            && UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) == null) return;
#endif
        if (cam.cameraType == CameraType.Preview) return;
#endif


        var culledChunksList = cullResults.culledChunks;

        if (terrainMeshes == null) return;
        if (culledChunksList == null) return;

        if (cullResults.needsRunning) {

            cullResults.UpdatePos();

            cullResults.needsRunning = false;
            cullResults.asyncCullTask = Task.Run(() => {
                cullResults.RunCulling();
            });
        }

        if (useIndirectInstancing) {
            foreach (var gMesh in terrainMeshes) {
                gMesh.SetDrawmatObjMatrices();
            }
        }

        for (int i = 0; i < culledChunksList.Count; i++) {
            var cullChunk = culledChunksList[i];
            var chunk = cullChunk.parentChunk;
            var gMesh = chunk.parentMesh;

            if (!gMesh.shouldDraw) {
                continue;
            }
            if (!gMesh.mesh) {
                gMesh.shouldDraw = false;
                gMesh.chunks = null;
                continue;
            }

            chunk.propertyBlock.SetFloat(_instancePctID, cullChunk.bladePct);
            chunk.propertyBlock.SetFloat(_instanceLodID, cullChunk.bladeCnt);

            if (useIndirectInstancing) {
                chunk.instanceCount = (uint)cullChunk.instancesToRender;
                Graphics.DrawMeshInstancedIndirect(gMesh.mesh, 0, gMesh.drawnMat, chunk.worldBounds, chunk.indirectArgs, 0, chunk.propertyBlock, shadowMode, receiveShadows, renderLayer, cullResults.cam);
            } else {

                gMesh.mesh.bounds = chunk.meshBounds;

                Graphics.DrawMeshInstanced(gMesh.mesh, chunk.submeshIdx, gMesh.drawnMat, gMesh.matrices, cullChunk.instancesToRender,
                    chunk.propertyBlock, shadowMode, receiveShadows, renderLayer, cullResults.cam);
            }
        }

    }

    //--------------------------------    
    //CULL STUFF---------------------
    //--------------------------------

    Dictionary<Camera, CameraCullResults> cameraCulls = new Dictionary<Camera, CameraCullResults>();
    class CameraCullResults {

        public Camera cam;
        public Vector3 pos;

        public GrassFlowRenderer grassFlow;

        public List<GrassCullChunk> culledChunks = new List<GrassCullChunk>();
        List<GrassCullChunk> culledChunksDblBfr = new List<GrassCullChunk>();

        public Task asyncCullTask;
        public bool needsRunning;

        public CameraCullResults(Camera inCam, List<GrassMesh> gMeshes, GrassFlowRenderer gf) {
            cam = inCam;
            grassFlow = gf;

            culledChunks = new List<GrassCullChunk>(gMeshes.Sum(x => x.chunksX * x.chunksX * x.chunksX));
            culledChunksDblBfr = new List<GrassCullChunk>(culledChunks.Capacity);

            needsRunning = true;
        }

        public void UpdatePos() {
            pos = cam.transform.position;
        }

        void SwapChunkBuffers() {
            var tmp = culledChunks;
            culledChunks = culledChunksDblBfr;
            culledChunksDblBfr = tmp;
        }


        public void RunCulling() {


            if (culledChunks == null) return;

            culledChunksDblBfr.Clear();


            foreach (var gMesh in grassFlow.terrainMeshes) {

                Vector3 lodParams = grassFlow.lodParams;
                float maxRenderDistSqr = grassFlow.maxRenderDistSqr;

                float instanceMult = gMesh.instanceCount;

                if (gMesh.matrices == null) continue;
                if (gMesh.chunks == null) continue;

                foreach (var grassChunk in gMesh.chunks) {



                    float camDist = grassChunk.worldBounds.SqrDistance(pos);
                    if (camDist > maxRenderDistSqr) {
                        continue;
                    }

                    GrassCullChunk chunk;
                    chunk.parentChunk = grassChunk;

                    camDist = Mathf.Sqrt(camDist) - lodParams.z;
                    if (camDist <= 0f) camDist = 0.0001f;
                    camDist = 1.0f / camDist;

                    float bladePct = Mathf.Pow(camDist * lodParams.x, lodParams.y);

                    chunk.bladePct = Mathf.Clamp01(bladePct);
                    chunk.bladeCnt = chunk.bladePct * instanceMult;

                    if (chunk.bladeCnt > instanceMult) chunk.bladeCnt = instanceMult;

                    chunk.instancesToRender = Mathf.CeilToInt(chunk.bladeCnt);

                    culledChunksDblBfr.Add(chunk);
                }

                gMesh.shouldDraw = true;
            }

            SwapChunkBuffers();
            needsRunning = true;
        }
    }

    

    async Task GetWaitForCullingTask() {
        foreach (var cull in cameraCulls) {
            var cTask = cull.Value.asyncCullTask;
            if (cTask != null && cTask.Status != TaskStatus.Faulted) await cTask;
        }
    }

    void ClearAllCulledChunks() {
        foreach (var cull in cameraCulls) {
            cull.Value.culledChunks.Clear();
        }
    }

    public async void ClearCulledChunks() {

        await GetWaitForCullingTask();

        ClearAllCulledChunks();
    }

    //struct for use in culling
    public struct GrassCullChunk {
        public MeshChunker.MeshChunk parentChunk;
        public int instancesToRender;
        public float bladePct;
        public float bladeCnt;
    }




    //--------------------------------    
    //RIPPLES-------------------------
    //--------------------------------
    bool gameStarted = false;
    private void LateUpdate() {
        gameStarted = true;
        runRipple = true;
    }

    void UpdateRipples() {
        if (runRipple && updateRipples) {
            runRipple = false;
            gfComputeShader.SetFloat(ripDeltaTimeHash, Time.deltaTime);
            gfComputeShader.Dispatch(updateRippleKernel, 1, 1, 1);
        }

        UpdateForces();
    }


    /// <summary>
    /// Adds a ripple into the ripple buffer that affects all grasses.
    /// Ripples are just that, ripples that animate accross the grass, a simple visual effect.
    /// </summary>
    /// <param name="pos">World position the ripple is placed at.</param>
    /// <param name="strength">How forceful the ripple is.</param>
    /// <param name="decayRate">How quickly the ripple dissipates.</param>
    /// <param name="speed">How fast the ripple moves across the grass.</param>
    /// <param name="startRadius">Start size of the ripple.</param>
    /// <param name="sharpness">How much this ripple appears like a ring rather than a circle.</param>
    public static void AddRipple(Vector3 pos, float strength = 1f, float decayRate = 2.5f, float speed = 25f, float startRadius = 0f, float sharpness = 0f) {
        if (!gfComputeShader) return;

        gfComputeShader.SetVector("pos", new Vector4(pos.x, pos.y, pos.z, strength));
        gfComputeShader.SetVector("drssParams", new Vector4(decayRate, startRadius, sharpness, speed));
        gfComputeShader.Dispatch(addRippleKernel, 1, 1, 1);
        updateRipples = true;
    }

    /// <summary>
    /// Adds a ripple into the ripple buffer that affects all grasses.
    /// Ripples are just that, ripples that animate accross the grass, a simple visual effect.
    /// </summary>
    /// <param name="pos">World position the ripple is placed at.</param>
    /// <param name="strength">How forceful the ripple is.</param>
    /// <param name="decayRate">How quickly the ripple dissipates.</param>
    /// <param name="speed">How fast the ripple moves across the grass.</param>
    /// <param name="startRadius">Start size of the ripple.</param>
    /// <param name="sharpness">How much this ripple appears like a ring rather than a circle.</param>
    public void AddARipple(Vector3 pos, float strength = 1f, float decayRate = 2.5f, float speed = 25f, float startRadius = 0f, float sharpness = 0f) {
        AddRipple(pos, strength, decayRate, speed, startRadius, sharpness);
    }




    //--------------------------------------------------------------------------------
    //------------------------FORCES---------------------------------------
    //--------------------------------------------------------------------------------

    /// <summary>
    /// Intermediary class to handle point source grass forces.
    /// <para>Do not manually create instances of this class. Instead, use GrassFlowRenderer.AddGrassForce</para>
    /// </summary>
    public class GrassForce {

        public int index = -1;

        public bool added { get; private set; }

        public void Add() {

            if (forcesCount >= maxForces) {
                return;
            }

            if (added) {
                return;
            }

            index = forcesCount;
            forceClassArray[forcesCount] = this;
            forcesCount++;
            added = true;
            forcesDirty = true;
        }

        public void Remove() {

            if (!added) {
                return;
            }

            if (forcesArray == null) {
                forcesCount = 0;
                return;
            }

            forcesCount--;
            forcesArray[index] = forcesArray[forcesCount];
            GrassForce swapForce = forceClassArray[forcesCount];
            swapForce.index = index;
            forceClassArray[index] = swapForce;


            index = -1;
            added = false;

            forcesDirty = true;
        }

        public Vector3 position {
            get {
                return forcesArray[index].pos;
            }
            set {
                forcesArray[index].pos = value;
                forcesDirty = true;
            }
        }

        public float radius {
            get {
                return forcesArray[index].drssParams.y;
            }
            set {
                forcesArray[index].drssParams.y = value;
                forcesArray[index].drssParams.z = 1f / (value * value);
                forcesDirty = true;
            }
        }

        public float strength {
            get {
                return forcesArray[index].drssParams.w;
            }
            set {
                forcesArray[index].drssParams.w = value;
                forcesDirty = true;
            }
        }
    }

    /// <summary>
    /// Adds a point-source constant force that pushes all grasses.
    /// <para>Store the returned force and change its values to update it.</para>
    /// </summary>
    public GrassForce AddForce(Vector3 pos, float radius, float strength) {
        return AddGrassForce(pos, radius, strength);
    }

    /// <summary>
    /// Removes the given GrassForce.
    /// </summary>
    public void RemoveForce(GrassForce force) {
        RemoveGrassForce(force);
    }
    /// <summary>
    /// Removes the given GrassForce.
    /// </summary>
    public static void RemoveGrassForce(GrassForce force) {
        force.Remove();
    }

    /// <summary>
    /// Adds a point-source constant force that pushes all grasses.
    /// <para>Store the returned force and change its values to update it.</para>
    /// </summary>
    public static GrassForce AddGrassForce(Vector3 pos, float radius, float strength) {
        if (forcesArray == null) {
            return null;
        }

        if (forcesCount >= maxForces) {
            return null;
        }

        GrassForce force = new GrassForce() {
            index = forcesCount,
            position = pos,
            radius = radius,
            strength = strength,
        };

        force.Add();

        return force;
    }


    void UpdateForces() {
        if (forcesDirty) {
            //print("update forces: " + forcesCount);
            forcesBuffer.SetData(forcesArray, 0, 0, forcesCount);
            forcesDirty = false;

            foreach (var gMesh in terrainMeshes) {
                gMesh.drawnMat?.SetInt(forcesCountID, forcesCount);
            }
        }
    }


    //--------------------------------    
    //PAINTING------------------------
    //--------------------------------    
    static int mapToPaintID = Shader.PropertyToID("mapToPaint");
    static int brushTextureID = Shader.PropertyToID("brushTexture");
    const float paintThreads = 8f;

    public GrassMesh GetGrassMeshFromTransform(Transform t) {
        foreach (var gMesh in terrainMeshes) {
            if (gMesh.terrainTransform == t) return gMesh;
        }

        return null;
    }

    /// <summary>
    /// Sets the texture to be used when calling paint functions.
    /// </summary>
    public static void SetPaintBrushTexture(Texture2D brushTex) {
        if (paintShader) paintShader.SetTexture(paintKernel, brushTextureID, brushTex);
    }

    /// <summary>
    /// Paints color onto the colormap.
    /// enableMapPainting needs to be turned on for this to work.
    /// Uses a global texture as the brush texture, should be set via SetPaintBrushTexture(Texture2D brushTex).
    /// </summary>
    /// <param name="texCoord">texCoord to paint at, usually obtained by a raycast.</param>
    /// <param name="clampRange">Clamp the painted values between this range. Not really used for colors but exists just in case.
    /// Should be set to 0 to 1 or greater than 1 for HDR colors.</param>
    public void PaintColor(GrassMesh gMesh, Vector2 texCoord, float brushSize, float brushStrength, Color colorToPaint, Vector2 clampRange) {
        PaintDispatch(texCoord, brushSize, brushStrength, colorToPaint, gMesh.colorMapRT, clampRange, 0f);
    }

    /// <summary>
    /// Paints parameters onto the paramMap.
    /// enableMapPainting needs to be turned on for this to work.
    /// Uses a global texture as the brush texture, should be set via SetPaintBrushTexture(Texture2D brushTex).
    /// </summary>
    /// <param name="texCoord">texCoord to paint at, usually obtained by a raycast.</param>
    /// <param name="densityAmnt">Amount density to paint.</param>
    /// <param name="heightAmnt">Amount height to paint.</param>
    /// <param name="flattenAmnt">Amount flatten to paint.</param>
    /// <param name="windAmnt">Amount wind to paint.</param>
    /// <param name="clampRange">Clamp the painted values between this range. Valid range for parameters is 0 to 1.</param>
    public void PaintParameters(GrassMesh gMesh, Vector2 texCoord, float brushSize, float brushStrength, float densityAmnt, float heightAmnt, float flattenAmnt, float windAmnt, Vector2 clampRange) {
        PaintDispatch(texCoord, brushSize, brushStrength, new Vector4(densityAmnt, heightAmnt, flattenAmnt, windAmnt), gMesh.paramMapRT, clampRange, 1f);
    }


    /// <summary>
    /// A more manual paint function that you most likely don't want to use.
    /// It's mostly only exposed so that the GrassFlowInspector can use it. But maybe you want to too, I'm not the boss of you.
    /// You could use this to paint onto your own RenderTextures.
    /// </summary>
    /// <param name="blendMode">Controls blend type: 0 for lerp towards, 1 for additive</param>
    public static void PaintDispatch(Vector2 texCoord, float brushSize, float brushStrength, Vector4 blendParams, RenderTexture mapRT, Vector2 clampRange, float blendMode) {
        if (!paintShader || !mapRT) return;

        //print(brushSize + " : "+ brushStrength + " : " + texCoord + " : " + blendParams + " : " +clampRange + " : " + blendMode);
        //srsBrushParams = (strength, radius, unused, alpha controls type/ 0 for lerp towards, 1 for additive)
        paintShader.SetVector(srsBrushParamsID, new Vector4(brushStrength, brushSize * 0.05f, 0, blendMode));
        paintShader.SetVector(clampRangeID, clampRange);

        paintShader.SetVector(brushPosID, texCoord);
        paintShader.SetVector(blendParamsID, blendParams);

        PaintShaderExecute(mapRT, paintKernel);
        //paintShader.Dispatch(paintKernel, Mathf.CeilToInt(mapRT.width / paintThreads), Mathf.CeilToInt(mapRT.height / paintThreads), 1);
    }

    static void PaintShaderExecute(RenderTexture mapRT, int pass) {
        //paintMat.SetTexture(mapToPaintID, mapRT);
        paintShader.SetTexture(pass, mapToPaintID, mapRT);

        RenderTexture tmpRT = RenderTexture.GetTemporary(mapRT.width, mapRT.height, 0, mapRT.format);
        if (!tmpRT.IsCreated()) {
            //I think theres some kind of bug on older versions of unity where sometimes,
            //at least in certain situations, RenderTexture.GetTemporary() returns you
            //a texture that hasn't actually been created. Go figure.
            //It'll still work fine with Graphics.Blit, but it won't work with Graphics.CopyTexture()
            //unless we make sure its created first like this
            //this will only happen once usually, as internally unity will reuse this texture next time we ask for it.
            //but will be discarded after a few frames of un-use
            tmpRT.Create();
        }
        //Graphics.CopyTexture(mapRT, tmpRT); //copytexture for some reason didnt work on URP last time i checked
        Graphics.Blit(mapRT, tmpRT);
        paintShader.SetTexture(pass, tmpMapRTID, tmpRT);

        paintShader.Dispatch(pass, Mathf.CeilToInt(mapRT.width / paintThreads), Mathf.CeilToInt(mapRT.height / paintThreads), 1);
        //Graphics.Blit(tmpRT, mapRT, paintMat, pass);
        RenderTexture.ReleaseTemporary(tmpRT);
    }

    /// <summary>
    /// Automatically controls grass density based on a splat layer from terrain data.
    /// </summary>
    /// <param name="splatLayer">Zero based index of the splat layer from the terrain to use.</param>
    /// <param name="mode">Controls how the tex is applied. 0 = additive, 1 = subtractive, 2 = replace.</param>
    /// <param name="splatTolerance">Controls opacity tolerance.</param>
    public void ApplySplatTex(GrassMesh gMesh, int splatLayer, int mode, float splatTolerance) {
        int channel = splatLayer % 4;
        int texIdx = splatLayer / 4;


        ApplySplatTex(gMesh.terrainObject.terrainData.alphamapTextures[texIdx], gMesh.paramMapRT, channel, mode, splatTolerance);
    }

    /// <summary>
    /// Automatically controls grass density based on a splat tex.
    /// </summary>
    /// <param name="splatAlphaMap">The particular splat alpha map texture that has the desired splat layer on it.</param>
    /// <param name="channel">Zero based index of the channel of the texture that represents the splat layer.</param>
    /// <param name="mode">Controls how the tex is applied. 0 = additive, 1 = subtractive, 2 = replace.</param>
    /// <param name="splatTolerance">Controls opacity tolerance.</param>
    public void ApplySplatTex(Texture2D splatAlphaMap, RenderTexture paramMapRT, int channel, int mode, float splatTolerance) {
        if (!enableMapPainting || !paramMapRT) {
            Debug.LogError("Couldn't apply splat tex, map painting not enabled!");
            return;
        }

        paintShader.SetTexture(splatKernel, "splatTex", splatAlphaMap);
        paintShader.SetTexture(splatKernel, "mapToPaint", paramMapRT);

        paintShader.SetInt("splatMode", mode);
        paintShader.SetInt("splatChannel", channel);

        paintShader.SetFloat("splatTolerance", splatTolerance);

        PaintShaderExecute(paramMapRT, splatKernel);
        //paintShader.Dispatch(splatKernel, Mathf.CeilToInt(paramMapRT.width / paintThreads), Mathf.CeilToInt(paramMapRT.width / paintThreads), 1);
    }


    //
    //Shader Property IDs
    //
    //base shader
    static int rippleBufferID = Shader.PropertyToID("rippleBuffer");
    static int forcesBufferID = Shader.PropertyToID("forcesBuffer");
    static int rippleCountID = Shader.PropertyToID("rippleCount");
    static int forcesCountID = Shader.PropertyToID("forcesCount");
    static int _NoiseTexID = Shader.PropertyToID("_NoiseTex");
    static int numTexturesID = Shader.PropertyToID("numTextures");
    static int numTexturesPctUVID = Shader.PropertyToID("numTexturesPctUV");
    //
    //instance props
    static int _instancePctID = Shader.PropertyToID("_instancePct");
    static int _instanceLodID = Shader.PropertyToID("_instanceLod");
    //
    //painting
    static int srsBrushParamsID = Shader.PropertyToID("srsBrushParams");
    static int clampRangeID = Shader.PropertyToID("clampRange");
    static int brushPosID = Shader.PropertyToID("brushPos");
    static int blendParamsID = Shader.PropertyToID("blendParams");
    static int tmpMapRTID = Shader.PropertyToID("tmpMapRT");

    static void ReleaseBuffers() {
        if (rippleBuffer != null) rippleBuffer.Release();
        if (forcesBuffer != null) forcesBuffer.Release();
        if (counterBuffer != null) counterBuffer.Release();
        rippleBuffer = null;
        counterBuffer = null;
        forcesBuffer = null;
        forcesArray = null;
        forceClassArray = null;
    }


    private void OnDestroy() {
        //double up to be safe
        UnHookRender();
        UnHookRender();

        ReleaseAssets();

        ReleaseBuffers();
    }
}
