
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
//using Microsoft.CSharp.RuntimeBinder;
using Unity.Entities.UniversalDelegates;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.CompilerServices;
using UnityEngine.XR;
using Unity.Physics;
using Unity.Physics.Systems;

namespace Abarabone.Common
{

    public abstract class PhysicsHitSystemBase<TEntityCommandBufferSystem> : CommandSystemBase<TEntityCommandBufferSystem>
        where TEntityCommandBufferSystem : EntityCommandBufferSystem
    {

        BuildPhysicsWorld buildPhysicsWorldSystem;// シミュレーショングループ内でないと実行時エラーになるみたい


        protected override void OnCreate()
        {
            base.OnCreate();

            this.buildPhysicsWorldSystem = this.World.GetExistingSystem<BuildPhysicsWorld>();
        }
        
        protected override void OnUpdate()
        {
            this.Dependency = JobHandle.CombineDependencies
                (this.Dependency, this.buildPhysicsWorldSystem.GetOutputDependency());


            base.OnUpdate();


            this.buildPhysicsWorldSystem.AddInputDependencyToComplete(this.Dependency);
        }

        //protected abstract void OnUpdateWithCommandBuffer(EntityCommandBuffer commandBuffer);

    }
}
