using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using POSMachine;
using POSMachine.Data;
using POSMachine.Models;

namespace POSMachine
{
    public partial class MainForm : Form
    {
        private User _currentUser;
        private List<Product> _products;
        private Order _currentOrder;
        private List<Category> _categories;

        public MainForm(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _products = DatabaseHelper.GetAllProducts();
            _categories = DatabaseHelper.GetAllCategories();

            try
            {
                // Initialize a new order
                NewOrder();

                // Set up the UI based on user role
                SetupUserInterface();

                // Load products into the product panel
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing the main form: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void NewOrder()
        {
            _currentOrder = new Order
            {
                OrderDate = DateTime.Now,
                UserId = _currentUser.Id,
                Items = new List<OrderItem>()
            };

            // Clear the cart display
            dataGridViewCart.Rows.Clear();

            // Reset the totals
            UpdateTotals();
        }

        private void SetupUserInterface()
        {
            // Set the title with current user
            this.Text = $"Simple POS - Modern Design - Logged in as {_currentUser.Username}";

            // Set up menu visibility based on role
            if (_currentUser.Role == UserRole.Manager)
            {
                // Manager sees all menu items
                productsToolStripMenuItem.Visible = true;
                categoriesToolStripMenuItem.Visible = true;
                helpToolStripMenuItem.Visible = true;
            }
            else
            {
                // Cashier sees limited menu items
                productsToolStripMenuItem.Visible = true;
                categoriesToolStripMenuItem.Visible = false;
                helpToolStripMenuItem.Visible = true;

                // Disable edit/delete for cashiers in the products menu
                manageProductsToolStripMenuItem.Visible = false;
            }
        }

        private void LoadProducts()
        {
            // Clear the product panel
            flowLayoutPanelProducts.Controls.Clear();

            // Group products by category
            var productsByCategory = _products.GroupBy(p => p.CategoryId);

            foreach (var categoryGroup in productsByCategory)
            {
                var category = _categories.FirstOrDefault(c => c.Id == categoryGroup.Key);
                string categoryName = category != null ? category.Name : "Uncategorized";

                // Create a modern category header
                Panel categoryPanel = new Panel
                {
                    Width = flowLayoutPanelProducts.Width - 40,
                    Height = 50,
                    BackColor = Color.FromArgb(55, 65, 81),
                    Margin = new Padding(5, 10, 5, 5)
                };

                Label categoryLabel = new Label
                {
                    Text = categoryName,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = false,
                    Width = categoryPanel.Width - 20,
                    Height = categoryPanel.Height,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Location = new Point(15, 0)
                };

                categoryPanel.Controls.Add(categoryLabel);
                flowLayoutPanelProducts.Controls.Add(categoryPanel);

                // Add products for this category with modern card design
                foreach (var product in categoryGroup)
                {
                    // Create modern product card
                    Panel productPanel = new Panel
                    {
                        Width = 200,
                        Height = 280,
                        Margin = new Padding(10),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle,
                        Cursor = Cursors.Hand
                    };

                    // Add product image container with modern styling
                    Panel imageContainer = new Panel
                    {
                        Width = 180,
                        Height = 140,
                        Location = new Point(10, 10),
                        BackColor = Color.FromArgb(248, 250, 252),
                        BorderStyle = BorderStyle.None
                    };

                    PictureBox productImage = new PictureBox
                    {
                        Width = 160,
                        Height = 120,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Location = new Point(10, 10),
                        BorderStyle = BorderStyle.None,
                        Cursor = Cursors.Hand
                    };

                    if (product.Image != null && product.Image.Length > 0)
                    {
                        try
                        {
                            using (var ms = new System.IO.MemoryStream(product.Image))
                            {
                                productImage.Image = Image.FromStream(ms);
                            }
                        }
                        catch
                        {
                            // Use placeholder if image fails to load
                            productImage.BackColor = Color.FromArgb(229, 231, 235);
                        }
                    }
                    else
                    {
                        // Modern placeholder design
                        productImage.BackColor = Color.FromArgb(229, 231, 235);
                    }

                    imageContainer.Controls.Add(productImage);
                    productPanel.Controls.Add(imageContainer);

                    // Add product code with modern styling
                    Label codeLabel = new Label
                    {
                        Text = $"#{product.Code}",
                        Location = new Point(15, 160),
                        Width = 170,
                        Height = 20,
                        Font = new Font("Segoe UI", 9, FontStyle.Regular),
                        ForeColor = Color.FromArgb(107, 114, 128),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Cursor = Cursors.Hand
                    };
                    productPanel.Controls.Add(codeLabel);

                    // Add product name with modern typography
                    Label nameLabel = new Label
                    {
                        Text = product.Name,
                        Location = new Point(15, 185),
                        Width = 170,
                        Height = 40,
                        Font = new Font("Segoe UI", 11, FontStyle.Bold),
                        ForeColor = Color.FromArgb(55, 65, 81),
                        TextAlign = ContentAlignment.TopLeft,
                        Cursor = Cursors.Hand
                    };
                    productPanel.Controls.Add(nameLabel);

                    // Add modern price label with accent color
                    Panel pricePanel = new Panel
                    {
                        Width = 170,
                        Height = 35,
                        Location = new Point(15, 235),
                        BackColor = Color.FromArgb(37, 99, 235),
                        Cursor = Cursors.Hand
                    };

                    Label priceLabel = new Label
                    {
                        Text = $"${product.Price:0.00}",
                        Width = pricePanel.Width,
                        Height = pricePanel.Height,
                        Font = new Font("Segoe UI", 12, FontStyle.Bold),
                        ForeColor = Color.White,
                        TextAlign = ContentAlignment.MiddleCenter,
                        BackColor = Color.Transparent,
                        Cursor = Cursors.Hand
                    };

                    pricePanel.Controls.Add(priceLabel);
                    productPanel.Controls.Add(pricePanel);

                    // Store product reference in Tag for easy access
                    productPanel.Tag = product;
                    imageContainer.Tag = product;
                    productImage.Tag = product;
                    nameLabel.Tag = product;
                    pricePanel.Tag = product;
                    priceLabel.Tag = product;
                    codeLabel.Tag = product;

                    // Add hover effects
                    productPanel.MouseEnter += (s, e) =>
                    {
                        productPanel.BackColor = Color.FromArgb(248, 250, 252);
                        pricePanel.BackColor = Color.FromArgb(29, 78, 216);
                    };

                    productPanel.MouseLeave += (s, e) =>
                    {
                        productPanel.BackColor = Color.White;
                        pricePanel.BackColor = Color.FromArgb(37, 99, 235);
                    };

                    // Add click events to all components using the Tag property
                    productPanel.Click += (s, e) => {
                        var p = (Product)((Control)s).Tag;
                        if (p != null) AddProductToCart(p);
                    };

                    imageContainer.Click += (s, e) => {
                        var p = (Product)((Control)s).Tag;
                        if (p != null) AddProductToCart(p);
                    };

                    productImage.Click += (s, e) => {
                        var p = (Product)((Control)s).Tag;
                        if (p != null) AddProductToCart(p);
                    };

                    nameLabel.Click += (s, e) => {
                        var p = (Product)((Control)s).Tag;
                        if (p != null) AddProductToCart(p);
                    };

                    pricePanel.Click += (s, e) => {
                        var p = (Product)((Control)s).Tag;
                        if (p != null) AddProductToCart(p);
                    };

                    priceLabel.Click += (s, e) => {
                        var p = (Product)((Control)s).Tag;
                        if (p != null) AddProductToCart(p);
                    };

                    codeLabel.Click += (s, e) => {
                        var p = (Product)((Control)s).Tag;
                        if (p != null) AddProductToCart(p);
                    };

                    flowLayoutPanelProducts.Controls.Add(productPanel);
                }
            }
        }

        private void AddProductToCart(Product product)
        {
            // Check if the product is already in the cart
            var existingItem = _currentOrder.Items.FirstOrDefault(item => item.ProductId == product.Id);

            if (existingItem != null)
            {
                // Increment quantity of existing item
                existingItem.Quantity++;

                // Update the datagrid display
                foreach (DataGridViewRow row in dataGridViewCart.Rows)
                {
                    if (Convert.ToInt32(row.Cells["ProductId"].Value) == product.Id)
                    {
                        row.Cells["Quantity"].Value = existingItem.Quantity;
                        row.Cells["Total"].Value = existingItem.Total.ToString("N2");
                        break;
                    }
                }
            }
            else
            {
                // Add new item to the order
                var newItem = new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1
                };

                _currentOrder.Items.Add(newItem);

                // Add to the datagrid
                int rowIndex = dataGridViewCart.Rows.Add();
                DataGridViewRow row = dataGridViewCart.Rows[rowIndex];

                row.Cells["ProductId"].Value = newItem.ProductId;
                row.Cells["ProductName"].Value = newItem.ProductName;
                row.Cells["Price"].Value = newItem.Price.ToString("N2");
                row.Cells["Quantity"].Value = newItem.Quantity;
                row.Cells["Total"].Value = newItem.Total.ToString("N2");
            }

            // Update the order totals
            UpdateTotals();
        }

        private void RemoveFromCart(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < dataGridViewCart.Rows.Count)
            {
                int productId = Convert.ToInt32(dataGridViewCart.Rows[rowIndex].Cells["ProductId"].Value);

                // Remove from order items
                var item = _currentOrder.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    _currentOrder.Items.Remove(item);
                }

                // Remove from grid
                dataGridViewCart.Rows.RemoveAt(rowIndex);

                // Update totals
                UpdateTotals();
            }
        }

        private void IncreaseQuantity(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < dataGridViewCart.Rows.Count)
            {
                int productId = Convert.ToInt32(dataGridViewCart.Rows[rowIndex].Cells["ProductId"].Value);

                // Update order item
                var item = _currentOrder.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    item.Quantity++;

                    // Update grid
                    dataGridViewCart.Rows[rowIndex].Cells["Quantity"].Value = item.Quantity;
                    dataGridViewCart.Rows[rowIndex].Cells["Total"].Value = item.Total.ToString("N2");

                    // Update totals
                    UpdateTotals();
                }
            }
        }

        private void DecreaseQuantity(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < dataGridViewCart.Rows.Count)
            {
                int productId = Convert.ToInt32(dataGridViewCart.Rows[rowIndex].Cells["ProductId"].Value);

                // Update order item
                var item = _currentOrder.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    if (item.Quantity > 1)
                    {
                        item.Quantity--;

                        // Update grid
                        dataGridViewCart.Rows[rowIndex].Cells["Quantity"].Value = item.Quantity;
                        dataGridViewCart.Rows[rowIndex].Cells["Total"].Value = item.Total.ToString("N2");
                    }
                    else
                    {
                        // Remove if quantity would be 0
                        RemoveFromCart(rowIndex);
                        return;
                    }

                    // Update totals
                    UpdateTotals();
                }
            }
        }

        private void UpdateTotals()
        {
            decimal total = _currentOrder.Items.Sum(item => item.Total);
            _currentOrder.Subtotal = total;
            _currentOrder.Total = total;

            lblTotalValue.Text = total.ToString("N2");
        }

        private void btnPay_Click(object sender, EventArgs e)
        {
            if (_currentOrder.Items.Count == 0)
            {
                MessageBox.Show("Please add items to the cart before proceeding to payment.",
                    "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Show payment form
            using (var paymentForm = new PaymentForm(_currentOrder.Total))
            {
                if (paymentForm.ShowDialog() == DialogResult.OK)
                {
                    // Complete the order
                    _currentOrder.AmountPaid = paymentForm.AmountPaid;
                    _currentOrder.Change = paymentForm.Change;

                    try
                    {
                        // Save the order to database
                        int orderId = DatabaseHelper.SaveOrder(_currentOrder);

                        // Print receipt
                        PrintReceipt(_currentOrder);

                        // Create a new order
                        NewOrder();

                        MessageBox.Show($"Order #{orderId} completed successfully!",
                            "Order Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving order: {ex.Message}",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void PrintReceipt(Order order)
        {
            // Create a new form to display the receipt
            using (var receiptForm = new ReceiptForm(order))
            {
                receiptForm.ShowDialog();
            }
        }

        private void btnNewInvoice_Click(object sender, EventArgs e)
        {
            if (_currentOrder.Items.Count > 0)
            {
                var result = MessageBox.Show("Current order will be discarded. Continue?",
                    "New Invoice", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    NewOrder();
                }
            }
            else
            {
                NewOrder();
            }
        }

        private void txtProductCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;

                string code = txtProductCode.Text.Trim();
                if (!string.IsNullOrEmpty(code))
                {
                    var product = DatabaseHelper.GetProductByCode(code);

                    if (product != null)
                    {
                        AddProductToCart(product);
                        txtProductCode.Clear();
                    }
                    else
                    {
                        MessageBox.Show($"Product with code '{code}' not found.",
                            "Product Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void dataGridViewCart_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Handle button clicks in the datagrid
                if (e.ColumnIndex == dataGridViewCart.Columns["Delete"].Index)
                {
                    RemoveFromCart(e.RowIndex);
                }
                else if (e.ColumnIndex == dataGridViewCart.Columns["Increase"].Index)
                {
                    IncreaseQuantity(e.RowIndex);
                }
                else if (e.ColumnIndex == dataGridViewCart.Columns["Decrease"].Index)
                {
                    DecreaseQuantity(e.RowIndex);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_currentOrder.Items.Count > 0)
            {
                var result = MessageBox.Show("Current order will be discarded. Exit anyway?",
                    "Exit Application", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
            else
            {
                Application.Exit();
            }
        }

        private void viewProductsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open the product list form
            using (var productListForm = new ProductListForm(_currentUser))
            {
                productListForm.ShowDialog();

                // Refresh products after form closes
                _products = DatabaseHelper.GetAllProducts();
                LoadProducts();
            }
        }

        private void manageProductsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open the product management form (only for manager)
            if (_currentUser.Role == UserRole.Manager)
            {
                using (var productManagementForm = new ProductManagementForm())
                {
                    productManagementForm.ShowDialog();

                    // Refresh products after form closes
                    _products = DatabaseHelper.GetAllProducts();
                    LoadProducts();
                }
            }
        }

        private void manageCategoriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Open the category management form (only for manager)
            if (_currentUser.Role == UserRole.Manager)
            {
                using (var categoryManagementForm = new CategoryManagementForm())
                {
                    categoryManagementForm.ShowDialog();

                    // Refresh categories after form closes
                    _categories = DatabaseHelper.GetAllCategories();
                    LoadProducts();
                }
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show help information
            MessageBox.Show(
                "Simple POS System - Modern Design\n\n" +
                "To add items to cart:\n" +
                "- Click on a product card\n" +
                "- Or enter the product code in the search box and press Enter\n\n" +
                "To change quantity:\n" +
                "- Use the + and − buttons in the cart\n\n" +
                "To remove an item:\n" +
                "- Click the 🗑 button in the cart\n\n" +
                "To complete a sale:\n" +
                "- Click the 💳 PAY NOW button and enter the amount paid",
                "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show about information
            MessageBox.Show(
                "Simple POS System\n" +
                "Version 2.0\n" +
                "© 2025 All Rights Reserved\n\n" +
                "Features:\n" +
                "• Card-based product display\n" +
                "• Visual design\n",
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}