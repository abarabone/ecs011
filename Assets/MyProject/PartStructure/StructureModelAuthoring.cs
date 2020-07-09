﻿using System.Collections;
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

namespace Abarabone.Structure.Aurthoring
{
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Model.Authoring;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;


    /// <summary>
    /// 
    /// </summary>
    public class StructureModelAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity//, IDeclareReferencedPrefabs
    {


        public Material Material;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var parts = this.GetComponentsInChildren<StructurePartAuthoring>();

            referencedPrefabs.AddRange(parts.Select(x => x.gameObject).Select(x=>UnityEditor.PrefabUtility.GetCorrespondingObjectFromOriginalSource(x)) );
        }


        /// <summary>
        /// 
        /// </summary>
        public async void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {



            return;


            createModelEntity_(conversionSystem, this.gameObject, this.Material);

            initInstanceEntityComponents_(conversionSystem, this.gameObject);

            return;


            void createModelEntity_
                (GameObjectConversionSystem gcs, GameObject main, Material srcMaterial)
            {
                var mat = new Material(srcMaterial);
                var mesh = main.GetComponentInChildren<MeshFilter>().sharedMesh;

                const BoneType BoneType = BoneType.TR;
                const int boneLength = 1;

                var modelEntity_ = gcs.CreateDrawModelEntityComponents(main, mesh, mat, BoneType, boneLength);
            }

            void initInstanceEntityComponents_(GameObjectConversionSystem gcs, GameObject main)
            {
                dstManager.SetName(entity, $"{this.name}");

                var em = gcs.DstEntityManager;


                var mainEntity = gcs.GetPrimaryEntity(main);

                var archetype = em.CreateArchetype(
                    typeof(ModelPrefabNoNeedLinkedEntityGroupTag),
                    typeof(DrawInstance.ModeLinkData),
                    typeof(DrawInstance.TargetWorkData),
                    typeof(Translation),
                    typeof(Rotation),
                    typeof(NonUniformScale)
                );
                em.SetArchetype(mainEntity, archetype);


                em.SetComponentData(mainEntity,
                    new DrawInstance.ModeLinkData
                    //new DrawTransform.LinkData
                    {
                        DrawModelEntity = gcs.GetFromModelEntityDictionary(main),
                    }
                );
                em.SetComponentData(mainEntity,
                    new DrawInstance.TargetWorkData
                    {
                        DrawInstanceId = -1,
                    }
                );

                em.SetComponentData(mainEntity,
                    new Translation
                    {
                        Value = float3.zero,
                    }
                );
                em.SetComponentData(mainEntity,
                    new Rotation
                    {
                        Value = quaternion.identity,
                    }
                );
                em.SetComponentData(mainEntity,
                    new NonUniformScale
                    {
                        Value = new float3(1.0f, 1.0f, 1.0f),
                    }
                ); ;
            }

        }


    }


}
