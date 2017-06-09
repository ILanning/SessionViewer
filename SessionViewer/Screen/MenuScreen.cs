using CommonCode;
using CommonCode.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace SessionViewer
{
    abstract class MenuScreen : Screen
    {
        public List<Button> buttons = new List<Button>();
        protected int selectedItem;
        protected string menuTitle;

        public MenuScreen(string title) : base()
        {
            menuTitle = title;
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < buttons.Count; i++)
                buttons[i].Update(gameTime);
        }

        public override void HandleInput(GameTime gameTime)
        {
            for (int i = 0; i < buttons.Count; i++)
                buttons[i].HandleInput();
            if (InputManager.IsKeyTriggered(Keys.Down))
            {
                selectedItem++;
                if (selectedItem > buttons.Count - 1)
                    selectedItem = 0;
            }
            if (InputManager.IsKeyTriggered(Keys.Up))
            {
                selectedItem--;
                if (selectedItem < 0)
                    selectedItem = buttons.Count - 1;
            }
            for (int i = 0; i < buttons.Count; i++)
            {
                //buttons[i].IsSelected = false;
                if (buttons[i].IsHovered)
                    selectedItem = i;
            }
            //buttons[selectedItem].IsSelected = true;
        }
    }
}
