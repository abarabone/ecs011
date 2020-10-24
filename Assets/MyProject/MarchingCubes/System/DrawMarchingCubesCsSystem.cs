﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    //[DisableAutoCreation]
    [UpdateAfter(typeof(BeginDrawCsBarier))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawSystemGroup))]
    public class DrawMarchingCubeCsSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();

            this.RequireSingletonForUpdate<MarchingCubeGlobalData>();
        }

        unsafe public struct V2
        {
            public float vec0, vec1, vec2, vec3;
            public float vec4, vec5, vec6, vec7;
            static public V2 v => new V2 {vec0=1, vec1=2, vec2=3, vec3=4, vec4=5, vec5=6, vec6=7, vec7=8 };
        }
        protected unsafe override void OnUpdate()
        {
            var globaldata = this.GetSingleton<MarchingCubeGlobalData>();

            var cubeInstances = globaldata.CubeInstances;
            //Debug.Log(instances.CubeInstances.length);
            if (cubeInstances.Length == 0) return;

            var cs = globaldata.GridCubeIdSetShader;
            var mat = globaldata.CubeMaterial;
            var buf = globaldata.DrawResources;


            var gridInstances = globaldata.GridInstances;
            buf.GridBuffer.SetData(gridInstances.AsArray().Reinterpret<V2>());//Debug.Log(gridInstances.AsArray().Length);
            //var ns = gridInstances.AsArray().Reinterpret<V2>();
            ////using var nd = new NativeArray<V2>(gridInstances.Length, Allocator.Temp);
            //var d = new V2[gridInstances.Length];// nd.Reinterpret<GridInstanceData>();
            ////buf.GridBuffer.GetData(d);Debug.Log($"{gridInstances[0].Position}, {gridInstances[0].ortho}: {d[0].Position}, {d[0].ortho}");
            //buf.GridBuffer.GetData(d); Debug.Log($"{ns[0].vec0}, {ns[0].vec4}: {math.asuint(d[0].vec0)}, {math.asuint(d[0].vec4)}");
            ////var grids = new Vector4[gridInstances.Length * 2];
            ////fixed (Vector4* pdst = grids)
            ////{
            ////    var psrc = gridInstances.GetUnsafePtr();
            ////    UnsafeUtility.MemCpy(pdst, psrc, gridInstances.Length * 2 * sizeof(float4));
            ////}
            ////mat.SetVectorArray("grids", grids);
            ////buf.GridBuffer.SetData(grids);
            ////mat.SetConstantBuffer_("grids", buf.GridBuffer);

            var e = new uint4[1];
            buf.StaticDataBuffer.GetData(e, 0, 0, 1);
            Debug.Log($"{e[0]}");

            var remain = (64 - (cubeInstances.Length & 0x3f)) & 0x3f;
            for (var i = 0; i < remain; i++) cubeInstances.AddNoResize(new CubeInstance { instance = 1 });
            buf.CubeInstancesBuffer.SetData(cubeInstances.AsArray());


            if(cs != null)
            {
                var dargparams = new IndirectArgumentsForDispatch(cubeInstances.Length >> 6, 1, 1);
                var dargs = buf.ArgsBufferForDispatch;
                dargs.SetData(ref dargparams);
                cs.Dispatch(0, cubeInstances.Length >> 6, 1, 1);//
            }


            var mesh = globaldata.DrawResources.mesh;
            var iargs = buf.ArgsBufferForInstancing;

            var instanceCount = cubeInstances.Length;
            var iargparams = new IndirectArgumentsForInstancing(mesh, instanceCount);
            iargs.SetData(ref iargparams);


            var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };//
            Graphics.DrawMeshInstancedIndirect(mesh, 0, mat, bounds, iargs);//
        }
    }
}
