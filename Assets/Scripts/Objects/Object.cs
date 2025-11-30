using UnityEngine;

// Mantenemos el mismo menú para que tus activos de datos sigan funcionando
[CreateAssetMenu(fileName = "New Object", menuName = "Objects/Object")]
// Hacemos que Object herede de BaseDataObject

public class Object : BaseDataObject
{
    [Header("Propiedades Únicas de Object")]
    [SerializeField] private int valor;

    // Solo mantenemos el getter para 'valor'
    public int Valor => valor;
}