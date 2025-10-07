using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDXUnitDataService
    {
        IEnumerable<T> GetItems<T>(DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        IEnumerable<T> GetItems<T>(string esqlWhereExpression, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        T GetItem<T>(Guid id, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        bool IsItemExisting(Guid id, string type);

        Guid Insert(DXUnit esqlObject);
        Guid Update(DXUnit esqlObject);
        bool Delete(DXUnit esqlObject);
        Guid InsertOrUpdate(DXUnit esqlObject);

        Guid Insert(string jObject);
        Guid Update(string jObject);
        bool Delete(string typeName, Guid id);
        Guid InsertOrUpdate(string jObject);

        Guid Insert(JObject jObject);
        Guid Update(JObject jObject);
        bool Delete(JObject jObject);
        Guid InsertOrUpdate(JObject jObject);

        IEnumerable<T> GetItems<T>(DXUnitHandlerBaseContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, DXUnitHandlerBaseContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        IEnumerable<T> GetItems<T>(string esqlWhereExpression, DXUnitHandlerBaseContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        T GetItem<T>(Guid id, DXUnitHandlerBaseContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        bool IsItemExisting(Guid id, string type, DXUnitHandlerBaseContext context);

        Guid Insert(DXUnit esqlObject, DXUnitHandlerBaseContext context);
        Guid Update(DXUnit esqlObject, DXUnitHandlerBaseContext context);
        Guid InsertOrUpdate(DXUnit esqlObject, DXUnitHandlerBaseContext context);
        bool Delete(DXUnit esqlObject, DXUnitHandlerBaseContext context);

        IEnumerable<ESQLModel> GetItems(string typeName, DXUnitHandlerBaseContext context);
        IEnumerable<ESQLModel> GetItems(string typeName, IEnumerable<Guid> ids, DXUnitHandlerBaseContext context);
        IEnumerable<ESQLModel> GetItems(string typeName, string esqlWhereExpression, DXUnitHandlerBaseContext context);
        ESQLModel GetItem(string typeName, Guid id, DXUnitHandlerBaseContext context);

        Guid Insert(string jObject, DXUnitHandlerBaseContext context);
        Guid Update(string jObject, DXUnitHandlerBaseContext context);
        bool Delete(string typeName, Guid id, DXUnitHandlerBaseContext context);
        Guid InsertOrUpdate(string jObject, DXUnitHandlerBaseContext context);

        Guid Insert(JObject jObject, DXUnitHandlerBaseContext context);
        Guid Update(JObject jObject, DXUnitHandlerBaseContext context);
        bool Delete(JObject jObject, DXUnitHandlerBaseContext context);
        Guid InsertOrUpdate(JObject jObject, DXUnitHandlerBaseContext context);
    }
}