using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Unity.Linq;
using Unity.Entities;
using System;

namespace DotsLite.Structure.Authoring
{

    using DotsLite.Model;
    using DotsLite.Draw.Authoring;
    using DotsLite.Common.Extension;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Character;//ObjectMain �͂����ɂ���A���O�ς���ׂ����H
    using DotsLite.Structure;
    using Unity.Physics;
    using Unity.Transforms;
    using DotsLite.Geometry;
    using DotsLite.Utilities;
    using DotsLite.EntityTrimmer.Authoring;

    using Material = UnityEngine.Material;
    using Unity.Physics.Authoring;
    
    public static class StructurePartUtility
    {

        // �����v���n�u���܂Ƃ߂邱�Ƃ͂ł��Ȃ����낤���H
        public static Entity CreateDebrisPrefab(GameObjectConversionSystem gcs, GameObject part, GameObject master = null)
        {
            var em_ = gcs.DstEntityManager;


            var types = em_.CreateArchetype
            (
                typeof(PartDebris.Data),

                typeof(DrawInstance.MeshTag),
                typeof(DrawInstance.ModelLinkData),
                typeof(DrawInstance.TargetWorkData),

                typeof(PhysicsVelocity),//�b��
                typeof(PhysicsGravityFactor),//�b��
                typeof(PhysicsMass),//�b��

                typeof(Marker.Rotation),
                typeof(Marker.Translation),

                //typeof(NonUniformScale),//�b��
                typeof(Prefab)
            );
            //var prefabEnt = gcs_.CreateAdditionalEntity(part_, types);
            var prefabEnt = em_.CreateEntity(types);


            em_.SetComponentData(prefabEnt,
                new PartDebris.Data
                {
                    LifeTime = 5.0f,
                }
            );

            //em_.SetComponentData(mainEntity,
            //    new NonUniformScale
            //    {
            //        Value = new float3(1, 1, 1)
            //    }
            //);
            em_.SetComponentData(prefabEnt,
                new DrawInstance.ModelLinkData
                {
                    DrawModelEntityCurrent = gcs.GetFromModelEntityDictionary(master ?? part),
                }
            );
            em_.SetComponentData(prefabEnt,
                new DrawInstance.TargetWorkData
                {
                    DrawInstanceId = -1,
                }
            );

            //var mass = part_.GetComponent<PhysicsBodyAuthoring>().CustomMassDistribution;
            //var mass = em_.GetComponentData<PhysicsCollider>(gcs_.GetPrimaryEntity(part_)).MassProperties;
            em_.SetComponentData(prefabEnt,
                //PhysicsMass.CreateDynamic( mass, 1.0f )
                PhysicsMass.CreateDynamic(MassProperties.UnitSphere, 1.0f)// �b�肾�����H
            );
            em_.SetComponentData(prefabEnt,
                new PhysicsGravityFactor
                {
                    Value = 1.0f,
                }
            );


            em_.SetName_(prefabEnt, $"{part.name} debris");

            return prefabEnt;
        }

    }
}

