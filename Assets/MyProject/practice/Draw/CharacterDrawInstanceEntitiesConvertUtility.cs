﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Abarabone.Draw.Authoring
{
    using Motion;
    using Draw;
    using Character;
    using Abarabone.Authoring;
    using Abarabone.Model.Authoring;

    using Abarabone.Common.Extension;
    using Abarabone.Misc;
    using Abarabone.Utilities;

    static public class CharacterDrawInstanceEntitiesConvertUtility
    {

        static public void CreateDrawInstanceEntities
            ( this GameObjectConversionSystem gcs, GameObject topGameObject, Transform[] bones )
        {

            var em = gcs.DstEntityManager;

            var drawInstanceEntity = createDrawInstanceEntity( gcs, topGameObject, bones.Length );

            setBoneComponentValues( gcs, bones, drawInstanceEntity );

            setStreamComponentValues( gcs, bones, drawInstanceEntity );

        }


        // ----------------------------------------------------------------------------------

        
        static public Entity createDrawInstanceEntity
            ( GameObjectConversionSystem gcs, GameObject top, int boneLength )
        {
            var em = gcs.DstEntityManager;


            var archetype = em.CreateArchetype
            (
                typeof( DrawInstanceModeLinkData ),
                typeof( DrawInstanceTargetWorkData )
            );
            var ent = gcs.CreateAdditionalEntity( top, archetype );
            
            em.SetName(ent, $"{top.name} draw" );


            em.SetComponentData( ent,
                new DrawInstanceModeLinkData
                {
                    DrawModelEntity = gcs.GetFromModelEntityDictionary( top ),
                }
            );

            em.SetComponentData( ent,
                new DrawInstanceTargetWorkData
                {
                    DrawInstanceId = -1,
                }
            );


            return ent;
        }


        static void setBoneComponentValues
            (
                GameObjectConversionSystem gcs,
                Transform[] bones, Entity drawInstanceEntity
            )
        {
            var em = gcs.DstEntityManager;

            var boneEntities = bones
                .Select( bone => gcs.GetPrimaryEntity( bone ) )
                .ToArray();

            addComponents_( em, boneEntities );
            setDrawComponet_( em, boneEntities, drawInstanceEntity );
            setBoneId_( em, boneEntities );

            return;


            void addComponents_( EntityManager em_, IEnumerable<Entity> boneEntities_ )
            {
                var addtypes = new ComponentTypes
                (
                    typeof( DrawTransformLinkData ),
                    typeof( DrawTransformIndexData ),
                    typeof( DrawTransformTargetWorkData )
                );

                em.AddComponents( boneEntities_, addtypes );
            }


            void setDrawComponet_
                ( EntityManager em_, IEnumerable<Entity> boneEntities_, Entity drawInstanceEntity_ )
            {
                var drawModelLinker = em_.GetComponentData<DrawInstanceModeLinkData>( drawInstanceEntity_ );

                em_.SetComponentData(
                    boneEntities_,
                    new DrawTransformLinkData
                    {
                        DrawInstanceEntity = drawInstanceEntity_,
                        DrawModelEntity = drawModelLinker.DrawModelEntity,
                    }
                );
            }

            void setBoneId_
                ( EntityManager em_, IEnumerable<Entity> boneEntities_ )
            {
                var boneLength = boneEntities_.Count();

                em_.SetComponentData( boneEntities_,
                    from i in Enumerable.Range( 0, boneLength )
                    select new DrawTransformIndexData { BoneLength = boneLength, BoneId = i }
                );
            }
        }


        static void setStreamComponentValues
            (
                GameObjectConversionSystem gcs,
                Transform[] bones, Entity drawInstanceEntity
            )
        {

            var em = gcs.DstEntityManager;

            var qStreamEntity =
                from bone in bones
                from ent in gcs.GetEntities( bone )
                where em.HasComponent<StreamRelationData>(ent)
                select ent
                ;

            em.AddComponentData( qStreamEntity, new StreamDrawLinkData { DrawEntity = drawInstanceEntity } );

        }


    }

}
