using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;

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
    public static class ColorPaletteDataUtility
    {

        /// <summary>
        /// �V�X�e���o�R�ŃJ���[�p���b�g�r���_�[���擾����B
        /// </summary>
        /// <returns></returns>
        public static ColorPaletteBufferBuilder GetColorPaletteBuilder(this GameObjectConversionSystem gcs)
        {
            return gcs.World.GetOrCreateSystem<ColorPaletteShaderBufferConversion>().Builder;
        }


        /// <summary>
        /// �p���b�g�z�񂩂�A�O���t�B�b�N�o�b�t�@�[���\�z����B
        /// </summary>
        public static GraphicsBuffer BuildColorPaletteShaderBuffer(this uint[] colors)
        {
            if (colors.Length == 0) return null;

            //var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.colors.Count, sizeof(uint4));
            var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, colors.Length, sizeof(uint));

            buf.SetData(colors);

            return buf;
        }





        ///// <summary>
        ///// ���f���P���̃p���b�g�z��𐶐�����B
        ///// </summary>
        //public static Color32[] ToColorPaletteEntry(
        //    this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts)
        //{
        //    // �E���f������ sub index ���Ƃ̐F�𒊏o
        //    // �Ecolor palette �ɓo�^�A�Ō�Ƀo�b�t�@���\�z
        //    // �E�o�b�t�@�̓V�[���ɂP��
        //    // �Ecolor palette �� base index ���A�C���X�^���X�Ɏ�������
        //    // �E�������A���łɓ����\���œo�^������΁A���� base index ���擾����
        //    // �P�̃��f�����\������􉽏�񂩂�A�J���[�p���b�g���\������J���[�𒊏o����B
        //    // ���ʂ̓J���[�̔z��ƂȂ�B�i�܂�A�J���[�p���b�g�P�́A���f���P�ɑ΂��č쐬�����j
        //    // �J���[�̃C���f�b�N�X�̓}�e���A���� Palette Sub Index �v���p�e�B�Ƀ��[�U�[���Z�b�g����B
        //    // ���ʂ̔z��́A���̃C���f�b�N�X���Ƀ\�[�g����Ă���A�C���f�b�N�X�ɊY������}�e���A�������݂��Ȃ������ꍇ�́A
        //    // (0, 0, 0, 0) �F�������Ƃ����B
        //    var q =
        //        from mmt in mmts
        //        from mat in mmt.mats
        //        select (index: getPaletteSubIndex_(mat), color: (Color32)mat.color)
        //        ;
        //    var colors = q.ToLookup(x => x.index, x => x.color);
        //    var maxIndex = colors.Max(x => x.Key);
        //    var qResult =
        //        from i in Enumerable.Range(0, maxIndex + 1)
        //        select colors.Contains(i)
        //            ? colors[i].First()
        //            : new Color32()
        //        ;
        //    return qResult.ToArray();


        //    /// <summary>
        //    /// �}�e���A������A�p���b�g�C���f�b�N�X�����擾����B
        //    /// �Y������v���p�e�B���Ȃ��ꍇ�̃C���f�b�N�X�́A0 �Ƃ���B
        //    /// </summary>
        //    static int getPaletteSubIndex_(Material mat) =>
        //        //mat?.HasInt("Palette Sub Index") ?? false
        //        mat?.HasProperty("Palette Sub Index") ?? false
        //            ? mat.GetInt("Palette Sub Index")
        //            : 0
        //        ;
        //}

    }



    /// <summary>
    /// ���f���C���X�^���X���ƂɃJ���[�p���b�g��o�^���A�O���t�B�b�N�o�b�t�@�p�̃J���[�z����\�z����B
    /// �܂��C���X�^���X�ɂ́A�o�b�t�@���̈ʒu���h�c�Ƃ��ĕԂ��B
    /// </summary>
    public class ColorPaletteBufferBuilder
    {

        Dictionary<string, (int id, Color32[] colors)> colors = new Dictionary<string, (int, Color32[])>();

        int nextIndex = 0;


        /// <summary>
        /// �P���f���C���X�^���X���̃p���b�g��o�^���A�h�c�i�ʒu�j��Ԃ��B
        /// </summary>
        public int RegistAndGetId(Color32[] values)
        {
            var key = toKey(values); Debug.Log(key);

            if (this.colors.TryGetValue(key, out var x))
            {
                return x.id;
            }

            this.colors[key] = (this.nextIndex, values);

            return addIndex_(values.Length);


            static string toKey(Color32[] keysrc)
            {
                var q =
                    from x in keysrc
                    select $"{x.r},{x.g},{x.b},{x.a}"
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
        /// �o�^���ꂽ���ׂẴJ���[�z���Ԃ��B
        /// </summary>
        public uint[] ToArray()
        {
            var q =
                from x in this.colors
                from y in x.Value.colors
                select y.ToUint()//y
                ;
            return q.ToArray();
        }
    }

}
