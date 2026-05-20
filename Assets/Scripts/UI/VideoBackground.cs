using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

[RequireComponent(typeof(RawImage))]
public class VideoBackground : MonoBehaviour
{
    [Header("Video Clip")]
    public VideoClip videoClip;
    
    [Header("Settings")]
    public bool loop = true;
    public bool playOnAwake = true;

    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private RenderTexture renderTexture;

    void Awake()
    {
        rawImage = GetComponent<RawImage>();
        
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.loop = loop;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        renderTexture = new RenderTexture(1920, 1080, 0);
        renderTexture.Create();
        videoPlayer.targetTexture = renderTexture;
        rawImage.texture = renderTexture;

        if (videoClip != null && playOnAwake)
        {
            videoPlayer.clip = videoClip;
            videoPlayer.Play();
        }
    }

    void Start()
    {
        if (videoClip != null && videoPlayer.clip == null)
        {
            videoPlayer.clip = videoClip;
            videoPlayer.Play();
        }
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}
