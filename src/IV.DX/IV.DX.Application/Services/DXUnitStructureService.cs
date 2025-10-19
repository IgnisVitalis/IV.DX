using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Application.Contracts.Models;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;

namespace IV.DX.Application.Services
{
    internal class DXUnitStructureService(
        IDXUnitGenericRepository dxGenericRepo,
        IDXUnitDataService dxUnitDataService,
        IDXEnumDataService dxEnumDataService,
        IDXStructureRepository dataStructureRepository,
        IDXEnumCoreRepository enumCoreRepository,
        IDXStructureCache dxStructureCache) : IDXUnitStructureService
    {
        public async Task<DXUnitDefinitionStructure> GetAsync(string name, CancellationToken ct = default)
        {
            var result = await dxUnitDataService.GetItemsAsync<DXUnitDefinitionUnit>($"DXObjectDefinitionMainElement.Name = '{name}'", ct: ct);

            if (result.Count() == 0)
                return null;

            if (result.Count() > 1)
                throw new InvalidOperationException($"More than one DXUnitDefinitionUnit found with name '{name}'");

            var mainDXUnitDefinition = result.Single();

            List<DXElementDefinitionStructure> singleItemMandatory = new List<DXElementDefinitionStructure>();
            List<DXElementDefinitionStructure> singleItemOptional = new List<DXElementDefinitionStructure>();
            List<DXElementDefinitionStructure> multiItemsMandatory = new List<DXElementDefinitionStructure>();
            List<DXElementDefinitionStructure> multiItemsOptional = new List<DXElementDefinitionStructure>();

            do
            {
                var mainDXElementDefintion = new DXElementDefinitionStructure()
                {
                    Name = mainDXUnitDefinition.DXObjectDefinitionMainElement.Name,
                    Columns = await this.GetColumnDefinitionsAsync(
                       mainDXUnitDefinition.DXObjectDefinitionMainElement.Name,
                       mainDXUnitDefinition.DXColumnDefinitionElement?.Announced, ct: ct)
                };

                singleItemMandatory.Add(mainDXElementDefintion);

                var blockInEntityDefinitions = mainDXUnitDefinition.DXElementInUnitDefinitionElement?.Announced;

                if (blockInEntityDefinitions != null)
                {
                    foreach (var blockInEntityDefinition in blockInEntityDefinitions)
                    {
                        var blockDefinition = await GetEnumDefinitionAsync(blockInEntityDefinition.DXElementDefinitionUnit, ct);

                        switch (blockInEntityDefinition.RelationType)
                        {
                            case DXElementInUnitTypeEnum.SingleOptional:
                                singleItemOptional.Add(blockDefinition);
                                break;
                            case DXElementInUnitTypeEnum.SingleMandatory:
                                singleItemMandatory.Add(blockDefinition);
                                break;
                            case DXElementInUnitTypeEnum.MultiOptional:
                                multiItemsOptional.Add(blockDefinition);
                                break;
                            case DXElementInUnitTypeEnum.MultiMandatory:
                                multiItemsMandatory.Add(blockDefinition);
                                break;
                        }
                    }
                }

                var baseDXUnitID = mainDXUnitDefinition.DXUnitInheritanceElement?.BaseDXUnit;

                if (!baseDXUnitID.HasValue)
                    break;

                mainDXUnitDefinition = await dxUnitDataService.GetItemAsync<DXUnitDefinitionUnit>(baseDXUnitID.Value, ct: ct);

            } while (true);

            return new DXUnitDefinitionStructure()
            {
                Name = name,
                MultiItemsMandatory = multiItemsMandatory.ToList(),
                MultiItemsOptional = multiItemsOptional.ToList(),
                SingleItemMandatory = singleItemMandatory.ToList(),
                SingleItemOptional = singleItemOptional.ToList(),
            };
        }

        private async Task<DXElementDefinitionStructure> GetEnumDefinitionAsync(Guid elementID, CancellationToken ct)
        {
            var block = await dxUnitDataService.GetItemAsync<DXElementDefinitionUnit>(elementID, ct: ct);

            if (block.DXColumnDefinitionElement == null)
                return new DXElementDefinitionStructure() { Name = block.DXObjectDefinitionMainElement.Name, Columns = Enumerable.Empty<DXColumnDefinitionStructure>() };

            else
                return new DXElementDefinitionStructure()
                {
                    Name = block.DXObjectDefinitionMainElement.Name,
                    Columns = await this.GetColumnDefinitionsAsync(block.DXObjectDefinitionMainElement.Name, block.DXColumnDefinitionElement?.Announced, ct)
                };
        }

        private async Task<IEnumerable<DXColumnDefinitionStructure>> GetColumnDefinitionsAsync(string dxElementName, IEnumerable<DXColumnDefinitionElement> columns, CancellationToken ct)
        {
            var list = new List<DXColumnDefinitionStructure>();

            var regularColumns = columns?
                .Where(c => !systemColumns.Contains(c.Name, StringComparer.OrdinalIgnoreCase))
                .Select(c =>
                {
                    return new DXColumnDefinitionStructure()
                    {
                        Name = c.Name,
                        ColumnType = c.ColumnType,
                        Length = c.Length,
                        Precision = c.Precision,
                        Scale = c.Scale,
                        AllowNull = c.AllowNull,
                        DefaultValue = c.DefaultValue,
                    };
                }) ?? Enumerable.Empty<DXColumnDefinitionStructure>();

            list.AddRange(regularColumns);

            var dxEnumDefinitions = dxGenericRepo.GetDXUnits<DXEnumDefinitionUnit>();
            var dxEnumDefinitionNames = dxEnumDefinitions.Select(x => x.DXObjectDefinitionMainElement.Name).ToList();

            var notNullEnumRelations = await dxUnitDataService.GetItemsAsync<DXRelationDefinitionUnit>($"DXRelationDefinitionMainElement.ObjectNameLeft = '{dxElementName}' AND DXRelationDefinitionMainElement.RelationType = 4", ct: ct);

            foreach (var enumRelation in notNullEnumRelations)
            {
                var enumDefinition = dxStructureCache.GetEnum(enumRelation.DXRelationDefinitionMainElement.ObjectNameRight);

                if (enumDefinition == null)
                    continue;

                var enumValues = await dxEnumDataService.GetItemsAsync(enumRelation.DXRelationDefinitionMainElement.ObjectNameRight, ct: ct);

                list.Add(new DXColumnDefinitionStructure()
                {
                    Name = enumRelation.DXRelationDefinitionMainElement.ObjectNameRight,
                    ColumnType = enumRelation.DXRelationDefinitionMainElement.RelationColumnTypeRight.Value,
                    AllowNull = false,
                    EnumValues = enumValues
                });
            }

            var nullableEnumRelations = await dxUnitDataService.GetItemsAsync<DXRelationDefinitionUnit>($"DXRelationDefinitionMainElement.ObjectNameLeft = '{dxElementName}' AND DXRelationDefinitionMainElement.RelationType = 6", ct: ct);

            foreach (var enumRelation in nullableEnumRelations)
            {
                var enumDefinition = dxStructureCache.GetEnum(enumRelation.DXRelationDefinitionMainElement.ObjectNameRight);

                if (enumDefinition == null)
                    continue;

                var enumValues = await dxEnumDataService.GetItemsAsync(enumRelation.DXRelationDefinitionMainElement.ObjectNameRight, ct: ct);           

                list.Add(new DXColumnDefinitionStructure()
                {
                    Name = enumRelation.DXRelationDefinitionMainElement.ObjectNameRight,
                    ColumnType = enumRelation.DXRelationDefinitionMainElement.RelationColumnTypeRight.Value,
                    AllowNull = true,
                    EnumValues = enumValues
                });
            }

            return list;
        }

        private readonly string[] systemColumns = new[] { "ID", "ObjectID", "TimeStamp" };
    }
}
