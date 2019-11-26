﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Abss.Geometry;
using System.Runtime.InteropServices;
using System;

namespace Abss.Motion
{
	

	public struct MotionInitializeData : IComponentData
	{
        public int MotionIndex;
        public float DelayTime;
        public float TimeScale;
        public bool IsLooping;
        public bool IsContinuous;
    }

    public struct MotionClipData : IComponentData
    {
        public BlobAssetReference<MotionBlobData> ClipData;
    }

    public struct MotionInfoData : IComponentData
	{
        public int MotionIndex;
    }

    public struct MotionStreamLinkData : IComponentData
    {
        public Entity PositionStreamTop;
        public Entity RotationStreamTop;
    }

    //public struct MotionATag : IComponentData// MotionB と区別するため暫定
    //{ }
    public struct MotionCursorData : IComponentData// MotionB 用
    {
        public float CurrentPosition;//CurrentPosition;
        public float TotalLength;
        public float Scale;
        //public StreamTimeProgressData Timer;
    }
    public struct MotionProgressTimerTag : IComponentData// MotionB 用
    { }




    public struct MotionBlend2WeightData : IComponentData
    {
        public float WeightNormalized0;
        public float WeightNormalized1;
    }
    public struct MotionBlend3WeightData : IComponentData
    {
        public float WeightNormalized0;
        public float WeightNormalized1;
        public float WeightNormalized2;
    }

    public struct BlendBoneFadeData : IComponentData
    {
        public float FadeTime;
    }

    

    static public class MotionOp
    {
        /// <summary>
        /// モーション初期化セット、disable があれば消す
        /// </summary>
        static public void Start( int entityIndex,
            ref EntityCommandBuffer.Concurrent cmd, Entity motinEntity, MotionInfoData motionInfo,
            int motionIndex, bool isLooping, float delayTime = 0.0f, bool isContinuous = true
        )
        {
            if( motionInfo.MotionIndex == motionIndex ) return;

            cmd.AddComponent( entityIndex, motinEntity,
                new MotionInitializeData
                {
                    MotionIndex = motionIndex,
                    DelayTime = delayTime,
                    IsContinuous = isContinuous,
                    IsLooping = isLooping,
                }
            );
            cmd.AddComponent( entityIndex, motinEntity, new MotionProgressTimerTag { } );
        }
        /// <summary>
        /// モーションとストリームに disable
        /// </summary>
        static public void Stop( int entityIndex, ref EntityCommandBuffer.Concurrent cmd, Entity motionEntity )
        {
            cmd.RemoveComponent<MotionProgressTimerTag>( entityIndex, motionEntity );
        }
        /// <summary>
        /// 
        /// </summary>
        static public void Change( int entityIndex,
            ref EntityCommandBuffer.Concurrent cmd, Entity motinEntity, int motionIndex, bool isContinuous = true
        )
        {
            cmd.AddComponent( entityIndex, motinEntity,
                new MotionInitializeData
                {
                    MotionIndex = motionIndex,
                    IsContinuous = isContinuous,
                }
            );
        }


        static public void SetWeight( ref MotionBlend2WeightData data, float weight0, float weight1 )
        {
            data.WeightNormalized0 = weight0 / ( weight0 + weight1 );
            data.WeightNormalized1 = 1.0f - data.WeightNormalized0;
        }
        static public void SetWeight( ref MotionBlend3WeightData data, float weight0, float weight1, float weight2 )
        {
            var totalWeight = weight0 + weight1 + weight2;
            data.WeightNormalized0 = weight0 / totalWeight;
            data.WeightNormalized1 = weight1 / totalWeight;
            data.WeightNormalized2 = 1.0f - ( data.WeightNormalized0 + data.WeightNormalized1 );
        }
    }


    static class MotionUtility
    {


        static public void InitializeCursor(
            ref this MotionCursorData motionCursor, ref MotionBlobUnit motionClip,
            float delayTime = 0.0f, float scale = 1.0f
        )
        {
            motionCursor.TotalLength = motionClip.TimeLength;
            motionCursor.CurrentPosition = -delayTime;
            motionCursor.Scale = scale;
        }
        

        /// <summary>
        /// キーバッファをストリーム先頭に初期化する。
        /// </summary>
        static public unsafe void InitializeKeys(
            ref this StreamNearKeysCacheData nearKeys,
            ref StreamKeyShiftData shift
        )
        {
            var index0 = 0;
            var index1 = math.min( 1, shift.KeyLength - 1 );
            var index2 = math.min( 2, shift.KeyLength - 1 );

            nearKeys.Time_From = shift.Keys[ index0 ].Time.x;
            nearKeys.Time_To = shift.Keys[ index1 ].Time.x;
            nearKeys.Time_Next = shift.Keys[ index2 ].Time.x;

            nearKeys.Value_Prev = shift.Keys[ index0 ].Value;
            nearKeys.Value_From = shift.Keys[ index0 ].Value;
            nearKeys.Value_To = shift.Keys[ index1 ].Value;
            nearKeys.Value_Next = shift.Keys[ index2 ].Value;

            shift.KeyIndex_Next = index2;
        }

        static public unsafe void InitializeKeysContinuous(
            ref this StreamNearKeysCacheData nearKeys,
            ref StreamKeyShiftData shift,
            float delayTimer = 0.0f// 再検討の余地あり（変な挙動あり）
        )
        {
            var index0 = 0;
            var index1 = math.min( 1, shift.KeyLength - 1 );

            nearKeys.Time_From = -delayTimer;
            nearKeys.Time_To = shift.Keys[ index0 ].Time.x;
            nearKeys.Time_Next = shift.Keys[ index1 ].Time.x;

            nearKeys.Value_To = shift.Keys[ index0 ].Value;
            nearKeys.Value_Next = shift.Keys[ index1 ].Value;

            shift.KeyIndex_Next = index1;
        }

    }
}
