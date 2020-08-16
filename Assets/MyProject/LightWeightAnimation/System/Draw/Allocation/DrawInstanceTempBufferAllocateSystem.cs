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
using Unity.Collections.LowLevel.Unsafe;


using Abarabone.Authoring;
using Abarabone.CharacterMotion;
using Abarabone.SystemGroup;
using Abarabone.Misc;

namespace Abarabone.Draw
{
    /// <summary>
    /// ジョブで依存関係をスケジュールしてバッファを確保したいが、うまくいかない
    /// そもそもジョブで確保できるのか、外部に渡せるのかもわからない
    /// </summary>
    //[DisableAutoCreation]
    [UpdateInGroup( typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup ) )]
    //[UpdateBefore( typeof( BoneToDrawInstanceSystem ) )]
    [UpdateAfter(typeof(DrawCullingSystem))]
    public class DrawInstanceTempBufferAllocateSystem : JobComponentSystem
    {



        EntityQuery drawQuery;



        protected override void OnCreate()
        {
            this.drawQuery = this.GetEntityQuery(
                ComponentType.ReadOnly<DrawModel.InstanceCounterData>(),
                ComponentType.ReadWrite<DrawModel.VectorBufferData>(),
                ComponentType.ReadOnly<DrawModel.BoneVectorSettingData>()
            );
        }



        protected override unsafe JobHandle OnUpdate( JobHandle inputDeps )
        {
            var nativeBuffers = this.GetComponentDataFromEntity<DrawSystem.NativeTransformBufferData>();
            var drawSysEnt = this.GetSingletonEntity<DrawSystem.NativeTransformBufferData>();

            var instanceOffsetType = this.GetComponentTypeHandle<DrawModel.VectorBufferData>();
            var instanceCounterType = this.GetComponentTypeHandle<DrawModel.InstanceCounterData>();// isReadOnly: true );
            var boneInfoType = this.GetComponentTypeHandle<DrawModel.BoneVectorSettingData>();// isReadOnly: true );

            var chunks = this.drawQuery.CreateArchetypeChunkArray( Allocator.TempJob );

            inputDeps = this.Job
                .WithBurst()
                .WithDeallocateOnJobCompletion( chunks )
                .WithNativeDisableParallelForRestriction( nativeBuffers )
                .WithCode(
                    () =>
                    {
                        var totalVectorLength = sumAndSetVectorOffsets_();

                        var nativeBuffer
                            = new SimpleNativeBuffer<float4, Temp>( totalVectorLength );

                        nativeBuffers[ drawSysEnt ]
                            = new DrawSystem.NativeTransformBufferData { Transforms = nativeBuffer };

                        calculateVectorOffsetPointersInBuffer_( nativeBuffer.pBuffer );
                    }
                )
                .Schedule( inputDeps );

            return inputDeps;



            int sumAndSetVectorOffsets_()
            {
                var sum = 0;
                for( var i = 0; i < chunks.Length; i++ )
                {
                    var chunk = chunks[ i ];
                    var offsets = chunk.GetNativeArray( instanceOffsetType );
                    var counters = chunk.GetNativeArray( instanceCounterType );
                    var infos = chunk.GetNativeArray( boneInfoType );

                    for( var j = 0; j < chunk.Count; j++ )
                    {
                        var instanceOffset = offsets[j].VectorLengthOfInstanceAdditionalData;

                        offsets[ j ] = new DrawModel.VectorBufferData
                        {
                            VectorOffsetPerModel = sum,
                            VectorLengthOfInstanceAdditionalData = instanceOffset,
                        };

                        var instanceCount = counters[ j ].InstanceCounter.Count;
                        var instanceVectorSize = infos[ j ].BoneLength * infos[ j ].VectorLengthInBone + instanceOffset;
                        var modelBufferSize = instanceCount * instanceVectorSize;

                        sum += modelBufferSize;
                    }
                }

                return sum;
            }

            void calculateVectorOffsetPointersInBuffer_( float4* pBufferStart )
            {
                for( var i = 0; i < chunks.Length; i++ )
                {
                    var chunk = chunks[ i ];
                    var offsets = chunk.GetNativeArray( instanceOffsetType );

                    for( var j = 0; j < chunk.Count; j++ )
                    {
                        var offset = offsets[ j ];

                        offset.pVectorPerModelInBuffer = pBufferStart + offset.VectorOffsetPerModel;

                        offsets[j] = offset;
                    }
                }
            }

        }

    }

}
