using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;

namespace DotsLite.Character.Action
{
    using DotsLite.Dependency;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;
    using DotsLite.Targeting;
    using DotsLite.Arms;
    using Motion = DotsLite.CharacterMotion.Motion;


    /// <summary>
    /// 歩き時のアクションステート
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateAfter(typeof(AntMoveDirectionSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public partial class AntrAttackActionSystem : DependencyAccessableSystemBase
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

            var motionInfos = this.GetComponentDataFromEntity<Motion.InfoData>(isReadOnly: true);
            var motionCursors = this.GetComponentDataFromEntity<Motion.CursorData>();
            //var motionWeights = this.GetComponentDataFromEntity<MotionBlend2WeightData>();

            var targetposs = this.GetComponentDataFromEntity<TargetSensorResponse.PositionData>(isReadOnly: true);
            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            var mvspds = this.GetComponentDataFromEntity<Move.SpeedParamaterData>(isReadOnly: true);

            //var triggers = this.GetComponentDataFromEntity<FunctionUnit.TriggerData>();

            var currentTime = this.Time.ElapsedTime;


            var post = cmdScope.CommandBuffer;
            this.Entities
                .WithName("Reset")
                .WithAll<AntAction.AttackState, CharacterAction.DamageState>()
                .ForEach((Entity entity) =>
                {
                    post.RemoveComponent<AntAction.AttackState>(entity);
                    post.AddComponent<AntAction.WalkState>(entity);
                })
                .Schedule();

            this.Entities
                .WithName("Action")
                .WithBurst()
                .WithAll<AntTag>()
                .WithNone<CharacterAction.DamageState>()
                .WithReadOnly(motionInfos)
                .WithReadOnly(targetposs)
                .WithReadOnly(poss)
                .WithReadOnly(mvspds)
                .WithNativeDisableParallelForRestriction(motionCursors)
                //.WithNativeDisableParallelForRestriction(triggers)// ヤバいかも
                .ForEach(
                    (
                        Entity entity_, int entityInQueryIndex,
                        ref AntAction.AttackState state,
                        in ActionState.MotionLinkDate mlink,
                        in ActionState.PostureLinkData plink,
                        in TargetSensorHolderLink.HolderLinkData sensorHolderLink,
                        in AntAction.AttackTimeRange tr_,
                        in DynamicBuffer<FunctionHolder.LinkData> flinks
                    )
                =>
                    {
                        var eqi = entityInQueryIndex;
                        var entity = entity_;


                        var tr = tr_;

                        switch (state.Phase)
                        {
                            case 0:
                                initPhase_(ref state, in mlink, in plink);
                                break;
                            case 1:
                                prevShotPhase_(ref state, in mlink, in sensorHolderLink, in flinks);
                                break;
                            case 2:
                                shotPhase_(ref state, in mlink, flinks);
                                break;
                            case 3:
                                afterShotPhase_(ref state, in mlink, in plink, in sensorHolderLink, in flinks);
                                break;
                        }

                        return;


                        void initPhase_(
                            ref AntAction.AttackState state,
                            in ActionState.MotionLinkDate mlink,
                            in ActionState.PostureLinkData plink)
                        {

                            cmd.AddComponent(eqi, plink.PostureEntity,
                                new Move.EasingSpeedData
                                {
                                    Rate = 3.0f,
                                    TargetSpeedPerSec = 0.0f,
                                }
                            );


                            var motion = new MotionOperator
                                (cmd, motionInfos, motionCursors, mlink.MotionEntity, eqi);

                            motion.Start(Motion_ant.attack02, isLooping: true, delayTime: 0.1f);
                            

                            state.Phase++;
                        }


                        void prevShotPhase_(
                            ref AntAction.AttackState state,
                            in ActionState.MotionLinkDate mlink,
                            in TargetSensorHolderLink.HolderLinkData sensorHolderLink,
                            in DynamicBuffer<FunctionHolder.LinkData> flinks)
                        {
                            var cursor = motionCursors[mlink.MotionEntity];
                            var normalPosision = cursor.CurrentPosition * math.rcp(cursor.TotalLength);

                            if (normalPosision >= tr.st)//0.2f)
                            {
                                state.Phase++;

                                cmd.AddComponent(eqi, flinks[0].FunctionEntity,
                                    new FunctionUnitAiming.HighAngleShotData
                                    {
                                        TargetPostureEntity = sensorHolderLink.HolderEntity,
                                        EndTime = (float)currentTime + (tr.ed - tr.st),
                                    }
                                );
                            }
                        }


                        void shotPhase_(
                            ref AntAction.AttackState state,
                            in ActionState.MotionLinkDate mlink,
                            in DynamicBuffer<FunctionHolder.LinkData> flinks)
                        {

                            var acidgun = flinks[0].FunctionEntity;

                            //triggers[acidgun] = new FunctionUnit.TriggerData
                            //{
                            //    IsTriggered = true,
                            //};


                            var cursor = motionCursors[mlink.MotionEntity];
                            var normalPosision = cursor.CurrentPosition * math.rcp(cursor.TotalLength);

                            if (normalPosision >= tr.ed)//0.3f)
                            {
                                state.Phase++;
                            }
                        }


                        void afterShotPhase_(
                            ref AntAction.AttackState state,
                            in ActionState.MotionLinkDate mlink,
                            in ActionState.PostureLinkData plink,
                            in TargetSensorHolderLink.HolderLinkData sensorHolderLink,
                            in DynamicBuffer<FunctionHolder.LinkData> flinks)
                        {
                            var targetpos = targetposs[sensorHolderLink.HolderEntity].Position;
                            var originpos = poss[plink.PostureEntity].Value;

                            if (math.distancesq(targetpos, originpos) > 15.0f * 15.0f)
                            {
                                cmd.RemoveComponent<AntAction.AttackState>(eqi, entity);
                                cmd.AddComponent<AntAction.WalkState>(eqi, entity);

                                cmd.AddComponent(eqi, plink.PostureEntity,
                                    new Move.EasingSpeedData
                                    {
                                        Rate = 5.0f,
                                        TargetSpeedPerSec = mvspds[plink.PostureEntity].SpeedPerSecMax,
                                    }
                                );

                                return;
                            }


                            var cursor = motionCursors[mlink.MotionEntity];
                            var normalPosision = cursor.CurrentPosition * math.rcp(cursor.TotalLength);

                            if (normalPosision > 0.95f)
                            {
                                state.Phase = 0;
                            }

                        }
                    }
                )
                .ScheduleParallel();
                

        }

    }
}

