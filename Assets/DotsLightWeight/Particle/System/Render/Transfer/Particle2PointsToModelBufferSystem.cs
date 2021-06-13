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

using System;

namespace DotsLite.Draw
{
    using DotsLite.CharacterMotion;
    using DotsLite.SystemGroup;
    using DotsLite.Geometry;
    using DotsLite.Character;
    using DotsLite.Particle;
    using DotsLite.Dependency;
    using DotsLite.Model;


    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
    //[UpdateAfter(typeof())]
    //[UpdateBefore( typeof( BeginDrawCsBarier ) )]
    [UpdateBefore(typeof(DrawMeshCsSystem))]
    public class Particle2PointsToModelBufferSystem : DependencyAccessableSystemBase
    {


        BarrierDependency.Sender bardep;

        protected override void OnCreate()
        {
            base.OnCreate();

            this.bardep = BarrierDependency.Sender.Create<DrawMeshCsSystem>(this);
        }

        protected unsafe override void OnUpdate()
        {
            using var barScope = bardep.WithDependencyScope();


            //var unitSizesOfDrawModel = this.GetComponentDataFromEntity<DrawModel.BoneUnitSizeData>( isReadOnly: true );
            var offsetsOfDrawModel = this.GetComponentDataFromEntity<DrawModel.InstanceOffsetData>(isReadOnly: true);

            this.Entities
                .WithBurst()
                .WithReadOnly(offsetsOfDrawModel)
                .WithAll<DrawInstance.PsylliumTag>()
                .WithNone<DrawInstance.MeshTag>()
                .WithNone<BillBoad.UvCursorData, BillBoad.CursorToUvIndexData>()
                .ForEach(
                    (
                        ref Psyllium.TranslationTailData tail,
                        in Translation pos,
                        in DrawInstance.TargetWorkData target,
                        in DrawInstance.ModelLinkData linker,
                        in Particle.AdditionalData additional
                    )
                =>
                    {
                        if (target.DrawInstanceId == -1) return;


                        var offsetInfo = offsetsOfDrawModel[linker.DrawModelEntityCurrent];

                        const int vectorLength = (int)BoneType.PtoP;
                        var lengthOfInstance = offsetInfo.VectorOffsetPerInstance + vectorLength;
                        var instanceBufferOffset = target.DrawInstanceId * lengthOfInstance;

                        var i = instanceBufferOffset + offsetInfo.VectorOffsetPerInstance;

                        var size = additional.Radius;
                        var color = math.asfloat(additional.Color.ToUint());

                        tail.Size = size;

                        var pModel = offsetInfo.pVectorOffsetPerModelInBuffer;
                        pModel[i + 0] = tail.PositionAndSize;
                        pModel[i + 1] = new float4(pos.Value, color);
                    }
                )
                .ScheduleParallel();
        }
    }

}
