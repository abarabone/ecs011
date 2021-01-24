using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


namespace Abarabone.Geometry
{

    public class TextureAtlasHolderData : IComponentData
    {
        // �Q�[���I�u�W�F�N�g�A�v���n�u����A�g���X���擾����
        public Dictionary<GameObject, Texture2D> objectToAtlas;

        // �A�g���X�ƃp�[�g�e�N�X�`������t�u������`���擾����
        public Dictionary<TextureHashKey, Rect> texHashToUvRect;
    }

    public static class TextureAtlasHolder
    {

        public static TextureAtlasHolderData GetTextureAtlasHolder(this GameObjectConversionSystem gcs)
        {
            var holder = gcs.GetSingleton<TextureAtlasHolderData>();
            if (holder != null) return holder;

            var newHolder = new TextureAtlasHolderData
            {
                texHashToUvRect = new Dictionary<TextureHashKey, Rect>(),
            };
            gcs.SetSingleton(holder);
            return newHolder;
        }

    }
}
