//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Runtime.CompilerServices;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using UnityEngine;
//using Unity.Entities;
//using UnityEngine.Rendering;
//using Unity.Collections;
//using Unity.Linq;
//using Unity.Mathematics;
//using Unity.Collections.LowLevel.Unsafe;

//namespace DotsLite.Geometry.Palette
//{
//    using DotsLite.Common.Extension;
//    using DotsLite.Utilities;
//    using DotsLite.Geometry.inner;
//    using DotsLite.Geometry.inner.unit;
//    using DotsLite.Structure.Authoring;
//    using DotsLite.Utility;
//    using DotsLite.Model.Authoring;
//    using DotsLite.Draw;
//    using DotsLite.Draw.Authoring.Palette;

//    //public class ColorPaletteBuilder
//    //{

//    //    List<Color32> colors = new List<Color32>();


//    //    public int AddAndGetId(Color32[] values)
//    //    {
//    //        var id = this.colors.Count;

//    //        this.colors.AddRange(values);

//    //        return id;
//    //    }

//    //    public unsafe GraphicsBuffer BuildShaderBuffer()
//    //    {
//    //        var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.colors.Count, sizeof(Color32));

//    //        buf.SetData(this.colors);

//    //        return buf;
//    //    }
//    //}


//    /// <summary>
//    /// ���f���̂t�u�p���b�g�����\�z����
//    /// </summary>
//    public static class UvPaletteMeshUtility
//    {

//        // tex rect �� atlas �P��
//        // atlas �P�ʂ� buffer ��
//        // buffer �ɂ� uv surface �����o�^���Ȃ�
//        // uv surfaces �� sub mesh ���Ƃ� sub index
//        // ���_�� uv sub index ����������
//        // ���� uv sub index �͓��� surface 
//        // uv �͓����ʍ\���P�ʂ� base index �w��@�F�Ⴂ�ł��������̂Ȃ�n�j
//        // �G�f�B�^�ł́A�T�u���b�V���P�ʂŃe�N�X�`�����w�肷�� scriptable object ����������
//        // sub index �� palette �Ɠ����}�e���A���� palette sub index �ł悢
//        // palette �̓J���[�Z�b�g�P�ʂ� base index �w��

//        /// <summary>
//        /// 
//        /// </summary>
//        public static void CalculateUvSubIndexParameter(
//            this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
//            ref AdditionalParameters p,
//            IEnumerable<int> texhashesForUvSurface)
//        {
//            var q =
//                from mmt in mmts
//                select
//                    from mat in mmt.mats
//                    select mat?.mainTexture?.GetHashCode() ?? 0
//                ;
//            var texhashPerSubMesh = q.ToArrayRecursive2();

//            var usedTexhashes = texhashPerSubMesh
//                .SelectMany(x => x)
//                .Distinct()
//                .ToArray();

//            p.texhashPerSubMesh = texhashPerSubMesh;

//        }
//    }

//}
