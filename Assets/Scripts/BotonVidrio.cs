using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BotonVidrio : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visual")]
    [SerializeField] private Image relleno;
    [SerializeField] private Image borde;
    [SerializeField] private Color rellenoNormal = new Color32(40, 55, 75, 140);
    [SerializeField] private Color rellenoHover  = new Color32(75, 100, 135, 200);
    [SerializeField] private Color bordeNormal   = new Color32(255, 255, 255, 110);
    [SerializeField] private Color bordeHover    = new Color32(255, 255, 255, 255);
    [SerializeField] private float velocidad = 12f;

    [Header("Audio")]
    [SerializeField] private AudioSource fuente;
    [SerializeField] private AudioClip sonidoHover;
    [SerializeField] private AudioClip sonidoClick;

    private bool hover;

    public void OnPointerEnter(PointerEventData e)
    {
        hover = true;
        if (fuente && sonidoHover) fuente.PlayOneShot(sonidoHover);
    }

    public void OnPointerExit(PointerEventData e) => hover = false;

    public void OnPointerClick(PointerEventData e)
    {
        if (fuente && sonidoClick) fuente.PlayOneShot(sonidoClick);
    }

    void Update()
    {
        float t = velocidad * Time.unscaledDeltaTime;
        relleno.color = Color.Lerp(relleno.color, hover ? rellenoHover : rellenoNormal, t);
        borde.color   = Color.Lerp(borde.color,   hover ? bordeHover   : bordeNormal,   t);
    }
}