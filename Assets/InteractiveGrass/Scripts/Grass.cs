using System.Collections.Generic;
using UnityEngine;

namespace InteractiveGrass.Scripts
{
    [ExecuteInEditMode]
    public class Grass : MonoBehaviour
    {
        private struct GrassInfo
        {
            public Matrix4x4 localToWorld;
            public Vector4 texParams;
        }

        [SerializeField] private Vector3 _windDirection = new Vector3(1,0,1);
        [SerializeField] [Range(0, 20)] private float _windStrength = 4;
        [SerializeField] [Range(1, 100)] private int _grassCount = 1;
        [SerializeField] private Mesh _grassMesh;
        [SerializeField] private Material _grassMaterial;
        [SerializeField] private Vector2 _grassQuadSize = new Vector2(1f, 1f);
        [SerializeField] private Transform[] _targets;
        [SerializeField] [Range(0, 10)] private float _offsetRadius;
        [SerializeField] [Range(0, 100)] private float _strength;
        [SerializeField] private float _boundsRadius = 100;
        private static readonly int WindDirection = Shader.PropertyToID("_WindDirection");
        private static readonly int WindStrength = Shader.PropertyToID("_WindStrength");
        private static readonly int GrassInfoBuffer = Shader.PropertyToID("_GrassBuffer");
        private static readonly int GrassQuadSize = Shader.PropertyToID("_QuadSize");
        private static readonly int TargetPositions = Shader.PropertyToID("_PlayerPositions");
        private static readonly int Strength = Shader.PropertyToID("_PushStrength");
        private static readonly int Length = Shader.PropertyToID("_Length");
        [SerializeField] private int _cachedGrassCount = -1;
        [SerializeField] private Vector2 _cachedGrassQuadSize;
        private ComputeBuffer _grassBuffer;
        [SerializeField] private int _grassTotalCount;
        private readonly List<GrassInfo> _grassInfoBuffer = new List<GrassInfo>();
        [SerializeField] private Mesh _mesh;
        private readonly List<Vector4> _positions = new List<Vector4>();

        private void Start()
        {
            UpdateBuffers();
        }

        private void Update()
        {
            if (_cachedGrassCount != _grassCount || !_cachedGrassQuadSize.Equals(_grassQuadSize))
            {
                UpdateBuffers();
            }

            _positions.Clear();
            foreach (var target in _targets)
            {
                Vector4 pos = target.TransformPoint(Vector3.zero);
                pos.w = _offsetRadius;
                _positions.Add(pos);
            }
            SetupMaterialParameters();
            Graphics.DrawMeshInstancedProcedural(_grassMesh, 0, _grassMaterial,
                new Bounds(Vector3.zero, new Vector3(_boundsRadius, _boundsRadius, _boundsRadius)), _grassTotalCount);
        }

        private void SetupMaterialParameters()
        {
            _grassMaterial.SetVectorArray(TargetPositions, _positions);
            _grassMaterial.SetInt(Length, _positions.Count);
            _grassMaterial.SetFloat(Strength, _strength);
            _grassMaterial.SetVector(WindDirection, new Vector4(_windDirection.x, _windDirection.y, _windDirection.z, 30));
            _grassMaterial.SetFloat(WindStrength, _windStrength);
        }

        [ContextMenu("UpdateBuffers")]
        private void UpdateBuffers()
        {
            if (_mesh == null)
                _mesh = GetComponent<MeshFilter>().sharedMesh;

            if (_mesh == null)
            {
                Debug.LogError("mesh is null");
                return;
            }
            _positions.Clear();
            _grassBuffer?.Release();
            _grassInfoBuffer.Clear();
            _grassTotalCount = 0;
            var triIndex = _mesh.triangles;
            var vertices = _mesh.vertices;
            var len = triIndex.Length;
            for (var i = 0; i < len; i += 3)
            {
                var vertex1 = vertices[triIndex[i]];
                var vertex2 = vertices[triIndex[i + 1]];
                var vertex3 = vertices[triIndex[i + 2]];
                var normal = Utils.CalculateTriangleNormal(vertex1, vertex2, vertex3).normalized;
                for (var j = 0; j < _grassCount; j++)
                {
                    var texScale = Vector2.one;
                    var texOffset = Vector2.zero;
                    var randPos = Utils.CalculateRandomTriangle(vertex1, vertex2, vertex3);
                    randPos += normal.normalized * 0.5f * _grassQuadSize.y;
                    var localToWorld = Matrix4x4.TRS(transform.TransformPoint(randPos),
                        Quaternion.FromToRotation(Vector3.up, normal) * Quaternion.Euler(0, Random.Range(0, 180), 0),
                        Vector3.one);
                    _grassInfoBuffer.Add(new GrassInfo
                    {
                        localToWorld = localToWorld,
                        texParams = new Vector4(texScale.x, texScale.y, texOffset.x, texOffset.y)
                    });
                    _grassTotalCount++;
                }
            }

            _grassBuffer = new ComputeBuffer(_grassTotalCount, 64 + 16);
            _grassBuffer.SetData(_grassInfoBuffer);
            _grassMaterial.SetBuffer(GrassInfoBuffer, _grassBuffer);
            _grassMaterial.SetVector(GrassQuadSize, _grassQuadSize);
            _cachedGrassCount = _grassCount;
            _cachedGrassQuadSize = _grassQuadSize;
        }

        private void OnDisable()
        {
            _grassBuffer?.Release();
            _grassBuffer = null;
        }
    }
}