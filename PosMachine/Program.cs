using System;
using System.Windows.Forms;
using POSMachine.Data;
using POSMachine.Models;

namespace POSMachine
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize the database
            try
            {
                DatabaseHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize database: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Show login form
            using (var loginForm = new LoginForm())
            {
                var result = loginForm.ShowDialog();

                if (result == DialogResult.OK && loginForm.CurrentUser != null)
                {
                    // Log in successful, open main form
                    Application.Run(new MainForm(loginForm.CurrentUser));
                }
            }
        }
    }
}