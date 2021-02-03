﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace Abarabone.Model.Authoring
{
    using Abarabone.Geometry;
    using Abarabone.Draw;
    using Abarabone.Utilities;
    using Abarabone.Common.Extension;
    using Abarabone.Draw.Authoring;


    /// <summary>
    /// モデル描画をまとめる工夫を行う。
    /// テクスチャアトラスとか。
    /// できればシェーダーでも描画順をまとめいたいけど、unity はマテリアル単位の設定なので無理かな…？
    /// unity が自動的にやってくれてる可能性もある。
    /// </summary>
    //[UpdateAfter(typeof(GameObjectBeforeConversionGroup))]
    [UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    public class ModelGroupAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
    {

        /// <summary>
        /// これを継承しないと、モデルビヘイビアを要求するものに登録できない。
        /// </summary>
        public abstract class ModelAuthoringBase : MonoBehaviour
        {
            public abstract (GameObject obj, Func<MeshElements<TIdx, TVtx>> f)[] BuildMeshCombiners<TIdx, TVtx>
                (Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasAndParameter tex = default)
                where TIdx : struct, IIndexUnit<TIdx>
                where TVtx : struct, IVertexUnit<TVtx>
            ;

            public abstract IEnumerable<GameObject> QueryMeshTopObjects();

            //public virtual void BuildMeshAndAtlasToDictionary(GameObjectConversionSystem gcs, IEnumerable<GameObject> objs) { }
        }


        public ModelAuthoringBase[] ModelPrefabs;

        //public bool MakeTexutreAtlus;


        public void DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
        {
            referencedPrefabs.AddRange( this.ModelPrefabs.Select(x => x.gameObject) );
        }


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {
            //var meshDict = conversionSystem.GetMeshDictionary();
            //var atlasDict = conversionSystem.GetTextureAtlasDictionary();

            var prefabModels = this.ModelPrefabs.Distinct();

            var qObj =
                from model in prefabModels
                from objtop in model.QueryMeshTopObjects()
                select objtop
                ;
            var objs = qObj.ToArray();

            var gcs = conversionSystem;

            var atlasDict = gcs.GetTextureAtlasDictionary();
            var meshDict = gcs.GetMeshDictionary();

            var tex = objs.ToAtlas(atlasDict);

            var ofss =
                from model in prefabModels
                select model.BuildMeshCombiners<UI32, PositionNormalUvVertex>(meshDict, tex);
            var qMObj =
                from ofs in ofss
                from of in ofs
                select of.obj;
            var qMesh =
                from e in ofss.SelectMany().Select(x => x.f.ToTask()).WhenAll().Result
                select e.CreateMesh()
                ;
            meshDict.AddRange(qMObj, qMesh);


            //var tex = toAtlas_();

            //if (tex.atlas != null) combineMeshes_(tex);


            //TextureAtlasAndParameter toAtlas_()
            //{
            //    var tobjs = objs
            //        .Where(x => !atlasDict.objectToAtlas.ContainsKey(x))
            //        //.Logging(x => x.name)
            //        .ToArray();

            //    if (tobjs.Length == 0) return default;

            //    var qMat =
            //        from obj in tobjs
            //        from r in obj.GetComponentsInChildren<Renderer>()
            //        from mat in r.sharedMaterials
            //        select mat
            //        ;

            //    var tex = qMat.QueryUniqueTextures().ToAtlasAndParameter();

            //    atlasDict.texHashToUvRect[tex.texhashes] = tex.uvRects;
            //    atlasDict.objectToAtlas.AddRange(tobjs, tex.atlas);

            //    return tex;
            //}

            //void combineMeshes_(TextureAtlasAndParameter tex)
            //{
            //    var mobjs = objs
            //        .Where(x => !meshDict.ContainsKey(x))
            //        .ToArray();
            //    var qSrc =
            //        from obj in mobjs
            //        let mmt = obj.QueryMeshMatsTransform_IfHaving()
            //        select (obj, mmt)
            //        ;
            //    var srcs = qSrc.ToArray();

            //    var qMeshSingle =
            //        from src in srcs
            //        where src.mmt.IsSingle()
            //        select src.mmt.First().mesh
            //        ;
            //    var qMeshSrc =
            //        from src in srcs
            //        where !src.mmt.IsSingle()
            //        select src.mmt.BuildCombiner<UI32, PositionNormalUvVertex>(src.obj.transform, tex).ToTask()
            //        ;
            //    var qMesh = qMeshSrc
            //        .WhenAll().Result
            //        .Select(x => x.CreateMesh())
            //        .Concat(qMeshSingle);

            //    meshDict.AddRange(mobjs, qMesh);
            //}



            //// atlas
            //var tobjs = objs
            //    .Where(x => !atlasDict.objectToAtlas.ContainsKey(x))
            //    .ToArray();
            //var qMat =
            //    from obj in tobjs
            //    from r in obj.GetComponentsInChildren<Renderer>()
            //    from mat in r.sharedMaterials
            //    select mat
            //    ;

            //var tex = qMat.QueryUniqueTextures().ToAtlasAndParameter();

            //atlasDict.texHashToUvRect[tex.texhashes] = tex.uvRects;
            //atlasDict.objectToAtlas.AddRange(tobjs, tex.atlas);


            //// mesh
            //var mobjs = objs
            //    .Where(x => !meshDict.ContainsKey(x))
            //    .ToArray();
            //var qSrc =
            //    from obj in mobjs
            //    let mmt = obj.QueryMeshMatsTransform_IfHaving()
            //    select (obj, mmt)
            //    ;
            //var srcs = qSrc.ToArray();

            //var qMeshSrc =
            //    from src in srcs
            //    where !src.mmt.IsSingle()
            //    select src.mmt.BuildCombiner<UI32, PositionNormalUvVertex>(src.obj.transform, tex).ToTask()
            //    ;
            //var qMesh = qMeshSrc
            //    .WhenAll().Result
            //    .Select(x => x.CreateMesh())
            //    .ToArray();

            //meshDict.AddRange(mobjs, qMesh);


            // モデルグループ自体にはエンティティは不要
            dstManager.DestroyEntity(entity);

            return;

        }
        

    }


    static public class aaa
    {

        static public TextureAtlasAndParameter ToAtlas
            (this IEnumerable<GameObject> objs, TextureAtlasDictionary.Data atlasDict)
        {
            var texobjs = objs
                .Where(x => !atlasDict.objectToAtlas.ContainsKey(x))
                //.Logging(x => x.name)
                .ToArray();

            if (texobjs.Length == 0) return default;

            var qMat =
                from obj in texobjs
                from r in obj.GetComponentsInChildren<Renderer>()
                from mat in r.sharedMaterials
                select mat
                ;

            var tex = qMat.QueryUniqueTextures().ToAtlasAndParameter();

            atlasDict.texHashToUvRect[tex.texhashes] = tex.uvRects;
            atlasDict.objectToAtlas.AddRange(texobjs, tex.atlas);

            return tex;
        }
    }

}
