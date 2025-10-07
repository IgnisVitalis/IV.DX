using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [DXElement("DXRelationDefinitionMainElement")]
    public class DXRelationDefinitionMainElement : ESQLBlock
    {
        [DXColumn("RelationType")]
        public DXRelationTypeEnum RelationType { get; set; }
        [DXColumn("ObjectNameLeft")]
        public string ObjectNameLeft { get; set; }
        [DXColumn("RelationNameLeft")]
        public string RelationNameLeft { get; set; }
        [DXColumn("ObjectNameRight")]
        public string ObjectNameRight { get; set; }
        [DXColumn("RelationNameRight")]
        public string RelationNameRight { get; set; }
        [DXColumn("RelationTable")]
        public string RelationTable { get; set; }
        [DXColumn("Kind")]
        public DXObjectKindEnum Kind { get; set; }
        private string _relationColumnNameLeft;
        [DXColumn("RelationColumnNameLeft")]
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
        [DXColumn("RelationColumnTypeLeft")]
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
        [DXColumn("RelationColumnNameRight")]
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
        [DXColumn("RelationColumnTypeRight")]
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