using RaceDay.Mvc.Models;

namespace RaceDay.Mvc.ViewModels
{
    public class CategoriesViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public List<CategoryInfo> Categories { get; set; } = new();
        public CategoryFormViewModel NewCategory { get; set; } = new();
    }
}
