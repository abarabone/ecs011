﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

using Abss.Cs;
using Abss.Arthuring;
using Abss.Misc;
using Abss.SystemGroup;

namespace Abss.Draw
{

    //[DisableAutoCreation]
    [UpdateAfter(typeof( BoneToDrawInstanceSystem ) )]
    [UpdateInGroup(typeof(DrawSystemGroup))]
    public class BeginDrawCsBarier : EntityCommandBufferSystem
    { }


    /// <summary>
    /// メッシュをインスタンシングバッファを使用してインスタンシング描画する
    /// </summary>
    //[AlwaysUpdateSystem]
    //[DisableAutoCreation]
    [UpdateAfter(typeof( BeginDrawCsBarier ) )]
    [UpdateInGroup(typeof(DrawSystemGroup))]
    public class DrawMeshCsSystem : JobComponentSystem
    {

        public int MaxInstance = 10000;



        // 描画用バッファ
        SimpleComputeBuffer<bone_unit> instanceTransformBuffer;
        SimpleIndirectArgsBuffer instanceArgumentsBuffer;
        InstancingIndirectArguments arguments;


        DrawMeshResourceHolder resourceHolder = new DrawMeshResourceHolder();

        public DrawMeshResourceHolder GetResourceHolder() => this.resourceHolder;


        NativeArray<float4> instanceBoneVectors;
        NativeArray<NativeSlice<float4>> instanceBoneVectorEveryModels;

        public NativeArray<NativeSlice<float4>> GetInstanceBoneVectorEveryModels() =>
            this.instanceBoneVectorEveryModels;


        NativeArray<ThreadSafeCounter<Persistent>> instanceCounters;

        public NativeArray<ThreadSafeCounter<Persistent>> GetInstanceCounters() =>
            this.instanceCounters;


        protected override void OnStartRunning()
        {
            createBuffers();
            allocVectors();
            allocInstanceCounters();

            return;


            void createBuffers()
            {
                this.instanceArgumentsBuffer.CreateBuffer();
                this.instanceTransformBuffer = new SimpleComputeBuffer<bone_unit>( "bones", 4 * 16 * this.MaxInstance );
            }

            void allocVectors()
            {
                var vectorLength = 4;
                var instanceMax = this.MaxInstance;
                var arrayLengths = this.resourceHolder.Units
                    .Select( x => vectorLength * x.Mesh.bindposes.Length * instanceMax )
                    .ToArray();

                this.instanceBoneVectorEveryModels =
                    new NativeArray<NativeSlice<float4>>( arrayLengths.Length, Allocator.Persistent );
                
                this.instanceBoneVectors =
                    new NativeArray<float4>( arrayLengths.Sum(), Allocator.Persistent );
                
                var start = 0;
                for( var i = 0; i < arrayLengths.Length; i++ )
                {
                    this.instanceBoneVectorEveryModels[ i ] = this.instanceBoneVectors.Slice( start, arrayLengths[ i ] );
                    start += arrayLengths[ i ];
                }
            }

            void allocInstanceCounters()
            {
                this.instanceCounters =
                    new NativeArray<ThreadSafeCounter<Persistent>>( this.resourceHolder.Units.Count, Allocator.Persistent );

                for( var i = 0; i < this.instanceCounters.Length; i++ )
                {
                    this.instanceCounters[ i ] = new ThreadSafeCounter<Persistent>( 0 );
                }
            }
        }
        protected override void OnStopRunning()
        {
            if( this.instanceCounters.IsCreated )
            {
                for( var i = 0; i < this.instanceCounters.Length; i++ )
                {
                    this.instanceCounters[ i ].Dispose();
                }
                this.instanceCounters.Dispose();
            }

            if( this.instanceBoneVectors.IsCreated ) this.instanceBoneVectors.Dispose();
            if( this.instanceBoneVectorEveryModels.IsCreated ) this.instanceBoneVectorEveryModels.Dispose();

            this.instanceTransformBuffer.Dispose();
            this.instanceArgumentsBuffer.Dispose();

            this.resourceHolder.Dispose();
        }

        public struct bone_unit
        {
            public float4 pos;
            public quaternion rot;
        }
        protected override JobHandle OnUpdate( JobHandle inputDeps )
        {

            var i = 0;
            var vcOffset = 0;
            foreach( var resource in this.resourceHolder.Units.Take(1) )
            {
                var mesh = resource.Mesh;
                var mat = resource.Material;
                var bounds = new Bounds() { center = Vector3.zero, size = Vector3.one * 1000.0f };
                var args = this.instanceArgumentsBuffer;

                var instanceCount = this.instanceCounters[ i ].Count;
                //using( var a = new InstancingIndirectArguments( mesh, (uint)instanceCount ) )
                //    args.Buffer.SetData( a.Arguments );
                var abuf = new SimpleIndirectArgsBuffer( mesh, (uint)instanceCount );

                var cbuf = new SimpleComputeBuffer<bone_unit>( "bones", 4 * 16 * this.MaxInstance );
                var srcBuffer = this.instanceBoneVectors.Reinterpret<float4, bone_unit>();
                var vectorLength = instanceCount * 16;
                this.instanceTransformBuffer.Buffer.SetData( srcBuffer, vcOffset/2, 0, vectorLength );
                mat.SetBuffer( this.instanceTransformBuffer );
                mat.SetInt( "boneLength", mesh.bindposes.Length );
                
                Graphics.DrawMeshInstancedIndirect( mesh, 0, mat, bounds, args );
                i++;
                vcOffset += this.MaxInstance * 4 * 16;
            }

            return inputDeps;
        }

    }
}
