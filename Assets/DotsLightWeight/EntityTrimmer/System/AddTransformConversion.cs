using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Physics.Authoring;
using Unity.Entities.Conversion;
using Unity.Entities.Hybrid;
using System.Linq;

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
    [UpdateBefore(typeof(DestroyBlankEntityConversion))]
    public class AddTransformConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            this.Entities
                .WithIncludeAll()
                .ForEach
            (
                // ������ Entity �͐M�p�ł��Ȃ��݂����i���ꂽ�j
                // �����������烏�[���h�̊֌W�łȂ񂩂���̂��H
                (/*Entity ent, */TransformAuthoring authoring) =>
                {
                    var tf = authoring.transform;
                    var ent = this.GetPrimaryEntity(tf);
                    //Debug.Log($"add all {tf.name}");

                    if (authoring.Translation) em.AddComponentData(ent, new Translation
                    {
                        Value = tf.position,
                    });

                    if (authoring.Rotation) em.AddComponentData(ent, new Rotation
                    {
                        Value = tf.rotation,
                    });

                    if (authoring.Scale) em.AddComponentData(ent, new Scale
                    {
                        Value = tf.lossyScale.magnitude,
                    });

                    if (authoring.NonUniformScale) em.AddComponentData(ent, new NonUniformScale
                    {
                        Value = tf.lossyScale,
                    });

                    //if (authoring.CompositeScale) em.AddComponentData(ent, new CompositeScale
                    //{
                    //    Value = tf.
                    //});

                    if (authoring.LocalToWorldMatrix) em.AddComponentData(ent, new LocalToWorld
                    {
                        Value = tf.localToWorldMatrix,
                    });
                }
            );

        }
    }

}
