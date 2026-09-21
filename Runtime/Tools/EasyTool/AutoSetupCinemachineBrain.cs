using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if CINEMACHINE_3
using Unity.Cinemachine;
#elif CINEMACHINE_2
using Cinemachine;
#endif

public class AutoSetupCinemachineBrain : MonoBehaviour
{
    private void Start()
    {
#if !CINEMACHINE_2 && !CINEMACHINE_3
        Debug.LogError("AutoSetupCinemachineBrain: 未安装 Cinemachine（需要 2.x 或 3.x）。", this);
        return;
#else
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("AutoSetupCinemachineBrain: 未找到 MainCamera。", this);
            return;
        }

        CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
        if (brain == null)
            brain = mainCam.gameObject.AddComponent<CinemachineBrain>();

        var directors = GetComponentsInChildren<PlayableDirector>(true);
        if (directors.Length == 0)
        {
            Debug.LogWarning("AutoSetupCinemachineBrain: 子节点上未找到 PlayableDirector。", this);
            return;
        }

        foreach (var director in directors)
        {
            if (director.playableAsset is not TimelineAsset timelineAsset)
                continue;

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
