using IV.DX.Kernel.Attributes;
using IV.DX.Kernel.Enums;

namespace IV.DX.Kernel.Models
{
    [ESQLBlockDefinition("DPRelationGenBlock")]
    public class DPRelationGenBlock : ESQLBlock
    {
        [ESQLColumnDefinition("RelationType")]
        public DPRelationTypeEnum RelationType { get; set; }
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
                    if (this.RelationType == DPRelationTypeEnum.ManyToMany
                        || this.RelationType == DPRelationTypeEnum.OneToMany
                        || this.RelationType == DPRelationTypeEnum.ZeroOneToMany
                        || this.RelationType == DPRelationTypeEnum.OneToZeroOne
                        || this.RelationType == DPRelationTypeEnum.ZeroOneToZeroOne)
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
                    if (this.RelationType == DPRelationTypeEnum.ManyToMany
                        || this.RelationType == DPRelationTypeEnum.OneToMany
                        || this.RelationType == DPRelationTypeEnum.ZeroOneToMany
                        || this.RelationType == DPRelationTypeEnum.OneToZeroOne
                        || this.RelationType == DPRelationTypeEnum.ZeroOneToZeroOne)
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
                    if (this.RelationType == DPRelationTypeEnum.ManyToMany
                        || this.RelationType == DPRelationTypeEnum.ManyToOne
                        || this.RelationType == DPRelationTypeEnum.ManyToZeroOne
                        || this.RelationType == DPRelationTypeEnum.ZeroOneToOne)
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
                    if (this.RelationType == DPRelationTypeEnum.ManyToMany
                        || this.RelationType == DPRelationTypeEnum.ManyToOne
                        || this.RelationType == DPRelationTypeEnum.ManyToZeroOne
                        || this.RelationType == DPRelationTypeEnum.ZeroOneToOne)
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

        public DPRelationGenBlock()
        {
            this.Kind = DXObjectKindEnum.Custom;
        }
    }
}