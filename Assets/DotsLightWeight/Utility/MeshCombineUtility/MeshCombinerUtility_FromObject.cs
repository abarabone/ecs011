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
	public static class FromObject
	{

		/// <summary>
		/// ���b�V���ƍގ��z������I�u�W�F�N�g�𒊏o���A���̑g��񋓂��ĕԂ��B�Е��ł� null �ł���΁A���O�����B
		/// </summary>
		public static IEnumerable<(Mesh mesh, Material[] mats, Transform tf)>
			QueryMeshMatsTransform_IfHaving(this IEnumerable<GameObject> gameObjects)
		{
			return
				from obj in gameObjects
				let r = obj.GetComponent<SkinnedMeshRenderer>().As()
				let mesh = r?.sharedMesh ?? obj.GetComponent<MeshFilter>().As()?.sharedMesh
				let mats = r?.sharedMaterials ?? obj.GetComponent<Renderer>().As()?.sharedMaterials
				where mesh != null && mats != null
				select (mesh, mats, obj.transform)
				;
		}


		public static IEnumerable<(Mesh mesh, Material[] mats, Transform tf)>
			QueryMeshMatsTransform_IfHaving(this GameObject topGameObject)
		=>
			topGameObject.GetComponentsInChildren<Transform>()
				.Select(x => x.gameObject)
				.QueryMeshMatsTransform_IfHaving();


	}
}