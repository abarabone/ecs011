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

namespace Abarabone.Structure.Authoring
{
    using Abarabone.Model;
    using Abarabone.Draw;
    using Abarabone.Model.Authoring;
    using Abarabone.Draw.Authoring;
    using Abarabone.Geometry;
    using Abarabone.Structure.Authoring;
    using Abarabone.Character;//ObjectMain はここにある、名前変えるべきか？

    using Abarabone.Common.Extension;
    using Abarabone.Structure;
    using Unity.Entities.UniversalDelegates;
    using Unity.Properties;
    using System.CodeDom;

    public class StructureBuildingModelAliasAuthoring
        : ModelGroupAuthoring.ModelAuthoringBase, IConvertGameObjectToEntity, IDeclareReferencedPrefabs//
    {

        public StructureBuildingModelAuthoring StructureModelPrefab;


        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            var prefab = this.StructureModelPrefab;//?.MasterPrefab;
            if (prefab == null) return;

            Debug.Log("prefab : " + prefab.name);
            referencedPrefabs.Add(prefab.gameObject);
        }

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var prefab = this.StructureModelPrefab;//?.MasterPrefab;
            if (prefab == null) return;

            Debug.Log("instantiate : " + prefab.name);
            //conversionSystem.CreateStructureEntities(prefab);

            var prefabent = conversionSystem.GetPrimaryEntity(prefab);
            //dstManager.Instantiate(prefabent);
            ////dstManager

            //dstManager.DestroyEntity(entity);

            dstManager.World.GetExistingSystem<InitializationSystemGroup>().PostUpdateCommands.Instantiate(prefabent);
        }

    }
}