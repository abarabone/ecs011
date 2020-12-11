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
	/// 
	/// </summary>
	public static class PerMesh
	{

		/// <summary>
		/// 
		/// </summary>
		public static IEnumerable<Vector3[]> QueryNormals(IEnumerable<Mesh> meshes)
		{
			return
				from mesh in meshes
				select mesh.normals ?? recalculateNormals_(mesh)
				;

			Vector3[] recalculateNormals_(Mesh mesh_)
			{
				mesh_.RecalculateNormals();
				return mesh_.normals;
			}
		}

		/// <summary>
		/// ���b�V�����ƂɁA�C���f�b�N�X�̃x�[�X�I�t�Z�b�g���N�G������B
		/// </summary>
		public static IEnumerable<int> QueryBaseVertex(IEnumerable<Vector3[]> vtxsEveryMeshes)
		{
			var qVtxCount = vtxsEveryMeshes // �܂��́umesh n �̒��_���v�̏W�����N�G������B
				.Select(vtxs => vtxs.Count())
				.Scan(seed: 0, (pre, cur) => pre + cur)
				;
			return qVtxCount.Prepend(0);
			// { 0 } + { mesh 0 �̒��_��, mesh 1 �̒��_��, ... }
		}

	}

}
