using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Physics.Authoring;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;
using Unity.Mathematics;
using Unity.Collections;
using System;

namespace DotsLite.EntityTrimmer.Authoring
{
    using Utilities;

    /// <summary>
    /// TransformConversion �ɂ���ĕt�^�����A�g�����X�t�H�[���n�̃R���|�[�l���g�f�[�^���폜����B
    /// ExcludeTransformConversion �Ƃ� �͂�
    /// </summary>
    /// transform conversion �� [UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    //[DisableAutoCreation]
    //[UpdateInGroup(typeof(GameObjectBeforeConversionGroup))]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    [UpdateBefore(typeof(RemoveTransformAllConversion))]
    public class CopyTransformToMarkerConversion1 : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            this.Entities
                .ForEach((Entity e, Transform tf) =>
                {
                    if (tf.)

                    Debug.Log(tf.name);

                });
        }
    }

}
