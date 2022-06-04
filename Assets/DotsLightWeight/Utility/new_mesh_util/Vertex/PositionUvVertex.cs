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

    [StructLayout(LayoutKind.Sequential)]
    public struct PositionUvVertex : IVertexUnit
    {
        public Vector3 Position;
        public Vector2 Uv;
    }

    public class PositionUvVertexBuilder : IVertexBuilder, ISetBufferParams
    {

        public TVtx[] Build<TVtx>(IEnumerable<SrcMeshUnit> srcmeshes, AdditionalParameters p)
            where TVtx : struct, IVertexUnit
        {
            var poss = srcmeshes.QueryConvertPositions(p).ToArray();
            var uvs = srcmeshes.QueryConvertUvs(p, channel: 0).ToArray();
            var qVtx =
                from x in (poss, uvs).Zip()
                select new PositionUvVertex
                {
                    Position = x.src0,
                    Uv = x.src1,
                };
            return qVtx.Cast<TVtx>().ToArray();
        }


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

}
