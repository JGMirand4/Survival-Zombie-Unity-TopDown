using UnityEngine;
using Unity.Cinemachine; // (Ou 'using Cinemachine;' dependendo da sua versão)
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineBasicMultiChannelPerlin noise;

    private void Awake()
    {
        Instance = this;
        noise = GetComponent<CinemachineBasicMultiChannelPerlin>();
        
        // Garante que o jogo comece sem tremer
        if (noise != null)
        {
            noise.AmplitudeGain = 0f;
            noise.FrequencyGain = 0f;
        }
    }

    public void Shake(float intensidade, float tempo)
    {
        if (noise != null)
        {
            // Para qualquer tremor anterior antes de começar um novo (evita bugar a câmera)
            StopAllCoroutines(); 
            StartCoroutine(ShakeRoutine(intensidade, tempo));
        }
    }

    private IEnumerator ShakeRoutine(float intensidade, float tempo)
    {
        // Aplica a força (distância)
        noise.AmplitudeGain = intensidade;
        
        // O SEGREDO ESTÁ AQUI: Frequência alta faz vibrar super rápido!
        noise.FrequencyGain = 15f; 
        
        yield return new WaitForSeconds(tempo);
        
        // Zera tudo para parar o tremor instantaneamente
        noise.AmplitudeGain = 0f;
        noise.FrequencyGain = 0f;
    }
}