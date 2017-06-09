using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using CommonCode;
using CommonCode.UI;

namespace SessionViewer
{
    class ErrorScreen : Screen
    {
        StringPositionColor[] errors;

        public ErrorScreen(string[] errorMessages) : base()
        {
            if(!ScreenManager.Globals.Fonts.FontLibrary.ContainsKey("Error"))
                ScreenManager.Globals.Fonts.AddFont("Error", ".//Fonts//ErrorFont.spritefont");
            Draw3DFirst = true;
            ScreenManager.IsMouseVisible = true;
            List<StringPositionColor> tempLineList = new List<StringPositionColor>();
            string[] tempErrors = new string[errorMessages.Length + 1];
            tempErrors[0] = "Error!  Please resolve these issues and retry.";
            for(int i = 1; i < tempErrors.Length; i++)
                tempErrors[i] = errorMessages[i];
            errorMessages = tempErrors;
            int yDist = 0;
            for (int i = 0; i < errorMessages.Length; i++)
            {
                int stringLength = (int)ScreenManager.Globals.Fonts["Error"].MeasureString(errorMessages[i]).X;
                if (stringLength > 790)
                { 
                    string clippedString = errorMessages[i];
                    while (ScreenManager.Globals.Fonts["Error"].MeasureString(clippedString).X > 790)
                    {
                        int lastSeperator = clippedString.LastIndexOf(" ");
                        if(lastSeperator == -1)
                            lastSeperator = clippedString.LastIndexOf("/");
                        clippedString = clippedString.Remove(lastSeperator);
                    }
                    tempLineList.Add(new StringPositionColor(clippedString, new Vector2(0, yDist), Color.White));
                    yDist += 15;
                    tempLineList.Add(new StringPositionColor(errorMessages[i].Replace(clippedString, "   "), new Vector2(0, yDist), Color.White));
                }
                else
                    tempLineList.Add(new StringPositionColor(errorMessages[i], new Vector2(0, yDist), Color.White));
                yDist += 15;
            }

            errors = tempLineList.ToArray();
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (InputManager.IsKeyTriggered(Keys.Escape))
                RemoveSelf();
        }

        public override void Update(GameTime gameTime)
        {
            ScreenManager.IsMouseVisible = true;
        }

        public override void Draw(GameTime gameTime)
        {
            for (int i = 0; i < errors.Length; i++)
                ScreenManager.Globals.sb.DrawString(ScreenManager.Globals.Fonts["Error"], errors[i].Text, errors[i].Position, errors[i].Color);
        }
    }
}
