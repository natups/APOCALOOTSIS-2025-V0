using UnityEngine;

// Define dónde puedes crear esta data en el menú de Unity
[CreateAssetMenu(fileName = "New Data Object", menuName = "Game/Base Data Object")]
public class BaseDataObject : ScriptableObject
{
    // Propiedades que ObjectData.cs necesita para funcionar:
    
    [Header("Configuración Visual del Objeto")]
    public string objectName = "Objeto Genérico"; // Usado en ZonaDeEntrega para el texto de la lista
    public Sprite objectSprite; // Usado en ObjectData para cambiar el sprite visual
    public Color displayColor = Color.white; // Usado en ObjectData para cambiar el color
}