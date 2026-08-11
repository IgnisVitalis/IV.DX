using IV.DX.Application.Contracts.Abstractions;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using IV.DX.Persistence.Contracts.Abstractions;
using IV.DX.Shared.IntTests;
using IV.DX.Shared.IntTests.Factories.Test;
using IV.DX.Shared.IntTests.Models.Test;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace IV.DX.Application.IntTests.Services
{
    [Collection("DX:one-time")]
    public class DXQueryResultProviderTests : IntTestController
    {
        private readonly IDXUnitDataService _dataService;
        private readonly IDXQueryResultProvider _queryProvider;
        private readonly IDXExecutionContextAccessor _executionContextAccessor;
        private readonly IDXUnitGenericRepository _genericRepo;
        private readonly IDXStructureCache _structureCache;

        public DXQueryResultProviderTests(DXTestFixture fx, ITestOutputHelper output) : base(fx, output)
        {
            _dataService = base.ServiceProvider.GetRequiredService<IDXUnitDataService>();
            _queryProvider = base.ServiceProvider.GetRequiredService<IDXQueryResultProvider>();
            _executionContextAccessor = base.ServiceProvider.GetRequiredService<IDXExecutionContextAccessor>();
            _genericRepo = base.ServiceProvider.GetRequiredService<IDXUnitGenericRepository>();
            _structureCache = base.ServiceProvider.GetRequiredService<IDXStructureCache>();
        }

        [Fact]
        public async Task GetAsync_WithFilterExpression_ReturnsOnlyMatchingItems()
        {
            var now = DateTime.UtcNow;
            var targetName = $"FltTarget_{Guid.NewGuid():N}";

            var userUnitDef = _structureCache.GetDXUnit("TUserUnit");

            var targetId = await _dataService.InsertAsync(TUserUnitFactory.GetItem(targetName, "Test", now.Date));
            var otherId = await _dataService.InsertAsync(TUserUnitFactory.GetItem($"FltOther_{Guid.NewGuid():N}", "Test", now.Date));
            var queryId = await InsertQueryUnitAsync(userUnitDef.Id, filterExpression: $"TUserMainElement.Name = '{targetName}'");

            try
            {
                using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
                {
                    SubjectId = "system:filter-expression-test",
                    IsSystem = true
                });

                var result = await _queryProvider.GetAsync(queryId);
                Assert.NotNull(result);

                var ids = ExtractContentIds(result);
                Assert.Contains(targetId, ids);
                Assert.DoesNotContain(otherId, ids);
            }
            finally
            {
                await _dataService.DeleteAsync(new DXQueryUnit { Id = queryId });
                await _dataService.DeleteAsync(new TUserUnit { Id = targetId });
                await _dataService.DeleteAsync(new TUserUnit { Id = otherId });
            }
        }

        [Fact]
        public async Task GetAsync_WithFilterExpression_WhenExpressionMatchesNothing_DoesNotReturnItem()
        {
            var now = DateTime.UtcNow;
            var nonExistentName = $"NeverExists_{Guid.NewGuid():N}";

            var userUnitDef = _structureCache.GetDXUnit("TUserUnit");

            var itemId = await _dataService.InsertAsync(TUserUnitFactory.GetItem($"FltNoMatch_{Guid.NewGuid():N}", "Test", now.Date));
            var queryId = await InsertQueryUnitAsync(userUnitDef.Id, filterExpression: $"TUserMainElement.Name = '{nonExistentName}'");

            try
            {
                using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
                {
                    SubjectId = "system:filter-expression-test",
                    IsSystem = true
                });

                var result = await _queryProvider.GetAsync(queryId);
                Assert.NotNull(result);

                var ids = ExtractContentIds(result);
                Assert.DoesNotContain(itemId, ids);
            }
            finally
            {
                await _dataService.DeleteAsync(new DXQueryUnit { Id = queryId });
                await _dataService.DeleteAsync(new TUserUnit { Id = itemId });
            }
        }

        [Fact]
        public async Task GetAsync_WhenQuerySelectsEncryptedString_MasksValue_Ok()
        {
            var cache = base.ServiceProvider.GetRequiredService<IDXStructureCache>();
            await cache.RefreshAsync();

            var now = DateTime.UtcNow;

            var unitName = $"DXQueryMaskedSecretUnit_{Guid.NewGuid():N}";

            var unitDefinition = new DXUnitDefinitionUnit
            {
                TimeStamp = now,
                Name = unitName,
                DXTitleExpression = "Secret",
                Kind = DXObjectKindEnum.Core,
                DXColumnDefinitionElement = new DXMultiElementsContainer<DXColumnDefinitionElement>
                {
                    Announced = new HashSet<DXColumnDefinitionElement>
                    {
                        new DXColumnDefinitionElement
                        {
                            TimeStamp = now,
                            Name = "Secret",
                            AllowNull = false,
                            ColumnType = DXColumnTypeEnum.EncryptedString
                        }
                    }
                }
            };

            var unitDefId = await _dataService.InsertOrUpdateAsync(unitDefinition);
            await cache.RefreshAsync();

            var plaintext = "super-secret";

            var insertBlock = new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta
                {
                    Kind = "DXUnit",
                    Type = unitName
                },
                Data = new DXData<DXUnitRecord>
                {
                    Items = new List<DXUnitRecord>
                    {
                        new DXUnitRecord
                        {
                            TimeStamp = now,
                            Fields = new Dictionary<string, JToken>
                            {
                                { "Secret", JToken.FromObject(plaintext) }
                            }
                        }
                    }
                }
            };

            var instanceId = await _dataService.InsertAsync(insertBlock);

            var query = new DXQueryUnit
            {
                TimeStamp = now,
                Name = $"Q_{Guid.NewGuid():N}",
                DXUnitDefinition = unitDefId,
                DXQueryColumnElement = new DXMultiElementsContainer<DXQueryColumnElement>
                {
                    Announced = new HashSet<DXQueryColumnElement>
                    {
                        new DXQueryColumnElement
                        {
                            TimeStamp = now,
                            Name = "Secret",
                            Expression = "Secret",
                            Order = 0
                        }
                    }
                }
            };

            var queryId = await _dataService.InsertOrUpdateAsync(query);

            var result = await _queryProvider.GetAsync(queryId);

            Assert.NotNull(result);

            var content = result["Content"] as JObject;
            Assert.NotNull(content);

            var contentBlock = content!.ToObject<DXDataBlock<DXUnitRecord>>();
            Assert.NotNull(contentBlock);

            var item = contentBlock!.Data?.Items?.SingleOrDefault(x => x.Id == instanceId);
            Assert.NotNull(item);

            var secret = item!.Fields != null && item.Fields.TryGetValue("Secret", out var v) ? v?.ToString() : null;
            Assert.Equal(string.Empty, secret);

            await _dataService.DeleteAsync(new DXDataBlock<DXUnitRecord>
            {
                Meta = new DXMeta { Kind = "DXUnit", Type = unitName },
                Data = new DXData<DXUnitRecord>
                {
                    Delete = new List<DXDeleteRef> { new DXDeleteRef { Id = instanceId } }
                }
            });

            await _dataService.DeleteAsync(new DXQueryUnit { Id = queryId });
        }

        [Fact]
        public async Task GetAsync_WithTransitiveRelationPathExpression_ReturnsLinkedFieldValue()
        {
            var now = DateTime.UtcNow;
            var passportUnitDef = _structureCache.GetDXUnit("TPassportUnit");

            var userId = await _dataService.InsertAsync(TUserUnitFactory.GetItem($"TUser_{Guid.NewGuid():N}", "Test", now.Date));
            var passportId = await _dataService.InsertAsync(TPassportUnitFactory.GetItem("SERIAL-TRANSITIVE", new TUserUnit { Id = userId }));

            var position = TPositionUnitFactory.GetItem("SeniorEngineer");
            position.User = userId;
            var positionId = await _dataService.InsertAsync(position);

            var query = new DXQueryUnit
            {
                TimeStamp = now,
                Name = $"Q_{Guid.NewGuid():N}",
                DXUnitDefinition = passportUnitDef.Id,
                DXQueryColumnElement = new DXMultiElementsContainer<DXQueryColumnElement>
                {
                    Announced = new HashSet<DXQueryColumnElement>
                    {
                        new DXQueryColumnElement
                        {
                            TimeStamp = now,
                            Name = "SerialNumber",
                            Expression = "TPassportMainElement.SerialNumber",
                            Order = 0
                        },
                        new DXQueryColumnElement
                        {
                            TimeStamp = now,
                            Name = "PositionName",
                            Expression = "U2U(User).U2U(Position).TPositionMainElement.Name",
                            Order = 1
                        }
                    }
                }
            };

            var queryId = await _dataService.InsertOrUpdateAsync(query);

            try
            {
                using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
                {
                    SubjectId = "system:transitive-relation-test",
                    IsSystem = true
                });

                var result = await _queryProvider.GetAsync(queryId);

                Assert.NotNull(result);

                var content = result["Content"] as JObject;
                Assert.NotNull(content);

                var contentBlock = content.ToObject<DXDataBlock<DXUnitRecord>>();
                Assert.NotNull(contentBlock);

                var item = contentBlock.Data?.Items?.SingleOrDefault(x => x.Id == passportId);
                Assert.NotNull(item);

                var positionName = item.Fields != null && item.Fields.TryGetValue("PositionName", out var v) ? v?.ToString() : null;
                Assert.Equal("SeniorEngineer", positionName);
            }
            finally
            {
                await _dataService.DeleteAsync(new DXQueryUnit { Id = queryId });
                await _dataService.DeleteAsync(new TPassportUnit { Id = passportId });
                await _dataService.DeleteAsync(new TPositionUnit { Id = positionId });
                await _dataService.DeleteAsync(new TUserUnit { Id = userId });
            }
        }

        [Fact]
        public async Task GetDXTitleExpressionsAsync_WhenReadAccessDenied_ThrowsUnauthorizedAccessException()
        {
            using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "query-provider-test-user",
                Access = DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Read, "DXRoleUnit")
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _queryProvider.GetDXTitleExpressionsAsync("DXUnitDefinitionUnit"));
        }

        [Fact]
        public async Task GetDXTitleExpressionsAsync_WhenTenantDeniedAndMembershipOrGroupAllowed_ThrowsUnauthorizedAccessException()
        {
            using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
            {
                SubjectId = "hierarchy-query-user",
                // Tenant grants a different type, so the narrowed result excludes DXUnitDefinitionUnit.
                Access = DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Read, "DXRoleUnit")
                    .Intersect(DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Read, "DXUnitDefinitionUnit"))
                    .Intersect(DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Read, "DXUnitDefinitionUnit"))
            });

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _queryProvider.GetDXTitleExpressionsAsync("DXUnitDefinitionUnit"));
        }

        [Fact]
        public async Task GetDXTitleExpressionsAsync_WhenAllowedOwnedOnly_AndNoOwnership_ReturnsEmpty()
        {
            var identityId = Guid.NewGuid();

            var recordId = await _dataService.InsertAsync(
                TUserUnitFactory.GetItem("QRP", "OwnedTest", DateTime.UtcNow.Date));

            try
            {
                // Context has identity but TUserUnit not in allowed types → AllowedOwnedOnly
                // TUserUnit.SupportsOwnership = false → no owned IDs found → empty result
                using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
                {
                    SubjectId = "qrp-owned-only-user",
                    IdentityId = identityId,
                    Access = DXAccessScope.ForOperation(DXUnitTypeAccessOperation.Read, "SomeOtherUnit")
                });

                var result = await _queryProvider.GetDXTitleExpressionsAsync("TUserUnit");

                Assert.Empty(result);
            }
            finally
            {
                var existing = _genericRepo.GetDXUnit<TUserUnit>(recordId);
                if (existing != null) _genericRepo.Delete(existing);
            }
        }


        [Fact]
        public async Task GetAsync_WithSelfRelationOnRootUnit_NonSelfU2URelationResolvesCorrectly()
        {
            var now = DateTime.UtcNow;
            var userUnitDef = _structureCache.GetDXUnit("TUserUnit");

            var manager = TUserUnitFactory.GetItem($"TManager_{Guid.NewGuid():N}", "Test", now.Date);
            var managerId = await _dataService.InsertAsync(manager);

            var subordinate = TUserUnitFactory.GetItem($"TSubordinate_{Guid.NewGuid():N}", "Test", now.Date);
            subordinate.Manager = managerId;
            var userId = await _dataService.InsertAsync(subordinate);

            var position = TPositionUnitFactory.GetItem("SelfRelationTestPosition");
            position.User = userId;
            var positionId = await _dataService.InsertAsync(position);

            var query = new DXQueryUnit
            {
                TimeStamp = now,
                Name = $"Q_{Guid.NewGuid():N}",
                DXUnitDefinition = userUnitDef.Id,
                FilterExpression = $"Id = '{userId}'",
                DXQueryColumnElement = new DXMultiElementsContainer<DXQueryColumnElement>
                {
                    Announced = new HashSet<DXQueryColumnElement>
                    {
                        new DXQueryColumnElement
                        {
                            TimeStamp = now,
                            Name = "PositionName",
                            Expression = "U2U(Position).TPositionMainElement.Name",
                            Order = 0
                        }
                    }
                }
            };

            var queryId = await _dataService.InsertOrUpdateAsync(query);

            try
            {
                using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
                {
                    SubjectId = "system:self-relation-test",
                    IsSystem = true
                });

                var result = await _queryProvider.GetAsync(queryId);

                Assert.NotNull(result);

                var content = result["Content"] as JObject;
                Assert.NotNull(content);

                var contentBlock = content.ToObject<DXDataBlock<DXUnitRecord>>();
                Assert.NotNull(contentBlock);

                var item = contentBlock.Data?.Items?.SingleOrDefault(x => x.Id == userId);
                Assert.NotNull(item);

                var positionName = item.Fields != null && item.Fields.TryGetValue("PositionName", out var v) ? v?.ToString() : null;
                Assert.Equal("SelfRelationTestPosition", positionName);
            }
            finally
            {
                await _dataService.DeleteAsync(new DXQueryUnit { Id = queryId });
                await _dataService.DeleteAsync(new TPositionUnit { Id = positionId });
                await _dataService.DeleteAsync(new TUserUnit { Id = userId });
                await _dataService.DeleteAsync(new TUserUnit { Id = managerId });
            }
        }

        [Fact]
        public async Task GetAsync_WithTransitiveRelationToDerivedUnitType_ReturnsDerivedTypeName()
        {
            var now = DateTime.UtcNow;
            var userUnitDef = _structureCache.GetDXUnit("TUserUnit");

            var userId = await _dataService.InsertAsync(TUserUnitFactory.GetItem($"TUser_{Guid.NewGuid():N}", "Test", now.Date));

            var computer = new TComputerUnit
            {
                User = userId,
                TDeviceMainElement = new TDeviceMainElement
                {
                    Model = "TransitiveTestComputer",
                    UUID = Guid.NewGuid()
                }
            };

            var computerId = await _dataService.InsertAsync(computer);

            var query = new DXQueryUnit
            {
                TimeStamp = now,
                Name = $"Q_{Guid.NewGuid():N}",
                DXUnitDefinition = userUnitDef.Id,
                DXQueryColumnElement = new DXMultiElementsContainer<DXQueryColumnElement>
                {
                    Announced = new HashSet<DXQueryColumnElement>
                    {
                        new DXQueryColumnElement
                        {
                            TimeStamp = now,
                            Name = "DeviceTypeName",
                            Expression = "U2U(Devices).U2U(DerivedDXUnitType).Name",
                            Order = 0
                        }
                    }
                }
            };

            var queryId = await _dataService.InsertOrUpdateAsync(query);

            try
            {
                using var _ = _executionContextAccessor.BeginScope(new DXExecutionContext
                {
                    SubjectId = "system:derived-unit-type-test",
                    IsSystem = true
                });

                var result = await _queryProvider.GetAsync(queryId);

                Assert.NotNull(result);

                var content = result["Content"] as JObject;
                Assert.NotNull(content);

                var contentBlock = content.ToObject<DXDataBlock<DXUnitRecord>>();
                Assert.NotNull(contentBlock);

                var item = contentBlock.Data?.Items?.SingleOrDefault(x => x.Id == userId);
                Assert.NotNull(item);

                var deviceTypeName = item.Fields != null && item.Fields.TryGetValue("DeviceTypeName", out var v) ? v?.ToString() : null;
                Assert.Equal("TComputerUnit", deviceTypeName);
            }
            finally
            {
                await _dataService.DeleteAsync(new DXQueryUnit { Id = queryId });
                await _dataService.DeleteAsync(new TComputerUnit { Id = computerId });
                await _dataService.DeleteAsync(new TUserUnit { Id = userId });
            }
        }

        private HashSet<Guid> ExtractContentIds(JObject result)
        {
            var contentBlock = result["Content"]?.ToObject<DXDataBlock<DXUnitRecord>>();
            return contentBlock?.Data?.Items?.Select(x => x.Id).ToHashSet() ?? new HashSet<Guid>();
        }

        private async Task<Guid> InsertQueryUnitAsync(Guid unitDefinitionId, string filterExpression = null)
        {
            var query = new DXQueryUnit
            {
                TimeStamp = DateTime.UtcNow,
                Name = $"Q_{Guid.NewGuid():N}",
                DXUnitDefinition = unitDefinitionId,
                FilterExpression = filterExpression,
                DXQueryColumnElement = new DXMultiElementsContainer<DXQueryColumnElement>
                {
                    Announced = new HashSet<DXQueryColumnElement>
                    {
                        new DXQueryColumnElement
                        {
                            TimeStamp = DateTime.UtcNow,
                            Name = "Name",
                            Expression = "TUserMainElement.Name",
                            Order = 0
                        }
                    }
                }
            };

            return await _dataService.InsertOrUpdateAsync(query);
        }

    }
}
