using System;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Linq;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace DotsLite.Geometry
{
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Geometry.inner;
    using DotsLite.Geometry.inner.unit;
    using DotsLite.Structure.Authoring;


    public static class MeshCombineUtility
    {


        public static Func<IMeshElements> BuildCombiner<TIdx, TVtx>(
            this SrcMeshesModelCombinePack srcmeshpack,
            AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
            where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        {
            return new TVtx().BuildCombiner<TIdx>(srcmeshpack.AsEnumerable, p);
        }


        /// <summary>
        /// 
        /// </summary>
        public static Task<MeshElements<TIdx, TVtx>> ToTask<TIdx, TVtx>(
            this Func<MeshElements<TIdx, TVtx>> f)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
            where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        =>
            Task.Run(f);

        public static Task<IMeshElements> ToTask(this Func<IMeshElements> f) =>
            Task.Run(f);

        public static Task<(TIdx[], TVtx[])> ToTask<TIdx, TVtx>(this Func<(TIdx[], TVtx[])> f)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
            where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        =>
            Task.Run(f);



        /// <summary>
        /// �ʂ̊֐���
        /// �K�v�ȃp�����[�^�����v�Z����悤�ɂ����ق�����������
        /// </summary>
        public static AdditionalParameters calculateParameters(
            this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
            Transform tfBase,  Transform[] tfBones,
            Func<int, Rect> texHashToUvRectFunc, Func<int, int> texHashToUvIndexFunc)
        {
            var mmts_ = mmts.ToArray();
            var meshes = mmts_
                .Select(x => x.mesh)
                .ToArray();

            var qMtPerMesh = mmts_
                .Select(x => x.tf.localToWorldMatrix);
            var qTexhashPerSubMesh =
                from mmt in mmts_
                select
                    from mat in mmt.mats
                    select mat?.mainTexture?.GetHashCode() ?? 0
                ;

            var mtBaseInv = tfBase.worldToLocalMatrix;


            var result = new AdditionalParameters
            {
                mtBaseInv = mtBaseInv,
                mtPerMesh = qMtPerMesh.ToArray(),
                texhashPerSubMesh = qTexhashPerSubMesh.ToArrayRecursive2(),
                //atlasHash = atlas?.GetHashCode() ?? 0,
                //texhashToUvRect = texHashToUvRect,
                texHashToUvRect = texHashToUvRectFunc,
                texHashToUvIndex = texHashToUvIndexFunc,
            };

            var qPartIdPerMesh =
                from mmt in mmts_
                    //.Do(x => Debug.Log($"part id is {x.tf.getInParent<StructurePartAuthoring>()?.PartId ?? -1} from {x.tf.getInParent<StructurePartAuthoring>()?.name ?? "null"}"))
                    //.Do(x => Debug.Log($"part id is {x.tf.findInParent<StructurePartAuthoring>()?.PartId ?? -1} from {x.tf.findInParent<StructurePartAuthoring>()?.name ?? "null"}"))
                select mmt.tf.getInParent<IStructurePart>()?.partId ?? -1
                //select mmt.tf.gameObject.GetComponentInParent<StructurePartAuthoring>()?.PartId ?? -1
                ;
            result.partIdPerMesh = qPartIdPerMesh.ToArray();

            if (tfBones == null) return result;
            if (!tfBones.Any()) return result;


            var qBoneWeights =
                from mesh in meshes
                select mesh.boneWeights
                ;
            var qMtInvs =
                from mesh in meshes
                select mesh.bindposes
                ;
            var qSrcBones = mmts_
                .Select(x => x.tf.GetComponentOrNull<SkinnedMeshRenderer>()?.bones ?? x.tf.WrapEnumerable().ToArray());
                ;
            result.boneWeightsPerMesh = qBoneWeights.ToArray();
            result.mtInvsPerMesh = qMtInvs.ToArray();
            result.srcBoneIndexToDstBoneIndex = (qSrcBones, tfBones).ToBoneIndexConversionDictionary();
            return result;
        }


        /// <summary>
        /// �p���b�g�̃T�u�C���f�b�N�X���A�T�u���b�V���P�ʂŗ񋓂���B
        /// �T�u�C���f�b�N�X�́A�}�e���A���� pallet sub index ����擾����B
        /// �}�e���A���� null �̏ꍇ�́A0 ��Ԃ��B
        /// </summary>
        public static void CalculatePalletSubIndexParameter(
            this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
            ref AdditionalParameters p)
        {
            var q =
                from mmt in mmts
                select
                    from mat in mmt.mats
                    select mat.GetPalletSubIndex()
                ;
            p.palletSubIndexPerSubMesh = q.ToArrayRecursive2();
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //public static void CalculateUvIndexParameter(
        //    this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
        //    ref AdditionalParameters p,
        //    IEnumerable<int> texhashesForUvSurface)
        //{
        //    var q =
        //        from mmt in mmts
        //        select
        //            from mat in mmt.mats
        //            select mat?.mainTexture?.GetHashCode() ?? 0
        //        ;
        //    var texhashPerSubMesh = q.ToArrayRecursive2();

        //    var usedTexhashes = texhashPerSubMesh
        //        .SelectMany(x => x)
        //        .Distinct()
        //        .ToArray();

        //    p.texhashPerSubMesh = texhashPerSubMesh;

        //}
        // tex rect �� atlas �P��
        // atlas �P�ʂ� buffer ��
        // buffer �ɂ� uv surface �����o�^���Ȃ�
        // uv surfaces �� sub mesh ���Ƃ� sub index
        // ���_�� uv sub index ����������
        // ���� uv sub index �͓��� surface 
        // uv �͓����ʍ\���P�ʂ� base index �w��@�F�Ⴂ�ł��������̂Ȃ�n�j
        // �G�f�B�^�ł́A�T�u���b�V���P�ʂŃe�N�X�`�����w�肷�� scriptable object ����������
        // sub index �� pallet �Ɠ����}�e���A���� pallet sub index �ł悢
        // pallet �̓J���[�Z�b�g�P�ʂ� base index �w��

        //static T findInParent<T>(this GameObject obj)
        //    where T : MonoBehaviour
        //=> obj
        //    .AncestorsAndSelf()
        //    .Select(x => x.GetComponent<T>())
        //    .FirstOrDefault()
        //    ;

        //static T findInParent<T>(this Transform tf) where T : MonoBehaviour => tf.gameObject.findInParent<T>();

        static T getInParent<T>(this GameObject obj)
            where T : IStructurePart
        //=> obj.GetComponentsInParent<T>(true).FirstOrDefault();
        => obj.GetComponentInParent<T>(true);

        static T getInParent<T>(this Transform tf)
            where T : IStructurePart
        => tf.gameObject.getInParent<T>();

    }


    public class ColorPalletBuilder
    {
        Dictionary<string, (int i, Color32[] colors)> idList = new Dictionary<string, (int, Color32[])>();

        int nextIndex;

        public int this[Color32[] values]
        {
            get => this.idList[toKey(values)].i;
        }

        public void Add(Color32[] values)
        {
            var key = toKey(values);
            if (this.idList.ContainsKey(key))
            {
                this.idList[key] = (this.nextIndex++, values);
            }
        }

        static string toKey(Color32[] keysrc)
        {
            var q =
                from x in keysrc
                select $"{x.r},{x.g},{x.b},{x.a}"
                ;
            return string.Join("/", q);
        }


    }

    public class UvPalletBuilder
    {
        Dictionary<string, (int i, Rect[] uvs)> idList = new Dictionary<string, (int, Rect[])>();

        int nextIndex;

        public int this[Rect[] values]
        {
            get => this.idList[toKey(values)].i;
        }

        public void Add(Rect[] values)
        {
            var key = toKey(values);
            if (this.idList.ContainsKey(key))
            {
                this.idList[key] = (this.nextIndex++, values);
            }
        }

        static string toKey(Rect[] keysrc)
        {
            var q =
                from x in keysrc
                select $"{x.x},{x.y}"
                ;
            return string.Join("/", q);
        }
    }

    public static class PalletUtility
    {

        public static int GetPalletSubIndex(this Material mat) =>
            //mat?.HasInt("Pallet Sub Index") ?? false
            mat?.HasProperty("Pallet Sub Index") ?? false
                ? mat.GetInt("Pallet Sub Index")
                : 0
            ;


        // �E���f������ sub index ���Ƃ̐F�𒊏o
        // �Ecolor pallet �ɓo�^�A�Ō�Ƀo�b�t�@���\�z
        // �E�o�b�t�@�̓V�[���ɂP��
        // �Ecolor pallet �� base index ���A�C���X�^���X�Ɏ�������
        // �E�������A���łɓ����\���œo�^������΁A���� base index ���擾����
        /// <summary>
        /// 
        /// </summary>
        public static Color32[] ToPalletColorEntry(
            this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts)
        {
            var q =
                from mmt in mmts
                from mat in mmt.mats
                select (index: mat.GetPalletSubIndex(), color: (Color32)mat.color)
                ;
            var colors = q.ToLookup(x => x.index, x => x.color);
            var maxIndex = colors.Max(x => x.Key);
            var qResult =
                from i in Enumerable.Range(0, maxIndex + 1)
                select colors.Contains(i)
                    ? colors[i].First()
                    : new Color32()
                ;
            return qResult.ToArray();
        }

        // �Euv pallet �ɓo�^�A�Ō�Ƀo�b�t�@���\�z
        // �E�o�b�t�@�̓V�[���łP��
        // �Euv pallet 
        /// <summary>
        /// 
        /// </summary>
        public static Rect[] ToPalletUvEntry(
            this IEnumerable<(Mesh mesh, Material[] mats, Transform tf)> mmts,
            Func<int, Rect> texHashToUvRectFunc)
        {
            var q =
                from mmt in mmts
                from mat in mmt.mats
                let index = mat.GetPalletSubIndex()
                let uv = texHashToUvRectFunc(mat.mainTexture.GetHashCode())
                select (index, uv)
                ;
            var uvs = q.ToLookup(x => x.index, x => x.uv);
            var maxIndex = uvs.Max(x => x.Key);
            var qResult =
                from i in Enumerable.Range(0, maxIndex + 1)
                select uvs.Contains(i)
                    ? uvs[i].First()
                    : new Rect()
                ;
            return qResult.ToArray();
        }
    }
}
