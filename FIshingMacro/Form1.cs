using FIshingMacro.Properties;

namespace FIshingMacro
{
    public partial class Form1 : Form
    {
        static KeyboardHook kbHook = new KeyboardHook();
        static bool isReady;
        public Form1()
        {
            InitializeComponent();
            kbHook.KeyDownEvent += KbHook_KeyDownEvent;
            textBox1.Text = ((Keys)Properties.Settings.Default.start).ToString();
            textBox2.Text = ((Keys)Properties.Settings.Default.end).ToString();
            label1.Text = "�J�n�L�[";
            label2.Text = "�I���L�[";
            button1.Text = "�J�n";
            Text = "�����ނ�@";
            MaximumSize = Size;
            MinimumSize = Size;
        }

        private void KbHook_KeyDownEvent(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case int start when start == Properties.Settings.Default.start:
                    Fishing.FullAutomaticalyFishing();
                    break;
                case int end when end == Properties.Settings.Default.end:
                    Fishing.StopMacro();
                    kbHook.UnHook();
                    Close();
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("�g�p����L�[���w�肵�Ă�������");
                return;
            }
            isReady = !isReady;
            if (isReady)
            {
                if (textBox1.Text == textBox2.Text)
                {
                    MessageBox.Show("�����L�[�͐ݒ�ł��܂���");
                    isReady = false;
                    return;
                }
                NativeMethods.AttachConsole();
                button1.Text = "��~";
                kbHook.Hook();
            }
            else
            {
                NativeMethods.FreeConsole();
                button1.Text = "�N��";
                kbHook.UnHook();
            }
        }

        private void textBox1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            textBox1.Text = e.KeyCode.ToString();
            Settings.Default.start = (int)e.KeyCode;
            Settings.Default.Save();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            kbHook.UnHook();
        }

        private void textBox2_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            textBox2.Text = e.KeyCode.ToString();
            Settings.Default.end = (int)e.KeyCode;
            Settings.Default.Save();
        }
    }
}
