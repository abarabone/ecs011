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

namespace DotsLite.MarchingCubes
{

    using DotsLite.Draw;
    using DotsLite.Utilities;



    static public partial class DotGridArea
    {

        //public struct InitializeData : IComponentData
        //{
        //    public GridFillMode FillMode;
        //}


        //public unsafe struct LinkToGridData : IComponentData, IDisposable
        //{
        //    public UnsafeList<DotGrid32x32x32Unsafe> Grids;

        //    public void Dispose()
        //    {
        //        foreach (var i in this.Grids)
        //        {
        //            if (i.pUnits != null) i.Dispose();
        //        }
        //        this.Grids.Dispose();
        //        Debug.Log("Link to grid data disposed");
        //    }
        //}

        public struct DotGridPrefabData : IComponentData
        {
            public Entity Prefab;
        }

        public struct InfoData : IComponentData
        {
            public int3 GridLength;
            //public int3 GridWholeLength;
        }
        public struct InfoWorkData : IComponentData
        {
            public int3 GridSpan;
        }

    }





}

