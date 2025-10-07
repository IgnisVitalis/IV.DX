using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [ESQLBlockDefinition("DXRelationDefinitionMainElement")]
    public class DXRelationDefinitionMainElement : ESQLBlock
    {
        [ESQLColumnDefinition("RelationType")]
        public DXRelationTypeEnum RelationType { get; set; }
        [ESQLColumnDefinition("ObjectNameLeft")]
        public string ObjectNameLeft { get; set; }
        [ESQLColumnDefinition("RelationNameLeft")]
        public string RelationNameLeft { get; set; }
        [ESQLColumnDefinition("ObjectNameRight")]
        public string ObjectNameRight { get; set; }
        [ESQLColumnDefinition("RelationNameRight")]
        public string RelationNameRight { get; set; }
        [ESQLColumnDefinition("RelationTable")]
        public string RelationTable { get; set; }
        [ESQLColumnDefinition("Kind")]
        public DXObjectKindEnum Kind { get; set; }
        private string _relationColumnNameLeft;
        [ESQLColumnDefinition("RelationColumnNameLeft")]
        public string RelationColumnNameLeft
        {
            get
            {
                if (string.IsNullOrEmpty(this._relationColumnNameLeft))
                {
                    if (this.RelationType == DXRelationTypeEnum.ManyToMany
                        || this.RelationType == DXRelationTypeEnum.OneToMany
                        || this.RelationType == DXRelationTypeEnum.ZeroOneToMany
                        || this.RelationType == DXRelationTypeEnum.OneToZeroOne
                        || this.RelationType == DXRelationTypeEnum.ZeroOneToZeroOne)
                    {
                        this._relationColumnNameLeft = "ID";
                    }
                }


                return this._relationColumnNameLeft;
            }
            set
            {
                this._relationColumnNameLeft = value;
            }
        }

        private DXColumnTypeEnum? _relationColumnTypeLeft;
        [ESQLColumnDefinition("RelationColumnTypeLeft")]
        public DXColumnTypeEnum? RelationColumnTypeLeft
        {
            get
            {
                if (!this._relationColumnTypeLeft.HasValue)
                {
                    if (this.RelationType == DXRelationTypeEnum.ManyToMany
                        || this.RelationType == DXRelationTypeEnum.OneToMany
                        || this.RelationType == DXRelationTypeEnum.ZeroOneToMany
                        || this.RelationType == DXRelationTypeEnum.OneToZeroOne
                        || this.RelationType == DXRelationTypeEnum.ZeroOneToZeroOne)
                    {
                        this._relationColumnTypeLeft = DXColumnTypeEnum.GUID;
                    }
                }


                return this._relationColumnTypeLeft;
            }
            set
            {
                this._relationColumnTypeLeft = value;
            }
        }

        private string _relationColumnNameRight;
        [ESQLColumnDefinition("RelationColumnNameRight")]
        public string RelationColumnNameRight
        {
            get
            {
                if (string.IsNullOrEmpty(this._relationColumnNameRight))
                {
                    if (this.RelationType == DXRelationTypeEnum.ManyToMany
                        || this.RelationType == DXRelationTypeEnum.ManyToOne
                        || this.RelationType == DXRelationTypeEnum.ManyToZeroOne
                        || this.RelationType == DXRelationTypeEnum.ZeroOneToOne)
                    {
                        this._relationColumnNameRight = "ID";
                    }
                }


                return this._relationColumnNameRight;
            }
            set
            {
                this._relationColumnNameRight = value;
            }
        }

        private DXColumnTypeEnum? _relationColumnTypeRight;
        [ESQLColumnDefinition("RelationColumnTypeRight")]
        public DXColumnTypeEnum? RelationColumnTypeRight
        {
            get
            {
                if (!this._relationColumnTypeRight.HasValue)
                {
                    if (this.RelationType == DXRelationTypeEnum.ManyToMany
                        || this.RelationType == DXRelationTypeEnum.ManyToOne
                        || this.RelationType == DXRelationTypeEnum.ManyToZeroOne
                        || this.RelationType == DXRelationTypeEnum.ZeroOneToOne)
                    {
                        this._relationColumnTypeRight = DXColumnTypeEnum.GUID;
                    }
                }


                return this._relationColumnTypeRight;
            }
            set
            {
                this._relationColumnTypeRight = value;
            }
        }

        public DXRelationDefinitionMainElement()
        {
            this.Kind = DXObjectKindEnum.Custom;
        }
    }
}