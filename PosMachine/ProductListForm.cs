using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using POSMachine.Data;
using POSMachine.Models;

namespace POSMachine
{
    public partial class ProductListForm : Form
    {
        private List<Product> _products;
        private List<Category> _categories;
        private User _currentUser;

        public ProductListForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _products = DatabaseHelper.GetAllProducts();
                _categories = DatabaseHelper.GetAllCategories();

                // Populate category filter
                cboFilter.Items.Clear();
                cboFilter.Items.Add("All Categories");
                foreach (var category in _categories)
                {
                    cboFilter.Items.Add(category.Name);
                }
                cboFilter.SelectedIndex = 0;

                // Display products
                DisplayProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayProducts(int? categoryId = null)
        {
            // Clear grid
            dataGridViewProducts.Rows.Clear();

            // Filter products by category if needed
            var filteredProducts = categoryId.HasValue
                ? _products.Where(p => p.CategoryId == categoryId.Value).ToList()
                : _products;

            // Add products to grid
            foreach (var product in filteredProducts)
            {
                var category = _categories.FirstOrDefault(c => c.Id == product.CategoryId);
                string categoryName = category != null ? category.Name : "Uncategorized";

                int rowIndex = dataGridViewProducts.Rows.Add();
                DataGridViewRow row = dataGridViewProducts.Rows[rowIndex];

                row.Cells["Id"].Value = product.Id;
                row.Cells["Code"].Value = product.Code;
                row.Cells["Name"].Value = product.Name;
                row.Cells["Price"].Value = product.Price;
                row.Cells["Category"].Value = categoryName;

                // Add image if available
                if (product.Image != null && product.Image.Length > 0)
                {
                    try
                    {
                        // Create a copy of the image data
                        byte[] imageData = (byte[])product.Image.Clone();

                        // Load the image
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            //Image image = Image.FromStream(ms);
                            //row.Cells["Image"].Value = image;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading image: {ex.Message}");
                        row.Cells["Image"].Value = null;
                    }
                }
            }
        }

        private void cboFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboFilter.SelectedIndex == 0)
            {
                // All categories
                DisplayProducts();
            }
            else
            {
                // Specific category
                string categoryName = cboFilter.SelectedItem.ToString();
                var category = _categories.FirstOrDefault(c => c.Name == categoryName);

                if (category != null)
                {
                    DisplayProducts(category.Id);
                }
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchTerm))
            {
                // Reset to current filter
                cboFilter_SelectedIndexChanged(sender, e);
                return;
            }

            // Clear grid
            dataGridViewProducts.Rows.Clear();

            // Filter products by search term
            var filteredProducts = _products.Where(p =>
                p.Code.ToLower().Contains(searchTerm) ||
                p.Name.ToLower().Contains(searchTerm)).ToList();

            // Further filter by category if needed
            if (cboFilter.SelectedIndex > 0)
            {
                string categoryName = cboFilter.SelectedItem.ToString();
                var category = _categories.FirstOrDefault(c => c.Name == categoryName);

                if (category != null)
                {
                    filteredProducts = filteredProducts.Where(p => p.CategoryId == category.Id).ToList();
                }
            }

            // Add filtered products to grid
            foreach (var product in filteredProducts)
            {
                var category = _categories.FirstOrDefault(c => c.Id == product.CategoryId);
                string categoryName = category != null ? category.Name : "Uncategorized";

                int rowIndex = dataGridViewProducts.Rows.Add();
                DataGridViewRow row = dataGridViewProducts.Rows[rowIndex];

                row.Cells["Id"].Value = product.Id;
                row.Cells["Code"].Value = product.Code;
                row.Cells["Name"].Value = product.Name;
                row.Cells["Price"].Value = product.Price;
                row.Cells["Category"].Value = categoryName;

                // Add image if available
                if (product.Image != null && product.Image.Length > 0)
                {
                    try
                    {
                        // Create a copy of the image data
                        byte[] imageData = (byte[])product.Image.Clone();

                        // Load the image
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            //Image image = Image.FromStream(ms);
                            //row.Cells["Image"].Value = image;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading image: {ex.Message}");
                        row.Cells["Image"].Value = null;
                    }
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}