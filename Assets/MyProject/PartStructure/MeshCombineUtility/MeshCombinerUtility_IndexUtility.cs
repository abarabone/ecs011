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


	public static class IndexUtility
	{

		/// <summary>
		/// ���]�i�X�P�[���̈ꕔ���}�C�i�X�j���b�V���ł���΁A�O�p�C���f�b�N�X���t���ɂ���B
		/// </summary>
		public static IEnumerable<int> ReverseEvery3_IfMinusScale(this IEnumerable<int> indices, Matrix4x4 mtObject)
		{
			if (mtObject.IsMuinusScale()) return reverseEvery3_(indices);

			return indices;

			IEnumerable<int> reverseEvery3_(IEnumerable<int> indecies_)
			{
				using (var e = indecies_.GetEnumerator())
				{
					while (e.MoveNext())
					{
						var i0 = e.Current; e.MoveNext();
						var i1 = e.Current; e.MoveNext();
						var i2 = e.Current;
						yield return i2;//210�ł������H
						yield return i1;
						yield return i0;
					}
				}
			}
		}
	}

}
