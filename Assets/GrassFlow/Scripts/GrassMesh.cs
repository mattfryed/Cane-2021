using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

using Object = UnityEngine.Object;
using Action = System.Action;
using GrassRenderType = GrassFlowRenderer.GrassRenderType;
using MeshChunk = GrassFlow.MeshChunker.MeshChunk;


namespace GrassFlow {

    [Serializable]
    public class GrassMesh {



        //serialized
        public GrassFlowRenderer owner;


        [Tooltip("Maximum number of instances to render. This number gets used with the LOD system to decrease number of instances in the distance.")]
        [SerializeField] private int _instanceCount = 30;
        public int instanceCount {
            get { return _instanceCount; }
            set {
                _instanceCount = value;
                UpdateTransform(false);
            }
        }


        [Tooltip("Mode this grass is for. Mesh will attach grass to the triangles of a mesh, terrain will attach grass to surface of a unity terrain object.")]
        public GrassRenderType renderType;

        [Tooltip("Mesh to attach grass to in mesh mode.")]
        public Mesh grassMesh;

        [Tooltip("Terrain object to attach grass to in terrain mode.")]
        public Terrain terrainObject;

        [Tooltip("Transform that the grass belongs to.")]
        public Transform terrainTransform;

        [Tooltip("Material to use to render the grass. The material should use one of the grassflow shaders.")]
        public Material grassMaterial;

        [Tooltip("Texture that controls grass color. The alpha channel of this texture is used to control how the color gets applied. " +
        "If alpha is 1, the color is also multiplied by material color, if 0, material color is ignored. Inbetween values work too.")]
        public Texture2D colorMap;

        [Tooltip("Texture that controls various parameters of the grass. Red channel = density. Green channel = height, Blue channel = flattenedness. Alpha channel = wind strength.")]
        public Texture2D paramMap;

        [Tooltip("Texture that controls which texture to use from the atlas in the grass texture atlas (if using one). " +
            "NOTE: Read the documentation for information about how this texture works.")]
        public Texture2D typeMap;

        [Tooltip("Amount of grass to render per mesh triangle in mesh mode. Technically controls the amount of grass per instance, per tri, meaning maximum total grass per tri = " +
                    "GrassPerTri * InstanceCount.")]
        public int grassPerTri = 4;


        [Tooltip("Maximum ratio at which the largest triangle can be subdivided. Basically it just controls the subdivision density when attempting to normalize the mesh. " +
            "You probably want to set this as low as possible while still providing good results.")]
        public float normalizeMaxRatio = 12f;


        public int chunksX = 5;
        public int chunksY = 1;
        public int chunksZ = 5;



        //nonserialized
        [NonSerialized] public Material drawnMat;

        [NonSerialized] public bool shouldDraw;


        public Mesh mesh;
        [NonSerialized] public MeshChunk[] chunks;
        [NonSerialized] public Matrix4x4[] matrices;

        [NonSerialized] public RenderTexture terrainHeightmap;
        [NonSerialized] public Texture terrainNormalMap;

        [NonSerialized] public RenderTexture colorMapRT;
        [NonSerialized] public RenderTexture paramMapRT;
        [NonSerialized] public RenderTexture typeMapRT;

        public Vector2 colorMapHalfPixUV;
        public Vector2 paramMapHalfPixUV;
        public Vector2 typeMapHalfPixUV;



        public Bounds worldBounds;

        public string name {
            get {
                return (terrainTransform ? terrainTransform.name : "No Transform") + " : " +
                    (renderType == GrassRenderType.Mesh ? (grassMesh ? grassMesh.name : "No Mesh") : (terrainObject ? terrainObject.name : "No Terrain")) + " : " +
                    (grassMaterial ? grassMaterial.name : "No Mat");
            }
        }

        public bool hasRequiredAssets {
            get {
                bool sharedAssets = grassMaterial && terrainTransform;

                if (renderType == GrassRenderType.Mesh) {
                    return (sharedAssets && grassMesh);
                } else {
                    return (sharedAssets && terrainObject);
                }
            }
        }


        public void Dispose() {
            drawnMat = null;
            Destroy(mesh);
            mesh = null;
        }

        void Destroy(Object obj) {
            if (Application.isPlaying) {
                Object.Destroy(obj);
            } else {
                Object.DestroyImmediate(obj);
            }
        }


        //used internally by the inspector to refresh current mesh after changing settings
        public async void Reload() {
            //owner.ClearCulledChunks();
            ReleaseAssets();
            shouldDraw = false;
            GetResources(true);
            MapSetup();
            await owner.LoadChunksForMesh(this);
        }

        public void RefreshDetailMaps() {
            ReleaseDetailMapRTs();
            MapSetup();
        }

        public void ReleaseDetailMapRTs() {
            if (colorMapRT) colorMapRT.Release(); colorMapRT = null;
            if (paramMapRT) paramMapRT.Release(); paramMapRT = null;
            if (typeMapRT) typeMapRT.Release(); typeMapRT = null;
        }
        public void MapSetup() {

            //basically calculate what a half pixel offset would be in UV space
            if (colorMap) colorMapHalfPixUV = new Vector2(1f / colorMap.width, 1f / colorMap.height) * 0.5f;
            if (paramMap) paramMapHalfPixUV = new Vector2(1f / paramMap.width, 1f / paramMap.height) * 0.5f;
            if (typeMap) typeMapHalfPixUV = new Vector2(1f / typeMap.width, 1f / typeMap.height) * 0.5f;

            if (!owner.enableMapPainting) return;
            CheckMap(colorMap, ref colorMapRT, RenderTextureFormat.ARGB32);
            CheckMap(paramMap, ref paramMapRT, RenderTextureFormat.ARGB32);
            CheckMap(typeMap, ref typeMapRT, RenderTextureFormat.R8);
        }

        void CheckMap(Texture2D srcMap, ref RenderTexture outRT, RenderTextureFormat format) {
            if (srcMap && !outRT) {
                RenderTexture oldRT = RenderTexture.active;
                outRT = new RenderTexture(srcMap.width, srcMap.height, 0, format, RenderTextureReadWrite.Linear) {
                    enableRandomWrite = true, filterMode = srcMap.filterMode, wrapMode = srcMap.wrapMode, name = srcMap.name + "RT"
                };
                outRT.Create();
                Graphics.Blit(srcMap, outRT);
                RenderTexture.active = oldRT;
            }
        }

        public void ReleaseAssets() {
            if (terrainHeightmap) terrainHeightmap.Release();
            terrainHeightmap = null;

            if (terrainNormalMap && terrainNormalMap.GetType() == typeof(RenderTexture)) {
                (terrainNormalMap as RenderTexture).Release();
            }
            terrainNormalMap = null;
        }

        public void UpdateMaps(bool enableMapPainting) {
            if (enableMapPainting) {
                if (colorMapRT) drawnMat.SetTexture(colorMapID, colorMapRT);
                if (paramMapRT) drawnMat.SetTexture(dhfParamMapID, paramMapRT);
                if (typeMapRT) drawnMat.SetTexture(typeMapID, typeMapRT);
            } else {
                if (colorMap) drawnMat.SetTexture(colorMapID, colorMap);
                if (paramMap) drawnMat.SetTexture(dhfParamMapID, paramMap);
                if (typeMap) drawnMat.SetTexture(typeMapID, typeMap);
            }
        }

        public void UpdateTerrain() {

            if (!terrainObject) return;

            if (terrainHeightmap) drawnMat.SetTexture(terrainHeightMapID, terrainHeightmap);
            if (owner.useTerrainNormalMap && terrainNormalMap) drawnMat.SetTexture(terrainNormalMapID, terrainNormalMap);
            else drawnMat.SetTexture(terrainNormalMapID, null);

            Vector3 terrainScale = terrainObject.terrainData.size;
            drawnMat.SetVector(terrainSizeID, new Vector4(terrainScale.x, terrainScale.y, terrainScale.z));
            //use the inverse terrain XZ scale  here to save using divisions in the shader
            drawnMat.SetVector(invTerrainSizeID, new Vector4(1f / terrainScale.x, 1f / terrainScale.y, 1f / terrainScale.z));
            drawnMat.SetVector(terrainChunkSizeID, new Vector4(terrainScale.x / chunksX, terrainScale.z / chunksZ));
            drawnMat.SetFloat(terrainExpansionID, owner.terrainExpansion);

            //offset by half a pixel so it aligns properly
            if (terrainHeightmap) {
                drawnMat.SetFloat(terrainMapOffsetID, 1f / terrainHeightmap.width * 0.5f);
            }
        }


        void MakeMatrices(Matrix4x4 tMatrix) {

            if (matrices == null || matrices.Length != instanceCount) {
                matrices = matrices = new Matrix4x4[instanceCount];
            }

            for (int i = 0; i < matrices.Length; i++) {
                matrices[i] = tMatrix;
            }
        }

        public async Task Update(bool isAsync) {
            GetResources();
            owner.UpdateShader(this);
            await UpdateTransform(isAsync);
        }

        public async Task UpdateTransform(bool isAsync) {

            if (!terrainTransform) return;

            Matrix4x4 tMatrix = terrainTransform.localToWorldMatrix;
            Vector3 pos = terrainTransform.position;

            if (owner.useIndirectInstancing) {
                SetIndirectArgs();
            }

            Action asyncAction = new Action(() => {
                if (!owner.useIndirectInstancing) {
                    MakeMatrices(tMatrix);
                }

                CalcWorldBounds(renderType, tMatrix, pos);
            });
            if (isAsync) await Task.Run(asyncAction); else asyncAction();

        }

        public void SetPropBlocks(GrassRenderType renderType) {
            foreach (MeshChunk chunk in chunks) {
                if (chunk.propertyBlock == null) {
                    chunk.propertyBlock = new MaterialPropertyBlock();
                }
                if (renderType == GrassRenderType.Terrain) {
                    chunk.propertyBlock.SetVector(_chunkPosID, chunk.chunkPos);
                }
            }
        }

        public void SetIndirectArgs() {
            if (chunks == null && owner.useIndirectInstancing) return;
            foreach (MeshChunk chunk in chunks) {
                chunk.SetIndirectArgs(mesh);
            }
        }

        public void ReleaseIndirectArgsBuffers() {
            if (chunks == null) return;
            foreach (MeshChunk chunk in chunks) {
                if (chunk.indirectArgs != null) {
                    chunk.indirectArgs.Release();
                    chunk.indirectArgs = null;
                }
            }
        }


        public void CalcWorldBounds(GrassRenderType renderType, Matrix4x4 tMatrix, Vector3 terrainPos) {

            if (chunks == null) return;

            for (int i = 0; i < chunks.Length; i++) {

                var chunk = chunks[i];

                if (renderType == GrassRenderType.Mesh) {

                    //need to transform the chunk bounds to match the new matrix
                    chunk.worldBounds.center = tMatrix.MultiplyPoint3x4(chunk.meshBounds.center);

                    //kinda dumb and inefficient but its the easiest way to make sure
                    //the bounds encapsulate the mesh if its been rotated
                    Vector3 ext = tMatrix.MultiplyVector(chunk.meshBounds.extents);
                    float maxExt = Mathf.Max(
                        Mathf.Abs(ext.x),
                        Mathf.Abs(ext.y),
                        Mathf.Abs(ext.z)
                    );
                    chunk.worldBounds.extents = new Vector3(maxExt, maxExt, maxExt);

                    //if(useManualCulling) {
                    //Vector3 ext = chunk.worldBounds.extents;
                    //    ext.x = Mathf.Abs(ext.x);
                    //    ext.y = Mathf.Abs(ext.y);
                    //    ext.z = Mathf.Abs(ext.z);
                    //    chunk.worldBounds.extents = ext;
                    //}
                } else {
                    chunk.worldBounds.center = chunk.meshBounds.center + terrainPos;

                }

                if (i == 0) {
                    worldBounds = chunk.worldBounds;
                } else {
                    worldBounds.Encapsulate(chunk.worldBounds);
                }
            }
        }


        public void GetResources(bool refreshMat = false) {
            if (!drawnMat || refreshMat) {
                drawnMat = owner.useMaterialInstance ? Object.Instantiate(grassMaterial) : grassMaterial;
                owner.UpdateShader(this);
            }


            if (renderType == GrassRenderType.Mesh) {
                drawnMat.EnableKeyword("RENDERMODE_MESH");
            } else {
                drawnMat.DisableKeyword("RENDERMODE_MESH");
            }

            //#if UNITY_EDITOR
            //        drawnMat.EnableKeyword("GRASS_EDITOR");
            //#else
            //        drawnMat.DisableKeyword("GRASS_EDITOR");
            //#endif

#if UNITY_2018_3_OR_NEWER
            drawnMat.DisableKeyword("BAKED_HEIGHTMAP");
#else
            drawnMat.EnableKeyword("BAKED_HEIGHTMAP");
#endif
        }


        public void SetDrawmatObjMatrices() {
            if (drawnMat) {
                drawnMat.SetMatrix("objToWorldMatrix", terrainTransform.localToWorldMatrix);
                drawnMat.SetMatrix("worldToObjMatrix", terrainTransform.worldToLocalMatrix);
            }
        }


        public GrassMesh Clone() {
            var gMesh = MemberwiseClone() as GrassMesh;

            gMesh.grassMesh = null;
            gMesh.terrainObject = null;
            gMesh.mesh = null;
            gMesh.matrices = null;

            return gMesh;
        }


        //shader prop IDs
        static int _chunkPosID = Shader.PropertyToID("_chunkPos");

        static int colorMapID = Shader.PropertyToID("colorMap");
        static int dhfParamMapID = Shader.PropertyToID("dhfParamMap");
        static int typeMapID = Shader.PropertyToID("typeMap");

        static int terrainHeightMapID = Shader.PropertyToID("terrainHeightMap");
        static int terrainNormalMapID = Shader.PropertyToID("terrainNormalMap");
        static int terrainSizeID = Shader.PropertyToID("terrainSize");
        static int invTerrainSizeID = Shader.PropertyToID("invTerrainSize");
        static int terrainChunkSizeID = Shader.PropertyToID("terrainChunkSize");
        static int terrainExpansionID = Shader.PropertyToID("terrainExpansion");
        static int terrainMapOffsetID = Shader.PropertyToID("terrainMapOffset");


        public static implicit operator bool(GrassMesh gMesh) => gMesh != null;

    }
}