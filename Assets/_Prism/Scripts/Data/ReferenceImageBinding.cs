using UnityEngine;

namespace Prism.Data
{
    /// <summary>
    /// XR 레퍼런스 이미지 이름을 크리처 베이스 데이터에 연결합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ReferenceImageBinding", menuName = "Prism/Reference Image Binding")]
    public sealed class ReferenceImageBinding : ScriptableObject
    {
        [SerializeField] private string referenceImageName;
        [SerializeField] private CreatureArchetype archetype;
        [SerializeField] private ElementType baseElement = ElementType.Neutral;

        /// <summary>XRReferenceImage 이름과 일치해야 하는 바인딩 키입니다.</summary>
        public string ReferenceImageName => referenceImageName;

        /// <summary>해당 이미지에서 생성할 베이스 아키타입입니다.</summary>
        public CreatureArchetype Archetype => archetype;

        /// <summary>해당 이미지의 기본 원소 타입입니다.</summary>
        public ElementType BaseElement => baseElement;
    }
}
