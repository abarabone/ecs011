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
        /// <returns></returns>
        //public static UvPaletteBuilder GetUvPaletteBuilder(this GameObjectConversionSystem gcs)
        //{
        //    return gcs.World.GetExistingSystem<ColorPaletteShaderBufferConversion>().Builder;
        //}


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

    }



    /// <summary>
    /// ���f���C���X�^���X���Ƃɂt�u�p���b�g��o�^���A�O���t�B�b�N�o�b�t�@�p�̂t�u�z����\�z����B
    /// �܂��C���X�^���X�ɂ́A�o�b�t�@���̈ʒu���h�c�Ƃ��ĕԂ��B
    /// </summary>
    public class UvPaletteBufferBuilder
    {

        Dictionary<int, Dictionary<string, int>> texIdDict = new 



        /// <summary>
        /// �P���f���C���X�^���X���̃p���b�g��o�^���A�h�c�i�ʒu�j��Ԃ��B
        /// </summary>
        public int RegistAndGetId(Texture2D atlas, Texture2D[] subtexs)
        {
            var key = tokey_(subtexs);

            if (this.texIdDict.TryGetValue(key, out var id))
            {
                return id;
            }

            var nextid = this.texIdDict.Count;

            this.texIdDict.Add(key, nextid);

            return nextid;


            static string toKey_(Texture2D[] keysrc)
            {
                var q =
                    from x in keysrc
                    select x.GetHashCode()
                    ;
                return string.Join("/", q);
            }

            int addIndex_(int length)
            {
                var index = this.nextIndex;
                this.nextIndex += length;
                return index;
            }
        }

        /// <summary>
        /// �o�^���ꂽ���ׂĂ̂t�u�I�t�Z�b�g�z���Ԃ��B
        /// </summary>
        public Rect[] ToUvRectArray(Texture2D atlas, HashToRect hashToRect)
        {
            var atlasHash = atlas.GetHashCode();
            var q =
                from x in this.texIdDict.Keys
                from y in x.subtexs
                select hashToRect[atlasHash, y.GetHashCode()]
                ;
            return q.ToArray();
        }
    }

}
