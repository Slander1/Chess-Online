using UnityEngine;

namespace ChessPieces.ChessPiecesChose
{
    public class ChessPiecesChosen : MonoBehaviour
    {
        public ChessPiece.Type type;
        [SerializeField] private MeshRenderer meshRenderer;

        public Material Material
        {
            get => meshRenderer.material;
            set => meshRenderer.material = value;
        }
    }
}
