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

namespace DotsLite.Structure.Authoring
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.Common.Extension;
    using DotsLite.Misc;
    using DotsLite.Model.Authoring;

    [Serializable]
    public class StructureModel<TIdx, TVtx> : LodMeshModel<TIdx, TVtx>
        where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
        where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
    {

        public StructureModel(GameObject obj, Shader shader) : base(obj, shader)
        { }


        //public override Transform TfRoot => this.Obj.GetComponentsInParent<StructureBuildingModelAuthoring>(true)
        //    .FirstOrDefault()
        //    .transform;

        public override void CreateModelEntity
            (GameObjectConversionSystem gcs, Mesh mesh, Texture2D atlas)
        {
            var mat = new Material(this.shader);
            mat.enableInstancing = true;
            mat.mainTexture = atlas;

            const BoneType boneType = BoneType.RT;
            const int boneLength = 1;
            const int vectorOffsetPerInstance = 4;

            gcs.CreateDrawModelEntityComponents
                (this.Obj, mesh, mat, boneType, boneLength, DrawModel.SortOrder.desc, vectorOffsetPerInstance);
        }


        //public override (GameObject obj, Func<IMeshElements> f) BuildMeshCombiner
        //    (
        //        SrcMeshesModelCombinePack meshpack,
        //        Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary
        //    )
        //{
        //    var atlas = atlasDictionary.objectToAtlas[this.Obj].GetHashCode();
        //    var texdict = atlasDictionary.texHashToUvRect;
        //    return (
        //        this.Obj,
        //        meshpack.BuildCombiner<TIdx, TVtx>(this.TfRoot, part => texdict[atlas, part], this.Bones)
        //    );
        //}
    }

}