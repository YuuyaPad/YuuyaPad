using CustomExceptionApp;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YuuyaPad
{
    internal static class Program
    {
        public static MyAppContext AppContext; // Shared context across apps

        [STAThread]
        static void Main()
        {
            // Windows Application Initialization
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Check Operating System
            Version current = Environment.OSVersion.Version;
            Version min = new Version(6, 1, 6790); // Windows 7 or later
            if (current < min)
            {
                MessageBox.Show(
                    $"YuuyaPad does not support your operating system. \n" +
                    $"The current version is {current}, but YuuyaPad requires Windows 7 or later." + Environment.NewLine + "Consider upgrading your version of Windows." + Environment.NewLine + "Click OK to exit the program.",
                    "YuuyaPad",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.None
                );
                return; // Abort Startup
            }

            // Exception handling settings
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            // UI thread exceptions
            Application.ThreadException += (s, e) =>
                ShowException(e.Exception, "An unhandled exception occurred on the UI thread.");

            // Exceptions on non-UI threads
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    ShowException(ex, "An unhandled exception occurred in a background thread.");
                else
                    ShowException(new Exception(e.ExceptionObject.ToString()), "A non-exception object was thrown.");
            };

            // Asynchronous task exceptions
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                e.SetObserved(); // Prevent process termination
                ShowException(e.Exception, "An unhandled exception occurred in the asynchronous task.");
            };

            // Load Settings
            AppSettings.Load();

            // Context Launch
            AppContext = new MyAppContext();
            Application.Run(AppContext);
        }

        // Common Exception Dialog Display
        private static void ShowException(Exception ex, string context)
        {
            try
            {
                using (var dialog = new ExceptionForm(ex, context))
                    dialog.ShowDialog();
            }
            catch
            {
                // If it is not possible to display the exception dialog, show it as a message box instead
                MessageBox.Show(
                    $"{context}\n\n{ex}",
                    "Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // Custom Application Context
        public class MyAppContext : ApplicationContext
        {
            public MyAppContext()
            {
                OpenForm1(new Form1());
            }

            // Open New Form1
            public void OpenForm1(Form1 form)
            {
                // This contains code that is for debugging purposes only.
#if DEBUG
                form.Text += " (Debug)";
#endif
                // End of Debug code

                form.FormClosed += Form_FormClosed;
                form.Show();
            }

            // Exit the app when all Form1 are closed
            private void Form_FormClosed(object sender, FormClosedEventArgs e)
            {
                if (!Application.OpenForms.OfType<Form1>().Any())
                    ExitThread();
            }
        }
    }
}
