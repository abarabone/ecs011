﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using Abss.Misc;

namespace Abss.Motion
{

    public struct MotionBlobAsset
    {



    }


    

	public struct MotionBlobData
	{
        public BlobArray<int> BoneParents;

		public BlobArray<MotionBlobUnit> Motions;
	}

	public struct MotionBlobUnit
	{
		public float	TimeLength;
		public Bounds	LocalAABB;
		public WrapMode	WrapMode;

		public BlobArray<SectionBlobUnit> Sections;
	}

	public struct SectionBlobUnit
	{
		public BlobArray<StreamBlobUnit> Streams;
	}

	public struct StreamBlobUnit
	{
		public BlobArray<KeyBlobUnit> Keys;
	}

	public struct KeyBlobUnit
	{
		public float	Time;
		public float4	Value;
	}


    public static partial class MotionClipUtility
	{

        static public BlobAssetReference<MotionBlobData> ConvertToBlobData( this MotionClip motionClip )
        {

            using( var builder = new BlobBuilder(Allocator.Temp) )
            {
                var srcMotions = motionClip.MotionData.Motions;
            
                ref var dstRoot = ref builder.ConstructRoot<MotionBlobData>();
                copyMotionToBlob( ref motionClip.MotionData, ref dstRoot, builder );
                
                return builder.CreateBlobAssetReference<MotionBlobData>( Allocator.Persistent );
            }

            void aaa()
            {
			    var qParentBones =
				    from parentPath in src.StreamPaths.Select( path => getParent(path) )	// ペアレントパス列挙
				    join pathIndex in src.StreamPaths.Select( (path,i) => (path,i) )		// 順序数を保持する索引と外部結合する
					    on parentPath equals pathIndex.path
					    into pathIndexes
				    from pathIndex in pathIndexes.DefaultIfEmpty( (path:"",i:-1) )			// 結合できないパスは -1
				    select pathIndex.i
				    ;

			    string getParent( string path ) => Path.GetDirectoryName(path).Replace("\\","/");
            }
            
            void copyMotionToBlob
                ( ref MotionDataInAsset srcRoot, ref MotionBlobData dstRoot, BlobBuilder builder )
            {
                var srcMotions = srcRoot.Motions;

                var dstMotions = builder.Allocate( ref dstRoot.Motions, srcRoot.Motions.Length );

                for( var i = 0; i < srcMotions.Length; i++ )
                {
                    copySectionToBlob( ref srcMotions[i], ref dstMotions[i], builder );
                    dstMotions[i].TimeLength    = srcMotions[i].TimeLength;
                    dstMotions[i].LocalAABB     = srcMotions[i].LocalAABB;
                    dstMotions[i].WrapMode      = srcMotions[i].WrapMode;
                }
            }

            void copySectionToBlob
                ( ref MotionDataUnit srcMotion, ref MotionBlobUnit dstMotion, BlobBuilder builder )
            {
                var srcSections = srcMotion.Sections;

                var dstSections = builder.Allocate( ref dstMotion.Sections, srcMotion.Sections.Length );
                
                for( var i = 0; i < srcSections.Length; i++ )
                {
                    copyStreamToBlob( ref srcSections[i], ref dstSections[i], builder );
                }
            }
            
            void copyStreamToBlob
                ( ref SectionDataUnit srcSection, ref SectionBlobUnit dstSection, BlobBuilder builder )
            {
                var srcStreams = srcSection.Streams;

                var dstStreams = builder.Allocate( ref dstSection.Streams, srcSection.Streams.Length );
                
                for( var i = 0; i < srcStreams.Length; i++ )
                {
                    copyKeyToBlob( ref srcStreams[i], ref dstStreams[i], builder );
                }
            }
            
            void copyKeyToBlob
                ( ref StreamDataUnit srcStream, ref StreamBlobUnit dstStream, BlobBuilder builder )
            {
                var srcKeys = srcStream.keys;

                var dstKeys = builder.Allocate( ref dstStream.Keys, srcKeys.Length );
                
                for( var i = 0; i < srcKeys.Length; i++ )
                {
                    dstKeys[i].Time     = srcKeys[i].Time;
                    dstKeys[i].Value    = srcKeys[i].Value;
                }
            }
        }
        
    }
}

