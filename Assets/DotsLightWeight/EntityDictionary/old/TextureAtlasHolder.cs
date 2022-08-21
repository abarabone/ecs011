using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace DotsLite.Geometry
{
    using DotsLite.Common.Extension;

    /// <summary>
    /// �e�N�X�`���A�g���X�Ɋւ������o�^���邽�߂̎����B
    /// ���������́A���f�����A�g���X�����A���e�N�X�`�����t�u�I�t�Z�b�g�����B
    /// </summary>
    public class TextureAtlasDictionary : GameObjectConversionSystem
    {
        public class Data : IComponentData
        {
            // ���f���v���n�u����A�g���X���擾����
            // ���f���ƃA�g���X�͂P�΂P�ł͂Ȃ��A���΂P
            public Dictionary<SourcePrefabKeyUnit, Texture2D> modelToAtlas;

            // �A�g���X�ƃp�[�g�e�N�X�`������t�u������`���擾����
            public HashToRect texHashToUvRect;
        }

        protected override void OnUpdate()
        { }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (this.HasSingleton<Data>())
            {
                this.EntityManager.DestroyEntity(this.GetSingletonEntity<Data>());
            }
        }
    }

    public static class TextureAtlasDictionaryExtension
    {

        public static TextureAtlasDictionary.Data GetTextureAtlasDictionary(this GameObjectConversionSystem gcs)
        {
            if (!gcs.HasSingleton<TextureAtlasDictionary.Data>()) return create_();

            return gcs.GetSingleton<TextureAtlasDictionary.Data>();


            TextureAtlasDictionary.Data create_()
            {
                var newent = gcs.EntityManager.CreateEntity(typeof(TextureAtlasDictionary.Data));
                var newholder = new TextureAtlasDictionary.Data
                {
                    modelToAtlas = new Dictionary<SourcePrefabKeyUnit, Texture2D>(),
                    texHashToUvRect = new HashToRect(),
                };
                gcs.EntityManager.SetComponentData(newent, newholder);
                return newholder;
            }
        }



        //public static void SetAtlasToDictionary
        //    (this GameObjectConversionSystem gcs, GameObject obj, TextureAtlasAndParameter atlasParams)
        //{
        //    var dict = gcs.GetTextureAtlasDictionary();

        //    dict.objectToAtlas[obj] = atlasParams.atlas;

        //    foreach (var (hash, uv) in (atlasParams.texhashes, atlasParams.uvRects).Zip())
        //    {
        //        dict.texHashToUvRect[hash.atlas, hash.part] = uv;
        //    }
        //}

    }
}
