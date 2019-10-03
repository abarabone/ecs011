﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

using Abss.Geometry;
using Abss.Utilities;
using Abss.Misc;
using Abss.Motion;
using Abss.Draw;

namespace Abss.Arthuring
{
    
    public class CharacterGroupArthuring : MonoBehaviour
    {
        
        public CharactorResourceUnit[] Resources;


        MotionPrefabHolder prefabs;

        List<Entity> ents = new List<Entity>();



        void Awake()
        {
            //passResourcesToDrawSystem();
            var w = World.Active;
            var em = w.EntityManager;

            this.prefabs = new MotionPrefabHolder( em, this.Resources );
            
            var dat = this.motionPrefabDatas[0];
            var ent = em.Instantiate( dat.Prefab );

            this.ents.Add( ent );
        }


        private void Update()
        {
            if( !Input.GetMouseButtonDown(0) ) return;
            
            foreach( var x in this.ents ) World.Active.EntityManager.DestroyEntity(x);
        }


        private void OnDisable()
        {
            this.prefabs.Dispose();
        }


        [Serializable]
        public struct CharactorResourceUnit
        {
            public Mesh[] SkinnedMesh;
            public Material Material;
            public MotionClip MotionClip;
        }





        void passResourcesToDrawSystem()
        {

            foreach( var x in this.Resources.Select((res,id)=>(id,res)) )
            {
                
            }
        }
        static DrawMeshResourceUnit createRenderingUnit( MotionClip motionClip )
        {
            return new DrawMeshResourceUnit();
        }

        





        class MotionPrefabHolder : IDisposable
        {

            public MotionPrefabUnit[] MotionPrefabResources { get; private set; }

            PrefabArchetypes prefabArchetypes;



            public MotionPrefabHolder( EntityManager em, CharactorResourceUnit[] resources )
            {
                var qPrefabs =
                    from x in resources.Select( ( res, id ) => (id, res) )
                    let motionClipData = x.res.MotionClip.ConvertToBlobData()
                    select new MotionPrefabUnit
                    {
                        Prefab = createMotionPrefab( em, motionClipData, this.prefabArchetypes ),
                        MotionClipData = motionClipData,
                    }
                    ;

                this.MotionPrefabResources = qPrefabs.ToArray();
            }
            static Entity createMotionPrefab
                ( EntityManager em, BlobAssetReference<MotionBlobData> motionClipData, PrefabArchetypes prefabArchetypes )
            {
                // モーションエンティティ生成
                var motionEntity = em.CreateEntity( prefabArchetypes.Motion );
                em.SetComponentData( motionEntity,
                    new MotionDataData
                    {
                        ClipData = motionClipData
                    }
                );

                // ストリームエンティティ生成
                var streamEntities = new NativeArray<Entity>( motionClipData.Value.BoneParents.Length * 2, Allocator.Temp );
                em.CreateEntity( prefabArchetypes.MotionStream, streamEntities );

                // リンク生成
                var linkedEntityGroup = streamEntities
                    .Select( streamEntity => new LinkedEntityGroup { Value = streamEntity } )
                    .Prepend( new LinkedEntityGroup { Value = motionEntity } )
                    .ToNativeArray( Allocator.Temp );

                // バッファに追加
                var mbuf = em.AddBuffer<LinkedEntityGroup>( motionEntity );
                mbuf.AddRange( linkedEntityGroup );

                // 一時領域破棄
                streamEntities.Dispose();
                linkedEntityGroup.Dispose();

                return motionEntity;
            }

            public void Dispose()
            {
                //this.motionPrefabDatas.Do( x => x.Dispose() );// .Do() が機能してない？？
                foreach( var x in this.MotionPrefabResources )
                    x.Dispose();
            }



            public struct MotionPrefabUnit : IDisposable
            {
                public Entity Prefab;
                public BlobAssetReference<MotionBlobData> MotionClipData;

                public void Dispose() => this.MotionClipData.Dispose();
            }

            class PrefabArchetypes
            {

                public readonly EntityArchetype Motion;
                public readonly EntityArchetype MotionStream;


                public PrefabArchetypes( EntityManager em )
                {
                    this.Motion = em.CreateArchetype
                    (
                        typeof( MotionInfoData ),
                        typeof( MotionDataData ),
                        typeof( MotionInitializeData ),
                        typeof( LinkedEntityGroup ),
                        typeof( Prefab )
                    );
                    this.MotionStream = em.CreateArchetype
                    (
                        typeof( StreamKeyShiftData ),
                        typeof( StreamNearKeysCacheData ),
                        typeof( StreamTimeProgressData ),
                        typeof( Prefab )
                    );
                }
            }
        }





    }




}


