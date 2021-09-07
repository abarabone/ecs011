using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Entities;


namespace DotsLite.Structure
{

    public static class StructureUtility
    {





        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeComponentsToNear(
            this DynamicBuffer<LinkedEntityGroup> children,
            EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex, Entity entity,
            ComponentDataFromEntity<Part.PartData> partData)
        {
            cmd.RemoveComponent<Main.FarTag>(uniqueIndex, entity);
            cmd.AddComponent<Main.NearTag>(uniqueIndex, entity);

            cmd.AddComponentToFar<Disabled>(uniqueIndex, children);
            cmd.RemoveComponentFromNearParts<Disabled>(uniqueIndex, children, partData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeComponentsToFar(
            this DynamicBuffer<LinkedEntityGroup> children,
            EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex, Entity entity,
            ComponentDataFromEntity<Part.PartData> partData)
        {
            cmd.RemoveComponent<Main.NearTag>(uniqueIndex, entity);
            cmd.AddComponent<Main.FarTag>(uniqueIndex, entity);

            cmd.RemoveComponentFromFar<Disabled>(uniqueIndex, children);
            cmd.AddComponentToNearParts<Disabled>(uniqueIndex, children, partData);
        }





        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeComponentsToSleep(
            this EntityCommandBuffer.ParallelWriter cmd, Entity entity, int uniqueIndex,
            Main.BinderLinkData binder,
            ComponentDataFromEntity<Part.PartData> parts,
            BufferFromEntity<LinkedEntityGroup> linkedGroups)
        {
            cmd.AddComponent<Main.SleepingTag>(uniqueIndex, entity);
            cmd.RemoveComponent<Unity.Physics.PhysicsVelocity>(uniqueIndex, entity);

            var children = linkedGroups[binder.BinderEntity];

            // transform once only
            cmd.AddComponentToFar<Bone.TransformOnlyOnceTag>(uniqueIndex, children);//, new Bone.TransformOnlyOnceTag { WithoutDisable = true });
            cmd.RemoveComponentFromFar<Disabled>(uniqueIndex, children);

            // transform once only
            cmd.AddAndRemoveComponentsFromNearParts<Bone.TransformOnlyOnceTag, Disabled>(uniqueIndex, children, parts);//, new Bone.TransformOnlyOnceTag { WithoutDisable = true });
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeComponentsToSleepOnFar(
            this EntityCommandBuffer.ParallelWriter cmd, Entity entity, int uniqueIndex,
            Main.BinderLinkData binder,
            ComponentDataFromEntity<Part.PartData> parts,
            BufferFromEntity<LinkedEntityGroup> linkedGroups)
        {
            cmd.AddComponent<Main.SleepingTag>(uniqueIndex, entity);
            cmd.RemoveComponent<Unity.Physics.PhysicsVelocity>(uniqueIndex, entity);

            var children = linkedGroups[binder.BinderEntity];
            
            // change to sleep
            cmd.RemoveComponentFromFar<Model.Bone.TransformTargetTag>(uniqueIndex, children);

            // transform once only
            cmd.AddAndRemoveComponentsFromNearParts<Bone.TransformOnlyOnceTag, Disabled>(uniqueIndex, children, parts);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeComponentsToSleepOnNear(
            this EntityCommandBuffer.ParallelWriter cmd, Entity entity, int uniqueIndex,
            Main.BinderLinkData binder,
            ComponentDataFromEntity<Part.PartData> parts,
            BufferFromEntity<LinkedEntityGroup> linkedGroups)
        {
            cmd.AddComponent<Main.SleepingTag>(uniqueIndex, entity);
            cmd.RemoveComponent<Unity.Physics.PhysicsVelocity>(uniqueIndex, entity);

            var children = linkedGroups[binder.BinderEntity];

            // transform once only
            cmd.AddComponentToFar<Bone.TransformOnlyOnceTag>(uniqueIndex, children);
            cmd.RemoveComponentFromFar<Disabled>(uniqueIndex, children);

            // change to sleep
            cmd.RemoveComponentFromNearParts<Model.Bone.TransformTargetTag>(uniqueIndex, children, parts);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ChangeComponentsToWakeUp(
            this EntityCommandBuffer.ParallelWriter cmd, Entity entity, int uniqueIndex,
            Main.BinderLinkData binder,
            ComponentDataFromEntity<Part.PartData> parts,
            BufferFromEntity<LinkedEntityGroup> linkedGroups)
        {
            cmd.RemoveComponent<Main.SleepingTag>(uniqueIndex, entity);
            cmd.AddComponent<Unity.Physics.PhysicsVelocity>(uniqueIndex, entity);

            var children = linkedGroups[binder.BinderEntity];
            cmd.AddComponentToFar<Model.Bone.TransformTargetTag>(uniqueIndex, children);
            cmd.AddComponentToNearParts<Model.Bone.TransformTargetTag>(uniqueIndex, children, parts);
        }




        ///// <summary>
        ///// far および near parts すべてのエンティティに、コンポーネントを付加する。
        ///// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static void AddComponentsToAllBones<T>
        //    (
        //        this DynamicBuffer<LinkedEntityGroup> children,
        //        EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
        //        ComponentDataFromEntity<Part.PartData> partData
        //    )
        //    where T : struct, IComponentData
        //{

        //    cmd.AddComponent<T>(uniqueIndex, children[2].Value);


        //    for (var i = 3; i < children.Length; i++)
        //    {
        //        var child = children[i].Value;
        //        if (!partData.HasComponent(child)) continue;

        //        cmd.AddComponent<T>(uniqueIndex, children[i].Value);
        //    }
        //}

        ///// <summary>
        ///// far および near parts すべてのエンティティから、コンポーネントを削除する。
        ///// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static void RemoveComponentsToAllBones<T>
        //    (
        //        this DynamicBuffer<LinkedEntityGroup> children,
        //        EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
        //        ComponentDataFromEntity<Part.PartData> partData
        //    )
        //    where T : struct, IComponentData
        //{

        //    cmd.RemoveComponent<T>(uniqueIndex, children[2].Value);


        //    for (var i = 3; i < children.Length; i++)
        //    {
        //        var child = children[i].Value;
        //        if (!partData.HasComponent(child)) continue;

        //        cmd.RemoveComponent<T>(uniqueIndex, children[i].Value);
        //    }
        //}




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponentToFar<T>(
            this EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
            DynamicBuffer<LinkedEntityGroup> children, T component = default)
        where T : struct, IComponentData
        {

            cmd.AddComponent<T>(uniqueIndex, children[2].Value, component);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddComponentToNearParts<T>(
            this EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
            DynamicBuffer<LinkedEntityGroup> children,
            ComponentDataFromEntity<Part.PartData> partData, T component = default)
        where T : struct, IComponentData
        {
            for (var i = 3; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (!partData.HasComponent(child)) continue;

                cmd.AddComponent(uniqueIndex, children[i].Value, component);
            }
        }
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //private static void AddComponentsToNearParts<T0, T1>(
        //    this EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
        //    DynamicBuffer<LinkedEntityGroup> children,
        //    ComponentDataFromEntity<Part.PartData> partData,
        //    T0 component0 = default, T1 component1 = default)
        //where T0 : struct, IComponentData
        //where T1 : struct, IComponentData
        //{
        //    for (var i = 3; i < children.Length; i++)
        //    {
        //        var child = children[i].Value;
        //        if (!partData.HasComponent(child)) continue;

        //        cmd.AddComponent<T0>(uniqueIndex, children[i].Value, component0);
        //        cmd.AddComponent<T1>(uniqueIndex, children[i].Value, component1);
        //    }
        //}




        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentFromFar<T>(
            this EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
            DynamicBuffer<LinkedEntityGroup> children)
        where T : struct, IComponentData
        {

            cmd.RemoveComponent<T>(uniqueIndex, children[2].Value);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentFromNearParts<T>(
            this EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
            DynamicBuffer<LinkedEntityGroup> children,
            ComponentDataFromEntity<Part.PartData> partData)
        where T : struct, IComponentData
        {
            for (var i = 3; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (!partData.HasComponent(child)) continue;

                cmd.RemoveComponent<T>(uniqueIndex, children[i].Value);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveComponentsFromNearParts<T0, T1>(
            this EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
            DynamicBuffer<LinkedEntityGroup> children,
            ComponentDataFromEntity<Part.PartData> partData)
        where T0 : struct, IComponentData
        where T1 : struct, IComponentData
        {
            for (var i = 3; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (!partData.HasComponent(child)) continue;

                cmd.RemoveComponent<T0>(uniqueIndex, children[i].Value);
                cmd.RemoveComponent<T1>(uniqueIndex, children[i].Value);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddAndRemoveComponentsFromNearParts<TAdd, TRemove>(
            this EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
            DynamicBuffer<LinkedEntityGroup> children,
            ComponentDataFromEntity<Part.PartData> partData, TAdd component0 = default)
        where TAdd : struct, IComponentData
        where TRemove : struct, IComponentData
        {
            for (var i = 3; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (!partData.HasComponent(child)) continue;

                cmd.AddComponent(uniqueIndex, children[i].Value, component0);
                cmd.RemoveComponent<TRemove>(uniqueIndex, children[i].Value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AddAddAndRemoveComponentsFromNearParts<TAdd0, TAdd1, TRemove>(
            this EntityCommandBuffer.ParallelWriter cmd, int uniqueIndex,
            DynamicBuffer<LinkedEntityGroup> children,
            ComponentDataFromEntity<Part.PartData> partData, TAdd0 component0 = default, TAdd1 component1 = default)
        where TAdd0 : struct, IComponentData
        where TAdd1 : struct, IComponentData
        where TRemove : struct, IComponentData
        {
            for (var i = 3; i < children.Length; i++)
            {
                var child = children[i].Value;
                if (!partData.HasComponent(child)) continue;

                cmd.AddComponent(uniqueIndex, children[i].Value, component0);
                cmd.AddComponent(uniqueIndex, children[i].Value, component1);
                cmd.RemoveComponent<TRemove>(uniqueIndex, children[i].Value);
            }
        }
    }
}
