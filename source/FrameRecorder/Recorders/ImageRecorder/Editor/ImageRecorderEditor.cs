using System;
using UnityEngine;
using UnityEngine.FrameRecorder;
using UnityEngine.FrameRecorder.Input;

namespace UnityEditor.FrameRecorder
{
    [CustomEditor(typeof(ImageRecorderSettings))]
    public class ImagRecorderEditor : RecorderEditor
    {
        SerializedProperty m_DestinationPath;
        SerializedProperty m_BaseFileName;
        SerializedProperty m_OutputFormat;

        string[] m_Candidates;
        
        [MenuItem("Window/Recorder/Video...")]
        static void ShowRecorderWindow()
        {
            RecorderWindow.ShowAndPreselectCategory("Video");
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (target == null)
                return;

            m_Candidates = new [] { "Command Buffered Camera", "Offscreen rendering", "Render Texture" };
            var pf = new PropertyFinder<ImageRecorderSettings>(serializedObject);
            m_Inputs = pf.Find(w => w.m_SourceSettings);
            m_DestinationPath = pf.Find(w => w.m_DestinationPath);
            m_BaseFileName = pf.Find(w => w.m_BaseFileName);
            m_OutputFormat = pf.Find(w => w.m_OutputFormat);
        }

        protected override void OnEncodingGroupGui()
        {
            // hiding this group by not calling parent class's implementation.  
        }

        protected override void OnInputGui()
        {
            var input = m_Inputs.GetArrayElementAtIndex(0).objectReferenceValue;

            var index = input.GetType() == typeof(CBRenderTextureInputSettings) ? 0 :
                        input.GetType() == typeof(AdamBeautyInputSettings) ? 1 : 2; 
            var newIndex = EditorGUILayout.Popup("Image Generator", index, m_Candidates);

            if (index != newIndex)
            {
                Type newType = null;
                switch (newIndex)
                {
                    case 0:
                        newType = typeof(CBRenderTextureInputSettings);
                        break;
                    case 1:
                        newType = typeof(AdamBeautyInputSettings);
                        break;
                    case 2:
                        newType = typeof(RenderTextureInputSettings);
                        break;
                }
                var newSettings = ScriptableObject.CreateInstance(newType) as RecorderInputSetting;
                if( newIndex == 0 )
                    (newSettings as CBRenderTextureInputSettings).m_FlipVertical = true;

                ChangeInputSettings(0, newSettings);
            }

            base.OnInputGui();
        }

        protected override void OnOutputGui()
        {
            EditorGUILayout.PropertyField(m_OutputFormat, new GUIContent("Output format"));

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Directory");
            m_DestinationPath.stringValue = EditorGUILayout.TextField(m_DestinationPath.stringValue);
            if (GUILayout.Button("...", GUILayout.Width(30)))
                m_DestinationPath.stringValue = EditorUtility.OpenFolderPanel( "Select output location", m_DestinationPath.stringValue, "");
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(m_BaseFileName, new GUIContent("File name"));

            base.OnOutputGui();
        }
    }
}
