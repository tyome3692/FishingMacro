﻿using System.Diagnostics;
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

        private static bool PicCheck(int threshold = 10)
        {
            Bitmap coordBit = NativeMethods.GetHiddenWindow();
            coordBit = coordBit.Clone(new Rectangle(coordBit.Width / 2 - 25, coordBit.Height / 2 + 25, 50, 65), coordBit.PixelFormat);

            int pos = 0;
            byte red = 255;
            byte green = 85;
            byte blue = 85;
            int diff = 0;
            BitmapData data = coordBit.LockBits(new Rectangle(0, 0, coordBit.Width, coordBit.Height), ImageLockMode.ReadOnly, coordBit.PixelFormat);
            byte[] buf = new byte[data.Stride * data.Height];
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);
            for (int h = 0; h < data.Height; h++)
            {
                for (int w = 0; w < data.Width; w++)
                {
                    pos = h * data.Stride + w * 4;//Format32bppArgbだと*4
                    diff = (Math.Abs(blue - buf[pos]) + Math.Abs(green - buf[pos + 1]) + Math.Abs(red - buf[pos + 2])) / 3;
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
        static int num;
        static Stopwatch rapSW = new Stopwatch();
        static TimeSpan allTime = new TimeSpan(0);

        public static async void FullAutomaticalyFishing()
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

        public static async Task AutoFishingT()
        {
            await Task.Delay(100).ConfigureAwait(false);

            while (isRunning)
            {
                NativeMethods.MouseEvent(MOUSEEVENTIF_RIGHTDOWN, 0, 0, 0, 0);
                NativeMethods.MouseEvent(MOUSEEVENTIF_RIGHTUP, 0, 0, 0, 0);
                rapSW.Restart();
                while (isRunning && !PicCheck())
                {
                    await Task.Delay(100);
                    string windowTitle = NativeMethods.GetCursorWindowTitle().Replace("\0", "", StringComparison.Ordinal);
                    if (windowTitle != "Minecraft 1.8.9")
                    {
                        Console.WriteLine(windowTitle);
                        Console.WriteLine("下にマイクラ以外のウィンドウがあるため、一時停止しました。");
                        isRunning = false;
                        break;
                    }
                    LogHistory();
                }
                allTime += rapSW.Elapsed;
                NativeMethods.MouseEvent(MOUSEEVENTIF_RIGHTDOWN, 0, 0, 0, 0);
                NativeMethods.MouseEvent(MOUSEEVENTIF_RIGHTUP, 0, 0, 0, 0);
                await Task.Delay(100);
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
        public static void MouseMoveCircle(int baseMove, int max = 40)
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
        private static void LogHistory()
        {
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"NOW : {DateTime.Now}");
            Console.WriteLine($"NUM : {num}");
            Console.WriteLine($"RAP : {rapSW.Elapsed}");
            Console.WriteLine($"ALL : {allTime}");
        }
        public static void StopMacro()
        {
            isRunning = false;
        }
    }
}
