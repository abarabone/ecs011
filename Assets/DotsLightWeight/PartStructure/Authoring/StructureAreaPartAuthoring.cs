using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using System.Threading.Tasks;
using Unity.Linq;
using UnityEditor;

namespace DotsLite.Structure.Authoring
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;
    using Unity.Physics.Authoring;
    using System.Runtime.InteropServices;
    using DotsLite.Common.Extension;
    using DotsLite.Utilities;
    using DotsLite.Structure.Authoring;

    public class StructureAreaPartAuthoring : ModelGroupAuthoring.ModelAuthoringBase, IStructurePart//, IConvertGameObjectToEntity
    {
        public bool DoNotPathDeform;            // ���b�V�����p�X�ό`�����Ȃ�
        //public bool IsPathProjectionToTerrain;  // �p�X���b�V����n�`�ɂ����ĕό`������
        public bool NoDebris;
        public bool UseUpInterpolation;

        public int PartId;
        public int partId { get => this.PartId; set => this.PartId = value; }

        public AreaPartModel<UI32, PositionNormalUvVertex> PartModel;

        protected new void Reset()
        {
            base.Reset();
            this.PartModel.objectTop = this.gameObject;
        }


        public override IEnumerable<IMeshModel> QueryModel =>
            this.PartModel
            .WrapEnumerable();

        //public void Convert
        //    (Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        //{
        //    dstManager.
        //}
    }
}