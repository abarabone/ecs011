﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Linq;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;
using Abss.Charactor;

namespace Abss.Arthuring
{

    public class CharactorAuthoring : PrefabSettingsAuthoring.ConvertToMainCustomPrefabEntityBehaviour
    {




        public override Entity Convert( EntityManager em, PrefabSettingsAuthoring.PrefabCreators creators )
        {

            var motionAuthor = this.GetComponent<MotionAuthoring>();
            var motionPrefab = motionAuthor.Convert( em, creators );

            return creators.Character.CreatePrefab( em, motionPrefab );
        }

    }


    public class CharactorPrefabCreator
    {
        
        EntityArchetype charactorPrefabArchetype;



        public CharactorPrefabCreator( EntityManager em )
        {

            this.charactorPrefabArchetype = em.CreateArchetype
            (
                typeof( LinkedEntityGroup ),
                typeof( Prefab )
            );

        }


        public Entity CreatePrefab( EntityManager em, Entity motionPrefab )
        {

            var chArchetype = this.charactorPrefabArchetype;

            var prefab = em.CreateEntity( chArchetype );
            var links = em.GetBuffer<LinkedEntityGroup>( prefab );
            
            links.Add( new LinkedEntityGroup { Value = prefab } );
            links.Add( new LinkedEntityGroup { Value = motionPrefab } );

            return prefab;
        }

    }

}

