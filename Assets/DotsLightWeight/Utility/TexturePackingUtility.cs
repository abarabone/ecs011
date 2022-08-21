using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

namespace DotsLite.Geometry
{

	using DotsLite.Common.Extension;
	using DotsLite.Utilities;


	/// <summary>
	/// �A�g���X�ƌ��e�N�X�`���̃n�b�V���l����A�t�u�I�t�Z�b�g��`��Ԃ������B
	/// �n�b�V���W�������`�W���ɂ��Ή�����B
	/// </summary>
	public class HashToRect
	{
		public Dictionary<(int atlas, int part), Rect> dict;

		public HashToRect()
		{
			this.dict = new Dictionary<(int atlas, int part), Rect>();
		}
		public HashToRect(TextureAtlasAndParameters texparams)
		{
			this.dict = (texparams.texhashes, texparams.uvRects).ToDictionary();
		}


		public Rect this[(int atlas, int part) hashes]
		{
			set => this.dict[hashes] = value;
			get => this.dict[hashes];
		}
		public Rect this[int atlas, int part]
        {
			set => this.dict[(atlas, part)] = value;
			get => this.dict[(atlas, part)];
		}

		public IEnumerable<Rect> this[IEnumerable<(int atlas, int part)> hashes]
		{
			set
			{
				foreach (var (hash, uv) in (hashes, value).Zip())
				{
					this[hash.atlas, hash.part] = uv;
				}
			}
            get
			{
				foreach (var hash in hashes)
				{
					yield return this[hash.atlas, hash.part];
				}
			}
		}

		//public static implicit operator
		//	Dictionary<(int atlas, int part), Rect>(HashToRect d) => d.dict;

		//public static implicit operator
		//	HashToRect(Dictionary<(int atlas, int part), Rect> d) => d != null ? new HashToRect { dict=d } : null;
	}


	/// <summary>
	/// ���f���ϊ����ȂǂɁA�p�����[�^�Ƃ��ĕێ�����e�N�X�`�����B
	/// �A�g���X�A���e�N�X�`���n�b�V���l�̏W���A�t�u�I�t�Z�b�g��`�̏W����ێ�����B
	/// </summary>
    public struct TextureAtlasAndParameters
    {
        public Texture2D atlas;
        public IEnumerable<(int atlas, int part)> texhashes;
        public IEnumerable<Rect> uvRects;

		//public Func<int, Rect> ToTexHashToUvRectFunc()
		//{
		//	var dict = new HashToRect { dict = (texhashes, uvRects).ToDictionary() };
		//	var atlashash = this.atlas.GetHashCode();
		//	return hash => dict[atlashash, hash];
		//}
}


    static class TexturePackingUtility
	{

		/// <summary>
		/// �e�N�X�`���̏W������A�g���X�𐶐����A�A�g���X�Ƃt�u�I�t�Z�b�g�z���Ԃ��B
		/// </summary>
		static public (Texture2D atlas, Rect[] uvRects) ToAtlas(this IEnumerable<Texture2D> srcTextures)
		{
			//var dstTexture = new Texture2D( 0, 0 );
			var dstTexture = new Texture2D
				(width: 0, height: 0, textureFormat: TextureFormat.ARGB32, mipChain: true);

			var uvRects = dstTexture.PackTextures
				(srcTextures.ToArray(), padding: 0, maximumAtlasSize: 4096, makeNoLongerReadable: true);

			return (dstTexture, uvRects);
		}

		/// <summary>
		/// �e�N�X�`���̏W������A�A�g���X�Ƃt�u�I�t�Z�b�g�z���Ԃ��B
		/// �������A�e�N�X�`�����P�����̎��́A�A�g���X�������ɓn�����e�N�X�`����Ԃ��B
		/// </summary>
		static public (Texture2D atlas, Rect[] uvRects) ToAtlasOrPassThrough(this IEnumerable<Texture2D> srcTextures)
		=>
			srcTextures.IsSingle()
				? (srcTextures.First(), new Rect[] { new Rect(0, 0, 1, 1) })
				: srcTextures.ToAtlas();



		/// <summary>
		/// �w�肵���Q�[���I�u�W�F�N�g�ȉ��̃q�G�����L�[����A���C���e�N�X�`���̏W����Ԃ��B
		/// �������A�d���Ȃ��B
		/// </summary>
		static public IEnumerable<Texture2D> QueryUniqueTextures(this GameObject obj) =>
			obj.GetComponentsInChildren<Renderer>()
				.SelectMany(r => r.sharedMaterials)
				.QueryUniqueTextures();
			//objects.QueryMeshMatsTransform_IfHaving()
			//	.Select(x => x.mats)
			//	.SelectMany()
			//	.QueryUniqueTextures();

		/// <summary>
		/// �}�e���A���̏W������A���C���e�N�X�`���̏W����Ԃ��B
		/// �������A�d���Ȃ��B
		/// </summary>
		static public IEnumerable<Texture2D> QueryUniqueTextures(this IEnumerable<Material> mats) =>
			mats.Select(mat => mat.mainTexture)
				.Where(x => x != null)
				.OfType<Texture2D>()
				.Distinct();



		/// <summary>
		/// �d���Ȃ��e�N�X�`���̏W������ATextureAtlasAndParameters �𐶐�����B
		/// </summary>
		static public TextureAtlasAndParameters ToAtlasOrPassThroughAndParameters(this IEnumerable<Texture2D> uniqueTextures)
		{
			var texs = uniqueTextures.ToArray();

			var (atlas, uvRects) = texs.ToAtlasOrPassThrough();

			var qKeyHash = texs.queryKeyHashes_(atlas);

			return new TextureAtlasAndParameters
			{
				atlas = atlas,
				texhashes = qKeyHash.Append((atlas.GetHashCode(), 0)).ToArray(),
				uvRects = uvRects.Append(new Rect(0, 0, 1, 1)),
			};
		}
		static IEnumerable<(int, int)> queryKeyHashes_(this IEnumerable<Texture2D> uniqueTextures, Texture2D atlas)
		=>
			from tex in uniqueTextures
			select (atlas.GetHashCode(), tex.GetHashCode())
			;





		///// <summary>
		///// 
		///// </summary>v
		//static public (IEnumerable<(int, int)>, IEnumerable<Rect>) queryHashAndUvRectPairs
		//	(this (IEnumerable<Texture2D> uniqueTextures, IEnumerable<Rect> uvRects) x, Texture2D atlas)
		//{
		//	var qKeys =
		//		from tex in x.uniqueTextures
		//		select (atlas.GetHashCode(), tex.GetHashCode())
		//		;
		//	var qKeysWithEmpty = qKeys.Append((atlas.GetHashCode(), 0));
		//	var qValueWithEmpty = x.uvRects.Append(new Rect(0, 0, 1, 1));

		//	return (qKeysWithEmpty, qValueWithEmpty);
		//}




		// �ȉ��͔p�~�\��

		static public (Vector2[][][] uvsss, int[][] texhashess) ToUvTranslateSource
			(this (IEnumerable<Mesh> meshes, IEnumerable<Material[]> matss) x)
        {
			var qSubmeshUv =
				from mesh in x.meshes
				select mesh.SubmeshVertices(mesh.uv)
				// ���_�͎Ⴂ submesh �̏��ɁA���ԂȂ��i�[����Ă���Ɖ��肷��
				;
			var qSubmeshTex =
				from mats in x.matss
				select
					from mat in mats
					select mat.mainTexture?.GetHashCode() ?? default
				;
			return (qSubmeshUv.ToArrayRecursive3(), qSubmeshTex.ToArrayRecursive2());
		}
		static public IEnumerable<IEnumerable<Vector2>> QueryTranslatedUv
			(this (Vector2[][][] uvsss, int[][] texhashess) uvsrc, Dictionary<int, Rect> texhashToUvRect)
		{
			var qMeshUvNew =
				from obj in uvsrc.Zip()
				select
					from x in obj.Zip()
					let uvs = x.src0
					let tex = x.src1
					from uv in uvs
					select uv.ScaleUv(texhashToUvRect.GetOrDefault(tex, new Rect(0, 0, 1, 1)))
				;

			return qMeshUvNew;
		}

		static public IEnumerable<IEnumerable<Vector2>> QueryTranslatedUv
			(this IEnumerable<Mesh> meshes, IEnumerable<Material[]> matss, Dictionary<Texture2D, Rect> texToUvRect)
		{
			var qSubmeshUv =
				from mesh in meshes
				select mesh.SubmeshVertices(mesh.uv)
				// ���_�͎Ⴂ submesh �̏��ɁA���ԂȂ��i�[����Ă���Ɖ��肷��
				;
			var qSubmeshTex =
				from mats in matss
				select
					from mat in mats
					select mat.mainTexture as Texture2D
				;

			var qMeshUvNew =
				from obj in (qSubmeshUv, qSubmeshTex).Zip()
				select
					from x in obj.Zip()
					let uvs = x.src0
					let tex = x.src1
					from uv in uvs
					select uv.ScaleUv(texToUvRect.GetOrDefault(tex, new Rect(0, 0, 1, 1)))
				;

			return qMeshUvNew;
		}

		static public IEnumerable<Mesh> MakeNewUvMeshes
			(this IEnumerable<IEnumerable<Vector2>> newUvs, IEnumerable<Mesh> meshes)
		{
			var qMeshUvTranslated =
				from x in (meshes, newUvs).Zip()
				let src = x.src0
				let dst = cloneMesh_(src, x.src1.ToArray())
				select dst
				;
			return qMeshUvTranslated;


			// ���ʂȃQ�[���I�u�W�F�N�g���쐬���Ă���̂ŁA�����Ƃ悢���b�V���N���[���@�ɕς���ׂ�
			static Mesh cloneMesh_(Mesh srcmesh, Vector2[] newuv)
			{
				var go = new GameObject("dummy for mesh copy");
				var mf = go.AddComponent<MeshFilter>();
				mf.mesh = srcmesh;
				GameObject.Destroy(go);

				var newmesh = mf.mesh;
				newmesh.uv = newuv;

				return newmesh;
			}
		}
	}
}
