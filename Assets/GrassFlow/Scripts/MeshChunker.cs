using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using System.Threading.Tasks;

using GrassCullChunk = GrassFlowRenderer.GrassCullChunk;


namespace GrassFlow {

    public class MeshChunker {



        class MeshChunkData {
            public List<int> tris = new List<int>();
            public Bounds bounds;

            public void CalculateBounds(List<Vector3> verts) {
                bounds = new Bounds(verts[tris[0]], Vector3.zero);

                bounds.Encapsulate(verts[tris[1]]);
                bounds.Encapsulate(verts[tris[2]]);

                for (int i = 3; i < tris.Count; i += 3) {
                    bounds.Encapsulate(verts[tris[i + 0]]);
                    bounds.Encapsulate(verts[tris[i + 1]]);
                    bounds.Encapsulate(verts[tris[i + 2]]);
                }
            }

            public static implicit operator bool(MeshChunkData data) => data != null;
        }


        public class MeshChunk {
            public GrassMesh parentMesh;
            public int submeshIdx;
            public Bounds worldBounds;
            public Bounds meshBounds;
            public Vector4 chunkPos;
            public MaterialPropertyBlock propertyBlock;

            public uint instanceCount {
                get { return indirectArgsArr[1]; }
                set {
                    if (indirectArgsArr[1] != value) {
                        indirectArgsArr[1] = value;
                        indirectArgs.SetData(indirectArgsArr);
                    }
                }
            }
            public uint[] indirectArgsArr;
            [System.NonSerialized] public ComputeBuffer indirectArgs;

            public void SetIndirectArgs(Mesh mesh) {
                //documentation for indirect args is seriously lacking so a lot of this doesnt make a ton of sense
                //but may as well set it up properly for future reference
                indirectArgsArr = new uint[] {
                mesh.GetIndexCount(submeshIdx), //index count per instance
                0, //instance count, placeholder for now
                mesh.GetIndexStart(submeshIdx), //start index location

#if UNITY_2018_1_OR_NEWER
                mesh.GetBaseVertex(submeshIdx), //base vertex location
#else
                0, //base vertex location
#endif

                0 //start instance location
            };

                //not at all sure why count is applied to the size stride rather than, you know, the count slot. but whatever.
                indirectArgs = new ComputeBuffer(1, indirectArgsArr.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
                //dont bother setting data here because it will be needed to be set later when rendering anyway
            }
        }

        static float map(float value, float from1, float to1, float from2, float to2) {
            if (to1 == from1) return 0;

            return Mathf.Clamp((value - from1) / (to1 - from1), from2, to2);
        }




        static void NormalizeMeshDensity(GrassMesh gF, List<int> tris, List<Vector3> verts, List<Vector3> norms, List<Vector2> uvs) {

            const float densityPredictionMultVerts = 6;
            const float baseAreaSubdiv = 2.25f;

            float[] areas = new float[tris.Count / 3];
            float minArea = float.MaxValue;
            float maxArea = 0;

            for (int i = 0; i < areas.Length; i++) {

                int triIdx = i * 3;
                Vector3 p1 = verts[tris[triIdx + 0]];
                Vector3 p2 = verts[tris[triIdx + 1]];
                Vector3 p3 = verts[tris[triIdx + 2]];

                float a = Vector3.Distance(p1, p2);
                float b = Vector3.Distance(p2, p3);
                float c = Vector3.Distance(p3, p1);
                float s = (a + b + c) * 0.5f;
                float area = Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));

                if (area < minArea) {
                    minArea = area;
                }
                if (area > maxArea) {
                    maxArea = area;
                }

                areas[i] = area;
            }

            minArea /= baseAreaSubdiv;
            if (maxArea / minArea > gF.normalizeMaxRatio) {
                minArea = maxArea / gF.normalizeMaxRatio;
            }



            tris.Capacity = (int)(tris.Capacity * baseAreaSubdiv);
            verts.Capacity = (int)(verts.Capacity * densityPredictionMultVerts);
            norms.Capacity = (int)(norms.Capacity * densityPredictionMultVerts);
            uvs.Capacity = (int)(uvs.Capacity * densityPredictionMultVerts);

            float triCount = tris.Count;
            float vertCount = verts.Count;

            for (int i = 0; i < areas.Length; i++) {

                int triIdx = i * 3;
                int t1 = tris[triIdx + 0];
                int t2 = tris[triIdx + 1];
                int t3 = tris[triIdx + 2];

                float area = areas[i];

                int subDivs = Mathf.RoundToInt(area / minArea);
                float step = 1f / subDivs;
                int prevIdx = triIdx;

                for (int s = 1; s < subDivs; s++) {

                    float t = s * step;

                    int newIdx = verts.Count;
                    verts.Add(Vector3.Lerp(verts[t1], verts[t3], t));
                    norms.Add(Vector3.Lerp(norms[t1], norms[t3], t));
                    uvs.Add(Vector2.Lerp(uvs[t1], uvs[t3], t));

                    if (s == 1) {
                        tris[triIdx + 2] = newIdx;

                    } else {
                        tris.Add(prevIdx);
                        tris.Add(t2);
                        tris.Add(newIdx);
                    }

                    if (s == subDivs - 1) {
                        tris.Add(newIdx);
                        tris.Add(t2);
                        tris.Add(t3);
                    }

                    prevIdx = newIdx;
                }
            }

            //Debug.Log("Tris - " + triCount + " : " + tris.Count + " : " + (tris.Count / triCount));
            //Debug.Log("Verts - " + vertCount + " : " + verts.Count + " : " + (verts.Count / vertCount));

        }

        public static async Task<MeshChunk[]> ChunkMesh(GrassMesh gMesh, float bladeHeight, bool normalize) {

            MeshChunk[] finalChunks = null;

            try {

                Mesh chunkedMesh = new Mesh();
                gMesh.mesh = chunkedMesh;

                Mesh meshToChunk = gMesh.grassMesh;
                Bounds meshBounds = meshToChunk.bounds;

                int vertCount = meshToChunk.vertexCount;
                int triCount = (int)meshToChunk.GetIndexCount(0);

                int xChunks = gMesh.chunksX;
                int yChunks = gMesh.chunksY;
                int zChunks = gMesh.chunksZ;

                int subdiv = gMesh.grassPerTri;

                if (subdiv < 0) subdiv = 0;
                subdiv += 1;

                List<int> tris = null;
                List<Vector3> verts = null;
                List<Vector3> norms = null;
                List<Vector2> uvs = null;

                List<MeshChunk> resultChunks = null;

                Action asyncAction = new Action(() => {

                    tris = new List<int>(triCount);
                    verts = new List<Vector3>(vertCount);
                    norms = new List<Vector3>(vertCount);
                    uvs = new List<Vector2>(vertCount);
                });
                if (GrassFlowRenderer.processAsync) await Task.Run(asyncAction); else asyncAction();



                meshToChunk.GetTriangles(tris, 0);
                meshToChunk.GetVertices(verts);
                meshToChunk.GetNormals(norms);
                meshToChunk.GetUVs(0, uvs);



                MeshChunkData[,,] meshChunks = null;
                int meshCount = 0;
                int pMapW = 0, pMapH = 0;
                int totalTris = 0;

                Color32[] pixels = null;
                if (Application.isPlaying && gMesh.owner.discardEmptyChunks && gMesh.paramMap) {
                    pixels = gMesh.paramMap.GetPixels32();
                    pMapW = gMesh.paramMap.width;
                    pMapH = gMesh.paramMap.height;
                }

                asyncAction = new Action(() => {
                    if (normalize) {
                        NormalizeMeshDensity(gMesh, tris, verts, norms, uvs);
                    }

                    if (gMesh.owner.discardEmptyChunks) {
                        BakeDensityToMesh(gMesh, pMapW, pMapH, pixels, tris, verts, norms, uvs);
                    }

                    meshChunks = new MeshChunkData[xChunks, yChunks, zChunks];
                    resultChunks = new List<MeshChunk>(meshChunks.Length);

                    int[] thisTris = new int[(subdiv + 1) * 3];
                    for (int i = 0; i < tris.Count; i += 3) {

                        int t1 = tris[i]; int t2 = tris[i + 1]; int t3 = tris[i + 2];
                        Vector3 checkVert = verts[t3];

                        int xIdx = (int)(map(checkVert.x, meshBounds.min.x, meshBounds.max.x, 0f, 0.99999f) * xChunks);
                        int yIdx = (int)(map(checkVert.y, meshBounds.min.y, meshBounds.max.y, 0f, 0.99999f) * yChunks);
                        int zIdx = (int)(map(checkVert.z, meshBounds.min.z, meshBounds.max.z, 0f, 0.99999f) * zChunks);

                        MeshChunkData cData = meshChunks[xIdx, yIdx, zIdx];
                        if (cData == null) meshChunks[xIdx, yIdx, zIdx] = (cData = new MeshChunkData());

                        for (int sub = 0; sub < thisTris.Length; sub += 3) {
                            thisTris[sub] = t1;
                            thisTris[sub + 1] = t2;
                            thisTris[sub + 2] = t3;
                        }

                        cData.tris.AddRange(thisTris);
                    }

                    foreach (var chunk in meshChunks) {
                        if (chunk?.tris.Count > 0) {
                            meshCount++;
                            chunk.CalculateBounds(verts);
                            totalTris += chunk.tris.Count;
                        }
                    }
                });
                if (GrassFlowRenderer.processAsync) await Task.Run(asyncAction); else asyncAction();

#if UNITY_2017_3_OR_NEWER
                if (totalTris / 3 >= 65535) {
                    chunkedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                }
#endif

                chunkedMesh.SetVertices(verts);
                chunkedMesh.SetNormals(norms);
                chunkedMesh.SetUVs(0, uvs);
                chunkedMesh.UploadMeshData(false);

                chunkedMesh.subMeshCount = meshCount;

                for (int cx = 0; cx < xChunks; cx++) {
                    for (int cy = 0; cy < yChunks; cy++) {
                        for (int cz = 0; cz < zChunks; cz++) {

                            MeshChunkData cData = meshChunks[cx, cy, cz];

                            if (cData?.tris.Count > 0) {

                                int subIdx = resultChunks.Count;
                                chunkedMesh.SetTriangles(cData.tris, subIdx, false);

                                resultChunks.Add(new MeshChunk() {
                                    parentMesh = gMesh,
                                    meshBounds = cData.bounds,
                                    submeshIdx = subIdx,
                                    propertyBlock = new MaterialPropertyBlock()
                                });
                            }
                        }
                    }
                }


                asyncAction = new Action(() => {
                    finalChunks = resultChunks.ToArray();

                    ExpandChunks(finalChunks, bladeHeight);
                });
                if (GrassFlowRenderer.processAsync) await Task.Run(asyncAction); else asyncAction();

            } catch (Exception ex) {
                Debug.LogException(ex);
                return null;
            }

            return finalChunks;
        }

        public static async Task<MeshChunk[]> ChunkTerrain(GrassMesh gMesh, float expandAmnt, float bladeHeight) {

            Mesh chunkedMesh = new Mesh();
            chunkedMesh.vertices = new Vector3[] { Vector3.zero };
            chunkedMesh.normals = new Vector3[] { Vector3.zero };
            chunkedMesh.SetTriangles(new int[gMesh.grassPerTri * 3], 0, false);
            gMesh.mesh = chunkedMesh;

            MeshChunk[] chunks = null;

            TerrainData terrain = gMesh.terrainObject.terrainData;

            Vector3 terrainScale = terrain.size;

            int xChunks = gMesh.chunksX;
            int yChunks = gMesh.chunksY;
            int zChunks = gMesh.chunksZ;

            Vector3 chunkSize = new Vector3(terrainScale.x / xChunks, terrainScale.y * 0.5f, terrainScale.z / zChunks);
            Vector3 halfChunkSize = chunkSize * 0.5f;

            chunks = new MeshChunk[xChunks * zChunks];

            int w = terrain.heightmapResolution - 1;
            int h = terrain.heightmapResolution - 1;
            float cWf = w / (float)xChunks;
            float cHf = h / (float)zChunks;
            int cW = (int)cWf;
            int cH = (int)cHf;

            float[,] tHeights = terrain.GetHeights(0, 0, terrain.heightmapResolution, terrain.heightmapResolution);

            Action asyncAction = new Action(() => {

                int index = 0;
                for (int z = 0; z < zChunks; z++) {
                    for (int x = 0; x < xChunks; x++) {

                        float maxHeight = 0;
                        float minHeight = 1;

                        int cXS = (int)(cWf * x);
                        int cZS = (int)(cHf * z);

                        for (int cX = cXS; cX < cXS + cW; cX++) {
                            for (int cZ = cZS; cZ < cZS + cH; cZ++) {

                                //still not entirely sure why this needs to be sampled backwards
                                //prob just the heightmap is in a different orientation..
                                float tH = tHeights[cZ, cX];

                                if (tH > maxHeight)
                                    maxHeight = tH;
                                if (tH < minHeight)
                                    minHeight = tH;
                            }
                        }

                        Vector3 chunkPos = Vector3.Scale(chunkSize, new Vector3(x, 0, z));
                        Vector3 mapChunkPos = new Vector4(chunkPos.x, chunkPos.z);

                        chunkPos += halfChunkSize;
                        chunkPos.y = chunkSize.y * (maxHeight + minHeight);

                        halfChunkSize.y = chunkSize.y * (maxHeight - minHeight);

                        chunks[index++] = new MeshChunk() {
                            parentMesh = gMesh,
                            meshBounds = new Bounds() {
                                center = chunkPos,
                                extents = halfChunkSize
                            },
                            worldBounds = new Bounds() {
                                extents = halfChunkSize
                            },
                            chunkPos = mapChunkPos,
                            submeshIdx = 0
                        };
                    }
                }

                ExpandChunks(chunks, 1f + expandAmnt, bladeHeight);
            });
            if (GrassFlowRenderer.processAsync) await Task.Run(asyncAction); else asyncAction();


            foreach (var chunk in chunks) {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                propBlock.SetVector("_chunkPos", chunk.chunkPos);
                chunk.propertyBlock = propBlock;
            }

            return chunks;
        }

        public static void ExpandChunks(MeshChunk[] chunks, float expandAmount, float bladeHeight) {
            Vector3 bladeBoundsExpand = new Vector3(bladeHeight, bladeHeight, bladeHeight);

            foreach (MeshChunk chunk in chunks) {
                Vector3 extents = chunk.meshBounds.extents;
                extents.x *= expandAmount;
                extents.z *= expandAmount;
                extents.y = chunk.meshBounds.extents.y;

                chunk.meshBounds.extents = extents + bladeBoundsExpand;
                chunk.worldBounds.extents = chunk.meshBounds.extents;
                //chunk.mesh.bounds = chunk.meshBounds;
            }

        }

        public static void ExpandChunks(MeshChunk[] chunks, float bladeHeight) {
            Vector3 bladeBoundsExpand = new Vector3(bladeHeight, bladeHeight, bladeHeight);

            foreach (MeshChunk chunk in chunks) {
                chunk.meshBounds.extents += bladeBoundsExpand;
            }
        }

        static int UVtoIdx(int w, int h, Vector2 uv) {
            return Mathf.Clamp(Mathf.RoundToInt(uv.x * w) + Mathf.RoundToInt(uv.y * h) * w, 0, w * h - 1);
        }

        const float densityThresh = 0.02f;
        const float byte255to01 = 0.0039215686274509803921568627451f;

        public static void BakeDensityToMesh(GrassMesh gF, int width, int height, Color32[] pixels, List<int> baseTris, List<Vector3> baseVerts, List<Vector3> baseNorms, List<Vector2> baseUvs) {

            if (pixels == null) return;

            if (baseUvs.Count == 0) {
                Debug.LogError("GrassFlow:BakeDensityToMesh: Base mesh does not have uvs!");
                return;
            }

            List<int> filledTris = new List<int>();

            int numPixChecked = 0;
            float densityAcc = 0f;
            Action<int> CheckPixel = (pIdx) => {
                numPixChecked++;
                densityAcc += pixels[pIdx].r;
            };

            Action<Vector2, Vector2> CheckSide = (uv1, uv2) => {
                for (float t = 0; t <= 1; t += 0.1f) {
                    CheckPixel(UVtoIdx(width, height, Vector3.Lerp(uv1, uv2, t)));
                }
            };

            for (int i = 0; i < baseTris.Count; i += 3) {
                int[] thisTri = new int[] { baseTris[i], baseTris[i + 1], baseTris[i + 2] };
                Vector2 uv1 = baseUvs[thisTri[0]]; Vector2 uv2 = baseUvs[thisTri[1]]; Vector2 uv3 = baseUvs[thisTri[2]];

                densityAcc = 0f;
                numPixChecked = 0;

                CheckSide(uv1, uv2);
                CheckSide(uv3, uv2);
                CheckSide(uv1, uv3);

                uv1 = Vector2.LerpUnclamped(uv1, uv2, 0.5f);
                Vector3 mid = Vector2.Lerp(uv1, uv3, 0.5f);
                CheckPixel(UVtoIdx(width, height, mid));

                uv2 = Vector2.LerpUnclamped(uv2, uv3, 0.5f);
                uv3 = Vector2.LerpUnclamped(uv1, uv3, 0.5f);

                CheckSide(uv1, uv2);
                CheckSide(uv3, uv2);
                CheckSide(uv1, uv3);

                densityAcc = densityAcc / numPixChecked * byte255to01;
                if (densityAcc > densityThresh) {
                    filledTris.AddRange(thisTri);
                }
            }

            var distinctTriIndexes = filledTris.Distinct().ToArray();

            Vector3[] verts = new Vector3[distinctTriIndexes.Length];
            Vector3[] norms = new Vector3[distinctTriIndexes.Length];
            Vector2[] uvs = new Vector2[distinctTriIndexes.Length];

            Dictionary<int, int> triMap = new Dictionary<int, int>();
            for (int i = 0; i < distinctTriIndexes.Length; i++) {
                int distinctTriIdx = distinctTriIndexes[i];
                triMap.Add(distinctTriIdx, i);

                verts[i] = baseVerts[distinctTriIdx];
                norms[i] = baseNorms[distinctTriIdx];
                uvs[i] = baseUvs[distinctTriIdx];
            }

            int[] remappedTris = new int[filledTris.Count];
            for (int i = 0; i < remappedTris.Length; i++) {
                remappedTris[i] = triMap[filledTris[i]];
            }

            baseTris.Clear();
            baseVerts.Clear();
            baseNorms.Clear();
            baseUvs.Clear();

            baseTris.AddRange(remappedTris);
            baseVerts.AddRange(verts);
            baseNorms.AddRange(norms);
            baseUvs.AddRange(uvs);
        }


    }//class
}//namespace