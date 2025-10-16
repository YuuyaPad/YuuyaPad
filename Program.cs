using CustomExceptionApp;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YuuyaPad
{
    internal static class Program
    {
        public static MyAppContext AppContext; // Keep it static here

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ShowException(e.Exception, "Unhandled exception on the UI thread"); // Unhandled exception on the UI thread
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    ShowException(ex, "Unhandled exception in background thread"); // Unhandled exception in background thread
                else
                    ShowException(new Exception(e.ExceptionObject.ToString()), "Throwing a non-exception object"); // Throwing a non-exception object
            };
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                e.SetObserved();
                ShowException(e.Exception, "Unhandled exception in asynchronous task"); //Unhandled exception in asynchronous task
            };

            AppContext = new MyAppContext(); // Keep in static
            Application.Run(AppContext);
        }

        private static void ShowException(Exception ex, string context)
        {
            try
            {
                using (var dialog = new ExceptionForm(ex, context))
                {
                    // Show Exception Dialog
                    dialog.ShowDialog();
                }
            }
            catch
            {
                // If the exception dialog cannot be displayed, it will be displayed as a MessageBox instead
                MessageBox.Show($"{context}\n\n{ex}", "Yuuya Pad - Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public class MyAppContext : ApplicationContext
        {
            public MyAppContext()
            {
                OpenForm1(new Form1());
            }

            public void OpenForm1(Form1 form)
            {
                form.FormClosed += Form_FormClosed;
                form.Show();
            }

            private void Form_FormClosed(object sender, FormClosedEventArgs e)
            {
                if (!Application.OpenForms.OfType<Form1>().Any())
                    ExitThread();
            }
        }
    }
}
