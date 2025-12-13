using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System; // Necesario para usar eventos (Action)

// -----------------------------------------------------------------------------
// CLASES DE DATOS
// Estas clases representan la estructura del JSON que devuelve la API
// -----------------------------------------------------------------------------

// Representa una palabra individual con su traducción
[System.Serializable]
public class WordItem
{
    public string id;        // ID único de la palabra
    public string key;       // Clave original (ej: "PLAY_BUTTON")
    public string translate; // Texto traducido
}

// Contiene los datos generales del proyecto y el listado de palabras
[System.Serializable]
public class TranslationData
{
    public string project;   // ID del proyecto
    public string language;  // Código del idioma
    public WordItem[] words; // Lista de palabras traducidas
}

// Clase raíz de la respuesta de la API
[System.Serializable]
public class APIResponse
{
    public TranslationData data; // Datos de traducción
    public string error;         // Mensaje de error (si existiera)
}

// -----------------------------------------------------------------------------
// API MANAGER
// Se encarga de descargar traducciones y proveerlas al resto del juego
// -----------------------------------------------------------------------------

public class APIManager : MonoBehaviour
{
    // Instancia Singleton para acceso global
    public static APIManager Instance;

    [Header("Configuración")]
    public string projectId = "8234c0bc-7208-423d-a87a-58b160420cc5";
    public string currentLanguage = "es"; // Idioma por defecto

    // Evento que se dispara cuando las traducciones ya están listas
    public event Action OnLanguageChanged;

    // Diccionario interno que guarda: Key -> Traducción
    private Dictionary<string, string> diccionario = new Dictionary<string, string>();

    void Awake()
    {
        // Implementación del patrón Singleton
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Cargar el idioma guardado previamente (si existe)
        if (PlayerPrefs.HasKey("SelectedLanguage"))
            currentLanguage = PlayerPrefs.GetString("SelectedLanguage");
    }

    void Start()
    {
        // Descarga las traducciones del idioma actual al iniciar el juego
        CambiarIdioma(currentLanguage);
    }

    // -------------------------------------------------------------------------
    // MÉTODO PÚBLICO
    // Se llama desde botones u otros scripts para cambiar el idioma
    // -------------------------------------------------------------------------
    public void CambiarIdioma(string nuevoCodigo) // Ej: "en", "pt", "es"
    {
        currentLanguage = nuevoCodigo;

        // Guarda el idioma seleccionado para futuras sesiones
        PlayerPrefs.SetString("SelectedLanguage", currentLanguage);
        PlayerPrefs.Save();

        // Inicia la descarga de traducciones
        StartCoroutine(DescargarTraducciones());
    }

    // -------------------------------------------------------------------------
    // COROUTINE
    // Descarga las traducciones desde la API
    // -------------------------------------------------------------------------
    IEnumerator DescargarTraducciones()
    {
        string url = $"https://traducila.vercel.app/api/translations/{projectId}/{currentLanguage}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Espera a que la solicitud termine
            yield return request.SendWebRequest();

            // Si la solicitud fue exitosa, procesa el JSON recibido
            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcesarRespuesta(request.downloadHandler.text);
            }
        }
    }

    // -------------------------------------------------------------------------
    // PROCESAMIENTO DE DATOS
    // Convierte el JSON en datos utilizables y llena el diccionario
    // -------------------------------------------------------------------------
    void ProcesarRespuesta(string json)
    {
        APIResponse respuesta = JsonUtility.FromJson<APIResponse>(json);

        // Verifica que los datos sean válidos
        if (respuesta != null && respuesta.data != null && respuesta.data.words != null)
        {
            // Limpia el diccionario antes de cargar nuevas traducciones
            diccionario.Clear();

            // Recorre todas las palabras recibidas
            foreach (WordItem item in respuesta.data.words)
            {
                // Guarda cada traducción usando la key como identificador
                if (!diccionario.ContainsKey(item.key))
                    diccionario.Add(item.key, item.translate);
            }

            // Notifica a todos los objetos suscritos que el idioma cambió
            OnLanguageChanged?.Invoke();
        }
    }

    // -------------------------------------------------------------------------
    // MÉTODO DE USO GENERAL
    // Devuelve la traducción correspondiente a una key
    // -------------------------------------------------------------------------
    public string Traducir(string keyOriginal)
    {
        // Si existe una traducción, la devuelve
        if (diccionario.ContainsKey(keyOriginal))
            return diccionario[keyOriginal];

        // Si no existe, devuelve la key original
        return keyOriginal;
    }
}
