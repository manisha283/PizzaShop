using PizzaShop.Entity.Models;
using PizzaShop.Entity.ViewModels;

namespace PizzaShop.Service.Interfaces;

public interface ICategoryItemService
{
    List<CategoryViewModel> GetCategory();

    CategoryViewModel GetCategoryById(long categoryId);

    Task<bool> AddCategory(CategoryViewModel model,string createrEmail);
    
    Task<bool> EditCategory(CategoryViewModel model);

    Task<bool> SoftDelete(long categoryId);

    Task<ItemsPaginationViewModel> GetPagedItems(long categoryId, int pageSize, int pageNumber, string search);

    Task<AddItemViewModel> GetEditItem(long itemId);

    Task<bool> UpdateItem(AddItemViewModel model);

    Task<bool> AddItem(AddItemViewModel model,string createrEmail);

    Task<bool> SoftDeleteItem(long id);

    Task<bool> MassDeleteItems(List<long> itemsList);


}
