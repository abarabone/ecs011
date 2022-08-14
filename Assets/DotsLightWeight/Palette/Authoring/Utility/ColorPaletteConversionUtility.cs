using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace DotsLite.Geometry.Palette
{
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;

    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Structure.Authoring;
    using DotsLite.Utility;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw;
    using DotsLite.Draw.Authoring.Palette;

    public static class ColorPaletteConversionUtility
    {

        public static void SetColorPalette(this GameObjectConversionSystem gcs, Entity ent, ColorPaletteAsset palette)
        {
            if (palette == null) return;

            var em = gcs.DstEntityManager;
            var paletteIdBase = gcs.GetColorPaletteBuilder().RegistAndGetId(palette.Colors);
            em.AddComponentData(ent, new Palette.ColorPaletteData
            {
                BaseIndex = paletteIdBase,
            });
            em.AddComponentData(ent, new Draw.DrawInstance.TransferSpecialTag { });
        }


        public static void AddColorPalletComponents(this GameObjectConversionSystem gcs, Entity ent)
        {
            var em = gcs.DstEntityManager;
            em.AddComponentData(ent, new Palette.ColorPaletteData { });
            em.AddComponentData(ent, new Draw.DrawInstance.TransferSpecialTag { });
        }
    }

    /// <summary>
    /// �C���X�^���X�p�J���[�p���b�g�o�^���[�e�B���e�B
    /// </summary>
    public static class ColorPaletteDataUtility
    {

        /// <summary>
        /// �C���X�^���X�P�ɑ΂��A�p���b�g�h�c�f�[�^��ǉ�����B
        /// �h�c�́A�p���b�g�̃o�b�t�@���̈ʒu�B�p���b�g�̓A�Z�b�g����擾����B
        /// </summary>
        public static void SetColorPaletteComponent(this GameObjectConversionSystem gcs, GameObject main, ColorPaletteAsset palette)
        {
            //if (model.GetType().GetGenericTypeDefinition() != typeof(MeshWithPaletteModel<,>).GetGenericTypeDefinition()) return;
            if (palette == null) return;

            var em = gcs.DstEntityManager;
            var ent = gcs.GetPrimaryEntity(main);

            em.AddComponentData(ent, new Palette.ColorPaletteData
            {
                BaseIndex = gcs.GetColorPaletteBuilder().RegistAndGetId(palette.Colors),
            });
        }

        /// <summary>
        /// �V�X�e���o�R�ŃJ���[�p���b�g�r���_�[���擾����B
        /// </summary>
        /// <returns></returns>
        public static ColorPaletteBuilder GetColorPaletteBuilder(this GameObjectConversionSystem gcs)
        {
            return gcs.World.GetExistingSystem<ColorPaletteShaderBufferConversion>().Palettes;
        }

        /// <summary>
        /// �p���b�g�z�񂩂�A�O���t�B�b�N�o�b�t�@�[���\�z����B
        /// </summary>
        public static GraphicsBuffer BuildShaderBuffer(this uint[] colors)
        {
            if (colors.Length == 0) return null;

            //var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, this.colors.Count, sizeof(uint4));
            var buf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, colors.Length, sizeof(uint));

            buf.SetData(colors);

            return buf;
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //public static void AddPalletLinkData_IfHas(this IMeshModel model, GameObjectConversionSystem gcs, Entity ent)
        //{
        //    var component = model as MonoBehaviour;
        //    var paletteAuthor = component.GetComponentInParent<ColorPaletteBufferAuthoring>();
        //    if (paletteAuthor == null) return;

        //    var em = gcs.DstEntityManager;
        //    em.AddComponentData(ent, new DrawModelShaderBuffer.ColorPaletteLinkData
        //    {
        //        BufferEntity = gcs.GetPrimaryEntity(paletteAuthor),
        //    });
        //}
    }
}
