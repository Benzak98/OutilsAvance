using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AnimWindowEditor : EditorWindow
{
    bool groupEnabled;
    double globalTimer = 0f;
    List<Animator> animatorList = new List<Animator>();
    List<bool> boolList = new List<bool>();

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/CustomEditor")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        AnimWindowEditor window = (AnimWindowEditor)EditorWindow.GetWindow(typeof(AnimWindowEditor));
        window.Show();
        AnimationMode.StartAnimationMode();
    }

    private void OnEnable()
    {
        EditorApplication.update += AnimationUpdate;
    }
    private void OnDisable()
    {
        EditorApplication.update -= AnimationUpdate;
    }

    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        
        if (GUILayout.Button("GenerateList"))
            GenerateList();

        groupEnabled = EditorGUILayout.BeginToggleGroup("AnimatorList", groupEnabled);
        
        for (int i = 0; i < animatorList.Count; i++)
        {
            AnimationClip[] animationClips = animatorList[i].runtimeAnimatorController.animationClips;
            if (GUILayout.Button(animatorList[i].name))
            {
                SelectGameObject(animatorList[i]);
                boolList[i] = !boolList[i];
            }
            if (boolList[i])
            {
                EditorGUILayout.BeginHorizontal();
                foreach (AnimationClip clip in animationClips)
                {
                    if (GUILayout.Button(clip.name))
                        PlayAndStopAnim();
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndToggleGroup();
    }

    void GenerateList()
    {
        AnimationMode.StopAnimationMode();
        animatorList.Clear();
        boolList.Clear();
        Animator[] allAnimators = FindObjectsOfType<Animator>();
        foreach (Animator animator in allAnimators)
        {
            animatorList.Add(animator);
            boolList.Add(false);
        }
    }

    void SelectGameObject(Animator animator)
    {
        Selection.activeGameObject = animator.gameObject;
    }

    void PlayAndStopAnim()
    {
        
    }

    void AnimationUpdate()
    {
        globalTimer += EditorApplication.timeSinceStartup;
        
    }

}
