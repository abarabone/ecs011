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
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class RemoveTransformAllSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            this.Entities
                .WithNone<ExcludeRemoveTransform>()
                .ForEach
            (
                (Entity ent) =>
                {
                    em.RemoveComponent<LocalToParent>(ent);
                    em.RemoveComponent<LocalToWorld>(ent);
                    em.RemoveComponent<PreviousParent>(ent);
                    em.RemoveComponent<Parent>(ent);
                    em.RemoveComponent<Child>(ent);
                    em.RemoveComponent<Translation>(ent);
                    em.RemoveComponent<Rotation>(ent);
                    em.RemoveComponent<NonUniformScale>(ent);
                    em.RemoveComponent<Scale>(ent);
                }
            );

        }
    }

}
