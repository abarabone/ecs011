using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Linq;
using System.Threading.Tasks;
using System;

namespace Abarabone.Geometry
{
	using Abarabone.Common.Extension;

	/// <summary>
	/// ���b�V�����e�v�f���ƂɌ�������B
	/// </summary>
	public static partial class MeshCombiner
	{

		/// <summary>
		/// Mesh �v�f����������f���Q�[�g��Ԃ��B�ʒu�Ƃt�u�Ɩ@���B
		/// </summary>
		static public Func<MeshCombinerElements> BuildNormalMeshElements
			(IEnumerable<GameObject> gameObjects, Transform tfBase, bool isCombineSubMeshes = true)
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving(gameObjects).ToArray();

			return BuildNormalMeshElements(mmts, tfBase, isCombineSubMeshes);
		}

		static public Func<MeshCombinerElements> BuildNormalMeshElements
			((Mesh mesh, Material[] mats, Transform tf)[] mmts, Transform tfBase, bool isCombineSubMeshes)
		{
			var f = BuildUnlitMeshElements(mmts, tfBase, isCombineSubMeshes);

			var nmss = (from x in mmts select x.mesh).To(PerMesh.QueryNormals).ToArray();

			return () =>
			{
				var me = f();

				me.Normals = ConvertUtility.ToNormalsArray(nmss, me.mtObjects, me.MtBaseInv);

				return me;
			};
		}

	}
}
