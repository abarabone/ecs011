﻿using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections.LowLevel.Unsafe;
using System;
using Unity.Jobs.LowLevel.Unsafe;
using System.Runtime.CompilerServices;

namespace DotsLite.Structure
{
    using DotsLite.Dependency;


    // parts の destroy とここでの wakeup components 着脱が競合する可能性もはらんでいるかも？？ em 専用 system をつくるべきなのかなぁ
    // ↑システムの順序をちゃんとしたら大丈夫だった！
    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    //[UpdateAfter(typeof(StructureEnvelopeMessageAllocationSystem))]
    [UpdateBefore(typeof(StructureEnvelopeMessageFreeJobSystem))]
    public class StructureEnvelopeMessageApplySystem : DependencyAccessableSystemBase
    {


        StructureEnvelopeMessageAllocationSystem allocationSystem;

        BarrierDependency.Sender freedep;
        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            base.OnCreate();

            this.allocationSystem = this.World.GetOrCreateSystem<StructureEnvelopeMessageAllocationSystem>();
            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
            this.freedep = BarrierDependency.Sender.Create<StructureEnvelopeMessageFreeJobSystem>(this);
        }

        protected override void OnUpdate()
        {
            using var freeScope = this.freedep.WithDependencyScope();
            using var cmdScope = this.cmddep.WithDependencyScope();


            var cmd = cmdScope.CommandBuffer.AsParallelWriter();


            //this.Dependency = new JobExecution
            //{
            //    Cmd = cmd,

            //    //compoundTags = this.GetComponentDataFromEntity<Main.CompoundColliderTag>(isReadOnly: true),
            //    binderLinks = this.GetComponentDataFromEntity<Structure.Main.BinderLinkData>(isReadOnly: true),

            //    parts = this.GetComponentDataFromEntity<Structure.Part.PartData>(isReadOnly: true),
            //    linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true),
            //}
            //.ScheduleParallelKey(this.allocationSystem.Reciever, 32, this.Dependency);
            this.Dependency = this.allocationSystem.Reciever.ScheduleKeyParallel2(
                this.Dependency, 32, new JobExecution
                {
                    Cmd = cmd,

                    //compoundTags = this.GetComponentDataFromEntity<Main.CompoundColliderTag>(isReadOnly: true),
                    binderLinks = this.GetComponentDataFromEntity<Structure.Main.BinderLinkData>(isReadOnly: true),

                    parts = this.GetComponentDataFromEntity<Structure.Part.PartData>(isReadOnly: true),
                    linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true),
                });
            //this.Dependency = new JobExecution2
            //{
            //    Cmd = cmd,

            //    //compoundTags = this.GetComponentDataFromEntity<Main.CompoundColliderTag>(isReadOnly: true),
            //    binderLinks = this.GetComponentDataFromEntity<Structure.Main.BinderLinkData>(isReadOnly: true),

            //    parts = this.GetComponentDataFromEntity<Structure.Part.PartData>(isReadOnly: true),
            //    linkedGroups = this.GetBufferFromEntity<LinkedEntityGroup>(isReadOnly: true),

            //    MessageHolder = this.allocationSystem.Reciever.Holder.messageHolder,
            //    KeyEntities = this.allocationSystem.Reciever.Holder.keyEntities.AsDeferredJobArray(),
            //}
            //.Schedule(this.allocationSystem.Reciever.Holder.keyEn tities, 32, this.Dependency);// this.allocationSystem.Reciever.combinealldependents(this.Dependency));
        }

        [BurstCompile]
        //public struct JobExecution : HitMessage<EnvelopeHitMessage>.IApplyJobExecutionForKey
        public struct JobExecution : IApplyJobExecutionForKey<EnvelopeHitMessage>
        {

            public EntityCommandBuffer.ParallelWriter Cmd;
            //public float CurrentTime;

            //[ReadOnly] public ComponentDataFromEntity<Main.CompoundColliderTag> compoundTags;
            [ReadOnly] public ComponentDataFromEntity<Main.BinderLinkData> binderLinks;

            [ReadOnly] public ComponentDataFromEntity<Part.PartData> parts;
            [ReadOnly] public BufferFromEntity<LinkedEntityGroup> linkedGroups;

            [BurstCompile]
            public void Execute(int index, Entity targetEntity, NativeMultiHashMap<Entity, EnvelopeHitMessage>.Enumerator msgs)
            {
                //if (this.compoundTags.HasComponent(targetEntity)) return;


                var damage = 0.0f;
                var force = float3.zero;

                //var corps = this.Corpss[targetEntity].Corps;


                //foreach (var msg in msgs)
                //{

                //}

                //this.Cmd.AddComponent(index, targetEntity, new Unity.Physics.PhysicsVelocity { });
                var binder = this.binderLinks[targetEntity];
                this.Cmd.ChangeComponentsToWakeUp(targetEntity, index, binder, this.parts, this.linkedGroups);
            }
        }
        //[BurstCompile]
        //struct JobExecution : IJobParallelForDefer
        //{
        //    [ReadOnly] public NativeArray<Entity> KeyEntities;
        //    [ReadOnly] public NativeMultiHashMap<Entity, EnvelopeHitMessage> MessageHolder;


        //    public EntityCommandBuffer.ParallelWriter Cmd;
        //    //public float CurrentTime;

        //    //[ReadOnly] public ComponentDataFromEntity<Main.CompoundColliderTag> compoundTags;
        //    [ReadOnly] public ComponentDataFromEntity<Main.BinderLinkData> binderLinks;

        //    [ReadOnly] public ComponentDataFromEntity<Part.PartData> parts;
        //    [ReadOnly] public BufferFromEntity<LinkedEntityGroup> linkedGroups;



        //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //    public void Execute(int index)
        //    {
        //        var targetEntity = this.KeyEntities[index];
        //        var msgs = this.MessageHolder.GetValuesForKey(targetEntity);

        //        //if (this.compoundTags.HasComponent(targetEntity)) return;


        //        var damage = 0.0f;
        //        var force = float3.zero;

        //        //var corps = this.Corpss[targetEntity].Corps;


        //        //foreach (var msg in msgs)
        //        //{

        //        //}

        //        //this.Cmd.AddComponent(index, targetEntity, new Unity.Physics.PhysicsVelocity { });
        //        var binder = this.binderLinks[targetEntity];
        //        this.Cmd.ChangeComponentsToWakeUp(targetEntity, index, binder, this.parts, this.linkedGroups);
        //    }
    }

}
