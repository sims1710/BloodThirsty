using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public enum PlayerState
{
    IDLE,
    MOVE,
    ATTACK,
    DAMAGED,
    DEBUFF,
    DEATH,
    OTHER,
}

public class SPUM_Prefabs : MonoBehaviour
{
    public float _version;
    public bool EditChk;
    public string _code;
    public Animator _anim;

    public Dictionary<string, List<AnimationClip>> StateAnimationPairs = new Dictionary<string, List<AnimationClip>>();

    public string UnitType;
    public List<SpumPackage> spumPackages = new List<SpumPackage>();
    public List<PreviewMatchingElement> ImageElement = new();
    public List<SPUM_AnimationData> SpumAnimationData = new();

    public List<AnimationClip> IDLE_List = new();
    public List<AnimationClip> MOVE_List = new();
    public List<AnimationClip> ATTACK_List = new();
    public List<AnimationClip> DAMAGED_List = new();
    public List<AnimationClip> DEBUFF_List = new();
    public List<AnimationClip> DEATH_List = new();
    public List<AnimationClip> OTHER_List = new();

    public void OverrideControllerInit()
    {
        foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
        {
            var stateText = state.ToString();
            switch (state)
            {
                case PlayerState.IDLE:
                    StateAnimationPairs[stateText] = IDLE_List;
                    break;
                case PlayerState.MOVE:
                    StateAnimationPairs[stateText] = MOVE_List;
                    break;
                case PlayerState.ATTACK:
                    StateAnimationPairs[stateText] = ATTACK_List;
                    break;
                case PlayerState.DAMAGED:
                    StateAnimationPairs[stateText] = DAMAGED_List;
                    break;
                case PlayerState.DEBUFF:
                    StateAnimationPairs[stateText] = DEBUFF_List;
                    break;
                case PlayerState.DEATH:
                    StateAnimationPairs[stateText] = DEATH_List;
                    break;
                case PlayerState.OTHER:
                    StateAnimationPairs[stateText] = OTHER_List;
                    break;
            }
        }
    }

    public bool allListsHaveItemsExist()
    {
        List<List<AnimationClip>> allLists = new List<List<AnimationClip>>()
        {
            IDLE_List, MOVE_List, ATTACK_List, DAMAGED_List, DEBUFF_List, DEATH_List, OTHER_List
        };

        return allLists.All(list => list.Count > 0);
    }

    [ContextMenu("PopulateAnimationLists")]
    public void PopulateAnimationLists()
    {
        IDLE_List = new();
        MOVE_List = new();
        ATTACK_List = new();
        DAMAGED_List = new();
        DEBUFF_List = new();
        DEATH_List = new();
        OTHER_List = new();

        var groupedClips = spumPackages
        .SelectMany(package => package.SpumAnimationData)
        .Where(spumClip => spumClip.HasData &&
                        spumClip.UnitType.Equals(UnitType) &&
                        spumClip.index > -1)
        .GroupBy(spumClip => spumClip.StateType)
        .ToDictionary(
            group => group.Key,
            group => group.OrderBy(clip => clip.index).ToList()
        );

        foreach (var kvp in groupedClips)
        {
            var stateType = kvp.Key;
            var orderedClips = kvp.Value;
            switch (stateType)
            {
                case "IDLE":
                    IDLE_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "MOVE":
                    MOVE_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "ATTACK":
                    ATTACK_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "DAMAGED":
                    DAMAGED_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "DEBUFF":
                    DEBUFF_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "DEATH":
                    DEATH_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
                case "OTHER":
                    OTHER_List.AddRange(orderedClips.Select(clip => LoadAnimationClip(clip.ClipPath)));
                    break;
            }
        }

        OverrideControllerInit();
    }

    public void PlayAnimation(PlayerState PlayState, int index)
    {
        if (_anim == null)
        {
            Debug.LogError("No animator component found!");
            return;
        }

        string stateStr = PlayState.ToString();
        List<AnimationClip> animList = StateAnimationPairs[stateStr];

        if (animList == null || animList.Count == 0)
        {
            Debug.LogWarning($"No animations found for state {PlayState}");
            return;
        }

        int safeIndex = Mathf.Clamp(index, 0, animList.Count - 1);

        if (animList[safeIndex] == null)
        {
            Debug.LogWarning($"Animation at index {safeIndex} for state {PlayState} is null!");
            return;
        }

        bool isMove = stateStr.Contains("MOVE");
        bool isDebuff = stateStr.Contains("DEBUFF");
        bool isDeath = stateStr.Contains("DEATH");
        _anim.SetBool("1_Move", isMove);
        _anim.SetBool("5_Debuff", isDebuff);
        _anim.SetBool("isDeath", isDeath);

        if (!isMove && !isDebuff)
        {
            AnimatorControllerParameter[] parameters = _anim.parameters;
            foreach (AnimatorControllerParameter parameter in parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Trigger)
                {
                    bool isTrigger = parameter.name.ToUpper().Contains(stateStr.ToUpper());
                    if (isTrigger)
                    {
                        _anim.SetTrigger(parameter.name);
                    }
                }
            }
        }
    }

    AnimationClip LoadAnimationClip(string clipPath)
    {
        AnimationClip clip = Resources.Load<AnimationClip>(clipPath.Replace(".anim", ""));

        if (clip == null)
        {
            Debug.LogWarning($"Failed to load animation clip '{clipPath}'.");
        }

        return clip;
    }
}
