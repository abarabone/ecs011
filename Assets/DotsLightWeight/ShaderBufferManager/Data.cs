using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

namespace DotsLite.Draw
{

    /// <summary>
    /// �J���[�p���b�g�̃O���t�B�b�N�o�b�t�@�f�[�^
    /// </summary>
    public static class ShaderBuffer
    {
        /// <summary>
        /// �R���o�[�W�������ɍ쐬����\�[�X�f�[�^
        /// </summary>
        public class ColorPaletteSrcData : IComponentData
        {
            public uint[] Colors;
            public int NameId;
        }

        /// <summary>
        /// �O���t�B�b�N�o�b�t�@
        /// </summary>
        public class ColorPaletteData : IComponentData
        {
            public GraphicsBuffer Buffer;
            public int NameId;
        }
    }


    /// <summary>
    /// draw model �����@�J���[�p���b�g�֌W�̃f�[�^
    /// </summary>
    public static class DrawModelShaderBuffer
    {
        /// <summary>
        /// �J���[�p���b�g�̃O���t�B�b�N�o�b�t�@�ւ̃����N
        /// </summary>
        public class ColorPaletteLinkData : IComponentData
        {
            public Entity BufferEntity;
        }
    }
}
