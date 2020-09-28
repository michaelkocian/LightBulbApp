using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static LightBulbApp.CustomTypes;

namespace LightBulbApp
{
    class Program
    {
        const string BT_ADDRESS = "11:22:33:80:F0:7E";
        const string KEYBOARD_ID = "/dev/input/event0"; // 0 stands for the device id

        //Program is based on the Reading Raw Keyboard Input from
        //https://raspberry-projects.com/pi/programming-in-c/keyboard-programming-in-c/reading-raw-keyboard-input

        //install:       sudo apt-get install evtest
        //run:           sudo evtest /dev/input/event0

        //also had a few clues but struct size was not correct
        //https://thehackerdiary.wordpress.com/2017/04/21/exploring-devinput-1/


        static async Task Main(string[] args)
        {
            await new Program().MainLoop();
        }

        public string ExeLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
        readonly Logger logger = new Logger();
        bool IsOn = false;
        int Brightness = 90;
        int colorPaletteIndex = 0;
        int longPressCounter = 0;

        string[] colorPalette = new string[] {
            "yeelight-colortemp.sh 0 6500", //blueish white
            "yeelight-colortemp.sh 0 5000",
            "yeelight-colortemp.sh 0 4000",
            "yeelight-colortemp.sh 0 3000",
            "yeelight-colortemp.sh 0 2000",
            "yeelight-colortemp.sh 0 1700", //yellowish white

            "yeelight-rgb.sh 0 255,255,255", //white grayish
            "yeelight-rgb.sh 0 255,80,0", //orange
            "yeelight-rgb.sh 0 255,0,0", //red
            "yeelight-rgb.sh 0 255,255,0", //yellow
            "yeelight-rgb.sh 0 0,255,255", //cyan
            "yeelight-rgb.sh 0 0,255,0", //green
            "yeelight-rgb.sh 0 255,0,255", // purple
            "yeelight-rgb.sh 0 0,0,255", //blue
            "yeelight-rgb.sh 0 255,29,151", // pink
            "yeelight-rgb.sh 0 134,45,255", //lightpurple
            "yeelight-rgb.sh 0 65,64,255", //lightblue
            "yeelight-rgb.sh 0 13,255,166", //lightgreen

        };


        public async Task MainLoop()
        {
            bool connectFirst = false;
            string bluetoothConnectCommand = "bluetoothctl connect " + BT_ADDRESS;

            while (true)
            {
                try
                {
                    if (connectFirst)
                    {
                        connectFirst = false;
                        await bluetoothConnectCommand.Bash(logger);
                        await Task.Delay(200);
                    }
                    await ReadCharacters();

                }
                catch (FileNotFoundException e1)
                {
                    if (e1.Message != $"Could not find file '{KEYBOARD_ID}'.")
                        throw;
                    connectFirst = true;
                    logger.LogInformation("No keyboard found. Connecting blueooth device");
                    await Task.Delay(5000);
                }
                catch (IOException e2)
                {
                    if (e2.Message != "No such device")
                        throw;
                    connectFirst = true;
                    await Task.Delay(1000);
                }
                catch (ShellException e3)
                {
                    if (!e3.Message.Contains(bluetoothConnectCommand))
                        throw;
                    connectFirst = true;
                    await Task.Delay(10000);
                }
                catch (UnauthorizedAccessException e4)
                {
                    logger.LogInformation("Waiting for file to be accessible " + e4.Message);
                    await Task.Delay(1000);
                }

            }

        }


        public async Task ReadCharacters()
        {
            using (FileStream reader = File.OpenRead(KEYBOARD_ID))  //keyboard with id 0
            {
                logger.LogInformation("Recieving Keys now!");
                byte[] buffer = new byte[16];
                while (true)
                {
                    //read current key information, count must be 16 even when we dont chect that atm !!
                    int count = await reader.ReadAsync(buffer, 0, buffer.Length);
                    logger.LogDebug(count + " time: " + ByteArrayToString(buffer)); //getting raw data
                    var input = CreateResult(buffer);
                    logger.LogDebug($" time: {input.Hms}.{input.Ms} type:{input.EventType} code:{input.Code} val:{input.Value}"); //getting properties from data
                    var command = DetermineCommand(input);
                    await GetBashAction(command).Bash(logger);
                }
            }
        }


        private string GetBashAction(Command c)
        {
            if (c == Command.DoNothing)
                return "";
            Console.WriteLine("Running command: " + c.ToString());
            longPressCounter = 0;
            switch (c)
            {
                case Command.ResetColor:
                    colorPaletteIndex = 7;
                    return $"{ExeLocation}/shellscripts/" + colorPalette[colorPaletteIndex];
                case Command.TurnOnOff:
                    IsOn = !IsOn;
                    return $"{ExeLocation}/shellscripts/yeelight-scene.sh 0 " + (IsOn ? "Off" : "On");
                case Command.LightUp:
                    if (Brightness < 100)
                        Brightness += 10;
                    else return "";
                    return $"{ExeLocation}/shellscripts/yeelight-brightness.sh 0 " + Brightness;
                case Command.LightDown:
                    if (Brightness > 0)
                        Brightness -= 10;
                    else return "";
                    return $"{ExeLocation}/shellscripts/yeelight-brightness.sh 0 " + Brightness;
                case Command.ColorNext:
                    colorPaletteIndex++;
                    if (colorPaletteIndex >= colorPalette.Length)
                        colorPaletteIndex = 0;
                    return $"{ExeLocation}/shellscripts/" + colorPalette[colorPaletteIndex];
                case Command.ColorPrev:
                    colorPaletteIndex--;
                    if (colorPaletteIndex < 0)
                        colorPaletteIndex = colorPalette.Length - 1;
                    return $"{ExeLocation}/shellscripts/" + colorPalette[colorPaletteIndex];
                case Command.StartDisco:
                    longPressCounter = 999;
                    return $"DISCOSPEED=120 {ExeLocation}/shellscripts/yeelight-scene.sh 0 Disco";

            }
            return "";
        }



        private Command DetermineCommand(Input input)
        {
            if (input.eventType == EvType.EV_KEY && input.value == Value.Pressed)
            {
                switch ((Code)input.Code)
                {
                    case Code.Alone: return Command.TurnOnOff;
                    case Code.Middle: return Command.ResetColor;
                    case Code.Up: return Command.LightUp;
                    case Code.Down: return Command.LightDown;
                    case Code.Left: return Command.ColorPrev;
                    case Code.Right: return Command.ColorNext;
                }
            }
            if (input.eventType == EvType.EV_KEY && input.value == Value.Repeated && input.code == Code.Middle)
            {
                longPressCounter++;
                logger.LogWarning($"Longpressing {longPressCounter}/30");
                if (longPressCounter == 30)
                    return Command.StartDisco;
            }

            return Command.DoNothing;
        }


        private Input CreateResult(byte[] buffer)
        {
            Array.Reverse(buffer, 0, buffer.Length);
            var input = new Input()
            {
                Hms = buffer[12] << 24 | buffer[13] << 16 | buffer[14] << 8 | buffer[15],
                Ms = buffer[08] << 24 | buffer[09] << 16 | buffer[10] << 8 | buffer[11],
                Value = buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3],
                Code = buffer[4] << 8 | buffer[5],
                EventType = buffer[6] << 8 | buffer[7],
            };
            return input;
        }


        public string ByteArrayToString(byte[] buffer)
        {
            StringBuilder hex = new StringBuilder(buffer.Length * 2);
            int i = 0;
            foreach (byte b in buffer)
            {
                hex.AppendFormat("{0:x2} ", b);
                i++;

                if (i == 4 || i == 6 || i == 8)
                    hex.Append("/ ");
            }
            return hex.ToString();
        }

    }


}
