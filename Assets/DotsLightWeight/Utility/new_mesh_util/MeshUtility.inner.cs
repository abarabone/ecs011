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

namespace Abarabone.Geometry.inner
{
    using Abarabone.Common.Extension;
    using Abarabone.Utilities;
    using Abarabone.Geometry.inner.unit;
    using Abarabone.Misc;


    static class MeshQyeryUtility
    {
        static public IEnumerable<MeshUnit> AsEnumerable(this Mesh.MeshDataArray meshDataArray)
        {
            var baseVertex = 0;
            for (var i = 0; i < meshDataArray.Length; i++)
            {
                yield return new MeshUnit(i, meshDataArray[i], baseVertex);

                baseVertex += meshDataArray[i].vertexCount;
            }
        }
    }


    static partial class ConvertIndicesUtility
    {

        public static IEnumerable<TIdx> QueryConvertIndexData<TIdx>
            (this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
            where TIdx : struct, IIndexUnit<TIdx>
        =>
            from x in srcmeshes.QuerySubMeshForIndexData<TIdx>(mtsPerMesh)
            from xsub in x.submeshes
            let submesh = xsub
            from tri in x.mt.IsMuinusScale()
                ? submesh.Elements().AsTriangle().Reverse()
                : submesh.Elements().AsTriangle()
            from idx in tri//.Do(x => Debug.Log(x))//
            select idx.Add(x.mesh.BaseVertex + submesh.Descriptor.baseVertex)
            ;
        
    }

    static class ConvertVertexUtility
    {

        static public IEnumerable<Vector3> QueryConvertPositions
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
        =>
            //from x in srcmeshes.QuerySubMeshForVertices<Vector3>(p, (md, arr) => md.GetVertices(arr), VertexAttribute.Position)
            //from xsub in x.submeshes
            //from vtx in xsub.submesh.Elements()
            //select (Vector3)math.transform(x.mt, vtx)
            from permesh in (srcmeshes.AsEnumerable(), p.mtsPerMesh).Zip()
            let mesh = permesh.src0.MeshData
            let mt = permesh.src1 * p.mtBaseInv
            from vtx in mesh.QueryMeshVertices<Vector3>((md, arr) => md.GetVertices(arr), VertexAttribute.Position)
            select(Vector3)math.transform(mt, vtx)
            ;


        static public IEnumerable<Vector3> QueryConvertNormals
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
        =>
            //from x in srcmeshes.QuerySubMeshForVertices<Vector3>(p, (md, arr) => md.GetNormals(arr), VertexAttribute.Normal)
            //from xsub in x.submeshes
            //from nm in xsub.submesh.Elements()
            //select (Vector3)math.mul(x.mt.rotation, nm)
            from mesh in srcmeshes.AsEnumerable()
            from nm in mesh.MeshData.QueryMeshVertices<Vector3>((md, arr) => md.GetNormals(arr), VertexAttribute.Normal)
            select (Vector3)math.mul(p.mtBaseInv.rotation, nm)
            ;


        static public IEnumerable<Vector2> QueryConvertUvs
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters p, int channel)
        =>
            p.texHashToUvRect != null
            ?
                from x in srcmeshes.QuerySubMeshForVertices<Vector2>(p, (md, arr) => md.GetUVs(channel, arr), VertexAttribute.TexCoord0)
                from xsub in x.submeshes
                from uv in xsub.submesh.Elements()
                select uv.ScaleUv(p.texHashToUvRect(xsub.texhash))
            :
                from mesh in srcmeshes.AsEnumerable()
                from uv in mesh.MeshData.QueryMeshVertices<Vector2>((md, arr) => md.GetUVs(channel, arr), VertexAttribute.TexCoord0)
                select uv
            ;


        static public IEnumerable<Vector3> QueryConvertPositionsWithBone
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
        =>
            from permesh in (srcmeshes.AsEnumerable(), p.boneWeights, p.mtsPerMesh).Zip()
            let mesh = permesh.src0.MeshData
            let weis = permesh.src1
            let mt = permesh.src2 * p.mtBaseInv
            from x in (mesh.QueryMeshVertices<Vector3>((md, arr) => md.GetVertices(arr), VertexAttribute.Position), weis).Zip()
            let vtx = x.src0
            let wei = x.src1
            select (Vector3)math.transform(mt, vtx)//p.mtInvs[wei.boneIndex0] * mt, vtx)
            ;
        static public IEnumerable<uint> QueryConvertBoneIndices
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
        =>
            from permesh in p.boneWeights
            from w in permesh
            select (uint)(
                w.boneIndex0 << 0 & 0xff |
                w.boneIndex1 << 8 & 0xff |
                w.boneIndex2 << 16 & 0xff |
                w.boneIndex3 << 24 & 0xff
            );
        static public IEnumerable<Vector4> QueryConvertBoneWeights
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
        =>
            from permesh in p.boneWeights
            from w in permesh
            select new Vector4(w.weight0, w.weight1, w.weight2, w.weight3)
            ;

    }


    static class SubmeshQyeryConvertUtility
    {

        public static IEnumerable<(Matrix4x4 mt, IEnumerable<(SubMeshUnit<T> submesh, int texhash)> submeshes)>
            QuerySubMeshForVertices<T>(
                this Mesh.MeshDataArray srcmeshes,
                AdditionalParameters p,
                Action<Mesh.MeshData, NativeArray<T>> getElementSrc,
                VertexAttribute attr
            ) where T : struct
        =>
            from x in (srcmeshes.AsEnumerable(), p.mtsPerMesh, p.texhashPerSubMesh).Zip()
            let submeshes = x.src0.MeshData.QuerySubmeshesForVertices(getElementSrc, attr)
            let mt = p.mtBaseInv * x.src1
            let texhashes = x.src2
            select (
                mt,
                from xsub in (submeshes, texhashes).Zip()//.Do(x => Debug.Log(x.src1+p.texhashToUvRect[x.src1].ToString()))
                let submesh = xsub.src0
                let texhash = xsub.src1
                select (submesh, texhash)
            );


        public static IEnumerable<(Matrix4x4 mt, MeshUnit mesh, IEnumerable<SubMeshUnit<T>> submeshes)>
            QuerySubMeshForIndexData<T>(this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
            where T : struct, IIndexUnit<T>
        =>
            from x in (srcmeshes.AsEnumerable(), mtsPerMesh).Zip()
            let mesh = x.src0
            let mt = x.src1
            select (
                mt,
                mesh,
                from xsub in mesh.MeshData.QuerySubmeshesForIndexData<T>()
                select xsub
            );
    }


    static class MeshElementsSourceUtility
    {

        public static IEnumerable<T> QueryMeshVertices<T>
            (this Mesh.MeshData meshdata, Action<Mesh.MeshData, NativeArray<T>> getElementSrc, VertexAttribute attr) where T : struct
        {
            var array = new NativeArray<T>(meshdata.vertexCount, Allocator.TempJob);
            if (meshdata.GetVertexAttributeDimension(attr) > 0)
            {
                getElementSrc(meshdata, array);
            }
            return array.rangeWithUsing(0, array.Length);
        }

        public static IEnumerable<SubMeshUnit<T>> QuerySubmeshesForVertices<T>
            (this Mesh.MeshData meshdata, Action<Mesh.MeshData, NativeArray<T>> getElementSrc, VertexAttribute attr) where T : struct
        {
            var array = new NativeArray<T>(meshdata.vertexCount, Allocator.TempJob);
            if (meshdata.GetVertexAttributeDimension(attr) > 0)
            {
                getElementSrc(meshdata, array);
            }
            return
                from i in 0.Inc(meshdata.subMeshCount)
                let desc = meshdata.GetSubMesh(i)
                select new SubMeshUnit<T>(i, desc, () => array.rangeWithUsing(desc.firstVertex, desc.vertexCount))
                ;
        }

        public static IEnumerable<SubMeshUnit<T>> QuerySubmeshesForIndexData<T>
            (this Mesh.MeshData meshdata) where T : struct, IIndexUnit<T>
        {
            return
                from i in 0.Inc(meshdata.subMeshCount)
                let desc = meshdata.GetSubMesh(i)
                select new SubMeshUnit<T>(i, desc, () => getIndexDataNativeArray_(desc))
                ;

            IEnumerable<T> getIndexDataNativeArray_(SubMeshDescriptor desc) =>
                meshdata.indexFormat switch
                {
                    IndexFormat.UInt16 when !(default(T) is ushort) =>
                        meshdata.GetIndexData<ushort>()
                            .Select(x => new T().Add(x))
                            .ToNativeArray(Allocator.TempJob)
                            .rangeWithUsing(desc.indexStart, desc.indexCount),

                    IndexFormat.UInt32 when !(default(T) is uint) =>
                        meshdata.GetIndexData<uint>()
                            .Select(x => new T().Add((int)x))
                            .ToNativeArray(Allocator.TempJob)
                            .rangeWithUsing(desc.indexStart, desc.indexCount),

                    _ =>
                        meshdata.GetIndexData<T>()
                            .range(desc.indexStart, desc.indexCount),
                };
        }

        static IEnumerable<T> rangeWithUsing<T>(this NativeArray<T> array, int first, int length) where T : struct
        {
            using (array) { foreach (var x in array.Range(first, length)) yield return x; }
        }
        static IEnumerable<T> range<T>(this NativeArray<T> array, int first, int length) where T : struct
        {
            foreach (var x in array.Range(first, length)) yield return x;
        }
    }

}
