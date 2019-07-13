using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WindowsInput;
using WindowsInput.Native;

namespace FishingBot
{
    class Program
    {
        const int DEFAULT_COUNTDOWN = 10;
        const int DEFAULT_REFRESHRATE = 120;
        const int MAX_SHOW_MESSAGE_DELAY = 10;
        const int NORMALIZING_LENGTH = 80;

        // avoid having to create a new one for each countdown etc
        private static readonly TimeSpan NegativeOneSecond = TimeSpan.FromSeconds(-1);

        static async Task Main(string[] args)
        {
            // get custom countDown
            TimeSpan countdown = GetTimeSpanFromArgs(args, new[] { "/countdown", "-c", "--countdown" }, TimeSpan.FromSeconds(DEFAULT_COUNTDOWN));
            if (countdown.TotalSeconds < 1) countdown = TimeSpan.Zero;

            // get custom refreshRate
            TimeSpan refreshRate = GetTimeSpanFromArgs(args, new[] { "/refreshrate", "-r", "--refreshrate" }, TimeSpan.FromSeconds(DEFAULT_REFRESHRATE));
            if(refreshRate.TotalSeconds < 1)
            {
                Console.WriteLine("The refresh rate has to be at least one second long.");
                return;
            }

            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                // setup cancellation (this works for ctrl+c and ctrl+break)
                Console.CancelKeyPress += (o, e) =>
                {
                    cts.Cancel();       // cancel the application
                    e.Cancel = true;    // prevent the immediate killing of the process
                };

                // introduction
                Console.WriteLine($"After starting to hold down the right mouse button, the bot will check if it's stil down with an interval of {refreshRate} and start holding it down again if necessary.");
                Console.WriteLine("Press ctrl + C or ctrl + break to stop the bot (this will release the right button if still down).");

                // show countdown to start
                await CountDown("Bot starts in {0:hh\\:mm\\:ss}", countdown, cts.Token, startWithNewLine: true, endWithNewLine: false);

                InputSimulator simulator = new InputSimulator();
                await RefreshPressEvery(simulator, refreshRate, cts.Token);

                // reset if the right button is still down
                if (simulator.InputDeviceState.IsKeyDown(VirtualKeyCode.RBUTTON))
                {
                    simulator.Mouse.RightButtonUp();
                }
            }
        }

        private static TimeSpan GetTimeSpanFromArgs(string[] args, IEnumerable<string> possibleArguments, TimeSpan fallbackValue)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (possibleArguments.Contains(args[i].ToLower()))
                {
                    var potentialValue = args[i + 1];
                    if (TimeSpan.TryParse(potentialValue, out TimeSpan tsValue))
                    {
                        return tsValue;
                    }
                }
            }

            return fallbackValue;
        }

        private static async Task RefreshPressEvery(IInputSimulator inputSimulator, TimeSpan delay, CancellationToken ct, TimeSpan? showMessageTime = null)
        {
            if (delay.TotalSeconds < 1) throw new ArgumentException("The delay has to be at least one second long", nameof(delay));

            // set delay for showing the messsage
            TimeSpan showMsgTime;
            if (showMessageTime.HasValue)
            {
                showMsgTime = showMessageTime.Value;
                if (delay.TotalSeconds <= showMsgTime.TotalSeconds) throw new ArgumentException("The show-message-time has to be shorter than the delay.", nameof(showMessageTime));
            }
            else
            {
                TimeSpan fraction = delay.Divide(20);
                showMsgTime = fraction.TotalSeconds < MAX_SHOW_MESSAGE_DELAY ? fraction : TimeSpan.FromSeconds(MAX_SHOW_MESSAGE_DELAY);
            }
                    
            delay = delay.Add(showMsgTime.Negate());

            while (!ct.IsCancellationRequested)
            {
                if (inputSimulator.InputDeviceState.IsKeyDown(VirtualKeyCode.RBUTTON))
                {
                    PrintOnCurrentLine("Right mouse button is already being held down. No action taken.");
                }
                else
                {
                    inputSimulator.Mouse.RightButtonDown();
                    PrintOnCurrentLine("Start holding down right mouse button.");
                }

                // show message for a bit
                try
                {
                    await Task.Delay(showMsgTime, ct);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                // count down to next refresh
                await CountDown("Next refresh in {0:hh\\:mm\\:ss}", delay, ct);
            }
        }

        private static async Task CountDown(string formatString, TimeSpan timespan, CancellationToken ct, bool startWithNewLine = false, bool endWithNewLine = false)
        {
            if (ct.IsCancellationRequested) return;
            if (timespan.TotalSeconds < 1) return;

            if (startWithNewLine) Console.WriteLine();

            // loop every second
            for (int i = (int)timespan.TotalSeconds; i > 0; i--)
            {
                // print remaining time
                PrintOnCurrentLine(formatString, timespan);

                try
                {
                    // wait one second but also handle cancellation
                    await Task.Delay(1000, ct);
                }
                catch (TaskCanceledException)
                {
                    return;
                }

                // remove one second from the timespan
                timespan = timespan.Add(NegativeOneSecond);
            }

            // print seconds <= 0
            PrintOnCurrentLine(formatString, timespan);

            if (endWithNewLine) Console.WriteLine();
        }

        private static void PrintOnCurrentLine(string format, params object[] args) =>
            PrintOnCurrentLine(String.Format(format, args));

        private static void PrintOnCurrentLine(string format, object arg0) =>
            PrintOnCurrentLine(String.Format(format, arg0));

        private static void PrintOnCurrentLine(string text)
        {
            string normalized = Normalize(text, NORMALIZING_LENGTH);
            Console.Write($"\r{normalized}");
            Console.SetCursorPosition(text.Length, Console.CursorTop);
        }

        private static string Normalize(string content, int length)
        {
            if (content.Length < length) return content.PadRight(length);
            if (content.Length > length) return content.Substring(0, length);

            return content;
        }
    }
}
