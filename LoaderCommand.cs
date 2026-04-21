using System;
using System.IO;
using System.Reflection;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace BM_AGR_Loader
{
    [Transaction(TransactionMode.Manual)]
    public class LoaderCommand : IExternalCommand
    {
        private static Assembly _loadedAssembly;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // 1. Путь к логике В КОРНЕ (как в 1.1.8)
                string loaderPath = Assembly.GetExecutingAssembly().Location;
                string logicDllPath = Path.Combine(Path.GetDirectoryName(loaderPath), "BM_AGR_PARAM.dll");

                if (!File.Exists(logicDllPath))
                {
                    message = "Файл логики не найден: " + logicDllPath;
                    return Result.Failed;
                }

                // 2. Читаем байты (Hot Reload)
                byte[] assemblyBytes = File.ReadAllBytes(logicDllPath);
                _loadedAssembly = Assembly.Load(assemblyBytes);

                // 3. РЕЗОЛВЕР ДЛЯ XAML (КРИТИЧНО!)
                // Помогает WPF найти типы внутри сборки, загруженной из памяти
                ResolveEventHandler resolver = (sender, args) =>
                {
                    if (args.Name.Contains("BM_AGR_PARAM")) return _loadedAssembly;
                    return null;
                };

                AppDomain.CurrentDomain.AssemblyResolve += resolver;

                try
                {
                    var type = _loadedAssembly.GetType("BM_AGR_PARAM.Commands.CheckCommand");
                    if (type == null)
                    {
                        message = "Класс CheckCommand не найден в сборке.";
                        return Result.Failed;
                    }

                    object instance = Activator.CreateInstance(type);
                    object[] parameters = new object[] { commandData, message, elements };

                    return (Result)type.GetMethod("Execute").Invoke(instance, parameters);
                }
                finally
                {
                    // Оставляем резолвер жить, пока открыто окно (или на всякий случай)
                    // AppDomain.CurrentDomain.AssemblyResolve -= resolver;
                }
            }
            catch (Exception ex)
            {
                var realEx = ex.InnerException ?? ex;
                message = "Ошибка Hot Reload (v4.0.0):\n" + realEx.Message + "\n\n" + realEx.StackTrace;
                return Result.Failed;
            }
        }
    }
}
