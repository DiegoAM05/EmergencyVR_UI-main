using UnityEngine;

/// <summary>
/// Metrónomo espacial: emite un clic a un ritmo fijo desde el objeto donde se coloque
/// (en el prototipo, el punto del pecho de la víctima). Genera el sonido por código,
/// así que no necesita ningún archivo de audio.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Metronome : MonoBehaviour
{
    [Tooltip("Compresiones por minuto del metrónomo (dentro del rango objetivo).")]
    public float beatsPerMinute = 110f;
    public bool playOnStart = true;
    [Range(0f, 1f)] public float volume = 0.6f;

    AudioSource source;
    AudioClip click;
    float nextBeat;
    bool running;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f; // 1 = sonido 3D, ubicado en el espacio
        source.volume = volume;
        click = CreateClick();
    }

    void Start()
    {
        if (playOnStart) StartMetronome();
    }

    public void StartMetronome()
    {
        running = true;
        nextBeat = Time.time;
    }

    public void StopMetronome()
    {
        running = false;
    }

    void Update()
    {
        if (!running || Time.time < nextBeat) return;
        source.PlayOneShot(click);
        nextBeat = Mathf.Max(nextBeat + 60f / beatsPerMinute, Time.time);
    }

    static AudioClip CreateClick()
    {
        const int sampleRate = 44100;
        int samples = (int)(sampleRate * 0.06f);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = i / (float)sampleRate;
            data[i] = Mathf.Sin(2f * Mathf.PI * 1000f * t) * Mathf.Exp(-t * 60f);
        }
        AudioClip clip = AudioClip.Create("MetronomeClick", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}