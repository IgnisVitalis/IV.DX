using IV.DX.Application.Contracts.HandlerContext;
using IV.DX.Kernel.Enums;
using IV.DX.Kernel.Models;
using Newtonsoft.Json.Linq;

namespace IV.DX.Application.Contracts.Abstractions
{
    public interface IDataService
    {
        IEnumerable<T> GetItems<T>(TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        IEnumerable<T> GetItems<T>(string esqlWhereExpression, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        T GetItem<T>(Guid id, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        bool IsItemExisting(Guid id, string type);

        Guid Insert(ESQLObject esqlObject);
        Guid Update(ESQLObject esqlObject);
        bool Delete(ESQLObject esqlObject);
        Guid InsertOrUpdate(ESQLObject esqlObject);

        Guid Insert(string jObject);
        Guid Update(string jObject);
        bool Delete(string typeName, Guid id);
        Guid InsertOrUpdate(string jObject);

        Guid Insert(JObject jObject);
        Guid Update(JObject jObject);
        bool Delete(JObject jObject);
        Guid InsertOrUpdate(JObject jObject);

        IEnumerable<T> GetItems<T>(DXUnitHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        IEnumerable<T> GetItems<T>(IEnumerable<Guid> ids, DXUnitHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        IEnumerable<T> GetItems<T>(string esqlWhereExpression, DXUnitHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        T GetItem<T>(Guid id, DXUnitHandlerBaseContext context, TypeOfEntityLoading typeOfLoading = TypeOfEntityLoading.Full) where T : ESQLObject, new();
        bool IsItemExisting(Guid id, string type, DXUnitHandlerBaseContext context);

        Guid Insert(ESQLObject esqlObject, DXUnitHandlerBaseContext context);
        Guid Update(ESQLObject esqlObject, DXUnitHandlerBaseContext context);
        Guid InsertOrUpdate(ESQLObject esqlObject, DXUnitHandlerBaseContext context);
        bool Delete(ESQLObject esqlObject, DXUnitHandlerBaseContext context);

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