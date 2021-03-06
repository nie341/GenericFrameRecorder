using UnityEngine;
using UnityEngine.FrameRecorder;

namespace UnityEditor.FrameRecorder
{
    public abstract class RecorderEditor : Editor
    {
        protected SerializedProperty m_Inputs;
        SerializedProperty m_Verbose;
        SerializedProperty m_FrameRateMode;
        SerializedProperty m_FrameRate;
        SerializedProperty m_DurationMode;
        SerializedProperty m_StartFrame;
        SerializedProperty m_EndFrame;
        SerializedProperty m_StartTime;
        SerializedProperty m_EndTime;
        SerializedProperty m_SynchFrameRate;
        SerializedProperty m_CaptureEveryNthFrame;

        protected virtual void OnEnable()
        {
            if (target != null)
            {
                var pf = new PropertyFinder<RecorderSettings>(serializedObject);
                m_Inputs = pf.Find(x => x.m_SourceSettings);
                m_Verbose = pf.Find(x => x.m_Verbose);
                m_FrameRateMode = pf.Find(x => x.m_FrameRateMode);
                m_FrameRate = pf.Find(x => x.m_FrameRate);
                m_DurationMode =  pf.Find(x => x.m_DurationMode);
                m_StartFrame =  pf.Find(x => x.m_StartFrame);
                m_EndFrame =  pf.Find(x => x.m_EndFrame);
                m_StartTime =  pf.Find(x => x.m_StartTime);
                m_EndTime =  pf.Find(x => x.m_EndTime);
                m_SynchFrameRate = pf.Find(x => x.m_SynchFrameRate);
                m_CaptureEveryNthFrame = pf.Find(x => x.m_CaptureEveryNthFrame);
            }
        }

        protected virtual void OnDisable() {}

        protected virtual void Awake() {}

        public bool isValid
        {
            get { return (target as RecorderSettings).isValid; }
        }

        public bool showBounds { get; set; }

        bool m_FoldoutInput = true;
        bool m_FoldoutEncoder = true;
        bool m_FoldoutTime = true;
        bool m_FoldoutBounds = true;
        bool m_FoldoutOutput = true;
        public override void OnInspectorGUI()
        {
            if (target == null)
                return;

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            OnInputGroupGui();
            OnOutputGroupGui();
            OnEncodingGroupGui();
            OnTimeGroupGui();
            OnBoundsGroupGui();
            OnExtraGroupsGui();

            EditorGUILayout.PropertyField( m_Verbose, new GUIContent( "Verbose logging" ) );

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }

        protected void AddSourceSettings(RecorderInputSetting sourceSettings)
        {
            sourceSettings.name = GUID.Generate().ToString();

            AssetDatabase.AddObjectToAsset(sourceSettings, serializedObject.targetObject);
            AssetDatabase.SaveAssets();

            m_Inputs.InsertArrayElementAtIndex(m_Inputs.arraySize);
            var arryItem = m_Inputs.GetArrayElementAtIndex(m_Inputs.arraySize-1);
            arryItem.objectReferenceValue = sourceSettings;
        }

        protected void ChangeInputSettings(int atIndex, RecorderInputSetting newSettings)
        {
            newSettings.name = GUID.Generate().ToString();

            AssetDatabase.AddObjectToAsset(newSettings, serializedObject.targetObject);
            AssetDatabase.SaveAssets();

            var arryItem = m_Inputs.GetArrayElementAtIndex(atIndex);
            UnityHelpers.Destroy(arryItem.objectReferenceValue, true);
            arryItem.objectReferenceValue = newSettings;
        }

        protected void PrepareInitialSources()
        {
            var recSettings = (RecorderSettings)target;
            if (recSettings.m_SourceSettings == null || recSettings.m_SourceSettings.Length == 0)
            {
                var newSettings = recSettings.GetDefaultSourcesSettings();
                foreach (var newSetting in newSettings)
                {
                    AddSourceSettings(newSetting);
                }
            }
        }

        protected virtual void OnInputGui()
        {
            bool multiInputs = m_Inputs.arraySize > 1;
            for( int i = 0; i < m_Inputs.arraySize; i++)
            {
                if (multiInputs)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.Foldout(true, "Input " + (i + 1));
                }
                var arrItem = m_Inputs.GetArrayElementAtIndex(i);
                var editor = Editor.CreateEditor( arrItem.objectReferenceValue );
                if( editor != null)
                    editor.OnInspectorGUI();

                if (multiInputs)
                    EditorGUI.indentLevel--;
            }
        }

        protected virtual void OnOutputGui()
        {
            EditorGUILayout.PropertyField(m_CaptureEveryNthFrame, new GUIContent("Every n'th frame"));
        }

        protected virtual void OnEncodingGui()
        {
        }

        protected virtual void OnTimeGui()
        {
            EditorGUILayout.PropertyField(m_FrameRateMode, new GUIContent("Frame rate mode"));
            var label = m_FrameRateMode.intValue == (int) FrameRateMode.Constant ? "Target fps" : "Max fps";
            EditorGUILayout.PropertyField(m_FrameRate, new GUIContent(label));
            if (m_FrameRateMode.intValue == (int)FrameRateMode.Constant)
            {
                EditorGUILayout.PropertyField(m_SynchFrameRate, new GUIContent("Sync. framerate"));
            }
        }

        protected virtual void OnBounds()
        {
            EditorGUILayout.PropertyField(m_DurationMode, new GUIContent("Recording Duration"));

            ++EditorGUI.indentLevel;
            switch ((DurationMode)m_DurationMode.intValue)
            {
                case DurationMode.Manual:
                {
                    break;
                }
                case DurationMode.SingleFrame:
                {
                    EditorGUILayout.PropertyField(m_StartFrame, new GUIContent("Frame #"));
                    m_EndFrame.intValue = m_StartFrame.intValue;
                    break;
                }
                case DurationMode.FrameInterval:
                {
                    EditorGUILayout.PropertyField(m_StartFrame, new GUIContent("First frame"));
                    EditorGUILayout.PropertyField(m_EndFrame, new GUIContent("Last frame"));
                    break;
                }
                case DurationMode.TimeInterval:
                {
                    EditorGUILayout.PropertyField(m_StartTime, new GUIContent("Start (sec)"));
                    EditorGUILayout.PropertyField(m_EndTime, new GUIContent("End (sec)"));
                    break;
                }
            }
            --EditorGUI.indentLevel;
        }

        protected virtual void OnInputGroupGui()
        {
            m_FoldoutInput = EditorGUILayout.Foldout(m_FoldoutInput, "Input(s)");
            if (m_FoldoutInput)
            {
                PrepareInitialSources();
                ++EditorGUI.indentLevel;
                OnInputGui();
                --EditorGUI.indentLevel;
            }         
        }

        protected virtual void OnOutputGroupGui()
        {
            m_FoldoutOutput = EditorGUILayout.Foldout(m_FoldoutOutput, "Output(s)");
            if (m_FoldoutOutput)
            {
                ++EditorGUI.indentLevel;
                OnOutputGui();
                --EditorGUI.indentLevel;
            }            
        }

        protected virtual void OnEncodingGroupGui()
        {
            m_FoldoutEncoder = EditorGUILayout.Foldout(m_FoldoutEncoder, "Encoding");
            if (m_FoldoutEncoder)
            {
                ++EditorGUI.indentLevel;
                OnEncodingGui();
                --EditorGUI.indentLevel;
            }        
        }

        protected virtual void OnTimeGroupGui()
        {
            m_FoldoutTime = EditorGUILayout.Foldout(m_FoldoutTime, "Time");
            if (m_FoldoutTime)
            {
                ++EditorGUI.indentLevel;
                OnTimeGui();
                --EditorGUI.indentLevel;
            }     
        }

        protected virtual void OnBoundsGroupGui()
        {
            if (showBounds)
            {
                m_FoldoutBounds = EditorGUILayout.Foldout(m_FoldoutBounds, "Bounds / Limits");
                if (m_FoldoutBounds)
                {
                    ++EditorGUI.indentLevel;
                    OnBounds();
                    --EditorGUI.indentLevel;
                }
            }  
        }

        protected virtual void OnExtraGroupsGui()
        {
            // nothing. this is for sub classes...
        }
    }
}
