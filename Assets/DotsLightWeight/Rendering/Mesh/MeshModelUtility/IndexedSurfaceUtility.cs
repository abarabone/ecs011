using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Linq;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Geometry
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Structure.Authoring;


    //public class UvPaletteBuilder
    //{
    //    Dictionary<string, (int i, Rect[] uvs)> idList = new Dictionary<string, (int, Rect[])>();

    //    int nextIndex;

    //    public int this[Rect[] values]
    //    {
    //        get => this.idList[toKey(values)].i;
    //    }

    //    public void Add(Rect[] values)
    //    {
    //        var key = toKey(values);
    //        if (this.idList.ContainsKey(key))
    //        {
    //            this.idList[key] = (this.nextIndex++, values);
    //        }
    //    }

    //    static string toKey(Rect[] keysrc)
    //    {
    //        var q =
    //            from x in keysrc
    //            select $"{x.x},{x.y}"
    //            ;
    //        return string.Join("/", q);
    //    }
    //}

    public static class IndexedSurfaceUtility
    {
        //// �Euv palette �ɓo�^�A�Ō�Ƀo�b�t�@���\�z
        //// �E�o�b�t�@�̓V�[���łP��
        //// �Euv palette 
        ///// <summary>
        ///// �P�̃��f�����\������􉽏�񂩂�A�T�[�t�F�X���\������ uv rect �𒊏o����B
        ///// ���ʂ� uv rect �̔z��ƂȂ�B
        ///// 
        ///// </summary>
        //public static Rect[] ToPaletteUvEntry(
        //    this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
        //    Func<int, Rect> texHashToUvRectFunc)
        //{
        //    var q =
        //        from mmt in mmts
        //        from mat in mmt.mats
        //        let index = mat.GetPaletteSubIndex()
        //        let uv = texHashToUvRectFunc(mat.mainTexture.GetHashCode())
        //        select (index, uv)
        //        ;
        //    var uvs = q.ToLookup(x => x.index, x => x.uv);
        //    var maxIndex = uvs.Max(x => x.Key);
        //    var qResult =
        //        from i in Enumerable.Range(0, maxIndex + 1)
        //        select uvs.Contains(i)
        //            ? uvs[i].First()
        //            : new Rect()
        //        ;
        //    return qResult.ToArray();
        //}
    }
}