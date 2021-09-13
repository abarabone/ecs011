using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
////using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;

using System.Runtime.InteropServices;
using UnityEngine.Assertions.Must;
using Unity.Physics;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using System.Runtime.CompilerServices;


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
    using DotsLite.Dependency;

    using Collider = Unity.Physics.Collider;
    using SphereCollider = Unity.Physics.SphereCollider;
    using RaycastHit = Unity.Physics.RaycastHit;
    using Unity.Physics.Authoring;

    //[DisableAutoCreation]
    [UpdateAfter(typeof(ParticleLifeTimeSystem))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class FadeSystem : SystemBase
    {

        protected override void OnUpdate()
        {

            var dt = this.Time.DeltaTime;
            var currentTime = (float)this.Time.ElapsedTime;


            //this.Entities
            //    .WithName("Initialize")
            //    .WithAll<Particle.LifeTimeInitializeTag>()
            //    .WithBurst()
            //    .ForEach(
            //        (
            //            int nativeThreadIndex, int entityInQueryIndex,
            //            ref BillBoad.RotationData rot
            //        ) =>
            //        {
            //            var tid = nativeThreadIndex;
            //            var eqi = entityInQueryIndex;
            //            var rnd = Random.CreateFromIndex((uint)(eqi * tid + math.asuint(dt)));

            //            rot.Direction = rnd.NextFloat2Direction();

            //        }
            //    )
            //    .ScheduleParallel();

            this.Entities
                .WithName("AlphaBlendAdd")
                .WithBurst()
                .ForEach((
                    ref Particle.OptionalData data,
                    ref BillBoad.AlphaFadeData fader) =>
                {
                    var next = fade4_(fader.xBlend_yAdd, dt);

                    var a = (int2)(next.xy * 255);
                    data.BlendColor.a = (byte)a.x;
                    data.AdditiveColor.a = (byte)a.y;

                    fader.xBlend_yAdd.Current = next;
                })
                .ScheduleParallel();
        }

        /// <summary>
        /// �l�𑝉�������B�������� speed per sec �ɂ��B
        /// speed �̓}�C�i�X�����蓾��B
        /// �������Amin �� max �͒����Ȃ��B
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float fade_(ref BillBoad.AnimationUnit fader, float dt)
        {
            fader.Delay -= dt;
            var speed = fader.SpeedPerSec * (1.0f - math.step(0, fader.Delay));
            
            var next = fader.Current + speed * dt;
            return math.clamp(next, fader.Min, fader.Max);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float4 fade4_(BillBoad.Animation4Unit fader, float dt)
        {
            fader.Delay -= dt;
            var speed = fader.SpeedPerSec * (1.0f - math.step(0, fader.Delay));

            var next = fader.Current + speed * dt;
            return math.clamp(next, fader.Min, fader.Max);
        }

    }


}

