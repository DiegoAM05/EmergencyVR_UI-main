using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private string escenaEntrenamiento = "Entrenamiento";
    [SerializeField] private string escenaEvaluacion = "Evaluacion";
    [SerializeField] private string escenaLibre = "Libre";
    [SerializeField] private float retardo = 0.35f; // deja sonar el clic

    private bool cargando;

    public void IniciarEntrenamiento() => Cargar(escenaEntrenamiento);
    public void IniciarEvaluacion() => Cargar(escenaEvaluacion);
    public void IniciarLibre() => Cargar(escenaLibre);

    private void Cargar(string escena)
    {
        if (cargando) return;

        if (!Application.CanStreamedLevelBeLoaded(escena))
        {
            Debug.LogWarning($"[Menu] La escena '{escena}' no está en Build Profiles.");
            return;
        }

        cargando = true;
        StartCoroutine(CargarConRetardo(escena));
    }

    private IEnumerator CargarConRetardo(string escena)
    {
        yield return new WaitForSecondsRealtime(retardo);
        SceneManager.LoadScene(escena);
    }
}