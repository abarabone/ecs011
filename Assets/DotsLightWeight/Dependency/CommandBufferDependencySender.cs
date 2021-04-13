using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

namespace Abarabone.Dependency
{
    public static partial class CommandBufferDependency
    {

        public struct Sender
        {

            EntityCommandBufferSystem commandSystem;
            DependencyAccessableSystemBase dependentSystem;


            public static Sender Create<TEntityCommandBufferSystem>(DependencyAccessableSystemBase system)
                where TEntityCommandBufferSystem : EntityCommandBufferSystem
            =>
                new Sender
                {
                    commandSystem = system.World.GetExistingSystem<TEntityCommandBufferSystem>(),
                    dependentSystem = system,
                };



            public EntityCommandBuffer CreateCommandBuffer() =>
                this.commandSystem.CreateCommandBuffer();


            public DisposableDependency WithDependencyScope()
            {
                return new DisposableDependency { parent = this };
            }


            public struct DisposableDependency : IDisposable
            {
                public Sender parent;

                public void Dispose()
                {
                    var cmd = this.parent.commandSystem;
                    var dep = this.parent.dependentSystem.GetOutputDependency();
                    cmd.AddJobHandleForProducer(dep);
                }
            }
        }

    }
}

