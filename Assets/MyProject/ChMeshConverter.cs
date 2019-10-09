﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Abss.Motion;

namespace Abss.Geometry
{
    
	static public class ChMeshConverter
	{

        
		static public Mesh ConvertToChMesh( Mesh srcmesh, MotionClip motionClip )
		{
            var dstmesh = new Mesh();

            var vtxs        = srcmesh.vertices;
            var boneWeights = srcmesh.boneWeights;
            var bindposes   = srcmesh.bindposes;

            dstmesh.vertices = ConvertVertices( vtxs, boneWeights, bindposes );
            dstmesh.normals = srcmesh.normals;
            dstmesh.uv = srcmesh.uv;
            dstmesh.triangles = srcmesh.triangles;
            dstmesh.SetUVs( channel: 1, CreateBoneWeightUvs( boneWeights, motionClip ) );
            dstmesh.bounds = srcmesh.bounds;
            dstmesh.colors = CreateBoneIndexColors( boneWeights, motionClip );
            dstmesh.bindposes = CreateReBindPoses( bindposes, motionClip );
            return dstmesh;

            //var newmesh = new Mesh();
            //newmesh.vertices = MeshUtility.ConvertVertices( vtxs, boneWeights, bindposes );
            //newmesh.boneWeights = boneWeights;
            //newmesh.bindposes = bindposes;
            //newmesh.normals = srcmesh.normals;
            //newmesh.uv = srcmesh.uv;
            //newmesh.triangles = srcmesh.triangles;
            //newmesh.AddBoneInfoFrom( uvChannelForWeight: 1, motionClip );
            //return newmesh;
        }


		/// <summary>
		/// 頂点をトランスフォームし、新しい配列として返す。
		/// </summary>
		static public Vector3[] ConvertVertices( Vector3[] vertices, BoneWeight[] weights, Matrix4x4[] matrices )
		{
			var q =
				from x in Enumerable.Zip( vertices, weights, (v, w) => (v, w) )
				select matrices[ x.w.boneIndex0 ].MultiplyPoint( x.v )
				;

			return q.ToArray();
		}
		
		///// <summary>
		///// メッシュに MotionClip から得たボーン情報を挿入する。
		///// インデックス／バインドポーズは変換後のメッシュ用に再解釈／整列され、Colors と uvChannelForWeight の uv に出力される。
		///// </summary>
		//static public Mesh AddBoneInfoFrom( this Mesh mesh, int uvChannelForWeight, MotionClip motionClip )
		//{
		//	var weights		= mesh.boneWeights;
		//	var bindPoses	= mesh.bindposes;
		//	var im			= motionClip.IndexMapFbxToMotion;

		//	var qIndices	= weights
		//		.Select( x => new Color(im[x.boneIndex0], im[x.boneIndex1], im[x.boneIndex2], im[x.boneIndex3]) );

		//	var qWeights	= weights
		//		.Select( x => new Vector4(x.weight0, x.weight1, x.weight2, x.weight3) );

		//	var qBindPoses	=
		//		from imotion in Enumerable.Range( 0, motionClip.StreamPaths.Length )
		//		join mapper in im.Select( (imotion, ifbx) => (imotion, ifbx) )
		//			on imotion equals mapper.imotion
		//		select bindPoses[ mapper.ifbx ]
		//		;

		//	mesh.colors	= qIndices.ToArray();
		//	mesh.SetUVs( uvChannelForWeight, qWeights.ToList() );
		//	mesh.bindposes = qBindPoses.ToArray();

		//	return mesh;
		//}

		static public Color[] CreateBoneIndexColors( BoneWeight[] boneWeights, MotionClip motionClip )
		{
			var im = motionClip.IndexMapFbxToMotion;

            var qIndices = boneWeights
                .Select( x => new Color(im[x.boneIndex0], im[x.boneIndex1], im[x.boneIndex2], im[x.boneIndex3]) );

            return qIndices.ToArray();
		}

		static public List<Vector4> CreateBoneWeightUvs( BoneWeight[] boneWeights, MotionClip motionClip )
		{
			var qWeights	= boneWeights
				.Select( x => new Vector4(x.weight0, x.weight1, x.weight2, x.weight3) );

			return qWeights.ToList();
		}

		static public Matrix4x4[] CreateReBindPoses( Matrix4x4[] bindPoses, MotionClip motionClip )
		{
			var im = motionClip.IndexMapFbxToMotion;

			var qBindPoses	=
				from imotion in Enumerable.Range( 0, motionClip.StreamPaths.Length )
				join mapper in im.Select( (imotion, ifbx) => (imotion, ifbx) )
					on imotion equals mapper.imotion
				select bindPoses[ mapper.ifbx ]
				;

			return qBindPoses.ToArray();
		}
	}
}