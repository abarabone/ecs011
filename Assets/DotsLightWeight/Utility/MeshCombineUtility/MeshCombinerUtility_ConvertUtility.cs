using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;
using Abarabone.Common.Extension;

namespace Abarabone.Geometry
{

	/// <summary>
	/// ���b�V���v�f�i���_�A�t�u�A�C���f�b�N�X�ȂǕʁX�j���Ƃ̃R���o�[�g���s���A�z��ɂ߂ĕԂ��B
	/// �֐��̈����͂��ׂ� Unity �I�u�W�F�N�g�ł͂Ȃ����߁A���̃N���X�̑S�֐��̓T�u�X���b�h�ŏ����ł���B
	/// </summary>
	static public class ConvertUtility
	{

		/// <summary>
		/// 
		/// </summary>
		public static Vector3[] ToVerticesArray
			(this IEnumerable<IEnumerable<Vector3>> verticesPerMeshes, IEnumerable<Matrix4x4> mtObjects, Matrix4x4 mtBaseInv)
		{
			var qVertex =
				from xy in (verticesPerMeshes, mtObjects).Zip((x, y) => (vtxs: x, mt: y))
				let mt = mtBaseInv * xy.mt
				from vtx in xy.vtxs
				select mt.MultiplyPoint3x4(vtx)
				;

			return qVertex.ToArray();
		}


		/// <summary>
		/// 
		/// </summary>
		public static Vector3[] ToNormalsArray
			(IEnumerable<IEnumerable<Vector3>> normalsPerMeshes, IEnumerable<Matrix4x4> mtObjects, Matrix4x4 mtBaseInv)
		{
			var qNormal =
				from xy in (normalsPerMeshes, mtObjects).Zip((x, y) => (nms: x, mt: y))
				let mt = mtBaseInv * xy.mt
				from nm in xy.nms
				select mt.MultiplyVector(nm)
				;

			return qNormal.ToArray();
		}


		/// <summary>
		/// 
		/// </summary>
		public static Color32[] ToColor32Array
			(
				IEnumerable<IEnumerable<int>> vertexCount_PerSubmeshPerMesh,
				IEnumerable<int> partsIds_PerMesh,
				IEnumerable<Material[]> materials_PerMesh,
				IEnumerable<Material> materialsCombined
			)
		{

			var vtxCount_PerMesh =
				from vtxCounts in vertexCount_PerSubmeshPerMesh select vtxCounts.Sum();

			// ���_���Ƃ̃p�[�c�h�c�����ׂăN�G������B
			var qPid_PerVtx =
				queryPartIdx_PerVtx_(vtxCount_PerMesh, partsIds_PerMesh);


			var qDstMatHashes = from mat in materialsCombined select mat.GetHashCode();
			var qSrcMatHashes = materials_PerMesh.To(PerSubMeshPerMesh.QueryMaterialHash);

			// �p���b�g�h�c�����ׂĂ̒��_���ƂɃN�G������B
			var qPallets_PerVtx =
				queryePallet_PerVtx_(vertexCount_PerSubmeshPerMesh, qSrcMatHashes, qDstMatHashes);


			// �p�[�c�h�c�ƃp���b�g�h�c�𒸓_�J���[�ɗ��Ƃ����ށB
			var qPidPallet_PerVtx =
				from xy in (qPid_PerVtx, qPallets_PerVtx).Zip((x, y) => (pidx: x, pallet: y))
				select new Color32
				(
					r: 0,
					g: (byte)xy.pallet,
					b: (byte)xy.pidx.elementIndex,
					a: (byte)xy.pidx.bitIndex
				);

			//return qPidPallet_PerVtx.Do(x=>Debug.Log(x)).ToArray();
			return qPidPallet_PerVtx.ToArray();


			/// <summary>
			/// �p�[�c�h�c�����ׂĂ̒��_���ƂɃN�G������B
			/// �p�[�c�h�c�́A�e�p�[�c�ɐݒ肳�ꂽ partid ����擾����B
			/// </summary>
			IEnumerable<(int int4Index, int memberIndex, int bitIndex)>
				queryPartIdx_PerVtx__(IEnumerable<int> vtxCount_PerMesh_, IEnumerable<int> partId_PerMesh_)
			{
				var qPidEveryParts =
					from pid in partId_PerMesh_
					select (
						//	int4Index:		pid >> 5 >> 2,
						//	memberIndex:	pid >> 5 & 0b_11,	// 0 ~ 3
						//	bitIndex:		pid & 0b_1_1111		// 0 ~ 31
						int4Index: pid / 24 >> 2,
						memberIndex: pid / 24 & 0b_11,  // 0 ~ 23
						bitIndex: pid % 24          // 0 ~ 23
													// unity �� int4[] ��]�������i���Ȃ��̂ŁAfloat �̎������͈͂őË�����B
					);

				var qPidEveryVertices =
					from xy in (vtxCount_PerMesh_, qPidEveryParts).Zip((x, y) => (vtxCount: x, pid: y))
					from pidsEveryVtxs in Enumerable.Repeat<(int, int, int)>(xy.pid, xy.vtxCount)
					select pidsEveryVtxs
					;

				return qPidEveryVertices;
			}
			IEnumerable<(int elementIndex, int bitIndex)>
				queryPartIdx_PerVtx_(IEnumerable<int> vtxCount_PerMesh_, IEnumerable<int> partId_PerMesh_)
			{
				var qPidEveryParts =
					from pid in partId_PerMesh_
					select (
						elementIndex: pid >> 5,
						bitIndex: pid & 31
					);

				var qPidEveryVertices =
					from xy in (vtxCount_PerMesh_, qPidEveryParts).Zip((x, y) => (vtxCount: x, pidx: y))
					from pidsEveryVtxs in Enumerable.Repeat<(int, int)>(xy.pidx, xy.vtxCount)
					select pidsEveryVtxs
					;

				return qPidEveryVertices;
			}

			/// <summary>
			/// �p���b�g�h�c�����ׂĂ̒��_���ƂɃN�G������B
			/// �p���b�g�h�c�́A�e�T�u���b�V���ɑΉ�����A������}�e���A���̃C���f�b�N�X���擾����B
			/// </summary>
			IEnumerable<int> queryePallet_PerVtx_
				(
					IEnumerable<IEnumerable<int>> vtxCount_PerSubmesh_,
					IEnumerable<IEnumerable<int>> matHash_PerSubmeshPerMesh_,
					IEnumerable<int> matHashesCombined_
				)
			{
				var qDstHashAndIdx = matHashesCombined_.Select((hash, i) => (hash, i));
				var qPalletIdx_PerSubmesh =
					from hashes in matHash_PerSubmeshPerMesh_
					select
						from srcMatHash in hashes
						join dstMat in qDstHashAndIdx on srcMatHash equals dstMat.hash
						select dstMat.i
					;
				// ���_���Ƃ� pallet index
				// pallet index �� ������̃}�e���A���ւ̓Y����
				// mat idx �́A�����łh�c���擾���ĐU��
				// submesh ���Ƃ� vertex count �ŁAsrc mat idx ��
				var qPalletIdx =
					from meshxy in (qPalletIdx_PerSubmesh, vtxCount_PerSubmesh_).Zip()
					from (int palletIdx, int vtxCount) xy in meshxy.Zip()
					from palletIdx in Enumerable.Repeat(xy.palletIdx, xy.vtxCount)
					select palletIdx
					;

				return qPalletIdx;
			}
		}


		/// <summary>
		/// �e�p�[�c���b�V���̎��C���f�b�N�X�����ׂČ������A�ЂƂ̔z��ɂ��ĕԂ��B
		/// ���̍ۊe���b�V���̒��_���́A���̃C���f�b�N�X�̃x�[�X�ƂȂ�B
		/// �܂��A�}�e���A���ʂ̃T�u���b�V�����A�ЂƂɓ��������B
		/// �i���_���́A�ʒu���ł͂Ȃ��x�[�X�̌����v�Z���邽�߂ɕK�v�ƂȂ�B�j
		/// </summary>
		public static int[][] ToIndicesArray
			(
				IEnumerable<Vector3[]> vertices_PerMesh,
				IEnumerable<IEnumerable<int[]>> indices_PerSubmeshPerMesh,
				IEnumerable<Matrix4x4> mtPart_PerMesh
			)
		{
			var mts = mtPart_PerMesh;

			var qBaseVtxs = PerMesh.QueryBaseVertex(vertices_PerMesh);

			var qIndex =
				from x in (indices_PerSubmeshPerMesh, mts, qBaseVtxs).Zip()
				select (idxss: x.src0, mt: x.src1, baseVtx: x.src2) into srcPerMesh
				from idxs in srcPerMesh.idxss
				from index in IndexUtility.ReverseEvery3_IfMinusScale(idxs, srcPerMesh.mt)
				select srcPerMesh.baseVtx + index;

			return new int[][] { qIndex.ToArray() };
			//return Enumerable.Repeat( qIndex, 1 ).ToArrayRecursive2();
		}

		/// <summary>
		/// �e�p�[�c���b�V���̎��C���f�b�N�X���������A�ŏI�I�ȃT�u���b�V�����Ƃ̔z��ɂ��ĕԂ��B
		/// ���̍ۊe���b�V���̒��_���́A���̃C���f�b�N�X�̃x�[�X�ƂȂ�B
		/// </summary>
		public static int[][] ToIndicesArray
			(
				IEnumerable<Vector3[]> vertices_PerMesh,
				IEnumerable<IEnumerable<int[]>> indices_PerSubmeshPerMesh,
				IEnumerable<Matrix4x4> mtPart_PerMesh,
				IEnumerable<Material[]> materials_PerMesh,
				IEnumerable<Material> materialsCombined
			)
		{
			var qDstMatHashes = from mat in materialsCombined select mat.GetHashCode();
			var qSrcMatHashes = materials_PerMesh.To(PerSubMeshPerMesh.QueryMaterialHash);

			var idxsss = indices_PerSubmeshPerMesh;
			var mts = mtPart_PerMesh;
			var qBaseVtxs = PerMesh.QueryBaseVertex(vertices_PerMesh);

			var qPerMesh = (idxsss, qSrcMatHashes, mts, qBaseVtxs)
				.Zip((x, y, z, w) => (idxss: x, hashes: y, mt: z, baseVtx: w));
			Debug.Log(materials_PerMesh.First().First().mainTexture.GetHashCode());
			var qSrcMatGroups =
				from src in qPerMesh
				from perSub in (src.idxss, src.hashes).Zip((x, y) => (idxs: x, hash: y, src.mt, src.baseVtx))
				group perSub by perSub.hash
				;

			var qIdxsPerDstMat =
				from dstHash in qDstMatHashes
				join srcs in qSrcMatGroups on dstHash equals srcs.Key
				select
					from src in srcs
					from idx in src.idxs.ReverseEvery3_IfMinusScale(src.mt)
					select src.baseVtx + idx;

			return qIdxsPerDstMat.ToArrayRecursive2();
		}


		public static int[][] ToUvArray
			(
				IEnumerable<Vector3[]> vertices_PerMesh,
				IEnumerable<IEnumerable<int[]>> indices_PerSubmeshPerMesh,
				IEnumerable<Matrix4x4> mtPart_PerMesh,
				IEnumerable<Material[]> materials_PerMesh,
				IEnumerable<Material> materialsCombined
			)
		{
			var qDstMatHashes = from mat in materialsCombined select mat.GetHashCode();
			var qSrcMatHashes = materials_PerMesh.To(PerSubMeshPerMesh.QueryMaterialHash);

			var idxsss = indices_PerSubmeshPerMesh;
			var mts = mtPart_PerMesh;
			var qBaseVtxs = PerMesh.QueryBaseVertex(vertices_PerMesh);

			var qPerMesh = (idxsss, qSrcMatHashes, mts, qBaseVtxs)
				.Zip((x, y, z, w) => (idxss: x, hashes: y, mt: z, baseVtx: w));
			
			var qSrcMatGroups =
				from src in qPerMesh
				from perSub in (src.idxss, src.hashes).Zip((x, y) => (idxs: x, hash: y, src.mt, src.baseVtx))
				group perSub by perSub.hash
				;

			var qIdxsPerDstMat =
				from dstHash in qDstMatHashes
				join srcs in qSrcMatGroups on dstHash equals srcs.Key
				select
					from src in srcs
					from idx in src.idxs.ReverseEvery3_IfMinusScale(src.mt)
					select src.baseVtx + idx;

			return qIdxsPerDstMat.ToArrayRecursive2();
		}

	}


}
