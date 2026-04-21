using System;
using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;
using BM_AGR_PARAM.Services;
using BM_AGR_PARAM.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BM_AGR_PARAM.UI
{
    public partial class CheckWindow : Window, INotifyPropertyChanged
    {
        private readonly AgrService _service;
        private List<ValidationResult> _results;

        public List<ValidationResult> Results
        {
            get => _results;
            set { _results = value; OnPropertyChanged(); }
        }

        public CheckWindow(Document doc)
        {
            InitializeComponent();
            _service = new AgrService(doc);
            this.DataContext = this;
            
            // Безопасное получение версии для Hot Reload (Location может быть пуст)
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            string version = "7.1.1";
            
            try {
                var attr = Attribute.GetCustomAttribute(assembly, typeof(System.Reflection.AssemblyFileVersionAttribute)) 
                           as System.Reflection.AssemblyFileVersionAttribute;
                if (attr != null) version = attr.Version;
            } catch { }

            this.Title = $"BM AGR Parameter Manager v{version}";

            RefreshList();
        }

        private void RefreshList()
        {
            Results = _service.CheckParameters();
        }

        private void Check_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void Fix_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _service.FixParameters();
                RefreshList();
                MessageBox.Show("Исправление завершено!", "АГР Параметры", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при исправлении: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
