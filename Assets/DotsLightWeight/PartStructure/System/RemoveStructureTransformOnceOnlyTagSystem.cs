using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace DotsLite.Draw
{
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.Structure;
    using System.Runtime.CompilerServices;
    using DotsLite.Dependency;


    /// <summary>
    /// �E��Ƀg�����X�t�H�[��������͖̂���
    /// �E�X���[�v���̓g�����X�t�H�[���s�v
    /// �@���X���[�v�ɐ؂�ւ�鎞�ɂs�e���I�t�ɂ���΂悢�H
    /// �@�� far �� near �� disable �ɂȂ��Ă���ق��ɔ��f����Ȃ�
    /// �E�؂�ւ�鎞�Ɉ�x�A�����g�����X�t�H�[������΂����H
    /// �@�E�R���C�_�̈ʒu�𐳂������邽�� �� far, near
    /// �@�E�f�u���̔����ʒu �� near
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Render.Draw.Transform.MonolithicBone))]
    //[UpdateAfter(typeof(SystemGroup.Presentation.Render.Draw.Transform.MotionBone))]
    //[UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    //[UpdateInGroup(typeof(InitializationSystemGroup))]
    //[UpdateAfter(typeof(BeginInitializationEntityCommandBufferSystem))]
    public class RemoveStructureTransformOnceOnlyTagSystem : DependencyAccessableSystemBase
    {


        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }

        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();

            this.Entities
                .WithBurst()
                //.WithAll<Bone.TransformOnlyOnceTag>()
                .ForEach((Entity entity, int entityInQueryIndex, ref Bone.TransformOnlyOnceTag init) =>
                {
                    //if (init.count++ < 1) return;// �b��@�ł���΃^�O���삾���ł�肽���񂾂��ǁc

                    var eqi = entityInQueryIndex;
                    cmd.RemoveComponent<Bone.TransformOnlyOnceTag>(eqi, entity);
                    cmd.RemoveComponent<Model.Bone.TransformTargetTag>(eqi, entity);
                    cmd.AddComponent<Disabled>(eqi, entity);
                })
                .ScheduleParallel();

        }


    }

}
