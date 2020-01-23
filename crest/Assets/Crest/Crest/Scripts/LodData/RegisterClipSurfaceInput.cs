﻿// Crest Ocean System

// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

using UnityEngine;

namespace Crest
{
    /// <summary>
    /// Registers a custom input to the clip surface simulation. Attach this to GameObjects that you want to use to
    /// clip the surface of the ocean.
    /// </summary>
    public class RegisterClipSurfaceInput : RegisterLodDataInput<LodDataMgrClipSurface>
    {
        [Tooltip("Uses the 'clip from geometry' shader. There are other clip shaders available.")]
        [SerializeField] bool _assignClipSurfaceMaterial = true;

        public override float Wavelength => 0f;

        protected override Color GizmoColor => new Color(0f, 1f, 1f, 0.5f);

        PropertyWrapperMPB _mpb;
        Renderer _rend;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (_assignClipSurfaceMaterial)
            {
                var rend = GetComponent<Renderer>();
                rend.material = new Material(Shader.Find("Crest/Inputs/Clip Surface/Add From Geometry"));
            }
        }

        protected override void Start()
        {
            base.Start();
            _rend = GetComponent<Renderer>();
        }

        private void LateUpdate()
        {
            if (OceanRenderer.Instance == null)
            {
                return;
            }

            // find which lod this object is overlapping
            var rect = new Rect(transform.position.x, transform.position.z, 0f, 0f);
            var lodIdx = LodDataMgrAnimWaves.SuggestDataLOD(rect);

            if (lodIdx > -1)
            {
                if (_mpb == null)
                {
                    _mpb = new PropertyWrapperMPB();
                }

                _rend.GetPropertyBlock(_mpb.materialPropertyBlock);

                var lodCount = OceanRenderer.Instance.CurrentLodCount;
                var lodDataAnimWaves = OceanRenderer.Instance._lodDataAnimWaves;
                _mpb.SetInt(LodDataMgr.sp_LD_SliceIndex, lodIdx);
                lodDataAnimWaves.BindResultData(_mpb);

                // blend LOD 0 shape in/out to avoid pop, if the ocean might scale up later (it is smaller than its maximum scale)
                bool needToBlendOutShape = lodIdx == 0 && OceanRenderer.Instance.ScaleCouldIncrease;
                float meshScaleLerp = needToBlendOutShape ? OceanRenderer.Instance.ViewerAltitudeLevelAlpha : 0f;

                // blend furthest normals scale in/out to avoid pop, if scale could reduce
                bool needToBlendOutNormals = lodIdx == lodCount - 1 && OceanRenderer.Instance.ScaleCouldDecrease;
                float farNormalsWeight = needToBlendOutNormals ? OceanRenderer.Instance.ViewerAltitudeLevelAlpha : 1f;
                _mpb.SetVector(OceanChunkRenderer.sp_InstanceData, new Vector3(meshScaleLerp, farNormalsWeight, lodIdx));

                _rend.SetPropertyBlock(_mpb.materialPropertyBlock);
            }
        }
    }
}
