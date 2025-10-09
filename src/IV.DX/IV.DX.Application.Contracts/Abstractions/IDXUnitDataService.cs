using IV.DX.Application.Contracts.Runtime;
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

        IEnumerable<T> GetItems<T>(IDXHandlerContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, IDXHandlerContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        IEnumerable<T> GetItems<T>(string esqlWhereExpression, IDXHandlerContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        T GetItem<T>(Guid id, IDXHandlerContext context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        bool IsItemExisting(Guid id, string type, IDXHandlerContext context);

        Guid Insert(DXUnit esqlObject, IDXHandlerContext context);
        Guid Update(DXUnit esqlObject, IDXHandlerContext context);
        Guid InsertOrUpdate(DXUnit esqlObject, IDXHandlerContext context);
        bool Delete(DXUnit esqlObject, IDXHandlerContext context);

        IEnumerable<DXModel> GetItems(string typeName, IDXHandlerContext context);
        IEnumerable<DXModel> GetItems(string typeName, IEnumerable<Guid> ids, IDXHandlerContext context);
        IEnumerable<DXModel> GetItems(string typeName, string esqlWhereExpression, IDXHandlerContext context);
        DXModel GetItem(string typeName, Guid id, IDXHandlerContext context);

        Guid Insert(string jObject, IDXHandlerContext context);
        Guid Update(string jObject, IDXHandlerContext context);
        bool Delete(string typeName, Guid id, IDXHandlerContext context);
        Guid InsertOrUpdate(string jObject, IDXHandlerContext context);

        Guid Insert(JObject jObject, IDXHandlerContext context);
        Guid Update(JObject jObject, IDXHandlerContext context);
        bool Delete(JObject jObject, IDXHandlerContext context);
        Guid InsertOrUpdate(JObject jObject, IDXHandlerContext context);
    }
}