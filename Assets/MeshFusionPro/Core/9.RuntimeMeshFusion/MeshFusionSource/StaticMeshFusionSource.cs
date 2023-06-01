using System;
using System.Collections.Generic;
using UnityEngine;

namespace NGS.MeshFusionPro
{
    public class StaticMeshFusionSource : MeshFusionSource
    {
        [SerializeField, HideInInspector]
        private MeshRenderer _renderer;

        [SerializeField, HideInInspector]
        private MeshFilter _filter;

        [SerializeField, HideInInspector]
        private Mesh _mesh;

        private CombineSource[] _sources;
        private CombinedObjectPart[] _parts;


        public override bool TryGetBounds(ref Bounds bounds)
        {
            if (_renderer != null)
            {
                bounds = _renderer.bounds;
                return true;
            }

            return false;
        }

        protected override void OnSourceCombinedInternal(ICombinedObject root, ICombinedObjectPart part)
        {
            for (int i = 0; i < _parts.Length; i++)
            {
                if (_parts[i] == null)
                {
                    _parts[i] = (CombinedObjectPart)part;
                    return;
                }
            }

            throw new Exception("Unexpected Behaviour");
        }


        protected override bool CheckCompatibilityAndGetComponents(out string incompatibilityReason)
        {
            incompatibilityReason = "";

            return CanCreateCombineSource(gameObject, ref incompatibilityReason, 
                ref _renderer, ref _filter, ref _mesh);
        }

        protected override void CreateSources()
        {
            if (_sources == null)
                _sources = new CombineSource[_mesh.subMeshCount];

            if (_parts == null)
                _parts = new CombinedObjectPart[_mesh.subMeshCount];

            for (int i = 0; i < _sources.Length; i++)
            {
                CombineSource source = new CombineSource(_mesh, _renderer, i);
                _sources[i] = source;
            }
        }

        protected override IEnumerable<ICombineSource> GetCombineSources()
        {
            if (_sources == null)
                yield break;

            for (int i = 0; i < _sources.Length; i++)
            {
                CombineSource source = _sources[i];

                if (source == null)
                    yield break;

                yield return source;
            }
        }

        protected override IEnumerable<ICombinedObjectPart> GetCombinedParts()
        {
            if (_parts == null)
                yield break;

            for (int i = 0; i < _parts.Length; i++)
            {
                CombinedObjectPart part = _parts[i];

                if (part == null)
                    yield break;

                yield return part;
            }
        }

        protected override void ClearSources()
        {
            if (_sources == null)
                return;

            for (int i = 0; i < _sources.Length; i++)
                _sources[i] = null;
        }

        protected override void ClearParts()
        {
            if (_parts == null)
                return;

            for (int i = 0; i < _parts.Length; i++)
                _parts[i] = null;
        }

        protected override void ToggleComponents(bool enabled)
        {
            _renderer.enabled = enabled;
        }
    }
}
