﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.IO;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;

namespace Abarabone.MarchingCubes
{

    using Abarabone.Draw;
    using Abarabone.Utilities;



    //static public partial class DotGrid
    //{
    //    public struct BufferData// : IComponentData
    //    {
    //        public UIntPtr pCubes;
    //        public int CubeCount;
    //    }
    //}

    static public partial class DotGridGlobal
    {

        //public struct BufferData : IComponentData
        //{
        //    public UnsafeList<UIntPtr> FreeGridStocks;
        //}


        [InternalBufferCapacity(2)]
        public struct FreeGridStockData : IBufferElementData
        {
            public UnsafeList<UIntPtr> FreeGridStocks;
        }

        [InternalBufferCapacity(2)]
        public struct DefualtGridData : IBufferElementData
        {
            public DotGrid32x32x32Unsafe DefaultGrid;
        }

        public struct InstanceWorkData : IComponentData
        {
            public UnsafeList<CubeInstance> CubeInstances;
            public UnsafeList<GridInstanceData> GridInstances;
        }

        public struct InfoData : IComponentData
        {
            //public int MaxDrawGridLength;
            public int MaxCubeInstanceLength;
        }

    }

    public class MarchingCubeGlobalData : IComponentData, IDisposable
    {
        public NativeList<CubeInstance> CubeInstances;
        public NativeList<GridInstanceData> GridInstances;

        public NativeArray<DotGrid32x32x32Unsafe> DefaultGrids;
        public FreeStockList FreeStocks;
        public unsafe FreeStockList* FreeStocksPtr
        {
            get { fixed (FreeStockList* p = &this.FreeStocks) return p; }
        }

        public void Init(int maxCubeInstances, int maxGridInstances, int maxFreeGrids)
        {
            this.CubeInstances = new NativeList<CubeInstance>(maxCubeInstances, Allocator.Persistent);
            this.GridInstances = new NativeList<GridInstanceData>(maxGridInstances, Allocator.Persistent);
            this.DefaultGrids = new NativeArray<DotGrid32x32x32Unsafe>(2, Allocator.Persistent);
            this.FreeStocks = new FreeStockList(maxFreeGrids);

            this.DefaultGrids[(int)GridFillMode.Blank] = DotGridAllocater.Alloc(GridFillMode.Blank);
            this.DefaultGrids[(int)GridFillMode.Solid] = DotGridAllocater.Alloc(GridFillMode.Solid);
        }

        public void Dispose()
        {
            this.DefaultGrids[(int)GridFillMode.Blank].Dispose();
            this.DefaultGrids[(int)GridFillMode.Solid].Dispose();

            this.FreeStocks.Dispose();
            this.DefaultGrids.Dispose();
            this.GridInstances.Dispose();
            this.CubeInstances.Dispose();
        }
    }

    public struct FreeStockList : IDisposable
    {
        DoubleSideStack<UIntPtr> stocks;


        public FreeStockList(int maxBufferLength) =>
            this.stocks = new DoubleSideStack<UIntPtr>(maxBufferLength);

        public void Dispose()
        {
            while (this.stocks.PopFromBack(out var p)) DotGridAllocater.Dispose(p);
            while (this.stocks.PopFromFront(out var p)) DotGridAllocater.Dispose(p);
            this.stocks.Dispose();
        }

        public DotGrid32x32x32Unsafe Rent(GridFillMode fillMode)
        {
            switch(fillMode)
            {
                case GridFillMode.Blank:
                    {
                        var p = this.stocks.PopFromFront();
                        if (p != null) return new DotGrid32x32x32Unsafe(p, 0);

                        var p_ = this.stocks.PopFromBack();
                        if (p_ != null) return DotGridAllocater.Fill(p_, fillMode);

                        return DotGridAllocater.Alloc(fillMode);
                    }
                case GridFillMode.Solid:
                    {
                        var p = this.stocks.PopFromBack();
                        if (p != null) return new DotGrid32x32x32Unsafe(p, 32 * 32 * 32);

                        var p_ = this.stocks.PopFromFront();
                        if (p_ != null) return DotGridAllocater.Fill(p_, fillMode);

                        return DotGridAllocater.Alloc(fillMode);
                    }
                default:
                    return new DotGrid32x32x32Unsafe();
            }
        }

        public unsafe void Back(DotGrid32x32x32Unsafe grid, GridFillMode fillMode)
        {
            var isBackSuccess = fillMode switch
            {
                GridFillMode.Blank => this.stocks.PushToFront((UIntPtr)grid.pUnits),
                GridFillMode.Solid => this.stocks.PushToBack((UIntPtr)grid.pUnits),
                _ => true
            };
            if (!isBackSuccess) DotGridAllocater.Dispose((UIntPtr)grid.pUnits);
        }
        public void Back(DotGrid32x32x32Unsafe grid) => Back(grid, grid.FillModeBlankOrSolid);
    }

    public unsafe struct DoubleSideStack<T> : IDisposable
        where T : unmanaged
    {
        //NativeArray<T> buffer;
        T *buffer;
        int bufferLength;
        public int FrontCount { get; private set; }
        public int BackCount { get; private set; }

        public DoubleSideStack(int maxLength)
        {
            //this.buffer = new NativeArray<T>(maxLength, Allocator.Persistent);
            this.buffer = (T*)UnsafeUtility.Malloc(sizeof(T) * maxLength, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
            this.bufferLength = maxLength;
            this.FrontCount = 0;
            this.BackCount = 0;
        }
        public void Dispose() => UnsafeUtility.Free(this.buffer, Allocator.Persistent);//this.buffer.Dispose();


        public bool PushToFront(T item)
        {
            if(this.FrontCount + this.BackCount < this.bufferLength)
            {
                var i = this.FrontCount++;
                this.buffer[i] = item;
                return true;
            }
            return false;
        }
        public bool PushToBack(T item)
        {
            if (this.FrontCount + this.BackCount < this.bufferLength)
            {
                var i = this.bufferLength - ++this.BackCount;
                this.buffer[i] = item;
                return true;
            }
            return false;
        }

        public bool PopFromFront(out T item)
        {
            if(this.FrontCount > 0)
            {
                var i = --this.FrontCount;
                item = this.buffer[i];
                return true;
            }
            item = default;
            return false;
        }
        public bool PopFromBack(out T item)
        {
            if (this.BackCount > 0)
            {
                var i = this.bufferLength - this.BackCount--;
                item = this.buffer[i];
                return true;
            }
            item = default;
            return false;
        }

        public T PopFromFront()
        {
            this.PopFromFront(out var item);
            return item;
        }
        public T PopFromBack()
        {
            this.PopFromBack(out var item);
            return item;
        }
    }



    static public partial class DotGridArea
    {

        public struct InitializeData : IComponentData
        {
            public GridFillMode FillMode;
        }


        public unsafe struct BufferData : IComponentData
        {
            public UnsafeList<DotGrid32x32x32Unsafe> Grids;
        }

        public struct InfoData : IComponentData
        {
            public int3 GridLength;
            public int3 GridWholeLength;
        }
        public struct InfoWorkData : IComponentData
        {
            public int3 GridSpan;
        }
    }




    static public partial class Resource
    {

        public class Initialize : IComponentData
        {
            public MarchingCubeAsset Asset;
            public int MaxGridLengthInShader;
        }




        public class DrawResourceData : IComponentData
        {
            public Mesh CubeMesh;
            public Material CubeMaterial;

            public ComputeShader GridCubeIdSetShader;
        }


        public class DrawBufferData : IComponentData//, IDisposable
        {
            public mc.DrawResources DrawResources;

            //public ComputeBuffer ArgsBufferForInstancing;
            //public ComputeBuffer ArgsBufferForDispatch;

            //public ComputeBuffer NormalBuffer;
            //public ComputeBuffer CubePatternBuffer;
            //public ComputeBuffer CubeVertexBuffer;
            //public ComputeBuffer GridBuffer;

            //public ComputeBuffer CubeInstancesBuffer;
            //public RenderTexture GridCubeIdBuffer;
            ////public ComputeBuffer GridCubeIdBuffer;


            //public void Dispose()
            //{
            //    Debug.Log("dispose");
            //    if (this.ArgsBufferForInstancing != null) this.ArgsBufferForInstancing.Dispose();
            //    if (this.ArgsBufferForDispatch != null) this.ArgsBufferForDispatch.Dispose();

            //    if (this.CubeInstancesBuffer != null) this.CubeInstancesBuffer.Dispose();
            //    if (this.GridCubeIdBuffer != null) this.GridCubeIdBuffer.Release();

            //    if (this.NormalBuffer != null) this.NormalBuffer.Dispose();
            //    if (this.CubePatternBuffer != null) this.CubePatternBuffer.Dispose();
            //    if (this.CubeVertexBuffer != null) this.CubeVertexBuffer.Dispose();
            //    if (this.GridBuffer != null) this.GridBuffer.Dispose();
            //}
        }

    }





    static public partial class DotGridGlobal
    {
        /// <summary>
        /// フリーストックからグリッドを貸与。
        /// ストックがなければ、新規に確保して返す。
        /// </summary>
        static public DotGrid32x32x32Unsafe RentGridFromFreeStocks
            (ref this DynamicBuffer<FreeGridStockData> freeStocker, GridFillMode fillMode)
        {
            ref var currentStocks = ref freeStocker.ElementAt((int)fillMode).FreeGridStocks;
            if (currentStocks.length > 0)
            {
                var p = currentStocks[--currentStocks.length];

                const int dotnum = DotGrid32x32x32Unsafe.dotNum;
                return new DotGrid32x32x32Unsafe(p, dotnum * (int)fillMode);
            }

            ref var otherStocks = ref freeStocker.ElementAt((int)fillMode ^ 1).FreeGridStocks;
            if (otherStocks.length > 0)
            {
                var p = otherStocks[--otherStocks.length];

                return DotGridAllocater.Fill(p, fillMode);
            }

            return DotGridAllocater.Alloc(fillMode);
        }

        static public DotGrid32x32x32Unsafe RentBlankGrid(ref this DynamicBuffer<FreeGridStockData> buf, GridFillMode fillMode) =>
            buf.RentGridFromFreeStocks(GridFillMode.Blank);
        static public DotGrid32x32x32Unsafe RentSolidGrid(ref this DynamicBuffer<FreeGridStockData> buf, GridFillMode fillMode) =>
            buf.RentGridFromFreeStocks(GridFillMode.Solid);


        /// <summary>
        /// 使い終わったグリッドを、フリーストックに戻す。
        /// 収容できなければ、破棄してしまう。
        /// </summary>
        static public unsafe void BackToFreeGridStocks
            (
                ref this DynamicBuffer<DotGridGlobal.FreeGridStockData> freeStocker,
                GridFillMode fillMode, DotGrid32x32x32Unsafe grid
            )
        {
            //freeStocker.ElementAt((int)fillMode).FreeGridStocks.Add((UIntPtr)grid.pUnits);

            ref var currentStocks = ref freeStocker.ElementAt((int)fillMode).FreeGridStocks;
            if (currentStocks.length < currentStocks.capacity)
            {
                currentStocks.AddNoResize((UIntPtr)grid.pUnits);
            }

            ref var otherStocks = ref freeStocker.ElementAt((int)~fillMode).FreeGridStocks;
            if (otherStocks.length < otherStocks.capacity)
            {
                currentStocks.AddNoResize((UIntPtr)grid.pUnits);
            }

            DotGridAllocater.Dispose((UIntPtr)grid.pUnits);
        }


        /// <summary>
        /// デフォルトグリッドを取得する。
        /// </summary>
        static public DotGrid32x32x32Unsafe GetDefaultGrid
            (ref this DynamicBuffer<DotGridGlobal.DefualtGridData> defaultGrids, GridFillMode fillMode)
        {
            return defaultGrids.ElementAt((int)fillMode).DefaultGrid;
        }

        public static DotGrid32x32x32Unsafe Blank(ref this DynamicBuffer<DefualtGridData> defaultGrids) =>
            defaultGrids.GetDefaultGrid(GridFillMode.Blank);
        public static DotGrid32x32x32Unsafe Solid(ref this DynamicBuffer<DefualtGridData> defaultGrids) =>
            defaultGrids.GetDefaultGrid(GridFillMode.Solid);
    }



    static public partial class GridArea
    {
        /// <summary>
        /// グリッドエリアから、指定した位置のグリッドポインタを取得する。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public unsafe DotGrid32x32x32UnsafePtr GetGridFromArea
            (
                //ref this (DotGridArea.BufferData, DotGridArea.InfoWorkData) x,
                ref DotGridArea.BufferData areaGrids,
                ref DotGridArea.InfoWorkData areaInfo,
                int ix, int iy, int iz
            )
        {
            //ref var areaGrids = ref x.Item1;
            //ref var areaInfo = ref x.Item2;

            var i3 = new int3(ix, iy, iz) + 1;
            var i = math.dot(i3, areaInfo.GridSpan);

            return new DotGrid32x32x32UnsafePtr { p = areaGrids.Grids.Ptr + i };
        }
    }



    static public partial class Resource
    {


        //    static public DrawBufferData CreateDrawBufferData(MarchingCubeAsset asset, int maxGridLength)
        //    {
        //        var buf = new DrawBufferData();

        //        buf.ArgsBufferForInstancing = ComputeShaderUtility.CreateIndirectArgumentsBufferForInstancing();
        //        buf.ArgsBufferForDispatch = ComputeShaderUtility.CreateIndirectArgumentsBufferForDispatch();

        //        buf.CubeInstancesBuffer = createCubeIdInstancingShaderBuffer_(32 * 32 * 32 * maxGridLength);
        //        buf.GridCubeIdBuffer = createGridCubeIdShaderBuffer_(maxGridLength);

        //        var vertexNormalDict = makeVertexNormalsDict_(asset.CubeIdAndVertexIndicesList); Debug.Log(vertexNormalDict.Count);
        //        buf.NormalBuffer = createNormalList_(vertexNormalDict);
        //        buf.CubePatternBuffer = createCubePatternBuffer_(asset.CubeIdAndVertexIndicesList, vertexNormalDict);
        //        buf.CubeVertexBuffer = createCubeVertexBuffer_(asset.BaseVertexList);
        //        buf.GridBuffer = createGridShaderBuffer_(512);

        //        return buf;


        //        ComputeBuffer createCubeIdInstancingShaderBuffer_(int maxUnitLength)
        //        {
        //            var buffer = new ComputeBuffer(maxUnitLength, Marshal.SizeOf<uint>());

        //            return buffer;
        //        }

        //        RenderTexture createGridCubeIdShaderBuffer_(int maxGridLength_)
        //        {
        //            var buffer = new RenderTexture(32 * 32, 32, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R8_UInt, 0);
        //            //var buffer = new RenderTexture(32 * 32, 32, 0, UnityEngine.Experimental.Rendering.GraphicsFormat.R32_UInt, 0);
        //            buffer.enableRandomWrite = true;
        //            buffer.dimension = TextureDimension.Tex2DArray;
        //            buffer.volumeDepth = maxGridLength_;
        //            buffer.Create();

        //            return buffer;
        //        }
        //        //static public ComputeBuffer createGridCubeIdShaderBuffer_( int maxGridLength )
        //        //{
        //        //    var buffer = new ComputeBuffer( 32 * 32 * 32 * maxGridLength, Marshal.SizeOf<uint>() );

        //        //    return buffer;
        //        //}


        //        float3 round_normal_(float3 x)
        //        {
        //            var digits = 5;

        //            return new float3((float)Math.Round(x.x, digits), (float)Math.Round(x.y, digits), (float)Math.Round(x.z, digits));
        //            //return new float3( new half3( x ) );
        //        }
        //        Dictionary<float3, int> makeVertexNormalsDict_(MarchingCubeAsset.CubeWrapper[] cubeIdsAndVtxIndexLists_)
        //        {
        //            return cubeIdsAndVtxIndexLists_
        //                .SelectMany(x => x.normalsForVertex)
        //                .Select(x => round_normal_(x))
        //                .Distinct(x => x)
        //                .Select((x, i) => (x, i))
        //                .ToDictionary(x => x.x, x => x.i);
        //        }

        //        ComputeBuffer createNormalList_(Dictionary<float3, int> normalToIdDict)
        //        {
        //            var buffer = new ComputeBuffer(normalToIdDict.Count, Marshal.SizeOf<Vector4>(), ComputeBufferType.Constant);

        //            var q =
        //                from n in normalToIdDict
        //                    //.OrderBy(x => x.Value)
        //                    //.Do(x => Debug.Log($"{x.Value} {x.Key}"))
        //                    .Select(x => x.Key)
        //                select new Vector4
        //                {
        //                    x = n.x,
        //                    y = n.y,
        //                    z = n.z,
        //                    w = 0.0f,
        //                };

        //            buffer.SetData(q.ToArray());

        //            return buffer;
        //        }

        //        ComputeBuffer createCubePatternBuffer_(MarchingCubeAsset.CubeWrapper[] cubeIdsAndVtxIndexLists_, Dictionary<float3, int> normalToIdDict)
        //        {
        //            //var buffer = new ComputeBuffer( 254, Marshal.SizeOf<uint4>() * 2, ComputeBufferType.Constant );
        //            var buffer = new ComputeBuffer(254 * 2, Marshal.SizeOf<uint4>(), ComputeBufferType.Constant);

        //            var q =
        //                from cube in cubeIdsAndVtxIndexLists_
        //                orderby cube.cubeId
        //                select new[]
        //                {
        //                    toTriPositionIndex_( cube.vertexIndices ),
        //                    toVtxNormalIndex_( cube.normalsForVertex, normalToIdDict )
        //                };
        //            //q.SelectMany(x=>x).ForEach( x => Debug.Log(x) );
        //            buffer.SetData(q.SelectMany(x => x).Select(x => math.asfloat(x)).ToArray());

        //            return buffer;


        //            uint4 toTriPositionIndex_(int[] indices)
        //            {
        //                var idxs = indices
        //                    .Concat(Enumerable.Repeat(0, 12 - indices.Length))
        //                    .ToArray();

        //                return new uint4
        //                {
        //                    x = (idxs[0], idxs[1], idxs[2], 0).PackToByte4Uint(),
        //                    y = (idxs[3], idxs[4], idxs[5], 0).PackToByte4Uint(),
        //                    z = (idxs[6], idxs[7], idxs[8], 0).PackToByte4Uint(),
        //                    w = (idxs[9], idxs[10], idxs[11], 0).PackToByte4Uint(),
        //                    //x = (uint)( idxs[ 0]<<0 & 0xff | idxs[ 1]<<8 & 0xff00 | idxs[ 2]<<16 & 0xff0000 ),
        //                    //y = (uint)( idxs[ 3]<<0 & 0xff | idxs[ 4]<<8 & 0xff00 | idxs[ 5]<<16 & 0xff0000 ),
        //                    //z = (uint)( idxs[ 6]<<0 & 0xff | idxs[ 7]<<8 & 0xff00 | idxs[ 8]<<16 & 0xff0000 ),
        //                    //w = (uint)( idxs[ 9]<<0 & 0xff | idxs[10]<<8 & 0xff00 | idxs[11]<<16 & 0xff0000 ),
        //                };
        //            }
        //            uint4 toVtxNormalIndex_(Vector3[] normals, Dictionary<float3, int> normalToIdDict_)
        //            {
        //                return new uint4
        //                {
        //                    x = (ntoi(0), ntoi(1), ntoi(2), ntoi(3)).PackToByte4Uint(),
        //                    y = (ntoi(4), ntoi(5), ntoi(6), ntoi(7)).PackToByte4Uint(),
        //                    z = (ntoi(8), ntoi(9), ntoi(10), ntoi(11)).PackToByte4Uint(),
        //                    //x = (uint)( ntoi(0,0) | ntoi(1,8) | ntoi( 2,16) | ntoi( 3,24) ),
        //                    //y = (uint)( ntoi(4,0) | ntoi(5,8) | ntoi( 6,16) | ntoi( 7,24) ),
        //                    //z = (uint)( ntoi(8,0) | ntoi(9,8) | ntoi(10,16) | ntoi(11,24) ),
        //                    w = 0,
        //                };
        //                //int ntoi( int i, int shift ) => (normalToIdDict_[ round_normal_(normals[ i ]) ] & 0xff) << shift;
        //                int ntoi(int i)
        //                {
        //                    Debug.Log($"{i} @ {round_normal_(normals[i])} => {normalToIdDict_[round_normal_(normals[i])]}");
        //                    return normalToIdDict_[round_normal_(normals[i])];
        //                }
        //            }
        //        }

        //        ComputeBuffer createCubeVertexBuffer_(Vector3[] baseVertices)
        //        {
        //            var buffer = new ComputeBuffer(12, Marshal.SizeOf<uint4>(), ComputeBufferType.Constant);

        //            ((int x, int y, int z) ortho1, (int x, int y, int z) ortho2)[] near_cube_offsets =
        //            {
        //                (( 0, 0, -1), ( 0, -1, 0)),
        //                (( -1, 0, 0), ( 0, -1, 0)),
        //                (( +1, 0, 0), ( 0, -1, 0)),
        //                (( 0, 0, +1), ( 0, -1, 0)),

        //                (( -1, 0, 0), ( 0, 0, -1)),
        //                (( +1, 0, 0), ( 0, 0, -1)),
        //                (( -1, 0, 0), ( 0, 0, +1)),
        //                (( +1, 0, 0), ( 0, 0, +1)),

        //                (( 0, 0, -1), ( 0, +1, 0)),
        //                (( -1, 0, 0), ( 0, +1, 0)),
        //                (( +1, 0, 0), ( 0, +1, 0)),
        //                (( 0, 0, +1), ( 0, +1, 0)),
        //            };
        //            (int ortho1, int ortho2, int slant)[] near_cube_ivtxs =
        //            {
        //                (3,8,11),
        //                (2,9,10),
        //                (1,10,9),
        //                (0,11,8),

        //                (5,6,7),
        //                (4,7,6),
        //                (7,4,5),
        //                (6,5,4),

        //                (11,0,3),
        //                (10,1,2),
        //                (9,2,1),
        //                (8,3,0),
        //            };

        //            var q =
        //                from v in Enumerable
        //                    .Zip(near_cube_offsets, near_cube_ivtxs, (x, y) => (ofs: x, ivtx: y))
        //                    .Zip(baseVertices, (x, y) => (x.ofs, x.ivtx, pos: y))
        //                let x = (v.ivtx.ortho1, v.ivtx.ortho2, v.ivtx.slant, 0).PackToByte4Uint()
        //                let y = (v.ofs.ortho1.x + 1, v.ofs.ortho1.y + 1, v.ofs.ortho1.z + 1, 0).PackToByte4Uint()
        //                let z = (v.ofs.ortho2.x + 1, v.ofs.ortho2.y + 1, v.ofs.ortho2.z + 1, 0).PackToByte4Uint()
        //                let w = ((int)(v.pos.x * 2) + 1, (int)(v.pos.y * 2) + 1, (int)(v.pos.z * 2) + 1, 0).PackToByte4Uint()
        //            //let x = v.ivtx.x<<0 & 0xff | v.ivtx.y<<8 & 0xff00 | v.ivtx.z<<16 & 0xff0000
        //            //let y = v.ofs.ortho1.x+1<<0 & 0xff | v.ofs.ortho1.y+1<<8 & 0xff00 | v.ofs.ortho1.z+1<<16 & 0xff0000
        //            //let z = v.ofs.ortho2.x+1<<0 & 0xff | v.ofs.ortho2.y+1<<8 & 0xff00 | v.ofs.ortho2.z+1<<16 & 0xff0000
        //            //let w = (int)(v.pos.x*2+1)<<0 & 0xff | (int)(v.pos.y*2+1)<<8 & 0xff00 | (int)(v.pos.z*2+1)<<16 & 0xff0000
        //            select new uint4(x, y, z, w)
        //                ;

        //            buffer.SetData(q.Select(x => math.asfloat(x)).ToArray());

        //            return buffer;
        //        }

        //        ComputeBuffer createGridShaderBuffer_(int maxGridLength_)
        //        {
        //            var buffer = new ComputeBuffer(maxGridLength_ * 2, Marshal.SizeOf<uint4>(), ComputeBufferType.Constant);

        //            return buffer;
        //        }

        //    }




        //    static public Mesh CreateMesh()
        //    {
        //        var mesh_ = new Mesh();
        //        mesh_.name = "marching cube unit";

        //        var qVtx =
        //            from i in Enumerable.Range(0, 12)
        //            select new Vector3(i % 3, i / 3, 0)
        //            ;
        //        var qIdx =
        //            from i in Enumerable.Range(0, 3 * 4)
        //            select i
        //            ;
        //        mesh_.vertices = qVtx.ToArray();
        //        mesh_.triangles = qIdx.ToArray();

        //        return mesh_;
        //    }

    }
}

