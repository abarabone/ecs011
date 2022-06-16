using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace DotsLite.LoadPath.Authoring
{
    using DotsLite.Geometry;
    using DotsLite.Model.Authoring;


    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(MeshModelBase), true)]
    public class MeshModelEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //����Inspector������\��
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();

            //�{�^����\��
            if (GUILayout.Button("Generate Source Prefab Key"))
            {
                var q = this.targets
                    .OfType<IMeshModel>();
                
                foreach (var model in q)
                {
                    model.GenerateSourcePrefabKey();

                    EditorUtility.SetDirty(model as MonoBehaviour);
                }

                AssetDatabase.SaveAssets();
            }

            GUILayout.EndHorizontal();
        }
    }
}
