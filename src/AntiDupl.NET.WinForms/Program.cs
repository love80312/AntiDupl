
/*
* AntiDupl.NET Program (http://ermig1979.github.io/AntiDupl).
*
* Copyright (c) 2002-2023 Yermalayeu Ihar.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace AntiDupl.NET.WinForms
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            CommandLine commandLine = CommandLine.Parse(args);

            string customSavePath = commandLine.UserPath;

            if (customSavePath != null)
            {
                DirectoryInfo directoryInfo =
                    new DirectoryInfo(customSavePath);

                if (!directoryInfo.Exists)
                {
                    throw new Exception(
                        String.Format(
                            "The directory '{0}' does not exist!",
                            customSavePath));
                }

                Resources.UserPath = customSavePath;
            }
            else
            {
                Resources.UserPath = Resources.GetDefaultUserPath();
            }

            Resources.Strings.Initialize();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 全程式預設字型。
            // 必須在建立任何 Form 之前設定。
            Application.SetDefaultFont(
                new Font(
                    "Microsoft JhengHei UI",
                    10F,
                    FontStyle.Regular,
                    GraphicsUnit.Point));

            // The color mode can't change once a window exists,
            // so the options are read here, before the main window.
            Options options = Options.Load();

            Application.SetColorMode(options.colorMode);

            Application.Run(
                new MainForm(options, commandLine));
        }
    }
}
