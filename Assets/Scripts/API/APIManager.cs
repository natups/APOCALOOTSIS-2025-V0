using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System; // Necesario para los Eventos

// --- CLASES DE DATOS ---
[System.Serializable]
public class WordItem { public string id; public string key; public string translate; }
[System.Serializable]
public class TranslationData { public string project; public string language; public WordItem[] words; }
[System.Serializable]
public class APIResponse { public TranslationData data; public string error; }

public class APIManager : MonoBehaviour
{
    public static APIManager Instance; // Singleton para acceso fácil

    [Header("Configuración")]
    public string projectId = "8234c0bc-7208-423d-a87a-58b160420cc5";
    public string currentLanguage = "es"; // Idioma por defecto

    // Evento que avisa "¡Ya llegaron las traducciones!"
    public event Action OnLanguageChanged;

    private Dictionary<string, string> diccionario = new Dictionary<string, string>();

    void Awake()
    {
        // Configurar Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // (Opcional) Cargar último idioma guardado
        if (PlayerPrefs.HasKey("SelectedLanguage"))
            currentLanguage = PlayerPrefs.GetString("SelectedLanguage");
    }

    void Start()
    {
        CambiarIdioma(currentLanguage);
    }

    // --- LLAMA A ESTO DESDE TUS BOTONES ---
    public void CambiarIdioma(string nuevoCodigo) // Ej: "en", "pt", "es"
    {
        currentLanguage = nuevoCodigo;
        
        // Guardar preferencia para la próxima vez que abra el juego
        PlayerPrefs.SetString("SelectedLanguage", currentLanguage);
        PlayerPrefs.Save();

        StartCoroutine(DescargarTraducciones());
    }

    IEnumerator DescargarTraducciones()
    {
        string url = $"https://traducila.vercel.app/api/translations/{projectId}/{currentLanguage}";
        Debug.Log($"Descargando idioma: {currentLanguage}...");

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ProcesarRespuesta(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error API: " + request.error);
            }
        }
    }

    void ProcesarRespuesta(string json)
    {
        APIResponse respuesta = JsonUtility.FromJson<APIResponse>(json);

        if (respuesta != null && respuesta.data != null && respuesta.data.words != null)
        {
            diccionario.Clear();
            foreach (WordItem item in respuesta.data.words)
            {
                // Guardamos Key -> Traducción
                if (!diccionario.ContainsKey(item.key))
                    diccionario.Add(item.key, item.translate);
            }

            Debug.Log("Diccionario actualizado. Avisando a los textos...");
            
            // Avisar a todos los textos que se actualicen
            OnLanguageChanged?.Invoke(); 
        }
    }

    public string Traducir(string keyOriginal)
    {
        if (diccionario.ContainsKey(keyOriginal))
            return diccionario[keyOriginal];
        
        return keyOriginal; // Si no hay traducción, devuelve el original
    }
}