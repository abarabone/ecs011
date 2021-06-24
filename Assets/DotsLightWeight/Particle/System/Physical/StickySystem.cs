using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
//using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Particle
{

    using DotsLite.Model;
    using DotsLite.Model.Authoring;
    using DotsLite.Arms;
    using DotsLite.Character;
    using DotsLite.Draw;
    using DotsLite.Particle;
    using DotsLite.CharacterMotion;
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.Collision;
    using DotsLite.SystemGroup;
    using DotsLite.Common.Extension;

    using Random = Unity.Mathematics.Random;

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogicSystemGroup))]
    [UpdateAfter(typeof(BulletInitializeSystem))]
    public class StickyInitializeSystem : SystemBase
    {


        protected override void OnUpdate()
        {
            this.Entities
                .WithBurst()
                .WithAll<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref Spring.StickyTEntityLastData stickylast,
                    in Bullet.InitializeFromEmitterData init) =>
                {

                    stickylast.Target = init.EffectMuzzleEntity;

                })
                .ScheduleParallel();

        }
    }

    //[DisableAutoCreation]
    [UpdateBefore(typeof(MoveSpringSystem))]
    [UpdateInGroup(typeof(SystemGroup.Simulation.Move.ObjectMoveSystemGroup))]
    public class StickySystem : SystemBase
    {


        protected override void OnUpdate()
        {

            var poss = this.GetComponentDataFromEntity<Translation>(isReadOnly: true);
            var rots = this.GetComponentDataFromEntity<Rotation>(isReadOnly: true);


            this.Entities
                .WithName("copy_self")
                .WithBurst()
                .WithAll<Spring.StickySelfFirstTag>()
                .WithNone<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref DynamicBuffer<LineParticle.TranslationTailLineData> tails,
                    in Translation pos) =>
                {

                    tails.ElementAt(0).Position = pos.Value;

                })
                .ScheduleParallel();

            this.Entities
                .WithName("copy_first_point")
                .WithBurst()
                .WithNone<Particle.LifeTimeInitializeTag>()
                .ForEach((
                    ref DynamicBuffer<LineParticle.TranslationTailLineData> tails,
                    //ref Translation pos,
                    in Spring.StickyPointFirstData stick) =>
                {

                    tails.ElementAt(0).Position = stick.Position.xyz;
                    //pos.Value = stick.Position.xyz;

                })
                .ScheduleParallel();

            this.Entities
                .WithName("copy_first_T")
                .WithBurst()
                .WithNone<Particle.LifeTimeInitializeTag>()
                .WithReadOnly(poss)
                .ForEach((
                    ref DynamicBuffer<LineParticle.TranslationTailLineData> tails,
                    in Spring.StickyTEntityFirstData stick) =>
                {

                    var pos = poss[stick.Target].Value;

                    tails.ElementAt(0).Position = pos;

                })
                .ScheduleParallel();

            this.Entities
                .WithName("copy_last_T")
                .WithBurst()
                .WithNone<Particle.LifeTimeInitializeTag>()
                .WithReadOnly(poss)
                .ForEach((
                    ref DynamicBuffer<LineParticle.TranslationTailLineData> tails,
                    in Spring.StickyTEntityLastData stick) =>
                {

                    var pos = poss[stick.Target].Value;

                    tails.ElementAt(tails.Length - 1).Position = pos;

                })
                .ScheduleParallel();

        }

    }

}

