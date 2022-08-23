using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace DotsLite.Geometry.inner.palette
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Misc;
    using DotsLite.Geometry;

    public static class UvPaletteMeshUtility
    {

        /// <summary>
        /// ���f���\���v�f�̃}�e���A���z�񂩂�A�}�e���A���̒ʂ��ԍ��z����\�z����B
        /// �ʂ��ԍ��́A�e�T�u���b�V���ɑ΂��ĕK�v�Ȃ̂ŁA�t�Ɍ����ƌ��̃��b�V���̃T�u�}�e���A���ƃT�u���b�V���͓������̕K�v������B
        /// </summary>
        public static void CalculateUvPaletteSubIndexParameter(
            this AdditionalParameters parameters,
            (Mesh mesh, Material[] mats, Transform tf)[] mmts)
        {
            var qMatLength =
                from mmt in mmts
                select mmt.mats.Length
                ;
            var qStartIndex = new[] { 0 }.Concat(qMatLength);

            var q =
                from x in (mmts, qStartIndex).Zip()
                let mmt = x.src0
                let ist = x.src1
                select
                    from imat in mmt.mats.Select((x, i) => i)
                    select ist + imat
                ;
            parameters.uvPaletteSubIndexPerSubMesh = q.ToArrayRecursive2();
        }
    }
}
