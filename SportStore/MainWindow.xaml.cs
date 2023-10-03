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
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SportStore.Models;
using SportStore.Resources;

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

                adminButtons.Visibility = Visibility.Hidden;
                if (user.RoleNavigation.Name == "Администратор")
                {
                    adminButtons.Visibility = Visibility.Visible;
                }
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

        private void addUserButtonClick(object sender, RoutedEventArgs e)
        {
            new AddProductWindow(null).ShowDialog();

        }
        private void EditProduct_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Product p = (sender as ListView).SelectedItem as Product;
            new AddProductWindow(p).ShowDialog();
        }
        private void delUserButton(object sender, RoutedEventArgs e)
        {
            using (SportStoreContext db = new SportStoreContext())
            {
                var product = (productlistView.SelectedItem) as Product;

                if (CanDeleteProduct(product))
                {
                    if (product != null)
                    {

                        if (MessageBox.Show($"Вы точно хотите удалить {product.Name}", "Внимание!",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            db.Products.Remove(product);
                            db.SaveChanges();
                            MessageBox.Show($"Товар {product.Name} удален!");
                            productlistView.ItemsSource = db.Products.ToList();
                            countProducts.Text = $"Количество: {db.Products.Count()}";
                        }
                    }
                }
            }
        }
        private bool CanDeleteProduct(Product product)
        {
            using (SportStoreContext db = new SportStoreContext())
            {
                // найдем все связанные товары с данным товаром
                List<RelatedProduct> rp = db.RelatedProducts.Where(p => p.ProductId == product.Id).ToList();

                // найдем, существуют ли товарные позиции из нашего списка связанных товаров и самого товара в заказах
                foreach (RelatedProduct r in rp)
                {
                    OrderProduct order = db.OrderProducts.Where(o => o.ProductId == r.RelatedProdutId).FirstOrDefault() as OrderProduct;
                    if (order is not null)
                    {
                        Product p = db.Products.Where(p => p.Id == r.RelatedProdutId).FirstOrDefault() as Product;
                        MessageBox.Show($"Товар {p.Name} связан с товаром {product.Name} присутствует в товарной позиции заказа {order.OrderId}. \n Товары нельзя удалить!");
                        return false;
                    }
                }


                return true;
            }
        }
    }
}
