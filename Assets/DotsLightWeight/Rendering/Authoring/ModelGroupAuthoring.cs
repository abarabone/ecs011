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
            public virtual (GameObject obj, Func<IMeshElements> f)[] BuildMeshCombiners
                (Dictionary<GameObject, Mesh> meshDictionary, TextureAtlasDictionary.Data atlasDictionary)
            { throw new NotImplementedException(); }

            public virtual Lazy<GameObject[]> MeshTopObjects => throw new NotImplementedException();


            //public Lazy<(Mesh mesh, Material[] mats, Transform tf)[][]> Mmtss { get; }
            //    = new Lazy<(Mesh mesh, Material[] mats, Transform tf)[][]>(() => this.mm);

            //protected virtual (Mesh mesh, Material[] mats, Transform tf)[][] mmtss { get; }

            //public virtual void BuildMeshAndAtlasToDictionary(GameObjectConversionSystem gcs, IEnumerable<GameObject> objs) { }
        }


        public ModelAuthoringBase[] ModelPrefabs;

        //public bool MakeTexutreAtlus;


        public void DeclareReferencedPrefabs( List<GameObject> referencedPrefabs )
        {
            referencedPrefabs.AddRange(this.ModelPrefabs.Select(x => x.gameObject));
        }


        public void Convert( Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem )
        {

            var meshDict = conversionSystem.GetMeshDictionary();
            var atlasDict = conversionSystem.GetTextureAtlasDictionary();

            var prefabModels = this.ModelPrefabs.Distinct();

            prefabModels
                .SelectMany(model => model.MeshTopObjects.Value)
                .PackTextureToDictionary(atlasDict);

            combineMeshToDictionary_();


            // モデルグループ自体にはエンティティは不要
            dstManager.DestroyEntity(entity);

            return;


            void combineMeshToDictionary_()
            {
                //var qMmtsPerObj =
                //    from model in prefabModels
                //    from obj in model.MeshTopObjects.Value
                //    select obj.QueryMeshMatsTransform_IfHaving()
                //    ;
                //var q = qMmtsPerObj.QueryMeshDataWithDisposingLast();
                //q.ToArray();

                //var qOfs =
                //    from model in prefabModels
                //    select model.BuildMeshCombiners(meshDict, atlasDict);
                //var ofss = qOfs.ToArray();
                //var qMObj = ofss.SelectMany().Select(of => of.obj);
                //var qMesh = ofss.SelectMany().Select(of => of.f.ToTask())
                //    .WhenAll().Result
                //    .Select(t => t.CreateMesh());
                ////var qMesh = ofss.SelectMany().Select(of => of.f().CreateMesh());
                //meshDict.AddRange(qMObj, qMesh);
            }
        }
        

    }


    static public class aaa
    {

        static public void PackTextureToDictionary
            (this IEnumerable<GameObject> objs, TextureAtlasDictionary.Data atlasDict)
        {
            var texobjs = objs
                .Where(x => !atlasDict.objectToAtlas.ContainsKey(x))
                //.Logging(x => x.name)
                .ToArray();

            if (texobjs.Length == 0) return;

            var qMat =
                from obj in texobjs
                from r in obj.GetComponentsInChildren<Renderer>()
                from mat in r.sharedMaterials
                select mat
                ;

            var tex = qMat.QueryUniqueTextures().ToAtlasOrPassThroughAndParameters();

            atlasDict.texHashToUvRect[tex.texhashes] = tex.uvRects;
            atlasDict.objectToAtlas.AddRange(texobjs, tex.atlas);
        }
    }

}
