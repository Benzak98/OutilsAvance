using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AnimWindowEditor : EditorWindow
{
    bool groupEnabled;
    bool animationClipWithSlider = false;
    float _lastEditorTime = 0f;
    List<float> _listSpeedAnimation = new List<float>();
    List<float> _samplerAnimation = new List<float>();
    List<Animator> animatorList = new List<Animator>();
    List<bool> animatorBoolList = new List<bool>();
    Dictionary<AnimationClip, bool> clipDico = new Dictionary<AnimationClip, bool>();
    AnimationClip currentClip = null;


    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/CustomEditor")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        AnimWindowEditor window = (AnimWindowEditor)EditorWindow.GetWindow(typeof(AnimWindowEditor));
        window.Show();
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += _OnPlayModeStateChange;
    }
    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= _OnPlayModeStateChange;
        StopAnimSimulation();
    }

    private void _OnPlayModeStateChange(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
            StopAnimSimulation();
    }

    void OnGUI()
    {
        //GUI.backgroundColor = Color.white;
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        
        if (GUILayout.Button("GenerateList"))
            GenerateList();

        for (int i = 0; i < animatorList.Count; i++)
        {
            AnimationClip[] animationClips = animatorList[i].runtimeAnimatorController.animationClips;
            
            foreach (AnimationClip clip in animationClips)
            {
                if(!clipDico.ContainsKey(clip))
                    clipDico.Add(clip, false);
            }

            if (GUILayout.Button(animatorList[i].name))
            {
                SelectGameObject(animatorList[i]);
                animatorBoolList[i] = !animatorBoolList[i];
            }
            if (animatorBoolList[i])
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < animationClips.Length; j++)
                {
                    if (GUILayout.Button(animationClips[j].name))
                    {
                        if (!clipDico[animationClips[j]])
                        {
                            StartAnimSimulation(animationClips[j]);
                            currentClip = animationClips[j];
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                _listSpeedAnimation[i] = EditorGUILayout.Slider(_listSpeedAnimation[i], 0f, 10f);
                if (currentClip != null)
                    _samplerAnimation[i] = EditorGUILayout.Slider(_samplerAnimation[i], 0f, currentClip.length);

                animationClipWithSlider = EditorGUILayout.BeginToggleGroup("ActiveSlider", animationClipWithSlider);
                EditorGUILayout.EndToggleGroup();

            }
        }
    }

    void GenerateList()
    {
        AnimationMode.StartAnimationMode();
        animatorList.Clear();
        animatorBoolList.Clear();
        Animator[] allAnimators = FindObjectsOfType<Animator>();
        foreach (Animator animator in allAnimators)
        {
            animatorList.Add(animator);
            animatorBoolList.Add(false);
            _listSpeedAnimation.Add(0f);
            _samplerAnimation.Add(0f);
        }
    }

    void SelectGameObject(Animator animator)
    {
        Selection.activeGameObject = animator.gameObject;
    }

    void OnEditorUpdate()
    {
        for (int i = 0; i < animatorList.Count; i++)
        {
            if (animatorList[i] == null)
                return;

            foreach (AnimationClip clip in animatorList[i].runtimeAnimatorController.animationClips)
            {
                if (clipDico[clip])
                {
                    if (!animationClipWithSlider)
                    {
                        float animTime = (Time.realtimeSinceStartup - _lastEditorTime) * _listSpeedAnimation[i];
                        if (animTime >= clip.length)
                            StopAnimSimulation(clip);
                        else
                        {
                            if (AnimationMode.InAnimationMode())
                                AnimationMode.SampleAnimationClip(animatorList[i].gameObject, clip, animTime);
                        }
                    }
                    else
                    {
                        float animTime = _samplerAnimation[i];
                        if (animTime >= clip.length)
                            StopAnimSimulation(clip);
                        else
                        {
                            if (AnimationMode.InAnimationMode())
                                AnimationMode.SampleAnimationClip(animatorList[i].gameObject, clip, animTime);
                        }
                    }
                   
                }
            }
        }
    }

    public void StartAnimSimulation(AnimationClip clip)
    {
        AnimationMode.StartAnimationMode();
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
        _lastEditorTime = Time.realtimeSinceStartup;
        clipDico[clip] = true;
    }

    public void StopAnimSimulation(AnimationClip clip)
    {
        AnimationMode.StopAnimationMode();
        EditorApplication.update -= OnEditorUpdate;
        clipDico[clip] = false;
    }

    public void StopAnimSimulation()
    {
        AnimationMode.StopAnimationMode();
        EditorApplication.update -= OnEditorUpdate;
    }
}
