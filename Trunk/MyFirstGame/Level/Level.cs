﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyFirstGame.GameObject;
using Microsoft.Xna.Framework;
using MyFirstGame.LevelObject;
using Microsoft.Xna.Framework.Graphics;
using MyFirstGame.References;

namespace MyFirstGame.LevelObject
{
    abstract class Level
    {
        public string Tag { get; set; }
        public int LevelNumber { get; set; }
        public List<Wave> Waves { get; set; }
        public double StartTimeInSeconds { get; set; }
        public double LevelLengthInSeconds 
        {
            get
            {
                double length = 0;
                foreach (Wave wave in Waves)
                {
                    length += wave.WaveLengthInSeconds;
                }
                return length;
            }
        
        }
        public double LevelElapsedTimeInSeconds
        {
            get
            {
                return Settings.Instance.GameTime.TotalRealTime.TotalSeconds - StartTimeInSeconds;
            }
        }
        public int CurrentWaveIndex { get; set; }
        public Texture2D Background { get; set; }
        public bool IsStarted { get; private set; }
        public bool IsEnded { get; private set; }

        public Level()
        {
            IsStarted = false;
            IsEnded = false;
        }

        public void StartLevel()
        {
            StartTimeInSeconds = Settings.Instance.GameTime.TotalRealTime.TotalSeconds;
            CurrentWaveIndex = 0;
            IsStarted = true;
        }

        //TODO: based on this code levels start waves automatically and
        //end automatically after the end of the last wave,
        //how will we handle score tallying or basically level intro/outro?
        public virtual void UpdateLevel()
        {            
            if (!Waves[CurrentWaveIndex].IsEnded)
            {
                //if ((ElapsedTimeInSeconds >= Waves[CurrentWave].StartTimeInSeconds)
                //    && (ElapsedTimeInSeconds <= Waves[CurrentWave].WaveLengthInSeconds))
                //{
                    if (Waves[CurrentWaveIndex].IsStarted)
                        Waves[CurrentWaveIndex].UpdateWave();
                    else
                        Waves[CurrentWaveIndex].StartWave();
                //}                        
            }
            else
            {
                CurrentWaveIndex += 1;
                if (CurrentWaveIndex == Waves.Count)
                {
                    EndLevel();
                }
            }
        }
        
        public void EndLevel()
        {
            IsEnded = true;
            foreach (Wave wave in Waves)
            {
                //TODO:clean up waves
            }
        }

    }
}