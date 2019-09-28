﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using UniRx;
//using UniRx.Triggers;
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

namespace Abss.Motion
{
	

	
	public struct StreamInitialLabel : IComponentData
	{}

	public struct StreamInitialLabelFor1pos : IComponentData
	{}


	public struct StreamCacheUnit
	{
		public float	TimeProgress;

		public StreamNearKeysCacheData	cache;
	}


	
	/// <summary>
	/// 現在キーの位置と、ストリームデータへの参照を保持する。
	/// </summary>
	public struct StreamKeyShiftData : IComponentData
	{
		public NativeSlice<KeyUnitInNative> Keys;
		
		public int      KeyIndex_Next;
	}

	/// <summary>
	/// 時間は deltaTime を加算して進める。
	/// （スタート時刻と現在時刻を比較する方法だと、速度変化や休止ができないため）
	/// </summary>
	public struct StreamTimeProgressData : IComponentData
	{
		public float	TimeProgress;	// スタート時は 0
		public float	TimeLength;
		public float	TimeScale;
	}

	/// <summary>
	/// 現在キー周辺のキーキャッシュデータ。
	/// キーがシフトしたときのみ、次のキーを読めば済むようにする。
	/// </summary>
	public struct StreamNearKeysCacheData : IComponentData
	{
		public float    Time_From;
		public float    Time_To;
		public float    Time_Next;

		// 補間にかけるための現在キー周辺４つのキー
		public float4   Value_Prev;
		public float4   Value_From;	// これが現在キー
		public float4   Value_To;
		public float4   Value_Next;
	}

	/// <summary>
	/// 現在キー周辺のキーと現在時間から保管した計算結果。
	/// </summary>
	public struct StreamInterpolatedData : IComponentData
	{
		public float4	Value;
	}

	

	// ストリーム拡張 -----------------------------------------------------------------------------------------------
	
	static public class StreamUtility
	{
		
		/// <summary>
		/// キーバッファをストリーム先頭に初期化する。
		/// </summary>
		static public void InitializeKeys(
			ref this StreamNearKeysCacheData	nearKeys,
			ref StreamKeyShiftData				shift,
			ref StreamTimeProgressData			progress,
			float								timeOffset = 0.0f
		)
		{
			var index0	= 0;
			var index1	= math.min( 1, shift.Keys.Length - 1 );
			var index2	= math.min( 2, shift.Keys.Length - 1 );
			
			nearKeys.Time_From = shift.Keys[ index0 ].Time.x;
			nearKeys.Time_To   = shift.Keys[ index1 ].Time.x;;
			nearKeys.Time_Next = shift.Keys[ index2 ].Time.x;

			nearKeys.Value_Prev = shift.Keys[ index0 ].Value;
			nearKeys.Value_From = shift.Keys[ index0 ].Value;
			nearKeys.Value_To	= shift.Keys[ index1 ].Value;
			nearKeys.Value_Next = shift.Keys[ index2 ].Value;

			shift.KeyIndex_Next		= index2;
			
			progress.TimeProgress	= timeOffset;
		}

		/// <summary>
		/// キーバッファを次のキーに移行する。終端まで来たら、最後のキーのままでいる。
		/// </summary>
		static public void ShiftKeysIfOverKeyTime(
			ref this StreamNearKeysCacheData	nearKeys,
			ref StreamKeyShiftData				shift,
			in  StreamTimeProgressData			progress
		)
		{
			if( progress.TimeProgress < nearKeys.Time_To ) return;


			var nextIndex	= math.min( shift.KeyIndex_Next + 1, shift.Keys.Length - 1 );
			var nextKey		= shift.Keys[ nextIndex ];
			
			nearKeys.Time_From	= nearKeys.Time_To;
			nearKeys.Time_To	= nearKeys.Time_Next;
			nearKeys.Time_Next	= nextKey.Time.x;

			nearKeys.Value_Prev	= nearKeys.Value_From;
			nearKeys.Value_From	= nearKeys.Value_To;
			nearKeys.Value_To	= nearKeys.Value_Next;
			nearKeys.Value_Next	= nextKey.Value;

			shift.KeyIndex_Next	= nextIndex;
		}
		
		/// <summary>
		/// キーバッファを次のキーに移行する。ループアニメーション対応版。
		/// </summary>
		static public void ShiftKeysIfOverKeyTimeForLooping(
			ref this StreamNearKeysCacheData	nearKeys,
			ref StreamKeyShiftData				shift,
			ref StreamTimeProgressData			timer
		)
		{
			if( timer.TimeProgress < nearKeys.Time_To ) return;


			var isEndOfStream	= timer.TimeProgress >= timer.TimeLength;
			
			var timeOffset	= getTimeOffsetOverLength( in timer, isEndOfStream );

			var nextIndex	= getNextKeyIndex( in shift, isEndOfStream );
			var nextKey		= shift.Keys[ nextIndex ];
			
			var time_from	= nearKeys.Time_To;
			var time_to		= nearKeys.Time_Next;
			var time_next	= nextKey.Time.x;

			nearKeys.Time_From	= time_from	- timeOffset;
			nearKeys.Time_To	= time_to - timeOffset;
			nearKeys.Time_Next	= time_next;

			nearKeys.Value_Prev	= nearKeys.Value_From;
			nearKeys.Value_From	= nearKeys.Value_To;
			nearKeys.Value_To	= nearKeys.Value_Next;
			nearKeys.Value_Next	= nextKey.Value;

			shift.KeyIndex_Next	= nextIndex;
			
			timer.TimeProgress	-= timeOffset;

			return;

			float getTimeOffsetOverLength( in StreamTimeProgressData progress_, bool isEndOfStream_ )
			{
				return math.select( 0.0f, progress_.TimeLength, isEndOfStream_ );
			}

			int getNextKeyIndex( in StreamKeyShiftData shift_, bool isEndOfStream_ )
			{
				var iKeyLast		= shift_.Keys.Length - 1;
				var iKeyNextNext	= shift_.KeyIndex_Next + 1;

				var isEndOfKey		= iKeyNextNext > iKeyLast;

				var iWhenStayInnerKey	= math.min( iKeyNextNext, iKeyLast );
				var iWhenOverLastKey	= iKeyNextNext - math.select( 0, iKeyLast, isEndOfKey );
				
				return math.select( iWhenStayInnerKey, iWhenOverLastKey, isEndOfStream_ );
				// こうなってくると、素直に分岐したほうがいいんだろうかｗ←いや、はやかった
			}
		}
		
		/// <summary>
		/// 時間を進める。
		/// </summary>
		static public void Progress( ref this StreamTimeProgressData timer, float deltaTime )
		{
			timer.TimeProgress += deltaTime * timer.TimeScale;
		}

		static public float CaluclateTimeNormalized
			( ref this StreamNearKeysCacheData nearKeys, float timeProgress )
		{
			var progress	= timeProgress - nearKeys.Time_From;
			var length		= nearKeys.Time_To - nearKeys.Time_From;

			var progress_div_length	= progress * math.rcp( length );

			return math.select( progress_div_length, 1.0f, length == 0.0f );// select( 偽, 真, 条件 );
		}

		/// <summary>
		/// 補完する。
		/// </summary>
		static public float4 Interpolate
			( ref this StreamNearKeysCacheData nearKeys, float normalizedTimeProgress )
		{
			
			//var s = 1;//math.sign( math.dot( nearKeys.Value_From, nearKeys.Value_To ) );

			return Utility.Interpolate(
				nearKeys.Value_Prev,
				nearKeys.Value_From,
				nearKeys.Value_To,// * s,
				nearKeys.Value_Next,// * s,
				math.saturate( normalizedTimeProgress )
			);
		}
	}

	// --------------------------------------------------------------------------------------------------------------

}