using System.Diagnostics;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace FIshingMacro
{
    internal class Fishing
    {
        private const uint MOUSEEVENTIF_MOVE = 0x0001;
        private const uint MOUSEEVENTIF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTIF_RIGHTUP = 0x0010;

        const byte RED = 255;
        const byte GREEN = 85;
        const byte BLUE = 85;

        const int WREFACTOR = 1936;
        const int HREFACTOR = 1024;


        private static bool PicCheck(int threshold = 10)
        {
            Bitmap coordBit = NativeMethods.GetHiddenWindow();
            if (coordBit.Width < 50)
                return false;

            int refactedX = (coordBit.Width / 2 - 25) * coordBit.Width / WREFACTOR;
            int refactedY = (coordBit.Height / 2 + 25) * coordBit.Height / HREFACTOR;
            int refactedW = 50 * coordBit.Width / WREFACTOR;
            int refactedH = 65 * coordBit.Height / HREFACTOR;

            Rectangle checkRect = new Rectangle(refactedX, refactedY, refactedW, refactedH);
            coordBit = coordBit.Clone(checkRect, coordBit.PixelFormat);

            Form1.tsp.SetRect(checkRect);

            BitmapData data = coordBit.LockBits(new Rectangle(0, 0, coordBit.Width, coordBit.Height), ImageLockMode.ReadOnly, coordBit.PixelFormat);
            byte[] buf = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);
            for (int h = 0; h < data.Height; h++)
            {
                for (int w = 0; w < data.Width; w++)
                {
                    int pos = h * data.Stride + w * 4;//Format32bppArgbだと*4
                    int diff = (Math.Abs(BLUE - buf[pos]) + Math.Abs(GREEN - buf[pos + 1]) + Math.Abs(RED - buf[pos + 2])) / 3;
                    if (diff < threshold)
                    {
                        coordBit.UnlockBits(data);
                        coordBit.Dispose();
                        return true;
                    }
                }
            }
            coordBit.UnlockBits(data);
            coordBit.Dispose();
            return false;
        }

        static bool isRunning;
        internal static async void FullAutomaticalyFishing()
        {
            Console.Clear();
            if (isRunning)
            {
                isRunning = false;
                return;
            }

            isRunning = true;
            await AutoFishingT().ConfigureAwait(false);
        }

        internal static async Task AutoFishingT()
        {
            await Task.Delay(100).ConfigureAwait(false);
            int num = 0;
            Stopwatch rapSW = new Stopwatch();
            TimeSpan allTime = new TimeSpan(0);
            while (isRunning)
            {
                NativeMethods.MouseEvent(MOUSEEVENTIF_RIGHTDOWN, 0, 0, 0, 0);
                NativeMethods.MouseEvent(MOUSEEVENTIF_RIGHTUP, 0, 0, 0, 0);
                rapSW.Restart();
                while (isRunning && !PicCheck())
                {
                    await Task.Delay(100).ConfigureAwait(true);
                    string windowTitle = NativeMethods.GetCursorWindowTitle().Replace("\0", "", StringComparison.Ordinal);
                    if (windowTitle != "Minecraft 1.8.9")
                    {
                        Console.WriteLine(windowTitle);
                        Console.WriteLine("下にマイクラ以外のウィンドウがあるため、一時停止しました。");
                        isRunning = false;
                        break;
                    }
                    LogHistory(num, rapSW.Elapsed, allTime);
                }
                allTime += rapSW.Elapsed;
                NativeMethods.MouseEvent(MOUSEEVENTIF_RIGHTDOWN, 0, 0, 0, 0);
                NativeMethods.MouseEvent(MOUSEEVENTIF_RIGHTUP, 0, 0, 0, 0);
                await Task.Delay(100).ConfigureAwait(true);
                num++;
                if (num % 14 == 0)
                {
                    MouseMoveCircle(40);
                }
                else if (num % 7 == 0)
                {
                    MouseMoveCircle(-40);
                }
                while (isRunning && PicCheck()) { } //二回防止
            }
            Console.WriteLine("stop");
        }
        internal static void MouseMoveCircle(int baseMove, int max = 40)
        {
            int angle = 180 - 180 * (max - 2) / max;
            Vector2 vec = new Vector2(baseMove * MathF.Sin(360 * MathF.PI / max / 180), baseMove * (1 - MathF.Cos(360 * MathF.PI / max / 180)));
            for (int i = 0; i < max; i++)
            {
                Vector2 rot = Rotate(vec, angle);
                vec = rot;   //再計算の為に必要
                NativeMethods.MouseEvent(MOUSEEVENTIF_MOVE, (int)rot.X, (int)rot.Y / 2, 0, 0);
                Thread.Sleep(1);
            }
        }
        private static Vector2 Rotate(Vector2 target, int angle)
        {
            Complex tar = new Complex(target.X, target.Y);
            float cos = MathF.Cos(angle * MathF.PI / 180);
            float sin = MathF.Sin(angle * MathF.PI / 180);
            Complex rot = new Complex(cos, sin);
            Complex result = tar * rot;
            return new Vector2((float)result.Real, (float)result.Imaginary);
        }
        private static void LogHistory(int num, TimeSpan rapTime, TimeSpan allTime)
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"NOW : {DateTime.Now}");
            Console.WriteLine($"NUM : {num}");
            Console.WriteLine($"RAP : {rapTime}");
            Console.WriteLine($"ALL : {allTime}");
        }
        internal static void StopMacro()
        {
            isRunning = false;
        }
    }
}
