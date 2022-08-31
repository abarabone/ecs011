using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace DotsLite.Geometry.Palette
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Structure.Authoring;
    using DotsLite.Utility;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring.Palette;



    /// <summary>
    /// �C���X�^���X�p�J���[�p���b�g�o�^���[�e�B���e�B
    /// </summary>
    public static class UvPaletteDataUtility
    {

        /// <summary>
        /// �V�X�e���o�R�ŃJ���[�p���b�g�r���_�[���擾����B
        /// </summary>
        public static UvPaletteBufferBuilder GetUvPaletteBuilder(this GameObjectConversionSystem gcs)
        {
            return gcs.World.GetOrCreateSystem<UvPaletteShaderBufferConversion>().Builder;
        }


        /// <summary>
        /// �p���b�g�z�񂩂�A�O���t�B�b�N�o�b�t�@�[���\�z����B
        /// </summary>
        public static GraphicsBuffer BuildUvPaletteShaderBuffer(this Rect[] uvrects)
        {
            if (uvrects.Length == 0) return null;

            var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, uvrects.Length, sizeof(float) * 4);

            buf.SetData(uvrects);

            return buf;
        }


        /// <summary>
        /// �A�g���X���Ƃɍ쐬�����A�t�u�p���b�g�o�b�t�@���i�[����G���e�B�e�B���擾����B
        /// </summary>
        public static Entity GetUvPaletteEntity(this GameObjectConversionSystem gcs, Texture2D atlas)
        {
            var holder = gcs.World.GetOrCreateSystem<UvPaletteShaderBufferConversion>().EntityHolder;

            if (holder.TryGetValue(atlas, out var ent))
            {
                return ent;
            }

            return holder[atlas] = ent;
        }

    }



    /// <summary>
    /// ���f���C���X�^���X���Ƃɂt�u�p���b�g��o�^���A�O���t�B�b�N�o�b�t�@�p�̂t�u�z����\�z����B
    /// �܂��C���X�^���X�ɂ́A�o�b�t�@���̈ʒu���h�c�Ƃ��ĕԂ��B
    /// </summary>
    public class UvPaletteBufferBuilder
    {

        Dictionary<int, Dictionary<string, int>> dictHolder =
            new Dictionary<int, Dictionary<string, int>>();



        /// <summary>
        /// �P���f���C���X�^���X���̃p���b�g��o�^���A�h�c�i�ʒu�j��Ԃ��B
        /// </summary>
        public int RegistAndGetId(Texture2D atlas, Texture2D[] subtexs)
        {
            var dict = getInnerDict_(atlas.GetHashCode());
            var key = toKey_(subtexs);

            if (dict.TryGetValue(key, out var id))
            {
                return id;
            }

            return dict[key] = dict.Count;


            Dictionary<string, int> getInnerDict_(int atlasHash)
            {
                if (this.dictHolder.TryGetValue(atlasHash, out var innerDict))
                {
                    return innerDict;
                }

                return this.dictHolder[atlasHash] = new Dictionary<string, int>();
            }

            static string toKey_(Texture2D[] keysrc)
            {
                var q =
                    from x in keysrc
                    select x.GetHashCode()
                    ;
                return string.Join("/", q);
            }
        }

        /// <summary>
        /// �o�^���ꂽ�t�u�I�t�Z�b�g�z���Ԃ��B
        /// �t�u��A�g���X���o�^����Ă��Ȃ��ꍇ�ł��A��̔z���Ԃ��B
        /// </summary>
        public Rect[] ToUvRectArray(Texture2D atlas, HashToRect hashToRect)
        {
            var atlasHash = atlas.GetHashCode();
            if (this.dictHolder.TryGetValue(atlasHash, out var innerDict))
            {
                return new Rect[0];
            }

            var q =
                from x in innerDict.Keys
                from y in x.Split('/')
                let subtexHash = int.Parse(y)
                select hashToRect[atlasHash, subtexHash]
                ;
            return q.ToArray();
        }
    }

}
