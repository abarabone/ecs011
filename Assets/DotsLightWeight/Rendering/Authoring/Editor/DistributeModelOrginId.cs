using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DotsLite.Model.Authoring
{
    using DotsLite.Geometry;


    [CustomEditor(typeof(ModelGroupAuthoring.ModelAuthoringBase), true)]
    public class DistributeModelOriginId : Editor
    {
        public override void OnInspectorGUI()
        {
            //GUILayout.BeginHorizontal();

            //�{�^����\��
            if (GUILayout.Button("set or get model origin id"))
            {
                var xs =
                    from model in this.targets
                        .OfType<ModelGroupAuthoring.ModelAuthoringBase>()
                    let prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(model)
                    select (model, prefab);

                foreach (var (model, prefab) in xs)
                {
                    Debug.Log(prefab?.SourcePrefabKey);
                    if (prefab != null)
                    {
                        model.SourcePrefabKey = prefab.SourcePrefabKey;
                    }
                    else
                    {
                        model.SetOridinId();
                    }
                    EditorUtility.SetDirty(model);
                }

                AssetDatabase.SaveAssets();
            }

            //GUILayout.EndHorizontal();

            //����Inspector������\��
            base.OnInspectorGUI();
        }
    }
}
