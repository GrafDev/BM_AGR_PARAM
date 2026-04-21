using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using BM_AGR_PARAM.Models;

namespace BM_AGR_PARAM.Services
{
    public class AgrService
    {
        private readonly Document _doc;

        // Список требований из скриншота пользователя
        private static readonly List<ParameterRequirement> Requirements = new List<ParameterRequirement>
        {
            new ParameterRequirement("RUS_Area", "Помещения, Зоны, Перекрытия, Панели, Стены, Топография", SpecTypeId.Area,
                BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Areas, BuiltInCategory.OST_Floors, 
                BuiltInCategory.OST_CurtainWallPanels, BuiltInCategory.OST_Walls, BuiltInCategory.OST_Topography),

            new ParameterRequirement("RUS_Height", "Стены, Колонны, Окна, Двери, Помещения, Зоны, Лестницы, Пандусы", SpecTypeId.Length,
                BuiltInCategory.OST_Walls, BuiltInCategory.OST_Columns, BuiltInCategory.OST_StructuralColumns, 
                BuiltInCategory.OST_Windows, BuiltInCategory.OST_Doors, BuiltInCategory.OST_Rooms, 
                BuiltInCategory.OST_Areas, BuiltInCategory.OST_Stairs, BuiltInCategory.OST_Ramps),

            new ParameterRequirement("RUS_Width", "Окна, Двери, Колонны, Панели, Лестницы, Пандусы", SpecTypeId.Length,
                BuiltInCategory.OST_Windows, BuiltInCategory.OST_Doors, BuiltInCategory.OST_Columns, 
                BuiltInCategory.OST_StructuralColumns, BuiltInCategory.OST_CurtainWallPanels, 
                BuiltInCategory.OST_Stairs, BuiltInCategory.OST_Ramps),

            new ParameterRequirement("RUS_Length", "Стены, Колонны, Импосты, Обобщенные модели", SpecTypeId.Length,
                BuiltInCategory.OST_Walls, BuiltInCategory.OST_Columns, BuiltInCategory.OST_StructuralColumns, 
                BuiltInCategory.OST_CurtainWallMullions, BuiltInCategory.OST_GenericModel),

            new ParameterRequirement("RUS_Thickness", "Стены, Перекрытия, Крыши, Панели", SpecTypeId.Length,
                BuiltInCategory.OST_Walls, BuiltInCategory.OST_Floors, BuiltInCategory.OST_Roofs, 
                BuiltInCategory.OST_CurtainWallPanels),

            new ParameterRequirement("RUS_Volume", "Стены, Перекрытия, Колонны, Помещения, Зоны", SpecTypeId.Volume,
                BuiltInCategory.OST_Walls, BuiltInCategory.OST_Floors, BuiltInCategory.OST_Columns, 
                BuiltInCategory.OST_StructuralColumns, BuiltInCategory.OST_Rooms, BuiltInCategory.OST_Areas),

            new ParameterRequirement("RUS_GlazingArea", "Окна, Панели витража", SpecTypeId.Area,
                BuiltInCategory.OST_Windows, BuiltInCategory.OST_CurtainWallPanels),

            new ParameterRequirement("RUS_SillHeight", "Окна", SpecTypeId.Length,
                BuiltInCategory.OST_Windows),

            new ParameterRequirement("RUS_Diametr", "Колонны", SpecTypeId.Length,
                BuiltInCategory.OST_Columns, BuiltInCategory.OST_StructuralColumns)
        };

        public AgrService(Document doc)
        {
            _doc = doc;
        }

        public List<ValidationResult> CheckParameters()
        {
            var results = new List<ValidationResult>();
            BindingMap bindingMap = _doc.ParameterBindings;

            foreach (var req in Requirements)
            {
                var res = new ValidationResult { Requirement = req };
                
                // Ищем привязку параметра по имени
                ElementBinding binding = FindBindingByName(bindingMap, req.Name, out Definition definition);

                if (binding == null)
                {
                    res.Status = ValidationStatus.Missing;
                }
                else
                {
                    // Проверяем категории
                    var boundCategories = binding.Categories;
                    bool allFound = true;

                    foreach (var bic in req.TargetCategories)
                    {
                        Category cat = _doc.Settings.Categories.get_Item(bic);
                        if (cat == null) continue;

                        if (!boundCategories.Contains(cat))
                        {
                            allFound = false;
                            res.MissingCategories.Add(cat);
                            res.MissingCategoriesNames.Add(cat.Name);
                        }
                    }

                    res.Status = allFound ? ValidationStatus.OK : ValidationStatus.Partial;
                }

                results.Add(res);
            }

            return results;
        }

        public void FixParameters()
        {
            using (Transaction trans = new Transaction(_doc, "Исправление параметров АГР"))
            {
                trans.Start();

                BindingMap bindingMap = _doc.ParameterBindings;
                var results = CheckParameters();

                foreach (var res in results.Where(r => r.Status == ValidationStatus.Partial))
                {
                    ElementBinding binding = FindBindingByName(bindingMap, res.Requirement.Name, out Definition def);
                    
                    if (binding is InstanceBinding instBinding)
                    {
                        CategorySet newCats = instBinding.Categories;
                        foreach (var cat in res.MissingCategories)
                        {
                            newCats.Insert(cat);
                        }

                        _doc.ParameterBindings.ReInsert(def, _doc.Application.Create.NewInstanceBinding(newCats));
                    }
                }

                trans.Commit();
            }
        }

        public void CreateMissingParameters()
        {
            var results = CheckParameters();
            var missing = results.Where(r => r.Status == ValidationStatus.Missing).ToList();

            if (!missing.Any()) return;

            string tempFile = Path.Combine(Path.GetTempPath(), "BM_Temp_SP.txt");
            string originalFile = _doc.Application.SharedParametersFilename;

            try
            {
                // 1. Создаем временный файл общих параметров
                if (File.Exists(tempFile)) File.Delete(tempFile);
                using (File.Create(tempFile)) { }

                _doc.Application.SharedParametersFilename = tempFile;
                DefinitionFile defFile = _doc.Application.OpenSharedParameterFile();
                DefinitionGroup group = defFile.Groups.Create("BM_AGR");

                using (Transaction trans = new Transaction(_doc, "Создание параметров АГР"))
                {
                    trans.Start();

                    foreach (var res in missing)
                    {
                        // Создаем определение
                        ExternalDefinitionCreationOptions opt = new ExternalDefinitionCreationOptions(res.Requirement.Name, res.Requirement.DataType);
                        Definition def = group.Definitions.Create(opt);

                        // Собираем категории
                        CategorySet cats = _doc.Application.Create.NewCategorySet();
                        foreach (var bic in res.Requirement.TargetCategories)
                        {
                            Category cat = _doc.Settings.Categories.get_Item(bic);
                            if (cat != null) cats.Insert(cat);
                        }

                        // Привязываем как параметр экземпляра в группу "Данные"
                        InstanceBinding binding = _doc.Application.Create.NewInstanceBinding(cats);
                        _doc.ParameterBindings.Insert(def, binding, GroupTypeId.Data);
                    }

                    trans.Commit();
                }
            }
            finally
            {
                _doc.Application.SharedParametersFilename = originalFile;
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        private ElementBinding FindBindingByName(BindingMap map, string name, out Definition def)
        {
            def = null;
            DefinitionBindingMapIterator it = map.ForwardIterator();
            while (it.MoveNext())
            {
                if (it.Key.Name == name)
                {
                    def = it.Key;
                    return it.Current as ElementBinding;
                }
            }
            return null;
        }
    }
}
