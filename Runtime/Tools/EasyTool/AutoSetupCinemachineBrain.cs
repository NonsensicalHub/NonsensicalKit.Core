using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 启动时确保 MainCamera 有 CinemachineBrain，并把子节点上所有
/// Timeline 的 Cinemachine Track 绑定到该 Brain。
/// 兼容 Cinemachine 2.x（命名空间 Cinemachine）与 3.x（Unity.Cinemachine）。
/// </summary>
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

#if CINEMACHINE_3
        var brain = mainCam.GetComponent<Unity.Cinemachine.CinemachineBrain>();
        if (brain == null)
            brain = mainCam.gameObject.AddComponent<Unity.Cinemachine.CinemachineBrain>();
#elif CINEMACHINE_2
        var brain = mainCam.GetComponent<Cinemachine.CinemachineBrain>();
        if (brain == null)
            brain = mainCam.gameObject.AddComponent<Cinemachine.CinemachineBrain>();
#endif

        var directors = GetComponentsInChildren<PlayableDirector>(true);
        if (directors.Length == 0)
        {
            Debug.LogWarning("AutoSetupCinemachineBrain: 子节点上未找到 PlayableDirector。", this);
            return;
        }

        foreach (var director in directors)
        {
            var timelineAsset = director.playableAsset as TimelineAsset;
            if (timelineAsset == null)
                continue;

            foreach (var track in timelineAsset.GetOutputTracks())
            {
#if CINEMACHINE_3
                if (track is Unity.Cinemachine.CinemachineTrack)
                    director.SetGenericBinding(track, brain);
#elif CINEMACHINE_2
                if (track is Cinemachine.CinemachineTrack)
                    director.SetGenericBinding(track, brain);
#endif
            }
        }
#endif
    }
}
