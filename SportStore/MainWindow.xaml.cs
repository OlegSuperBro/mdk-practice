using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SportStore.Models;

namespace SportStore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<string> sortList = new List<string>() { "По возрастанию цены", "По убыванию цены" };
        List<string> filterList;

        public MainWindow(User user)
        {
            InitializeComponent();

            using (SportStoreContext db = new SportStoreContext())
            {
                if (user != null)
                {
                    statusUser.Text = user.RoleNavigation.Name;
                }
                else
                {
                    statusUser.Text = "Гость";
                }
                productlistView.ItemsSource = db.Products.ToList();
            
                sortUserComboBox.ItemsSource = sortList;

                filterList = db.Products.Select(u => u.Manufacturer).Distinct().ToList();
                filterList.Insert(0, "Все производители");
                filterUserComboBox.ItemsSource = filterList.ToList();
                countProducts.Text = $"Количество: {db.Products.Count()}";
            }
        }

        private void exitButtonClick(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
        private void sortUserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }
        private void filterUserComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateProducts();
        }
        private void searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProducts();
        }
        private void UpdateProducts()
        {
            using (SportStoreContext db = new SportStoreContext())
            {
                var currentProducts = db.Products.ToList();
                productlistView.ItemsSource = currentProducts;

                //Сортировка
                if (sortUserComboBox.SelectedIndex != -1)
                {
                    if (sortUserComboBox.SelectedValue == "По убыванию цены")
                    {
                        currentProducts = currentProducts.OrderByDescending(u => u.Cost).ToList();
                    }

                    if (sortUserComboBox.SelectedValue == "По возрастанию цены")
                    {
                        currentProducts = currentProducts.OrderBy(u => u.Cost).ToList();
                    }
                }

                // Фильтрация
                if (filterUserComboBox.SelectedIndex != -1)
                {
                    if (db.Products.Select(u => u.Manufacturer).Distinct().ToList().Contains(filterUserComboBox.SelectedValue))
                    {
                        currentProducts = currentProducts.Where(u => u.Manufacturer == filterUserComboBox.SelectedValue.ToString()).ToList();
                    }
                    else
                    {
                        currentProducts = currentProducts.ToList();
                    }
                }

                // Поиск

                if (searchBox.Text.Length > 0)
                {
                    currentProducts = currentProducts.Where(u => u.Name.Contains(searchBox.Text) || u.Description.Contains(searchBox.Text)).ToList();
                }
                productlistView.ItemsSource = currentProducts;

                countProducts.Text = $"Количество: {currentProducts.Count} из {db.Products.ToList().Count}";
            }
        }
        private void сlearButton_Click(object sender, RoutedEventArgs e)
        {
            searchBox.Text = "";
            sortUserComboBox.SelectedIndex = -1;
            filterUserComboBox.SelectedIndex = -1;
        }
    }
}
