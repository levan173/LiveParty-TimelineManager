using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 过场动画管理器 功能包括如下
/// 播放过场动画，暂停过场动画，提前结束过场动画
/// 创建人 黎凡
/// </summary>
[RequireComponent(typeof(PlayableDirector))]
public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance;

    public PlayableDirector mainDirector;

    private List<PlayableDirector> allDirectors = new List<PlayableDirector>();

    [Header("测试功能的动画")]
    public List<TimelineAsset> testTimeline;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        mainDirector = GetComponent<PlayableDirector>();
    }


    /// <summary>
    /// 将主Director中的Binding复制到新生成的Director中
    /// </summary>
    /// <param name="timelineAsset"></param>
    /// <param name="new_playableDirector"></param>
    private void ResetTimelineBinding(TimelineAsset timelineAsset, PlayableDirector new_playableDirector)
    {
        mainDirector.playableAsset = timelineAsset;
        new_playableDirector.playableAsset = timelineAsset;

        List<PlayableBinding> newBindingList = new List<PlayableBinding>();
        List<PlayableBinding> oldBindingList = new List<PlayableBinding>();

        foreach (PlayableBinding pb in mainDirector.playableAsset.outputs)
        {
            //  Debug.Log(pb);
            oldBindingList.Add(pb);
        }

        foreach (PlayableBinding pb in new_playableDirector.playableAsset.outputs)
        {
            // Debug.Log(pb);
            newBindingList.Add(pb);
        }

        new_playableDirector.playableAsset = timelineAsset;

        for (int i = 0; i < oldBindingList.Count; i++)
        {
            new_playableDirector.SetGenericBinding(newBindingList[i].sourceObject, mainDirector.GetGenericBinding(oldBindingList[i].sourceObject));
        }

        mainDirector.playableAsset = null;
    }


    //新建playableDirector
    private PlayableDirector CreateDirector(TimelineAsset asset)
    {
        GameObject gameObj = new GameObject(asset.name);
        PlayableDirector director = gameObj.AddComponent<PlayableDirector>();
        director.extrapolationMode = DirectorWrapMode.None; //初始化
        director.playOnAwake = false;
        allDirectors.Add(director);
        return gameObj.GetComponent<PlayableDirector>();
    }

    /// <summary>
    /// 播放Timeline,可以传入一个方法在timeline播放结束时调用
    /// </summary>
    /// <param name="asset"></param>
    public void PlayTimeline(TimelineAsset asset,Action afterStopFunc = null)
    {
        var director = CreateDirector(asset);
        ResetTimelineBinding(asset, director);
        director.Play();
        if (afterStopFunc != null)
        {
            director.stopped += (x) => { afterStopFunc.Invoke(); };
        }
        director.stopped += (x) =>
        {
            allDirectors.Remove(director);
            Destroy(director.gameObject);
        };
    }

    [Button]
    private void TestPlay()
    {
        foreach (var current in testTimeline)
        {
            PlayTimeline(current,()=> {
                Debug.Log("After timeline stop function triggered!");
            });
        }
    }

    [Button]
    public void PauseAllTimeline()
    {
        foreach(var current in allDirectors)
        {
            current.Pause();
        }
    }

    //暂停指定Timeline
    public void PauseTimeline(TimelineAsset asset)
    {
        foreach (var current in allDirectors)
        {
            if (current.playableAsset.Equals(asset))
            {
                current.Pause();
                break;
            }
        }
    }


    [Button]
    public void ResumeAllTimeline()
    {
        foreach(var current in allDirectors)
        {
            current.Resume();
        }
    }

    //恢复指定Timeline
    public void ResumeTimeline(TimelineAsset asset)
    {
        foreach (var current in allDirectors)
        {
            if (current.playableAsset.Equals(asset))
            {
                current.Resume();
                break;
            }
        }
    }

    [Button]
    public void StopAllTimeline()
    {
        List<PlayableDirector> temp = new List<PlayableDirector>();
        temp.AddRange(allDirectors);
        foreach(var current in temp)
        {
            current.Stop();
        }
    }

    //停下指定Timeline
    public void StopTimeline(TimelineAsset asset)
    {
        List<PlayableDirector> temp = new List<PlayableDirector>();
        temp.AddRange(allDirectors);
        foreach (var current in temp)
        {
            if (current.playableAsset.Equals(asset))
            {
                current.Stop();
                break;
            }
        }
    }

}
