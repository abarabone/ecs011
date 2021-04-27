using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Entities;
using Unity.Transforms;
//using Unity.Rendering;
using Unity.Properties;
using Unity.Burst;
using Abarabone.Geometry;

namespace Abarabone.Hit
{

    static public partial class Hit
    {

        public enum HitType
        {
            charactor,
            part,

        }


        // �R���W���������G���e�B�e�B�ɕt���A�q�b�g�����̓K�p�ΏۂɗU������B
        public struct TargetData : IComponentData
        {
            public Entity StateEntity;
            public HitType HitType;
        }


    }

}