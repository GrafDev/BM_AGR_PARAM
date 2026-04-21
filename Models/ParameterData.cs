using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace BM_AGR_PARAM.Models
{
    public class ParameterRequirement
    {
        public string Name { get; set; }
        public string DisplayCategories { get; set; }
        public BuiltInCategory[] TargetCategories { get; set; }
        public ForgeTypeId DataType { get; set; }

        public ParameterRequirement(string name, string displayCategories, ForgeTypeId dataType, params BuiltInCategory[] targetCategories)
        {
            Name = name;
            DisplayCategories = displayCategories;
            DataType = dataType;
            TargetCategories = targetCategories;
        }
    }

    public enum ValidationStatus
    {
        OK,
        Missing,
        Partial, // Bound to some but not all categories
        WrongType // If we decided to check types, but currently not used
    }

    public class ValidationResult
    {
        public ParameterRequirement Requirement { get; set; }
        public ValidationStatus Status { get; set; }
        public List<string> MissingCategoriesNames { get; set; } = new List<string>();
        public List<Category> MissingCategories { get; set; } = new List<Category>();

        public string StatusColor => Status == ValidationStatus.OK ? "#4CAF50" : (Status == ValidationStatus.Partial ? "#FF9800" : "#F44336");
        public string StatusText => Status == ValidationStatus.OK ? "OK" : (Status == ValidationStatus.Partial ? "Частично" : "Отсутствует");
        
        // Новые поля для UI без конвертеров
        public string MissingCategoriesDescription => MissingCategoriesNames.Count > 0 ? "Отсутствуют в: " + string.Join(", ", MissingCategoriesNames) : "";
        public bool HasMissingCategories => MissingCategoriesNames.Count > 0;
    }
}
