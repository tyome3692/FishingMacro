using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Buffers;

namespace FIshingMacro
{
    internal partial class NativeMethods
    {
        [LibraryImport("user32.dll")]
        private static partial IntPtr GetForegroundWindow();

        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        private static partial int GetWindowTextLengthW(IntPtr hWnd);

        [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        private static partial int GetWindowTextW(IntPtr hWnd, [Out] char[] lpString, int nMaxCount);

        [LibraryImport("user32.dll")]
        private static partial IntPtr WindowFromPoint(POINT point);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [LibraryImport("kernel32.dll")]
        private static partial IntPtr GetConsoleWindow();

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool PrintWindow(IntPtr hwnd, IntPtr hDC, uint nFlags);


        [LibraryImport("user32.dll", EntryPoint = "mouse_event")]
        internal static partial void MouseEvent(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);

        private const int SWP_SHOWWINDOW = 0x0040;
        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            int Left;
            int Top;
            int Right;
            int Bottom;
            internal int width
            {
                get
                {
                    return Right - Left;
                }
            }
            internal int height
            {
                get
                {
                    return Bottom - Top;
                }
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            internal POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            internal int x, y;
        }

        #region Console

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();
        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool FreeConsole();

        [LibraryImport("user32.dll")]
        private static partial IntPtr GetSystemMenu(IntPtr hWnd, UInt32 bRevert);

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial UInt32 RemoveMenu(IntPtr hMenu, UInt32 nPosition, UInt32 wFlags);
        private const UInt32 SC_CLOSE = 0x0000F060;
        private const UInt32 SC_MAXIMIZE = 0xF030;
        private const UInt32 MF_BYCOMMAND = 0x0;

        [LibraryImport("kernel32.dll", SetLastError = true)]
        private static partial IntPtr GetStdHandle(int nStdHandle);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        const int STD_INPUT_HANDLE = -10;
        const uint ENABLE_QUICK_EDIT = 0x0040;
        #endregion

        /// <summary>
        /// 整形済みのコンソールウィンドウをアタッチして最前面に固定する
        /// </summary>
        internal static void AttachConsole()
        {
            AllocConsole();

            Console.Title = "自動釣り機";
            Console.CursorVisible = false;
#pragma warning disable CA2000
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
#pragma warning restore CA2000
            Console.CancelKeyPress += Console_CancelKeyPress;
            Console.OutputEncoding = Encoding.UTF8;

            IntPtr consoleHandle = GetConsoleWindow();
            IntPtr closeMenuHandle = GetSystemMenu(consoleHandle, 0);

            if (RemoveMenu(closeMenuHandle, SC_CLOSE, MF_BYCOMMAND) == 0)
            {
                Console.WriteLine($"RemoveMenu関数が失敗しました。Error : {Marshal.GetLastWin32Error()}");
            }
            if (RemoveMenu(closeMenuHandle, SC_MAXIMIZE, MF_BYCOMMAND) == 0)
            {
                Console.WriteLine($"RemoveMenu関数が失敗しました。Error : {Marshal.GetLastWin32Error()}");
            }

            uint consoleMode = 0;
            IntPtr inputHandle = GetStdHandle(STD_INPUT_HANDLE);
            if (!GetConsoleMode(inputHandle, out consoleMode))
            {
                Console.WriteLine(Marshal.GetLastWin32Error());
            }
            consoleMode &= ~ENABLE_QUICK_EDIT;
            SetConsoleMode(inputHandle, consoleMode);

            SetWindowPos(GetConsoleWindow(), -1, -7, 310, 250, 200, SWP_SHOWWINDOW);
        }
        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
        }

        /// <summary>
        /// ウィンドウのハンドルからタイトルを取得する
        /// </summary>
        /// <param name="hWnd">対象のハンドル</param>
        /// <returns></returns>
        private static string GetWindowTitleFromhWnd(IntPtr hWnd)
        {
            int retCode = 0;
            int titleLen = GetWindowTextLengthW(hWnd) + 1;
            char[] chars = ArrayPool<char>.Shared.Rent(titleLen);
            retCode = GetWindowTextW(hWnd, chars, titleLen);
            return new string(chars);
        }

        /// <summary>
        /// マウスカーソルの下にあるアクティブウィンドウのタイトルを取得する
        /// </summary>
        /// <returns></returns>

        public static string GetCursorWindowTitle()
        {
            POINT point = new POINT(Cursor.Position.X, Cursor.Position.Y);
            IntPtr hWnd = WindowFromPoint(point);
            return GetWindowTitleFromhWnd(hWnd);
        }
        public static Bitmap GetHiddenWindow()
        {
            IntPtr handle = GetForegroundWindow();

            //ウィンドウサイズ取得
            GetWindowRect(handle, out RECT rect);

            Bitmap img = new Bitmap(1, 1);
            //ウィンドウをキャプチャする
            if (rect.width != 0 && rect.height != 0)
            {
                img = new Bitmap(rect.width, rect.height);
                Graphics memg = Graphics.FromImage(img);
                IntPtr dc = memg.GetHdc();
                PrintWindow(handle, dc, 0);

                memg.ReleaseHdc(dc);
                memg.Dispose();
            }           
            return img;
        }
    }
}
