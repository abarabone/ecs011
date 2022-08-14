using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Linq;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

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

    //public class ColorPaletteBuilder
    //{

    //    List<Color32> colors = new List<Color32>();


    //    public int AddAndGetId(Color32[] values)
    //    {
    //        var id = this.colors.Count;

    //        this.colors.AddRange(values);

    //        return id;
    //    }

    //    public unsafe GraphicsBuffer BuildShaderBuffer()
    //    {
    //        var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.colors.Count, sizeof(Color32));

    //        buf.SetData(this.colors);

    //        return buf;
    //    }
    //}


    /// <summary>
    /// ���f���̃J���[�p���b�g�����\�z����
    /// </summary>
    public static class ColorPaletteGeometryUtility
    {

        /// <summary>
        /// �p���b�g�̃T�u�C���f�b�N�X���A�T�u���b�V���P�ʂŗ񋓂���B
        /// �T�u�C���f�b�N�X�́A�}�e���A���� palette sub index ����擾����B
        /// �}�e���A���� null �̏ꍇ�́A0 ��Ԃ��B
        /// </summary>
        public static void CalculatePaletteSubIndexParameter(
            this AdditionalParameters parameters,
            (Mesh mesh, Material[] mats, Transform tf)[] mmts)
        {
            var q =
                from mmt in mmts
                select
                    from mat in mmt.mats
                    select mat.GetPaletteSubIndex()
                ;
            parameters.paletteSubIndexPerSubMesh = q.ToArrayRecursive2();
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //public static void CalculateUvIndexParameter(
        //    this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
        //    ref AdditionalParameters p,
        //    IEnumerable<int> texhashesForUvSurface)
        //{
        //    var q =
        //        from mmt in mmts
        //        select
        //            from mat in mmt.mats
        //            select mat?.mainTexture?.GetHashCode() ?? 0
        //        ;
        //    var texhashPerSubMesh = q.ToArrayRecursive2();

        //    var usedTexhashes = texhashPerSubMesh
        //        .SelectMany(x => x)
        //        .Distinct()
        //        .ToArray();

        //    p.texhashPerSubMesh = texhashPerSubMesh;

        //}
        // tex rect �� atlas �P��
        // atlas �P�ʂ� buffer ��
        // buffer �ɂ� uv surface �����o�^���Ȃ�
        // uv surfaces �� sub mesh ���Ƃ� sub index
        // ���_�� uv sub index ����������
        // ���� uv sub index �͓��� surface 
        // uv �͓����ʍ\���P�ʂ� base index �w��@�F�Ⴂ�ł��������̂Ȃ�n�j
        // �G�f�B�^�ł́A�T�u���b�V���P�ʂŃe�N�X�`�����w�肷�� scriptable object ����������
        // sub index �� palette �Ɠ����}�e���A���� palette sub index �ł悢
        // palette �̓J���[�Z�b�g�P�ʂ� base index �w��


        /// <summary>
        /// �}�e���A������A�p���b�g�C���f�b�N�X�����擾����B
        /// �Y������v���p�e�B���Ȃ��ꍇ�̃C���f�b�N�X�́A0 �Ƃ���B
        /// </summary>
        public static int GetPaletteSubIndex(this Material mat) =>
            //mat?.HasInt("Palette Sub Index") ?? false
            mat?.HasProperty("Palette Sub Index") ?? false
                ? mat.GetInt("Palette Sub Index")
                : 0
            ;


        // �E���f������ sub index ���Ƃ̐F�𒊏o
        // �Ecolor palette �ɓo�^�A�Ō�Ƀo�b�t�@���\�z
        // �E�o�b�t�@�̓V�[���ɂP��
        // �Ecolor palette �� base index ���A�C���X�^���X�Ɏ�������
        // �E�������A���łɓ����\���œo�^������΁A���� base index ���擾����
        // �P�̃��f�����\������􉽏�񂩂�A�J���[�p���b�g���\������J���[�𒊏o����B
        // ���ʂ̓J���[�̔z��ƂȂ�B�i�܂�A�J���[�p���b�g�P�́A���f���P�ɑ΂��č쐬�����j
        // �J���[�̃C���f�b�N�X�̓}�e���A���� Palette Sub Index �v���p�e�B�Ƀ��[�U�[���Z�b�g����B
        // ���ʂ̔z��́A���̃C���f�b�N�X���Ƀ\�[�g����Ă���A�C���f�b�N�X�ɊY������}�e���A�������݂��Ȃ������ꍇ�́A
        // (0, 0, 0, 0) �F�������Ƃ����B
        /// <summary>
        /// ���f���P���̃p���b�g�z��𐶐�����B
        /// </summary>
        public static Color32[] ToPaletteColorEntry(
            this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts)
        {
            var q =
                from mmt in mmts
                from mat in mmt.mats
                select (index: mat.GetPaletteSubIndex(), color: (Color32)mat.color)
                ;
            var colors = q.ToLookup(x => x.index, x => x.color);
            var maxIndex = colors.Max(x => x.Key);
            var qResult =
                from i in Enumerable.Range(0, maxIndex + 1)
                select colors.Contains(i)
                    ? colors[i].First()
                    : new Color32()
                ;
            return qResult.ToArray();
        }


    }

}
