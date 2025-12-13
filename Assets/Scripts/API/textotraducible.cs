using UnityEngine;
using TMPro; // Necesario para trabajar con TextMeshPro

// -----------------------------------------------------------------------------
// TEXTO TRADUCIBLE
// Este script permite que un texto se actualice automáticamente
// cuando cambia el idioma del juego
// -----------------------------------------------------------------------------

public class textotraducible : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Clave EXACTA de la traducción definida en la web (ej: ¡SALTA!)")]
    public string keyID;

    // Referencia al componente TextMeshPro del objeto
    private TMP_Text miTexto;

    void Start()
    {
        // Obtiene el componente de texto del mismo GameObject
        miTexto = GetComponent<TMP_Text>();

        // Si no se asignó una key manualmente,
        // se usa el texto actual como clave de traducción
        if (string.IsNullOrEmpty(keyID))
            keyID = miTexto.text;

        // Se suscribe al evento del APIManager para
        // actualizar el texto cuando cambie el idioma
        APIManager.Instance.OnLanguageChanged += ActualizarTexto;

        // Actualiza el texto al iniciar el juego,
        // por si las traducciones ya están cargadas
        ActualizarTexto();
    }

    void OnDestroy()
    {
        // Se desuscribe del evento para evitar errores
        // cuando el objeto se destruye o se cambia de escena
        if (APIManager.Instance != null)
            APIManager.Instance.OnLanguageChanged -= ActualizarTexto;
    }

    // -------------------------------------------------------------------------
    // ACTUALIZACIÓN DE TEXTO
    // Obtiene la traducción correspondiente y la asigna al texto
    // -------------------------------------------------------------------------
    public void ActualizarTexto()
    {
        // Verifica que el APIManager y el texto existan
        if (APIManager.Instance != null && miTexto != null)
        {
            // Reemplaza el texto por su traducción correspondiente
            miTexto.text = APIManager.Instance.Traducir(keyID);
        }
    }
}
