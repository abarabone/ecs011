﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Linq;

namespace DotsLite.Model.Authoring
{
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Structure.Authoring;
    using DotsLite.Utilities;
    using DotsLite.Common.Extension;
    using DotsLite.Misc;

    [Serializable]
    public class MeshWithPaletteModel<TIdx, TVtx> : MeshModel<TIdx, TVtx>
        where TIdx : struct, IIndexUnit<TIdx>//, ISetBufferParams
        where TVtx : struct, IVertexUnit//<TVtx>, ISetBufferParams
    {


        [SerializeField]
        ColorPaletteAsset Palette;


        public override Func<Mesh.MeshDataArray> BuildMeshCombiner(
            SrcMeshesModelCombinePack meshpack,
            Dictionary<IMeshModel, Mesh> meshDictionary,
            TextureAtlasDictionary.Data atlasDictionary)
        {
            var atlas = atlasDictionary.modelToAtlas[this].GetHashCode();
            var texdict = atlasDictionary.texHashToUvRect;
            var p = this.QueryMmts.calculateParameters(
                this.TfRoot, this.QueryBones?.ToArray(), subtexhash => texdict[atlas, subtexhash], null);

            // パレット向けの暫定
            this.QueryMmts.CalculatePaletteSubIndexParameter(ref p);

            return () => meshpack.CreateMeshData(this.idxBuilder, this.vtxBuilder, p);
        }


        public override void InitModelEntity(GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            base.InitModelEntity(gcs, mesh, atlas);

            var paletteAuthor = this.objectTop.GetComponentInParent<ColorPaletteBufferAuthoring>();
            if (paletteAuthor == null) return;

            var em = gcs.DstEntityManager;
            var ent = gcs.GetPrimaryEntity(this);
            em.AddComponentData(ent, new DrawModelShaderBuffer.ColorPaletteLinkData
            {
                BufferEntity = gcs.GetPrimaryEntity(paletteAuthor),
            });
        }

    }


    [Serializable]
    public class LodMeshWithPaletteModel<TIdx, TVtx> : MeshWithPaletteModel<TIdx, TVtx>, IMeshModelLod
        where TIdx : struct, IIndexUnit<TIdx>//, ISetBufferParams
        where TVtx : struct, IVertexUnit//<TVtx>, ISetBufferParams
    {

        [SerializeField]
        public float limitDistance;

        [SerializeField]
        public float margin;


        public float LimitDistance => this.limitDistance;
        public float Margin => this.margin;
    }
}