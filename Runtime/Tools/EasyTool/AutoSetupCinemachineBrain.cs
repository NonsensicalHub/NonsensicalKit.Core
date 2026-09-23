using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_6000_0_OR_NEWER
using Unity.Cinemachine;
#else
using Cinemachine;
#endif

public class AutoSetupCinemachineBrain : MonoBehaviour
{
    private void Start()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("AutoSetupCinemachineBrain: 未找到 MainCamera。", this);
            return;
        }

#if UNITY_6000_0_OR_NEWER
        // Unity 6+ / Cinemachine 3.x
        var manager = mainCam.GetComponent<CinemachineCameraManager>();
        if (manager == null)
            manager = mainCam.gameObject.AddComponent<CinemachineCameraManager>();

        var directors = GetComponentsInChildren<PlayableDirector>(true);
        foreach (var director in directors)
        {
            if (director.playableAsset is not TimelineAsset timelineAsset) continue;
            foreach (var track in timelineAsset.GetOutputTracks())
            {
                if (track is CinemachineTrack)
                {
                    director.SetGenericBinding(track, manager);
                    break;
                }
            }
        }
#else
        // Unity 2022 及以下 / Cinemachine 2.x
        var brain = mainCam.GetComponent<CinemachineBrain>();
        if (brain == null)
            brain = mainCam.gameObject.AddComponent<CinemachineBrain>();

        var directors = GetComponentsInChildren<PlayableDirector>(true);
        foreach (var director in directors)
        {
            if (director.playableAsset is not TimelineAsset timelineAsset) continue;
            foreach (var track in timelineAsset.GetOutputTracks())
            {
                if (track is CinemachineTrack)
                {
                    director.SetGenericBinding(track, brain);
                    break;
                }
            }
        }
#endif
    }
}
