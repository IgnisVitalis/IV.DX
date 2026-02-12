```mermaid
classDiagram
direction LR

%% DX Relations (storage entities) - detailed view
namespace "Units & Elements" {
  class DXUnitDefinitionUnit
  class DXElementDefinitionUnit
}

namespace "Relation Entities" {
  class DXUnitToUnitRelationElement {
    +RelationType : DXRelationTypeEnum
    +TargetDXUnit : DXUnitDefinitionUnit
    +OwnRelationName : string(50)
    +TargetRelationName : string(50)
  }

  class DXUnitToElementRelationElement {
    +RelationType : DXRelationTypeEnum
    +TargetDXElement : DXElementDefinitionUnit
    +OwnRelationName : string(50)
    +TargetRelationName : string(50)
  }

  class DXElementToUnitRelationElement {
    +RelationType : DXRelationTypeEnum
    +TargetDXUnit : DXUnitDefinitionUnit
    +OwnRelationName : string(50)
    +TargetRelationName : string(50)
  }
}

%% How they hang together (as in the draw.io storage model)
DXElementDefinitionUnit "1" *-- "0..*" DXElementToUnitRelationElement : contains
DXUnitDefinitionUnit "1" *-- "0..*" DXUnitToUnitRelationElement : contains

DXUnitToUnitRelationElement "1" *-- "0..*" DXUnitToElementRelationElement : contains

%% Explicit cross-links
DXUnitToUnitRelationElement "*" --> "1" DXUnitDefinitionUnit : TargetDXUnit
DXUnitToElementRelationElement "*" --> "1" DXElementDefinitionUnit : TargetDXElement
DXElementToUnitRelationElement "*" --> "1" DXUnitDefinitionUnit : TargetDXUnit

%% Convenience semantic links (optional)
DXElementDefinitionUnit --> DXUnitToElementRelationElement : RelatedDXUnits
DXUnitDefinitionUnit --> DXElementToUnitRelationElement : RelatedDXElements

```
