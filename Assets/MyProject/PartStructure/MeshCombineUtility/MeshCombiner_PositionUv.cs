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
	/// ���b�V�����e�v�f���ƂɌ�������B
	/// </summary>
	public static partial class MeshCombiner
	{


		/// <summary>
		/// Mesh �v�f����������f���Q�[�g��Ԃ��B�ʒu�Ƃt�u�̂݁B
		/// </summary>
		static public Func<MeshCombinerElements> BuildUnlitMeshElements
			(IEnumerable<GameObject> gameObjects, Transform tfBase, bool isCombineSubMeshes = true)
		{
			var mmts = FromObject.QueryMeshMatsTransform_IfHaving(gameObjects).ToArray();

			return BuildUnlitMeshElements(mmts, tfBase, isCombineSubMeshes);
		}

		static public Func<MeshCombinerElements> BuildUnlitMeshElements
			((Mesh mesh, Material[] mats, Transform tf)[] mmts, Transform tfBase, bool isCombineSubMeshes)
		{
			var f = BuildBaseMeshElements(mmts, tfBase, isCombineSubMeshes);

			var uvss = (from x in mmts select x.mesh.uv).ToArray();

			return () =>
			{
				var me = f();

				me.Uvs = uvss.SelectMany(uvs => uvs).ToArray();

				return me;
			};
		}


	}
}
