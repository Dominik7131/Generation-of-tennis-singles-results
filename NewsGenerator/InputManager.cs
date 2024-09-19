

namespace NewsGenerator
{
    static class InputManager
    {
        public static MatchParameters matchParameters { get; set; } = new MatchParameters();
        public static bool isInputLoaded { get; set; } = false;

        public static bool LoadInput(string filePath = "", bool isRandomInput = false)
        {
            if (!UIComponents.isInputChanged && isInputLoaded && !isRandomInput && filePath == "")
            {
                return true;
            }
            isInputLoaded = matchParameters.LoadMatchFromInput(filePath, isRandomInput);
            if (isInputLoaded)
            {
                UIComponents.SetInput();
            }
            UIComponents.isInputChanged = false;
            return isInputLoaded;
        }

        public static bool CheckInput()
        {
            if (!isInputLoaded || UIComponents.isInputChanged)
            {
                return LoadInput();
            }
            return true;
        }
    }
}
