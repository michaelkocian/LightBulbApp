namespace LightBulbApp
{
    public class CustomTypes
    {


        public enum Value
        {
            Released = 0,
            Pressed = 1,
            Repeated = 2,
        }

        public enum EvType
        {
            EV_SYN = 0,
            EV_KEY = 1,
            EV_ABS = 3,
            EV_MSC = 4,
        }

        public enum Code
        {
            SYS_REPORT = 0,
            One,
            Alone = 2,
            Middle = 3,
            Up = 4,
            Down = 5,
            Left = 6,
            Right = 7,

            Alone2 = 320,
            Middle2 = 320,
            Down2 = 114,
            Up2 = 115,
            Left2 = 320,
            Right2 = 320,

        }

        public enum Command
        {
            DoNothing,
            ResetColor,
            TurnOnOff,
            LightUp,
            LightDown,
            ColorNext,
            ColorPrev,
            StartDisco,
        }


        public struct Input
        {
            public int Hms;
            public int Ms;
            public int EventType;
            public int Code;
            public int Value;

            public Value value => (Value)Value;
            public Code code => (Code)Code;
            public EvType eventType => (EvType)EventType;
        }



    }
}
