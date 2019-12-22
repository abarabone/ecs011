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
using UnityEngine.InputSystem;
using UnityEngine.Assertions;

using Collider = Unity.Physics.Collider;
using SphereCollider = Unity.Physics.SphereCollider;
using RaycastHit = Unity.Physics.RaycastHit;

using Abss.Misc;
using Abss.Utilities;
using Abss.Character;
using Abss.Physics;
using Abss.SystemGroup;

namespace Abss.Character
{

    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Simulation.Move.ObjectMoveSystemGroup ) )]
    public class IsGroundAroundSystem : JobComponentSystem
    {
        
        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            this.buildPhysicsWorldSystem = this.World.GetOrCreateSystem<BuildPhysicsWorld>();
        }
        

        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            inputDeps = new IsGroundAroundJob
            {
                CollisionWorld = this.buildPhysicsWorldSystem.PhysicsWorld,//.CollisionWorld,
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }




        [BurstCompile]
        struct IsGroundAroundJob : IJobForEachWithEntity
            <GroundHitResultData, GroundHitSphereData, Translation, Rotation>
        {

            [ReadOnly] public PhysicsWorld CollisionWorld;


            public unsafe void Execute(
                Entity entity, int index,
                [WriteOnly] ref GroundHitResultData ground,
                [ReadOnly] ref GroundHitSphereData sphere,
                [ReadOnly] ref Translation pos,
                [ReadOnly] ref Rotation rot
            )
            {
                //var a = new NativeList<DistanceHit>( Allocator.Temp );
                var rtf = new RigidTransform( rot.Value, pos.Value );

                var hitInput = new PointDistanceInput
                {
                    Position = math.transform( rtf, sphere.Center ),
                    MaxDistance = sphere.Distance,
                    Filter = sphere.Filter,
                };
                //var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref a );// 自身のコライダを除外できればシンプルになるんだが…

                var collector = new AnyDistanceHitExcludeSelfCollector( sphere.Distance, entity, CollisionWorld.Bodies );
                var isHit = this.CollisionWorld.CalculateDistance( hitInput, ref collector );

                //var castInput = new RaycastInput
                //{
                //    Start = math.transform( rtf, sphere.Center ),
                //    End = math.transform( rtf, sphere.Center ) + ( math.up() * -sphere.Distance ),
                //    Filter = sphere.filter,
                //};
                //var collector = new AnyHitExcludeSelfCollector2( 1.0f, entity, this.CollisionWorld.Bodies );
                //var isHit = this.CollisionWorld.CastRay( castInput, ref collector );

                //ground = new GroundHitResultData { IsGround = ( isHit && a.Length > 1 ) };
                //ground.IsGround = a.Length > 1;
                ground.IsGround = collector.NumHits > 0;
                //ground.IsGround = isHit;

                //a.Dispose();
            }
        }


    }

}
