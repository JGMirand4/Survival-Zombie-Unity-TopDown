using UnityEngine;
using UnityEngine.Video;

public class VideoWebFixo : MonoBehaviour
{
    void Start()
    {
        // Se o jogo estiver rodando NA INTERNET (GitHub Pages)...
        #if UNITY_WEBGL && !UNITY_EDITOR
            VideoPlayer vp = GetComponent<VideoPlayer>();
            vp.url = Application.streamingAssetsPath + "/background.mp4";
            vp.Play();
        #endif

        // Se estiver rodando no Editor, ele simplesmente ignora e não faz nada, 
        // evitando aquele erro vermelho chato no console!
    }
}