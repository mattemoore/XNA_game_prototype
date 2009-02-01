﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyFirstGame.GameObject;
using Microsoft.Xna.Framework;

namespace MyFirstGame.LevelObject
{
    class FirstLevel : Level
    {
        public FirstLevel()
        {
            Tag = "This is the first level.";
            Background = References.Textures.Instance.FirstLevelBackground;

            Sprites = new List<Sprite>();
            FirstSprite fs = new FirstSprite();
            fs.Position = new Vector2(100, 200);
            Sprites.Add(fs);
                       
            Waves = new List<Wave>();
            FirstWave fw = new FirstWave();
            fw.Tag = "Wave1";
            Waves.Add(fw);

        }

        public override void UpdateLevel()
        {
            base.UpdateLevel();
        }
    }
}
