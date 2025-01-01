
using FIshingMacro.Properties;

namespace FIshingMacro
{
    public partial class Form1 : Form
    {
        static KeyboardHook kbHook = new KeyboardHook();
        static int optionKeyCode;

        public Form1()
        {
            InitializeComponent();
            kbHook.KeyDownEvent += KbHook_KeyDownEvent;
            NativeMethods.AttachConsole();
            textBox1.Text = ((Keys)Properties.Settings.Default.key).ToString();
            Console.Title = "自動釣り機";
            label1.Text = "使用するキー";
            button1.Text = "開始";
            Text = "自動釣り機";

            optionKeyCode = Settings.Default.key;
            MaximumSize = Size;
            MinimumSize = Size;
        }

        private void KbHook_KeyDownEvent(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case (int)Keys.F10:
                    Fishing.StopMacro();
                    kbHook.UnHook();
                    Close();
                    break;
                case int start when start == optionKeyCode:
                    Fishing.FullAutomaticalyFishing();
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("使用するキーを指定してください");
                return;
            }
            kbHook.Hook();
            Hide();
        }

        private void textBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            textBox1.Text = e.KeyCode.ToString();
            optionKeyCode = (int)e.KeyCode;
            Settings.Default.key = optionKeyCode;
            Settings.Default.Save();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            kbHook.UnHook();
        }
    }
}
