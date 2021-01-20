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

namespace Abarabone.Geometry.inner1
{
    using Abarabone.Common.Extension;
    using Abarabone.Utilities;



    public struct AdditionalParameters
    {
        public Matrix4x4 mtBaseInv;
        public IEnumerable<Matrix4x4> mtsPerMesh;
        public IEnumerable<int> texhashPerSubMesh;
        public Dictionary<int, Rect> texhashToUvRect;
    }


    public struct MeshUnit
    {
        public MeshUnit(int i, Mesh.MeshData meshdata, int baseVertex)
        {
            this.MeshIndex = i;
            this.MeshData = meshdata;
            this.BaseVertex = baseVertex;
        }
        public readonly int MeshIndex;
        public readonly Mesh.MeshData MeshData;
        public readonly int BaseVertex;
    }


    public struct SubMeshUnit<T> where T : struct
    {
        public SubMeshUnit(int i, SubMeshDescriptor descriptor, NativeArray<T> srcArray)
        {
            this.SubMeshIndex = i;
            this.Descriptor = descriptor;
            this.srcArray = srcArray;
        }
        public readonly int SubMeshIndex;
        public readonly SubMeshDescriptor Descriptor;
        readonly NativeArray<T> srcArray;

        public IEnumerable<T> Indices() => this.srcArray.Range(this.Descriptor.indexStart, this.Descriptor.indexCount);
        public IEnumerable<T> Vertices() => this.srcArray.Range(this.Descriptor.firstVertex, this.Descriptor.vertexCount);
        public IEnumerable<T> IndicesWithUsing(){ using (this.srcArray) return Indices(); }
        public IEnumerable<T> VerticesWithUsing(){ using (this.srcArray) return Vertices(); }
    }


    static partial class ConvertIndicesUtility
    {

        public static IEnumerable<TIdx> QueryConvertIndices<TIdx>
            (this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
            where TIdx : struct, IIndexUnit<TIdx>
        {
            return
                from xsub in srcmeshes.QuerySubMeshForIndexData<TIdx>(mtsPerMesh)
                let mesh = xsub.mesh
                let submesh = xsub.submesh
                let mt = xsub.mt
                from tri in mt.IsMuinusScale()
                    ? submesh.Indices().AsTriangle().Reverse()
                    : submesh.Indices().AsTriangle()
                from idx in tri.Do(x => Debug.Log(x))
                select idx.Add(mesh.BaseVertex + submesh.Descriptor.baseVertex)
            ;
        }

    }

    static class VertexUtility
    {

        static public IEnumerable<Vector3> QueryConvertPositions
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
        =>
            from x in srcmeshes.QuerySubMeshForElements<Vector3>(p, (md, arr) => md.GetVertices(arr))
            from xsub in x.submeshes
            from vtx in xsub.submesh.VerticesWithUsing()
            select (Vector3)math.transform(x.mt, vtx)
            ;


        static public IEnumerable<Vector2> QueryConvertUvs
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters p)
        =>
            from x in srcmeshes.QuerySubMeshForElements<Vector2>(p, (md, arr) => md.GetUVs(0, arr))
            from xsub in x.submeshes
            from uv in xsub.submesh.VerticesWithUsing()
            select p.texhashToUvRect != null
                ? uv.ScaleUv(p.texhashToUvRect[xsub.texhash])
                : uv
            ;

    }

    static class MeshElementsSourceUtility
    {

        public static IEnumerable<SubMeshUnit<T>> ElementsPerSubMeshWithAlloc<T>
            (this Mesh.MeshData meshdata, Action<Mesh.MeshData, NativeArray<T>> getElementSrc) where T : struct
        {
            var array = new NativeArray<T>(meshdata.vertexCount, Allocator.Temp);
            getElementSrc(meshdata, array);
            return meshdata.elementsInSubMesh(array);
        }

        public static IEnumerable<SubMeshUnit<T>> VertexDataPerSubMesh<T>(this Mesh.MeshData meshdata) where T : struct =>
            meshdata.elementsInSubMesh(meshdata.GetVertexData<T>());

        public static IEnumerable<SubMeshUnit<T>> IndexDataPerSubMesh<T>(this Mesh.MeshData meshdata) where T : struct =>
            meshdata.elementsInSubMesh(meshdata.GetIndexData<T>());



        static IEnumerable<SubMeshUnit<T>> elementsInSubMesh<T>
            (this Mesh.MeshData meshdata, NativeArray<T> srcArray) where T : struct
        =>
            from i in 0.Inc(meshdata.subMeshCount)
            let desc = meshdata.GetSubMesh(i)
            select new SubMeshUnit<T>(i, desc, srcArray)
            ;
        
    }


    static class SubmeshQyeryConvertUtility
    {


        public static IEnumerable<(Matrix4x4 mt, IEnumerable<(SubMeshUnit<T> submesh, int texhash)> submeshes)>
            QuerySubMeshForElements<T>
            (this Mesh.MeshDataArray srcmeshes, AdditionalParameters adpars, Action<Mesh.MeshData, NativeArray<T>> getElementSrc)
            where T : struct
        =>
            from x in (srcmeshes.AsEnumerable(), adpars.mtsPerMesh).Zip()
            let submeshes = x.src0.MeshData.ElementsPerSubMeshWithAlloc<T>(getElementSrc)
            let mt = adpars.mtBaseInv * x.src1
            select (
                mt,
                from xsub in (submeshes, adpars.texhashPerSubMesh).Zip()
                let submesh = xsub.src0
                let texhash = xsub.src1
                select (submesh, texhash)
            );


        public static IEnumerable<(MeshUnit mesh, SubMeshUnit<T> submesh, Matrix4x4 mt)>
            QuerySubMeshForIndexData<T>(this Mesh.MeshDataArray srcmeshes, IEnumerable<Matrix4x4> mtsPerMesh)
            where T : struct
        =>
            from x in (srcmeshes.AsEnumerable(), mtsPerMesh).Zip()
            let mesh = x.src0
            let mt = x.src1
            from xsub in mesh.MeshData.IndexDataPerSubMesh<T>()
            select (mesh, submesh: xsub, mt)
            ;
    }

    static class MeshQyeryConvertUtility
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
}
namespace Abarabone.Geometry1
{
    using Abarabone.Common.Extension;
    using Abarabone.Utilities;
    using Abarabone.Geometry.inner;



    public struct MeshElements<TIdx> where TIdx : struct
    {
        public TIdx[] idxs;
        public Vector3[] poss;
        public Vector2[] uvs;
        public Vector3[] nms;
    }


    public interface ISetBufferParams
    {
        void SetBufferParams(Mesh.MeshData meshdata, int elementLength);
    }

    public interface IIndexUnit<T>
    {
        //public int Get();
        //public T Set(int newValue);
        T Add(int otherValue);
    }

    public struct UI16 : IIndexUnit<UI16>, ISetBufferParams
    {
        public ushort value;
        //public int Get() => this.value; 
        //public UI16 Set(int newValue) { this.value = (ushort)newValue; return this; }
        public UI16 Add(int otherValue) => new UI16 { value = (ushort)(otherValue + this.value) };

        public void SetBufferParams(Mesh.MeshData meshdata, int indexLength)
        {
            meshdata.SetIndexBufferParams(indexLength, IndexFormat.UInt16);
        }
    }

    public struct UI32 : IIndexUnit<UI32>, ISetBufferParams
    {
        public uint value;
        //public int Get() => (int)this.value;
        //public UI32 Set(int newValue) { this.value = (uint)newValue; return this; }
        public UI32 Add(int otherValue) => new UI32 { value = (uint)(otherValue + this.value) };

        public void SetBufferParams(Mesh.MeshData meshdata, int indexLength)
        {
            meshdata.SetIndexBufferParams(indexLength, IndexFormat.UInt32);
        }
    }


    public interface IVertexUnit<TVtx>
        where TVtx : struct
    {
        MeshElements<TIdx> BuildCombiner<TIdx>
            (IEnumerable<GameObject> gameObjects, Mesh.MeshData srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>;

        IEnumerable<TVtx> SelectAll<TIdx>(MeshElements<TIdx> src) where TIdx : struct;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionVertex : IVertexUnit<PositionVertex>, ISetBufferParams
    {
        public Vector3 Position;

        public MeshElements<TIdx> BuildCombiner<TIdx>
            (IEnumerable<GameObject> gameObjects, Mesh.MeshData srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>
        =>
            new MeshElements<TIdx>
            {
                idxs = srcmeshes.QueryConvertIndices<TIdx>(p.mtsPerMesh).ToArray(),
                poss = srcmeshes.QueryConvertPositions(p).ToArray(),
            };

        public IEnumerable<PositionVertex> SelectAll<TIdx>(MeshElements<TIdx> src) where TIdx : struct =>
            from x in src.poss
            select new PositionVertex
            {
                Position = x
            };

        public void SetBufferParams(Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionUvVertex : IVertexUnit<PositionUvVertex>, ISetBufferParams
    {
        public Vector3 Position;
        public Vector2 Uv;

        public MeshElements<TIdx> BuildCombiner<TIdx>
            (IEnumerable<GameObject> gameObjects, Mesh.MeshData srcmeshes, AdditionalParameters p)
            where TIdx : struct, IIndexUnit<TIdx>
        =>
            new MeshElements<TIdx>
            {
                idxs = srcmeshes.QueryConvertIndices<TIdx>(p.mtsPerMesh).ToArray(),
                poss = srcmeshes.QueryConvertPositions(p).ToArray(),
                uvs = srcmeshes.QueryConvertUvs(p).ToArray(),
            };

        public IEnumerable<PositionUvVertex> SelectAll<TIdx>(MeshElements<TIdx> src) where TIdx : struct =>
            from x in (src.poss.Do(x => Debug.Log(x)), src.uvs).Zip()
            select new PositionUvVertex
            {
                Position = x.src0,
                Uv = x.src1,
            };

        public void SetBufferParams(Mesh.MeshData meshdata, int vertexLength)
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            };
            meshdata.SetVertexBufferParams(vertexLength, layout);
        }
    }

    //static void AsPositionNormalUv(this Mesh.MeshData meshdata, int vertexLength)
    //{
    //    var layout = new[]
    //    {
    //            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
    //            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
    //            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
    //        };
    //    meshdata.SetVertexBufferParams(vertexLength, layout);
    //}

    public static class MeshCombineUtility
    {

        static Func<MeshElements<TIdx>> buildCombiner<TIdx, TVtx>
            (this IEnumerable<GameObject> gameObjects, Transform tfBase, Dictionary<int, Rect> texhashToUvRect = null)
            where TIdx : struct, IIndexUnit<TIdx>
            where TVtx : struct, IVertexUnit<TVtx>
        {
            var (srcmeshes, p) = gameObjects.calculateParametors(tfBase);

            return () => new TVtx().BuildCombiner<TIdx>(gameObjects, srcmeshes, p);
        }
        

        static (Mesh.MeshDataArray, AdditionalParameters) calculateParametors
            (this IEnumerable<GameObject> gameObjects, Transform tfBase, Dictionary<int, Rect> texhashToUvRect = null)
        {
            var mmts = FromObject.QueryMeshMatsTransform_IfHaving(gameObjects).ToArray();

            var meshesPerMesh = mmts.Select(x => x.mesh).ToArray();
            var mtsPerMesh = mmts.Select(x => x.tf.localToWorldMatrix).ToArray();
            var texhashesPerSubMesh = (
                from mmt in mmts
                from mat in mmt.mats
                select mat.mainTexture?.GetHashCode() ?? 0
            ).ToArray();

            var mtBaseInv = tfBase.worldToLocalMatrix;

            var srcmeshes = Mesh.AcquireReadOnlyMeshData(meshesPerMesh);

            return (srcmeshes, new AdditionalParameters
            {
                mtsPerMesh = mtsPerMesh,
                texhashPerSubMesh = texhashesPerSubMesh,
                mtBaseInv = mtBaseInv,
                texhashToUvRect = texhashToUvRect,
            });
        }

    }


    static public partial class MeshCreatorUtility
    {

        public static Mesh CreateMesh<TIdx, TVtx>(this MeshElements<TIdx> meshElements)
            where TIdx : struct, IIndexUnit<TIdx>, ISetBufferParams
            where TVtx : struct, IVertexUnit<TVtx>, ISetBufferParams
        {
            var dstmeshes = Mesh.AllocateWritableMeshData(1);
            var dstmesh = new Mesh();

            var src = meshElements;
            var dst = dstmeshes[0];

            var idxs = src.idxs.ToArray();
            dst.setBufferParams<TIdx>(idxs.Length);
            dst.GetIndexData<TIdx>().CopyFrom(idxs);

            var vtxs =  meshElements.selectAll<TIdx, TVtx>().ToArray();
            dst.setBufferParams<TVtx>(vtxs.Length);
            dst.GetVertexData<TVtx>().CopyFrom(vtxs);

            dst.subMeshCount = 1;
            dst.SetSubMesh(0, new SubMeshDescriptor(0, idxs.Length));
            
            Mesh.ApplyAndDisposeWritableMeshData(dstmeshes, dstmesh);
            dstmesh.RecalculateBounds();

            return dstmesh;
        }



        static IEnumerable<TVtx> selectAll<TIdx, TVtx>(this MeshElements<TIdx> src)
            where TIdx : struct, IIndexUnit<TIdx>
            where TVtx : struct, IVertexUnit<TVtx>
        =>
            new TVtx().SelectAll(src);

        static void setBufferParams<T>(this Mesh.MeshData meshdata, int elementLength)
            where T : struct, ISetBufferParams
        =>
            new T().SetBufferParams(meshdata, elementLength);


    }

}
