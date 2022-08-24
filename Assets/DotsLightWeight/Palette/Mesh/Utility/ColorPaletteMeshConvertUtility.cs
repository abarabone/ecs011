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

    public static class ColorPaletteMeshUtility
    {

        /// <summary>
        /// ���b�V���\�z�p�̃p�����[�^�Ƃ��āA
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
                    select getPaletteSubIndex_(mat)
                ;
            parameters.paletteSubIndexPerSubMesh = q.ToArrayRecursive2();


            /// <summary>
            /// �}�e���A������A�p���b�g�C���f�b�N�X�����擾����B
            /// �Y������v���p�e�B���Ȃ��ꍇ�̃C���f�b�N�X�́A0 �Ƃ���B
            /// </summary>
            static int getPaletteSubIndex_(Material mat) =>
                (mat?.HasProperty("_PaletteSubIndex") ?? false)
                    ? mat.GetInt("_PaletteSubIndex")
                    : 0
                ;
        }
    }
}
