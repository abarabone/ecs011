using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace DotsLite.Particle.Aurthoring
{
    using DotsLite.Model;
    using DotsLite.Draw;
    using DotsLite.Model.Authoring;
    using DotsLite.Draw.Authoring;
    using DotsLite.Geometry;

    /// <summary>
    /// �����b�V���Ƃ̃A�g���X�Ή��͌��
    /// </summary>
    public class ParticleModelSourceAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {

        public Shader DrawShader;
        public Texture2D Texture;


        public int2 TextureSize;
        public int2 Division;
        public TextureFormat TextureFormat;
        public bool UseMipmap;
        public bool UseLinear;

        [SerializeField]
        public SrcTexture[] SrcTexutres;
        [Serializable]
        public struct SrcTexture
        {
            public Texture2D texuture;
            public int2 indexOfLeftTop;
            public int2 cellUsage;
        }


        /// <summary>
        /// �p�[�e�B�N�����ʂŎg�p���郂�f���G���e�B�e�B���쐬����B
        /// �ŏI�I�� prefab �R���|�[�l���g���폜����B�i unity �̑z��ƈ���Ęc�݂����肻���c�j
        /// </summary>
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {

            createModelEntity_(conversionSystem, entity, this.gameObject, this.DrawShader, this.createMesh(), this.Texture);

            

            return;


            static void createModelEntity_
                (GameObjectConversionSystem gcs, Entity entity, GameObject main, Shader shader, Mesh mesh, Texture tex)
            {
                var mat = new Material(shader);
                mat.mainTexture = tex;

                const BoneType BoneType = BoneType.P1p;
                const int boneLength = 1;

                gcs.InitDrawModelEntityComponents(main, entity, mesh, mat, BoneType, boneLength);
            }

            void addParamComponents_(GameObjectConversionSystem gcs, Entity ent)
            {

            }

            void packTexture_()
            {

                var renderTexture = new RenderTexture(this.TextureSize.x, this.TextureSize.y, 32);


                var q =
                    from src in this.SrcTexutres
                    let top = 
                    let left =
                    
                Graphics.Blit(source, renderTexture);



                RenderTexture.active = renderTexture;

                // RenderTexture.active�̓��e��texture�ɏ�������
                var texture = new Texture2D(this.TextureSize.x, this.TextureSize.y, this.TextureFormat, this.UseMipmap, this.UseLinear);
                texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
                RenderTexture.active = null;

                // �s�v�ɂȂ����̂ō폜
                RenderTexture.DestroyImmediate(renderTexture);

                // png�Ƃ��ĕۑ�
                System.IO.File.WriteAllBytes(savePath, texture.EncodeToPNG());

                AssetDatabase.Refresh();

                // �ۑ��������̂����[�h���Ă���Ԃ�
                return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
            }
        }

        Mesh createMesh()
        {

            float height = 0.5f;// 1.0f;
            float width = 0.5f;// 1.0f;

            Mesh mesh = new Mesh();
            mesh.name = "particle";

            mesh.vertices = new Vector3[]
            {
                new Vector3 (-width, height, 0),     // 0
                new Vector3 (width, height, 0),           // 1
                new Vector3 (-width , -height, 0),     // 2
                new Vector3 (width , -height, 0),           // 3
            };

            mesh.uv = new Vector2[]
            {
                new Vector2 (0, 0),
                new Vector2 (1, 0),
                new Vector2 (0, 1),
                new Vector2 (1, 1),
            };

            mesh.triangles = new int[]
            {
                0, 1, 2,
                1, 3, 2,
            };

            return mesh;
        }

    }


    [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
    public class RemovePrefabComponentsConversion : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        { }
        //{
        //    var em = this.DstEntityManager;

        //    this.Entities
        //        .WithAll<DrawModel.GeometryData, Prefab>()
        //        .ForEach(ent =>
        //        {
        //            em.RemoveComponent<Prefab>(ent);
        //        });
        //}
        protected override void OnDestroy()
        {
            var em = this.DstEntityManager;

            var desc0 = new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(DrawModel.GeometryData),
                    typeof(Prefab)
                }
            };
            using var q = em.CreateEntityQuery(desc0);

            using var ents = q.ToEntityArray(Allocator.Temp);
            foreach (var ent in ents)
            {
                em.RemoveComponent<Prefab>(ent);
            }
        }
    }

}
