﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

using Abss.Cs;
using Abss.Arthuring;
using Abss.SystemGroup;

namespace Abss.Motion
{
    
    //[UpdateAfter(typeof())]
    [UpdateInGroup(typeof(MotionSystemGroup))]
    public class MotionProgressSystem : JobComponentSystem
    {

        
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {


            inputDeps = new MotionProgressJob
            {
                DeltaTime = Time.deltaTime,
            }
            .Schedule( this, inputDeps );


            return inputDeps;
        }



        /// <summary>
        /// ストリーム回転 → 補間
        /// </summary>
        [BurstCompile, RequireComponentTag(typeof(MotionProgressTimerTag))]
        struct MotionProgressJob : IJobForEach
            <MotionCursorData>
        {

            public float DeltaTime;


            public void Execute(
                ref MotionCursorData cursor
            )
            {

                progressTimeForLooping( ref cursor );

                cursor.Progress( this.DeltaTime );

            }


            void progressTimeForLooping(
                ref MotionCursorData cousor
            )
            {
                var isEndOfStream = cousor.CurrentPosition >= cousor.TotalLength;

                var timeOffset = getTimeOffsetOverLength( in cousor, isEndOfStream );

                cousor.CurrentPosition -= timeOffset;

                return;


                float getTimeOffsetOverLength( in MotionCursorData cursor_, bool isEndOfStream_ )
                {
                    return math.select( 0.0f, cursor_.TotalLength, isEndOfStream_ );
                }
            }

        }
    }

}
