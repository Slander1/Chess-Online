using ChessPiaces;
using UnityEngine;

public class ChessPiacesChosen : MonoBehaviour
{
    public ChessPiece.Type type;
    [SerializeField] private MeshRenderer meshRenderer;

    public Material material
    {
        get => meshRenderer.material;
        set => meshRenderer.material = value;
    }
}
