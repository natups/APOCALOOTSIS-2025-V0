using UnityEngine;
using TMPro; // Necesario para TextMeshPro

public class textotraducible : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Aquí debes poner la Key EXACTA que está en la web (ej: ¡SALTA!)")]
    public string keyID; 

    private TMP_Text miTexto;

    void Start()
    {
        miTexto = GetComponent<TMP_Text>();

        // Si no pusiste ID manual, usa el texto que ya tiene escrito como ID
        if (string.IsNullOrEmpty(keyID))
            keyID = miTexto.text;

        // Suscribirse al evento: Cuando cambie el idioma, actualízame
        APIManager.Instance.OnLanguageChanged += ActualizarTexto;

        // Intentar traducir al iniciar (por si el APIManager ya está listo)
        ActualizarTexto();
    }

    void OnDestroy()
    {
        // Desuscribirse para evitar errores al cambiar de escena
        if (APIManager.Instance != null)
            APIManager.Instance.OnLanguageChanged -= ActualizarTexto;
    }

    public void ActualizarTexto()
    {
        if (APIManager.Instance != null && miTexto != null)
        {
            miTexto.text = APIManager.Instance.Traducir(keyID);
        }
    }
}