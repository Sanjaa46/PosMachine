// PaymentForm.cs
using System;
using System.Windows.Forms;

namespace POSMachine
{
    public partial class PaymentForm : Form
    {
        public decimal AmountPaid { get; private set; }
        public decimal Change { get; private set; }

        private decimal _totalAmount;

        public PaymentForm(decimal totalAmount)
        {
            InitializeComponent();

            _totalAmount = totalAmount;
            lblTotalAmount.Text = _totalAmount.ToString("N2");
            txtAmountPaid.Focus();
            txtAmountPaid.Select();
        }

        private void btnPay_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(txtAmountPaid.Text, out decimal amountPaid))
            {
                if (amountPaid < _totalAmount)
                {
                    MessageBox.Show("Amount paid must be greater than or equal to the total amount.",
                        "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AmountPaid = amountPaid;
                Change = amountPaid - _totalAmount;

                // Display change
                lblChange.Text = Change.ToString("N2");

                // Enable completion
                btnComplete.Enabled = true;
            }
            else
            {
                MessageBox.Show("Please enter a valid amount.",
                    "Invalid Amount", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnComplete_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}