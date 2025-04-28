using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PosMachine.Data;
using PosMachine.Models;
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
            if (_currentCategory == null && _categories.Any(c => c.Name == categoryName))
            {
                MessageBox.Show("A category with this name already exists.",
                    "Duplicate Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Create or update category in database
                using (var connection = new System.Data.SQLite.SQLiteConnection(
                    "Data Source=posdb.sqlite;Version=3;"))
                {
                    connection.Open();

                    if (_currentCategory == null)
                    {
                        // Insert new category
                        string sql = "INSERT INTO Categories (Name) VALUES (@Name); SELECT last_insert_rowid();";

                        using (var command = new System.Data.SQLite.SQLiteCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Name", categoryName);
                            int categoryId = Convert.ToInt32(command.ExecuteScalar());

                            _currentCategory = new Category { Id = categoryId, Name = categoryName };
                        }
                    }
                    else
                    {
                        // Update existing category
                        string sql = "UPDATE Categories SET Name = @Name WHERE Id = @Id;";

                        using (var command = new System.Data.SQLite.SQLiteCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Name", categoryName);
                            command.Parameters.AddWithValue("@Id", _currentCategory.Id);
                            command.ExecuteNonQuery();

                            _currentCategory.Name = categoryName;
                        }
                    }
                }

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

            // Check if category is in use
            bool categoryInUse = false;
            using (var connection = new System.Data.SQLite.SQLiteConnection(
                "Data Source=posdb.sqlite;Version=3;"))
            {
                connection.Open();

                string sql = "SELECT COUNT(*) FROM Products WHERE CategoryId = @CategoryId;";

                using (var command = new System.Data.SQLite.SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@CategoryId", _currentCategory.Id);
                    int count = Convert.ToInt32(command.ExecuteScalar());

                    categoryInUse = count > 0;
                }
            }

            if (categoryInUse)
            {
                MessageBox.Show("This category cannot be deleted because it is used by one or more products.",
                    "Category In Use", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete '{_currentCategory.Name}'?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    // Delete from database
                    using (var connection = new System.Data.SQLite.SQLiteConnection(
                        "Data Source=posdb.sqlite;Version=3;"))
                    {
                        connection.Open();

                        string sql = "DELETE FROM Categories WHERE Id = @Id;";

                        using (var command = new System.Data.SQLite.SQLiteCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@Id", _currentCategory.Id);
                            command.ExecuteNonQuery();
                        }
                    }

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

        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lstCategories = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.txtName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LimeGreen;
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(584, 60);
            this.panel1.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.Silver;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(497, 12);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 35);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(12, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(216, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Category Management";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 60);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lstCategories);
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel2);
            this.splitContainer1.Panel2.Controls.Add(this.txtName);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Size = new System.Drawing.Size(584, 301);
            this.splitContainer1.SplitterDistance = 194;
            this.splitContainer1.TabIndex = 1;
            // 
            // lstCategories
            // 
            this.lstCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCategories.FormattingEnabled = true;
            this.lstCategories.Location = new System.Drawing.Point(12, 31);
            this.lstCategories.Name = "lstCategories";
            this.lstCategories.Size = new System.Drawing.Size(170, 251);
            this.lstCategories.TabIndex = 1;
            this.lstCategories.SelectedIndexChanged += new System.EventHandler(this.lstCategories_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Categories List:";
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.btnClear);
            this.panel2.Controls.Add(this.btnDelete);
            this.panel2.Controls.Add(this.btnSave);
            this.panel2.Location = new System.Drawing.Point(3, 249);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(380, 49);
            this.panel2.TabIndex = 3;
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.Silver;
            this.btnClear.FlatAppearance.BorderSize = 0;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.ForeColor = System.Drawing.Color.White;
            this.btnClear.Location = new System.Drawing.Point(258, 7);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(94, 35);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Clear Form";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.Red;
            this.btnDelete.Enabled = false;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(139, 7);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(113, 35);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "Delete Category";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.LimeGreen;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(14, 7);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(113, 35);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Add Category";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(69, 41);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(264, 20);
            this.txtName.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(25, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Name:";
            // 
            // CategoryManagementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Name = "CategoryManagementForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Category Management";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}