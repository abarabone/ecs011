using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.InputSystem;

namespace DotsLite.Character
{
    using DotsLite.Misc;
    using DotsLite.Utilities;
    using DotsLite.SystemGroup;
    using DotsLite.Character;
    using DotsLite.CharacterMotion;
    using DotsLite.Targeting;
    using DotsLite.Dependency;


    // �z���_�[����Z���T�[���N������
    // �Z���T�[�̓C���^�[�o�����Ƃ� WakeupFindTag �����č��G���N������
    // ���G���P�x���s�����炷���� WakeupFindTag ���O���A�L���v�`������������|�[�����O��������
    // �L���v�`���͎��̃C���^�[�o����̍��G�܂ő���
    // ���G�Ɏ��s�����P�[�X���A���G�̂P�P�[�X�Ƃ��ĔC�ӂ̈ʒu���|�[�����O���適������

    //[DisableAutoCreation]
    [UpdateInGroup(typeof(SystemGroup.Presentation.Logic.ObjectLogic))]
    public class TargetSensorWakeupAndCopyPositionSystem : DependencyAccessableSystemBase
    {

        CommandBufferDependency.Sender cmddep;


        protected override void OnCreate()
        {
            this.cmddep = CommandBufferDependency.Sender.Create<BeginInitializationEntityCommandBufferSystem>(this);
        }


        protected override void OnUpdate()
        {
            using var cmdScope = this.cmddep.WithDependencyScope();


            //var sensorLinks = this.GetComponentDataFromEntity<TargetSensor.LinkTargetMainData>(isReadOnly: true);
            var sensorPoss = this.GetComponentDataFromEntity<TargetSensorResponse.PositionData>(isReadOnly: true);
            var disables = this.GetComponentDataFromEntity<Disabled>(isReadOnly: true);

            var cmd = cmdScope.CommandBuffer.AsParallelWriter();

            var currentTime = this.Time.ElapsedTime;

            this.Entities
                .WithBurst()
                //.WithReadOnly(sensorLinks)
                .WithReadOnly(sensorPoss)
                .WithReadOnly(disables)
                .WithNativeDisableContainerSafetyRestriction(sensorPoss)
                .ForEach(
                    (
                        Entity entity, int entityInQueryIndex,
                        ref TargetSensorResponse.PositionData response,
                        ref DynamicBuffer<TargetSensorHolder.SensorNextTimeData> nexts,
                        in DynamicBuffer<TargetSensorHolder.SensorLinkData> links
                    )
                =>
                    {

                        for (var i = 0; i < links.Length; i++)
                        {
                            var link = links[i];
                            var sensor = link.SensorEntity;

                            var nexttime = nexts[i].NextTime;
                            if (currentTime >= nexttime)
                            {

                                cmd.AddComponent<TargetSensor.WakeupFindTag>(entityInQueryIndex, sensor);

                                cmd.AddComponent<TargetSensor.AcqurireTag>(entityInQueryIndex, sensor);


                                nexts[i] = new TargetSensorHolder.SensorNextTimeData
                                {
                                    NextTime = (float)(currentTime + link.Interval),
                                };
                            }
                        }

                        for (var i = 0; i < links.Length; i++)
                        {
                            var link = links[i];
                            var sensor = link.SensorEntity;

                            if (!disables.HasComponent(sensor))
                            {
                                response.Position = sensorPoss[sensor].Position;

                                break;
                            }
                        }

                    }
                )
                .ScheduleParallel();
        }


    }

}

