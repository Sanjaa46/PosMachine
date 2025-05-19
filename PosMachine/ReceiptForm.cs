using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using System.Windows.Forms;
using POSMachine.Models;

namespace POSMachine
{
    public partial class ReceiptForm : Form
    {
        private Order _order;
        private PrintDocument _printDocument;

        public ReceiptForm(Order order)
        {
            InitializeComponent();

            _order = order;
            LoadReceipt();

            // Set up printing
            _printDocument = new PrintDocument();
            _printDocument.PrintPage += PrintDocument_PrintPage;
            printPreviewControl.Document = _printDocument;
        }

        private void LoadReceipt()
        {
            // Generate receipt content
            StringBuilder sb = new StringBuilder();

            // Header
            sb.AppendLine("SIMPLE POS");
            sb.AppendLine("123 Main Street");
            sb.AppendLine("City, State, 12345");
            sb.AppendLine("Phone: (123) 456-7890");
            sb.AppendLine(new string('-', 40));

            // Order info
            sb.AppendLine($"Order #: {_order.Id}");
            sb.AppendLine($"Date: {_order.OrderDate:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine(new string('-', 40));

            // Items
            sb.AppendLine("Item                  Qty   Price    Total");
            sb.AppendLine(new string('-', 40));

            foreach (var item in _order.Items)
            {
                // Format item name to fit in column (trim if needed)
                string itemName = item.ProductName.Length > 18
                    ? item.ProductName.Substring(0, 15) + "..."
                    : item.ProductName.PadRight(18);

                sb.AppendLine($"{itemName} {item.Quantity,4}  {item.Price,6:0.00}  {item.Total,8:0.00}");
            }

            sb.AppendLine(new string('-', 40));

            // Totals - no tax fields
            sb.AppendLine($"{"Subtotal:",30} {_order.Subtotal,10:0.00}");
            sb.AppendLine($"{"Total:",30} {_order.Total,10:0.00}");
            sb.AppendLine($"{"Amount Paid:",30} {_order.AmountPaid,10:0.00}");
            sb.AppendLine($"{"Change:",30} {_order.Change,10:0.00}");

            // Footer
            sb.AppendLine(new string('-', 40));
            sb.AppendLine("Thank you for your purchase!");
            sb.AppendLine("Please come again.");

            // Set receipt text to preview
            lblReceipt.Text = sb.ToString();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Set up font and position
            Font font = new Font("Courier New", 10);
            float lineHeight = font.GetHeight();
            float x = 10;
            float y = 10;

            // Print the receipt
            string[] lines = lblReceipt.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                e.Graphics.DrawString(line, font, Brushes.Black, x, y);
                y += lineHeight;
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            using (var printDialog = new PrintDialog())
            {
                printDialog.Document = _printDocument;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    _printDocument.Print();
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}