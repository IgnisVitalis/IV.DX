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

        IEnumerable<T> GetItems<T>(DXUnitHandlerBaseContextOld context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, DXUnitHandlerBaseContextOld context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        IEnumerable<T> GetItems<T>(string esqlWhereExpression, DXUnitHandlerBaseContextOld context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        T GetItem<T>(Guid id, DXUnitHandlerBaseContextOld context, DXLoadingType typeOfLoading = DXLoadingType.Full) where T : DXUnit, new();
        bool IsItemExisting(Guid id, string type, DXUnitHandlerBaseContextOld context);

        Guid Insert(DXUnit esqlObject, DXUnitHandlerBaseContextOld context);
        Guid Update(DXUnit esqlObject, DXUnitHandlerBaseContextOld context);
        Guid InsertOrUpdate(DXUnit esqlObject, DXUnitHandlerBaseContextOld context);
        bool Delete(DXUnit esqlObject, DXUnitHandlerBaseContextOld context);

        IEnumerable<DXModel> GetItems(string typeName, DXUnitHandlerBaseContextOld context);
        IEnumerable<DXModel> GetItems(string typeName, IEnumerable<Guid> ids, DXUnitHandlerBaseContextOld context);
        IEnumerable<DXModel> GetItems(string typeName, string esqlWhereExpression, DXUnitHandlerBaseContextOld context);
        DXModel GetItem(string typeName, Guid id, DXUnitHandlerBaseContextOld context);

        Guid Insert(string jObject, DXUnitHandlerBaseContextOld context);
        Guid Update(string jObject, DXUnitHandlerBaseContextOld context);
        bool Delete(string typeName, Guid id, DXUnitHandlerBaseContextOld context);
        Guid InsertOrUpdate(string jObject, DXUnitHandlerBaseContextOld context);

        Guid Insert(JObject jObject, DXUnitHandlerBaseContextOld context);
        Guid Update(JObject jObject, DXUnitHandlerBaseContextOld context);
        bool Delete(JObject jObject, DXUnitHandlerBaseContextOld context);
        Guid InsertOrUpdate(JObject jObject, DXUnitHandlerBaseContextOld context);
    }
}