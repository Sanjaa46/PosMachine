// ProductManagementForm.cs
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using POSMachine.Data;
using POSMachine.Models;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace POSMachine
{
    public partial class ProductManagementForm : Form
    {
        private List<Product> _products;
        private List<Category> _categories;
        private Product _currentProduct;

        public ProductManagementForm()
        {
            InitializeComponent();
            LoadCategories();
            LoadProducts();
        }

        private void LoadCategories()
        {
            try
            {
                _categories = DatabaseHelper.GetAllCategories();

                // Populate category dropdown
                cboCategory.Items.Clear();
                foreach (var category in _categories)
                {
                    cboCategory.Items.Add(category.Name);
                }

                if (cboCategory.Items.Count > 0)
                {
                    cboCategory.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadProducts()
        {
            try
            {
                _products = DatabaseHelper.GetAllProducts();

                // Populate product list
                lstProducts.Items.Clear();
                foreach (var product in _products)
                {
                    lstProducts.Items.Add($"{product.Code} - {product.Name} (${product.Price})");
                }

                // Clear form
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            _currentProduct = null;
            txtCode.Text = "";
            txtName.Text = "";
            txtPrice.Text = "";
            if (cboCategory.Items.Count > 0)
            {
                cboCategory.SelectedIndex = 0;
            }
            pictureBox.Image = null;

            btnSave.Text = "Add Product";
            btnDelete.Enabled = false;
        }

        private void lstProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstProducts.SelectedIndex >= 0 && lstProducts.SelectedIndex < _products.Count)
            {
                _currentProduct = _products[lstProducts.SelectedIndex];

                // Populate form with selected product
                txtCode.Text = _currentProduct.Code;
                txtName.Text = _currentProduct.Name;
                txtPrice.Text = _currentProduct.Price.ToString("0.00");

                // Select category
                var category = _categories.FirstOrDefault(c => c.Id == _currentProduct.CategoryId);
                if (category != null)
                {
                    int index = cboCategory.Items.IndexOf(category.Name);
                    if (index >= 0)
                    {
                        cboCategory.SelectedIndex = index;
                    }
                }

                // Show image if available
                if (_currentProduct.Image != null && _currentProduct.Image.Length > 0)
                {
                    try
                    {
                        using (var ms = new MemoryStream(_currentProduct.Image))
                        {
                            pictureBox.Image = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading product image: {ex.Message}",
                            "Image Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        pictureBox.Image = null;
                    }
                }
                else
                {
                    pictureBox.Image = null;
                }

                // Update buttons
                btnSave.Text = "Update Product";
                btnDelete.Enabled = true;
            }
        }

        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
                openFileDialog.Title = "Select Product Image";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        pictureBox.Image = Image.FromFile(openFileDialog.FileName);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading image: {ex.Message}",
                            "Image Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validate form
            if (string.IsNullOrEmpty(txtCode.Text) || string.IsNullOrEmpty(txtName.Text) ||
                string.IsNullOrEmpty(txtPrice.Text) || cboCategory.SelectedIndex < 0)
            {
                MessageBox.Show("Please fill in all required fields.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if product code already exists (for new products)
            if (_currentProduct == null && _products.Any(p => p.Code == txtCode.Text.Trim()))
            {
                MessageBox.Show("A product with this code already exists.",
                    "Duplicate Code", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get selected category
            var categoryName = cboCategory.SelectedItem.ToString();
            var category = _categories.FirstOrDefault(c => c.Name == categoryName);

            if (category == null)
            {
                MessageBox.Show("Selected category not found.",
                    "Category Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Create or update product
                if (_currentProduct == null)
                {
                    _currentProduct = new Product();
                }

                _currentProduct.Code = txtCode.Text.Trim();
                _currentProduct.Name = txtName.Text.Trim();
                _currentProduct.Price = price;
                _currentProduct.CategoryId = category.Id;

                // Get image data if available
                if (pictureBox.Image != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        pictureBox.Image.Save(ms, ImageFormat.Jpeg);
                        _currentProduct.Image = ms.ToArray();
                    }
                }
                else
                {
                    _currentProduct.Image = null;
                }

                // Save to database
                DatabaseHelper.SaveProduct(_currentProduct);

                // Reload products
                LoadProducts();

                MessageBox.Show("Product saved successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_currentProduct == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete '{_currentProduct.Name}'?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.DeleteProduct(_currentProduct.Id);

                    // Reload products
                    LoadProducts();

                    MessageBox.Show("Product deleted successfully.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting product: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}