﻿using System.Collections;
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

using Abss.Misc;
using Abss.Utilities;
using Abss.SystemGroup;
using Abss.Character;
using Abss.Motion;

namespace Abss.Character
{

    /// <summary>
    /// 歩き時のアクションステート
    /// 
    /// </summary>
    //[DisableAutoCreation]
    [UpdateAfter(typeof(PlayerMoveDirectionSystem))]
    [UpdateInGroup( typeof( SystemGroup.Presentation.Logic.ObjectLogicSystemGroup ) )]
    public class AntrWalkActionSystem : JobComponentSystem
    {

        EntityCommandBufferSystem ecb;



        protected override void OnCreate()
        {
            this.ecb = this.World.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
            //this.ecb = this.World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();

        }


        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new AntWalkActionJob
            {
                Commands = this.ecb.CreateCommandBuffer().ToConcurrent(),

                MotionInfos = this.GetComponentDataFromEntity<MotionInfoData>( isReadOnly: true ),
                MotionCursors = this.GetComponentDataFromEntity<MotionCursorData>(),
                MotionWeights = this.GetComponentDataFromEntity<MotionBlend2WeightData>(),

                WallHitResults = this.GetComponentDataFromEntity<WallHitResultData>( isReadOnly: true ),
                Wallings = this.GetComponentDataFromEntity<WallHunggingData>( isReadOnly: true ),

                Rotations = this.GetComponentDataFromEntity<Rotation>(),
                GravityFactors = this.GetComponentDataFromEntity<PhysicsGravityFactor>(),
            }
            .Schedule( this, inputDeps );
            this.ecb.AddJobHandleForProducer( inputDeps );

            return inputDeps;
        }


        [RequireComponentTag(typeof(AntTag))]
        struct AntWalkActionJob : IJobForEachWithEntity
            <AntWalkActionState, MoveHandlingData, CharacterLinkData>
        {

            public EntityCommandBuffer.Concurrent Commands;

            [ReadOnly] public ComponentDataFromEntity<MotionInfoData> MotionInfos;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<MotionCursorData> MotionCursors;
            [NativeDisableParallelForRestriction]
            public ComponentDataFromEntity<MotionBlend2WeightData> MotionWeights;
            
            [ReadOnly] public ComponentDataFromEntity<WallHitResultData> WallHitResults;
            [ReadOnly] public ComponentDataFromEntity<WallHunggingData> Wallings;

            [NativeDisableParallelForRestriction]
            [WriteOnly] public ComponentDataFromEntity<Rotation> Rotations;
            [NativeDisableParallelForRestriction]
            [WriteOnly] public ComponentDataFromEntity<PhysicsGravityFactor> GravityFactors;


            public void Execute(
                Entity entity, int jobIndex,
                ref AntWalkActionState state,
                [ReadOnly] ref MoveHandlingData hander,
                [ReadOnly] ref CharacterLinkData linker
            )
            {
                ref var acts = ref hander.ControlAction;

                var motion = new MotionOperator( this.Commands, this.MotionInfos, this.MotionCursors, linker.MainMotionEntity, jobIndex );

                motion.Start( Motion_ant.walking, isLooping: true, delayTime: 0.1f );

                //this.Rotations[ linker.PostureEntity ] =
                //    new Rotation { Value = quaternion.LookRotation( math.normalize( acts.MoveDirection ), math.up() ) };


            }
        }

    }
}
