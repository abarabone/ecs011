using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DotsLite.LoadPath.Authoring
{
    using DotsLite.Geometry;
    using DotsLite.Structure.Authoring;


    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(PathAuthoring))]
    public class PathAuthoringInEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //����Inspector������\��
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();

            //�{�^����\��
            if (GUILayout.Button("make path part meshes"))
            {
                var parts = this.targets
                    .OfType<PathAuthoring>()
                    //.SelectMany(st => st.GetComponentsInChildren<StructurePartAuthoring>())
                    //.Select(pt => (pt, PrefabUtility.GetCorrespondingObjectFromOriginalSource(pt.gameObject)))
                    ;
                //foreach (var (pt, masterPrefab) in parts)
                //{
                //    (pt.PartModelOfMasterPrefab as PartModel<UI32, PositionNormalUvVertex>)
                //        .SetObject(masterPrefab ?? pt.gameObject);
                //    Debug.Log($"{pt.name} <- {pt.PartModelOfMasterPrefab.Obj.name}");

                //    EditorUtility.SetDirty(pt);
                //}
                foreach (var pt in parts)
                {
                    pt.CreatePathParts();

                    EditorUtility.SetDirty(pt);
                }

                AssetDatabase.SaveAssets();
            }

            GUILayout.EndHorizontal();
        }
    }
}
