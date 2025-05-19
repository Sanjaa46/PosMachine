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

        // Tax rates
        private const decimal CGST_RATE = 0.03m; // 3%
        private const decimal IGST_RATE = 0.04m; // 4%

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
            this.Text = $"Simple POS - Logged in as {_currentUser.Username}";

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

                // Create a header for the category
                Label categoryLabel = new Label
                {
                    Text = categoryName,
                    Font = new Font("Arial", 12, FontStyle.Bold),
                    Dock = DockStyle.Top,
                    Width = flowLayoutPanelProducts.Width - 10,
                    BackColor = Color.LightGray,
                    Padding = new Padding(5)
                };

                flowLayoutPanelProducts.Controls.Add(categoryLabel);

                // Add products for this category
                foreach (var product in categoryGroup)
                {
                    // Create a product panel
                    Panel productPanel = new Panel
                    {
                        Width = 150,
                        Height = 170,
                        Margin = new Padding(5),
                        BackColor = Color.White,
                        BorderStyle = BorderStyle.FixedSingle,
                        Tag = product
                    };

                    // Add product image (or placeholder)
                    PictureBox productImage = new PictureBox
                    {
                        Width = 140,
                        Height = 100,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Location = new Point(5, 5),
                        BorderStyle = BorderStyle.FixedSingle
                    };

                    if (product.Image != null && product.Image.Length > 0)
                    {
                        using (var ms = new System.IO.MemoryStream(product.Image))
                        {
                            productImage.Image = Image.FromStream(ms);
                        }
                    }
                    else
                    {
                        // Use placeholder image based on category
                        switch (categoryName)
                        {
                            case "Pizza":
                                productImage.BackColor = Color.LightYellow;
                                break;
                            case "Pasta":
                                productImage.BackColor = Color.LightPink;
                                break;
                            case "Sandwich":
                                productImage.BackColor = Color.LightBlue;
                                break;
                            default:
                                productImage.BackColor = Color.LightGray;
                                break;
                        }
                    }

                    productPanel.Controls.Add(productImage);

                    // Add code label
                    Label codeLabel = new Label
                    {
                        Text = $"Code: {product.Code}",
                        Location = new Point(5, 110),
                        Width = 140,
                        Font = new Font("Arial", 8)
                    };
                    productPanel.Controls.Add(codeLabel);

                    // Add product name
                    Label nameLabel = new Label
                    {
                        Text = product.Name,
                        Location = new Point(5, 128),
                        Width = 140,
                        Font = new Font("Arial", 9, FontStyle.Bold)
                    };
                    productPanel.Controls.Add(nameLabel);

                    // Add price
                    Label priceLabel = new Label
                    {
                        Text = $"${product.Price:N0}",
                        Location = new Point(5, 146),
                        Width = 140,
                        TextAlign = ContentAlignment.MiddleCenter,
                        Font = new Font("Arial", 10, FontStyle.Bold)
                    };
                    productPanel.Controls.Add(priceLabel);

                    // Add click event to add product to cart
                    productPanel.Click += (s, e) => AddProductToCart(product);
                    productImage.Click += (s, e) => AddProductToCart(product);
                    nameLabel.Click += (s, e) => AddProductToCart(product);
                    priceLabel.Click += (s, e) => AddProductToCart(product);

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
                        row.Cells["Total"].Value = existingItem.Total;
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
                row.Cells["Price"].Value = newItem.Price;
                row.Cells["Quantity"].Value = newItem.Quantity;
                row.Cells["Total"].Value = newItem.Total;
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
                    dataGridViewCart.Rows[rowIndex].Cells["Total"].Value = item.Total;

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
                        dataGridViewCart.Rows[rowIndex].Cells["Total"].Value = item.Total;
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
            decimal subtotal = _currentOrder.Items.Sum(item => item.Total);
            decimal cgst = subtotal * CGST_RATE;
            decimal igst = subtotal * IGST_RATE;
            decimal total = subtotal + cgst + igst;

            _currentOrder.Subtotal = subtotal;
            _currentOrder.CGST = cgst;
            _currentOrder.IGST = igst;
            _currentOrder.Total = total;

            // Update the UI
            lblSubtotalValue.Text = subtotal.ToString("0.0");
            lblCGSTValue.Text = cgst.ToString("0.0");
            lblIGSTValue.Text = igst.ToString("0.0");
            lblTotalValue.Text = total.ToString("0.0");
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
                "Simple POS System\n\n" +
                "To add items to cart:\n" +
                "- Click on a product image\n" +
                "- Or enter the product code in the text box and press Enter\n\n" +
                "To change quantity:\n" +
                "- Use the + and - buttons in the cart\n\n" +
                "To remove an item:\n" +
                "- Click the X button in the cart\n\n" +
                "To complete a sale:\n" +
                "- Click the Pay button and enter the amount paid",
                "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show about information
            MessageBox.Show(
                "Simple POS System\n\n" +
                "Version 1.0\n" +
                "© 2025 All Rights Reserved",
                "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newInvoiceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.productsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewProductsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageProductsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.categoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageCategoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblSeparator1 = new System.Windows.Forms.Label();
            this.txtProductCode = new System.Windows.Forms.TextBox();
            this.lblHeader = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnSettings = new System.Windows.Forms.Button();
            this.btnNewInvoice = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanelProducts = new System.Windows.Forms.FlowLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.lblTotalValue = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lblIGSTValue = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblCGSTValue = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblSubtotalValue = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnPay = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.dataGridViewCart = new System.Windows.Forms.DataGridView();
            this.ProductId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProductName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Price = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Decrease = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Quantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Increase = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Total = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Delete = new System.Windows.Forms.DataGridViewButtonColumn();
            this.label3 = new System.Windows.Forms.Label();
            this.menuStrip.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCart)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.productsToolStripMenuItem,
            this.categoriesToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1184, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newInvoiceToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newInvoiceToolStripMenuItem
            // 
            this.newInvoiceToolStripMenuItem.Name = "newInvoiceToolStripMenuItem";
            this.newInvoiceToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.newInvoiceToolStripMenuItem.Text = "New Invoice";
            this.newInvoiceToolStripMenuItem.Click += new System.EventHandler(this.btnNewInvoice_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(135, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(138, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // productsToolStripMenuItem
            // 
            this.productsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewProductsToolStripMenuItem,
            this.manageProductsToolStripMenuItem});
            this.productsToolStripMenuItem.Name = "productsToolStripMenuItem";
            this.productsToolStripMenuItem.Size = new System.Drawing.Size(66, 20);
            this.productsToolStripMenuItem.Text = "Products";
            // 
            // viewProductsToolStripMenuItem
            // 
            this.viewProductsToolStripMenuItem.Name = "viewProductsToolStripMenuItem";
            this.viewProductsToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.viewProductsToolStripMenuItem.Text = "View Products";
            this.viewProductsToolStripMenuItem.Click += new System.EventHandler(this.viewProductsToolStripMenuItem_Click);
            // 
            // manageProductsToolStripMenuItem
            // 
            this.manageProductsToolStripMenuItem.Name = "manageProductsToolStripMenuItem";
            this.manageProductsToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.manageProductsToolStripMenuItem.Text = "Manage Products";
            this.manageProductsToolStripMenuItem.Click += new System.EventHandler(this.manageProductsToolStripMenuItem_Click);
            // 
            // categoriesToolStripMenuItem
            // 
            this.categoriesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageCategoriesToolStripMenuItem});
            this.categoriesToolStripMenuItem.Name = "categoriesToolStripMenuItem";
            this.categoriesToolStripMenuItem.Size = new System.Drawing.Size(75, 20);
            this.categoriesToolStripMenuItem.Text = "Categories";
            // 
            // manageCategoriesToolStripMenuItem
            // 
            this.manageCategoriesToolStripMenuItem.Name = "manageCategoriesToolStripMenuItem";
            this.manageCategoriesToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.manageCategoriesToolStripMenuItem.Text = "Manage Categories";
            this.manageCategoriesToolStripMenuItem.Click += new System.EventHandler(this.manageCategoriesToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem1,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // helpToolStripMenuItem1
            // 
            this.helpToolStripMenuItem1.Name = "helpToolStripMenuItem1";
            this.helpToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.helpToolStripMenuItem1.Text = "Help";
            this.helpToolStripMenuItem1.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LimeGreen;
            this.panel1.Controls.Add(this.lblSeparator1);
            this.panel1.Controls.Add(this.txtProductCode);
            this.panel1.Controls.Add(this.lblHeader);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1184, 70);
            this.panel1.TabIndex = 1;
            // 
            // lblSeparator1
            // 
            this.lblSeparator1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSeparator1.ForeColor = System.Drawing.Color.White;
            this.lblSeparator1.Location = new System.Drawing.Point(222, 25);
            this.lblSeparator1.Name = "lblSeparator1";
            this.lblSeparator1.Size = new System.Drawing.Size(2, 30);
            this.lblSeparator1.TabIndex = 3;
            // 
            // txtProductCode
            // 
            this.txtProductCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtProductCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtProductCode.Location = new System.Drawing.Point(243, 30);
            this.txtProductCode.Name = "txtProductCode";
            this.txtProductCode.Size = new System.Drawing.Size(164, 19);
            this.txtProductCode.TabIndex = 2;
            this.txtProductCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtProductCode_KeyDown);
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHeader.ForeColor = System.Drawing.Color.White;
            this.lblHeader.Location = new System.Drawing.Point(12, 19);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(183, 31);
            this.lblHeader.TabIndex = 1;
            this.lblHeader.Text = "Simple POS ";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnSettings);
            this.panel2.Controls.Add(this.btnNewInvoice);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(838, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(346, 70);
            this.panel2.TabIndex = 0;
            // 
            // btnSettings
            // 
            this.btnSettings.BackColor = System.Drawing.Color.White;
            this.btnSettings.FlatAppearance.BorderSize = 0;
            this.btnSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSettings.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnSettings.Location = new System.Drawing.Point(302, 11);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(41, 41);
            this.btnSettings.TabIndex = 2;
            this.btnSettings.Text = "⚙";
            this.btnSettings.UseVisualStyleBackColor = false;
            // 
            // btnNewInvoice
            // 
            this.btnNewInvoice.BackColor = System.Drawing.Color.White;
            this.btnNewInvoice.FlatAppearance.BorderSize = 0;
            this.btnNewInvoice.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnNewInvoice.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNewInvoice.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnNewInvoice.Location = new System.Drawing.Point(173, 14);
            this.btnNewInvoice.Name = "btnNewInvoice";
            this.btnNewInvoice.Size = new System.Drawing.Size(123, 35);
            this.btnNewInvoice.TabIndex = 1;
            this.btnNewInvoice.Text = "New Invoice";
            this.btnNewInvoice.UseVisualStyleBackColor = false;
            this.btnNewInvoice.Click += new System.EventHandler(this.btnNewInvoice_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(20, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Invoice No: 3  ";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 94);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.flowLayoutPanelProducts);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panel3);
            this.splitContainer1.Panel2.Controls.Add(this.dataGridViewCart);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Size = new System.Drawing.Size(1184, 567);
            this.splitContainer1.SplitterDistance = 456;
            this.splitContainer1.TabIndex = 2;
            // 
            // flowLayoutPanelProducts
            // 
            this.flowLayoutPanelProducts.AutoScroll = true;
            this.flowLayoutPanelProducts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanelProducts.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanelProducts.Name = "flowLayoutPanelProducts";
            this.flowLayoutPanelProducts.Size = new System.Drawing.Size(456, 567);
            this.flowLayoutPanelProducts.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel3.Controls.Add(this.lblTotalValue);
            this.panel3.Controls.Add(this.label11);
            this.panel3.Controls.Add(this.lblIGSTValue);
            this.panel3.Controls.Add(this.label9);
            this.panel3.Controls.Add(this.lblCGSTValue);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Controls.Add(this.lblSubtotalValue);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.btnPay);
            this.panel3.Controls.Add(this.btnSave);
            this.panel3.Location = new System.Drawing.Point(3, 424);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(718, 143);
            this.panel3.TabIndex = 2;
            // 
            // lblTotalValue
            // 
            this.lblTotalValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotalValue.BackColor = System.Drawing.Color.Red;
            this.lblTotalValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTotalValue.ForeColor = System.Drawing.Color.White;
            this.lblTotalValue.Location = new System.Drawing.Point(596, 101);
            this.lblTotalValue.Name = "lblTotalValue";
            this.lblTotalValue.Size = new System.Drawing.Size(112, 23);
            this.lblTotalValue.TabIndex = 9;
            this.lblTotalValue.Text = "481.5";
            this.lblTotalValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(440, 104);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(46, 17);
            this.label11.TabIndex = 8;
            this.label11.Text = "Total";
            // 
            // lblIGSTValue
            // 
            this.lblIGSTValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblIGSTValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblIGSTValue.Location = new System.Drawing.Point(596, 72);
            this.lblIGSTValue.Name = "lblIGSTValue";
            this.lblIGSTValue.Size = new System.Drawing.Size(112, 15);
            this.lblIGSTValue.TabIndex = 7;
            this.lblIGSTValue.Text = "18.0";
            this.lblIGSTValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(440, 72);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(64, 15);
            this.label9.TabIndex = 6;
            this.label9.Text = "IGST   4%";
            // 
            // lblCGSTValue
            // 
            this.lblCGSTValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCGSTValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCGSTValue.Location = new System.Drawing.Point(596, 47);
            this.lblCGSTValue.Name = "lblCGSTValue";
            this.lblCGSTValue.Size = new System.Drawing.Size(112, 15);
            this.lblCGSTValue.TabIndex = 5;
            this.lblCGSTValue.Text = "13.5";
            this.lblCGSTValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(440, 47);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(67, 15);
            this.label7.TabIndex = 4;
            this.label7.Text = "CGST   3%";
            // 
            // lblSubtotalValue
            // 
            this.lblSubtotalValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSubtotalValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSubtotalValue.Location = new System.Drawing.Point(596, 22);
            this.lblSubtotalValue.Name = "lblSubtotalValue";
            this.lblSubtotalValue.Size = new System.Drawing.Size(112, 15);
            this.lblSubtotalValue.TabIndex = 3;
            this.lblSubtotalValue.Text = "450.0";
            this.lblSubtotalValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(440, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 15);
            this.label5.TabIndex = 2;
            this.label5.Text = "Subtotal:";
            // 
            // btnPay
            // 
            this.btnPay.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnPay.FlatAppearance.BorderSize = 0;
            this.btnPay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPay.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPay.ForeColor = System.Drawing.Color.White;
            this.btnPay.Location = new System.Drawing.Point(151, 13);
            this.btnPay.Name = "btnPay";
            this.btnPay.Size = new System.Drawing.Size(142, 46);
            this.btnPay.TabIndex = 1;
            this.btnPay.Text = "SAVE && PRINT";
            this.btnPay.UseVisualStyleBackColor = false;
            this.btnPay.Click += new System.EventHandler(this.btnPay_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.LimeGreen;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(3, 13);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(142, 46);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "SAVE";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // dataGridViewCart
            // 
            this.dataGridViewCart.AllowUserToAddRows = false;
            this.dataGridViewCart.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewCart.BackgroundColor = System.Drawing.Color.White;
            this.dataGridViewCart.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridViewCart.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewCart.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ProductId,
            this.ProductName,
            this.Price,
            this.Decrease,
            this.Quantity,
            this.Increase,
            this.Total,
            this.Delete});
            this.dataGridViewCart.Location = new System.Drawing.Point(3, 25);
            this.dataGridViewCart.Name = "dataGridViewCart";
            this.dataGridViewCart.RowHeadersVisible = false;
            this.dataGridViewCart.Size = new System.Drawing.Size(718, 393);
            this.dataGridViewCart.TabIndex = 1;
            this.dataGridViewCart.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewCart_CellContentClick);
            // 
            // ProductId
            // 
            this.ProductId.HeaderText = "ProductId";
            this.ProductId.Name = "ProductId";
            this.ProductId.Visible = false;
            // 
            // ProductName
            // 
            this.ProductName.HeaderText = "ITEM";
            this.ProductName.Name = "ProductName";
            this.ProductName.ReadOnly = true;
            this.ProductName.Width = 200;
            // 
            // Price
            // 
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.Price.DefaultCellStyle = dataGridViewCellStyle1;
            this.Price.HeaderText = "PRICE";
            this.Price.Name = "Price";
            this.Price.ReadOnly = true;
            this.Price.Width = 80;
            // 
            // Decrease
            // 
            this.Decrease.HeaderText = "";
            this.Decrease.Name = "Decrease";
            this.Decrease.ReadOnly = true;
            this.Decrease.Text = "-";
            this.Decrease.UseColumnTextForButtonValue = true;
            this.Decrease.Width = 30;
            // 
            // Quantity
            // 
            this.Quantity.HeaderText = "QTY.";
            this.Quantity.Name = "Quantity";
            this.Quantity.ReadOnly = true;
            this.Quantity.Width = 50;
            // 
            // Increase
            // 
            this.Increase.HeaderText = "";
            this.Increase.Name = "Increase";
            this.Increase.ReadOnly = true;
            this.Increase.Text = "+";
            this.Increase.UseColumnTextForButtonValue = true;
            this.Increase.Width = 30;
            // 
            // Total
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
            this.Total.DefaultCellStyle = dataGridViewCellStyle2;
            this.Total.HeaderText = "PRICE";
            this.Total.Name = "Total";
            this.Total.ReadOnly = true;
            this.Total.Width = 80;
            // 
            // Delete
            // 
            this.Delete.HeaderText = "";
            this.Delete.Name = "Delete";
            this.Delete.ReadOnly = true;
            this.Delete.Text = "X";
            this.Delete.UseColumnTextForButtonValue = true;
            this.Delete.Width = 30;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(3, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 17);
            this.label3.TabIndex = 0;
            this.label3.Text = "Cart:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1184, 661);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Simple POS";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCart)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem productsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem categoriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newInvoiceToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewProductsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageProductsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageCategoriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Button btnNewInvoice;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtProductCode;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanelProducts;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView dataGridViewCart;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnPay;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblTotalValue;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblIGSTValue;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label lblCGSTValue;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblSubtotalValue;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label lblSeparator1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ProductId;
        private System.Windows.Forms.DataGridViewTextBoxColumn ProductName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Price;
        private System.Windows.Forms.DataGridViewButtonColumn Decrease;
        private System.Windows.Forms.DataGridViewTextBoxColumn Quantity;
        private System.Windows.Forms.DataGridViewButtonColumn Increase;
        private System.Windows.Forms.DataGridViewTextBoxColumn Total;
        private System.Windows.Forms.DataGridViewButtonColumn Delete;
    }
}