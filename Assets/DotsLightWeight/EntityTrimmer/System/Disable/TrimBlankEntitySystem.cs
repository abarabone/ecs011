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

    [DisableAutoCreation]
    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class TrimBlankEntitySystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            var em = this.DstEntityManager;

            // ��̃G���e�B�e�B�����擾�ł���N�G�����Ȃ񂩂Ȃ��񂩂�
            //this.Entities
            //    .ForEach(
            //        (Entity ent) =>
            //        {
            //            var em = this.DstEntityManager;

            //            if (em.GetComponentCount(ent) > 0) return;

            //            em.DestroyEntity(ent);
            //        }
            //    );

        }
    }

}
