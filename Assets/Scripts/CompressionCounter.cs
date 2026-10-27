using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum CompressionRateState { SinDatos, Lento, Correcto, Rapido }

/// <summary>
/// Cuenta compresiones torácicas a partir del movimiento vertical de las manos/controles
/// sobre el pecho de la víctima y calcula el ritmo en compresiones por minuto.
/// Los umbrales son valores de ajuste de juego (afinar probando), no valores clínicos.
/// </summary>
public class CompressionCounter : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Punto del pecho de la víctima (centro de la zona de compresión).")]
    public Transform chestCenter;
    [Tooltip("Manos o controles que comprimen (Left Controller y Right Controller del XR Origin).")]
    public Transform[] hands;

    [Header("Zona de compresión (metros)")]
    public float zoneRadius = 0.15f;
    public float heightAboveChest = 0.30f;

    [Header("Detección de una compresión (metros)")]
    [Tooltip("Cuánto debe bajar la mano desde su punto más alto para contar una compresión.")]
    public float pushDepth = 0.04f;
    [Tooltip("Cuánto debe subir la mano para poder contar la siguiente.")]
    public float releaseHeight = 0.02f;
    [Tooltip("Evita contar doble cuando ambas manos bajan a la vez.")]
    public float minIntervalSeconds = 0.25f;

    [Header("Ritmo objetivo (compresiones por minuto)")]
    public float minRate = 100f;
    public float maxRate = 120f;
    public int sampleSize = 8;
    public float resetAfterSeconds = 2.5f;

    [Header("Modo escritorio (pruebas sin visor)")]
    public bool allowKeyboardFallback = true;
    public Key fallbackKey = Key.F;
    public bool showDebugGui = true;

    [Header("Eventos")]
    public UnityEvent onCompression;
    public UnityEvent<float> onRateUpdated;

    public int TotalCompressions { get; private set; }
    public float CurrentRate { get; private set; }
    public CompressionRateState State { get; private set; } = CompressionRateState.SinDatos;

    class HandState { public bool active, pressed; public float top, bottom; }

    HandState[] states;
    readonly Queue<float> timestamps = new Queue<float>();
    float lastCompressionTime = -999f;
    GUIStyle guiStyle;

    void Start()
    {
        states = new HandState[hands != null ? hands.Length : 0];
        for (int i = 0; i < states.Length; i++) states[i] = new HandState();
    }

    void Update()
    {
        if (chestCenter == null) return;

        for (int i = 0; i < states.Length; i++)
        {
            if (hands[i] == null) continue;
            TrackHand(hands[i].position, states[i]);
        }

        if (allowKeyboardFallback && Keyboard.current != null &&
            Keyboard.current[fallbackKey].wasPressedThisFrame)
        {
            RegisterCompression();
        }

        if (timestamps.Count > 0 && Time.time - lastCompressionTime > resetAfterSeconds)
        {
            timestamps.Clear();
            CurrentRate = 0f;
            State = CompressionRateState.SinDatos;
            onRateUpdated?.Invoke(CurrentRate);
        }
    }

    void TrackHand(Vector3 pos, HandState s)
    {
        Vector3 d = pos - chestCenter.position;
        bool inZone = new Vector2(d.x, d.z).magnitude <= zoneRadius &&
                      d.y >= -0.05f && d.y <= heightAboveChest;

        if (!inZone) { s.active = false; return; }

        if (!s.active)
        {
            s.active = true; s.pressed = false;
            s.top = pos.y; s.bottom = pos.y;
        }

        if (!s.pressed)
        {
            if (pos.y > s.top) s.top = pos.y;
            if (s.top - pos.y >= pushDepth)
            {
                s.pressed = true;
                s.bottom = pos.y;
                RegisterCompression();
            }
        }
        else
        {
            if (pos.y < s.bottom) s.bottom = pos.y;
            if (pos.y - s.bottom >= releaseHeight)
            {
                s.pressed = false;
                s.top = pos.y;
            }
        }
    }

    void RegisterCompression()
    {
        float now = Time.time;
        if (now - lastCompressionTime < minIntervalSeconds) return;

        lastCompressionTime = now;
        TotalCompressions++;
        timestamps.Enqueue(now);
        while (timestamps.Count > sampleSize) timestamps.Dequeue();

        if (timestamps.Count >= 3)
        {
            float[] t = timestamps.ToArray();
            float span = t[t.Length - 1] - t[0];
            if (span > 0.01f)
            {
                CurrentRate = (t.Length - 1) / span * 60f;
                State = CurrentRate < minRate ? CompressionRateState.Lento
                      : CurrentRate > maxRate ? CompressionRateState.Rapido
                      : CompressionRateState.Correcto;
                onRateUpdated?.Invoke(CurrentRate);
            }
        }

        onCompression?.Invoke();
    }

    void OnGUI()
    {
        if (!showDebugGui) return;
        if (guiStyle == null)
            guiStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, normal = { textColor = Color.white } };

        GUI.Label(new Rect(20, 20, 520, 160),
            $"Compresiones: {TotalCompressions}\n" +
            $"Ritmo: {CurrentRate:0} /min  (objetivo {minRate:0}-{maxRate:0})\n" +
            $"Estado: {State}", guiStyle);
    }

    void OnDrawGizmosSelected()
    {
        if (chestCenter == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(chestCenter.position, zoneRadius);
        Gizmos.DrawWireSphere(chestCenter.position + Vector3.up * heightAboveChest, zoneRadius);
    }
}