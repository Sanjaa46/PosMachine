using System;
using System.Windows.Forms;
using POSMachine.Data;
using POSMachine.Models;

namespace POSMachine
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize the database
            DatabaseHelper.InitializeDatabase();

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