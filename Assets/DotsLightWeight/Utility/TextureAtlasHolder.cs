using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace Abarabone.Geometry
{

    public class TextureAtlasDictionary : GameObjectConversionSystem
    {
        public class Data : IComponentData
        {
            // �Q�[���I�u�W�F�N�g�A�v���n�u����A�g���X���擾����
            public Dictionary<GameObject, Texture2D> objectToAtlas;

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

        public static TextureAtlasDictionary.Data GetTextureAtlasHolder(this GameObjectConversionSystem gcs)
        {
            if (!gcs.HasSingleton<TextureAtlasDictionary.Data>()) return create_();

            return gcs.GetSingleton<TextureAtlasDictionary.Data>();


            TextureAtlasDictionary.Data create_()
            {
                var newent = gcs.DstEntityManager.CreateEntity(typeof(TextureAtlasDictionary.Data));
                var newholder = new TextureAtlasDictionary.Data
                {
                    objectToAtlas = new Dictionary<GameObject, Texture2D>(),
                    texHashToUvRect = new Dictionary<(int atlas, int part), Rect>(),
                };
                gcs.DstEntityManager.SetComponentData(newent, newholder);
                return newholder;
            }
        }

    }
}
