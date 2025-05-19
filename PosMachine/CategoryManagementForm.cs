// CategoryManagementForm.cs
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using POSMachine.Data;
using POSMachine.Models;

namespace POSMachine
{
    public partial class CategoryManagementForm : Form
    {
        private List<Category> _categories;
        private Category _currentCategory;

        public CategoryManagementForm()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                _categories = DatabaseHelper.GetAllCategories();

                // Populate category list
                lstCategories.Items.Clear();
                foreach (var category in _categories)
                {
                    lstCategories.Items.Add(category.Name);
                }

                // Clear form
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearForm()
        {
            _currentCategory = null;
            txtName.Text = "";

            btnSave.Text = "Add Category";
            btnDelete.Enabled = false;
        }

        private void lstCategories_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstCategories.SelectedIndex >= 0 && lstCategories.SelectedIndex < _categories.Count)
            {
                _currentCategory = _categories[lstCategories.SelectedIndex];

                // Populate form with selected category
                txtName.Text = _currentCategory.Name;

                // Update buttons
                btnSave.Text = "Update Category";
                btnDelete.Enabled = true;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validate form
            if (string.IsNullOrEmpty(txtName.Text.Trim()))
            {
                MessageBox.Show("Please enter a category name.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string categoryName = txtName.Text.Trim();

            // Check if category name already exists (for new categories)
            if (_currentCategory == null && _categories.Any(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("A category with this name already exists.",
                    "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Create or update category
                if (_currentCategory == null)
                {
                    _currentCategory = new Category { Name = categoryName };
                }
                else
                {
                    _currentCategory.Name = categoryName;
                }

                // Save to database
                DatabaseHelper.SaveCategory(_currentCategory);

                // Reload categories
                LoadCategories();

                MessageBox.Show("Category saved successfully.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving category: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (_currentCategory == null) return;

            var result = MessageBox.Show($"Are you sure you want to delete '{_currentCategory.Name}'?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.DeleteCategory(_currentCategory.Id);

                    // Reload categories
                    LoadCategories();

                    MessageBox.Show("Category deleted successfully.",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting category: {ex.Message}",
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