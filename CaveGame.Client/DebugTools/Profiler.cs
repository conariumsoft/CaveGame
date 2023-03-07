using CaveGame.Common;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CaveGame.Client.DebugTools
{
    public class ProfilerEntry
    {
        public string Name { get; set; }
        public Stopwatch Timer { get; set; }
        public double RecordedTime { get; set; }

        public ProfilerEntry()
        {
            Timer = new Stopwatch();
        }
    }
    public class ProfilerRegion
    {
        public string Name { get; set; }
        public Stopwatch Timer { get; set; }
        public double RecordedTime { get; set; }


        public Dictionary<string, ProfilerEntry> Functions;

        public ProfilerRegion()
        {
            Timer = new Stopwatch();
            Functions = new Dictionary<string, ProfilerEntry>();
        }
    }

    public static class Profiler
    {
        static ProfilerRegion currentRegion;
        static ProfilerEntry currentFunction;
        public static Dictionary<string, ProfilerRegion> Regions = new Dictionary<string, ProfilerRegion>();

        public static void StartRegion(string regionName)
        {
            if (!Regions.ContainsKey(regionName))
                Regions.Add(regionName, new ProfilerRegion { Name = regionName });

            currentRegion = Regions[regionName];
            //currentRegion.Timer.Reset();
            currentRegion.Timer.Start();
        }

        public static void EndRegion(string regionName)
        {
            currentRegion = Regions[regionName];
            currentRegion.Timer.Stop();
            currentRegion.RecordedTime = currentRegion.Timer.Elapsed.TotalMilliseconds;
            currentRegion.Timer.Reset();
        }
        
        public static void EndRegion()
        {
            if (currentRegion == null)
                return;

            currentRegion.Timer.Stop();
            currentRegion.RecordedTime = currentRegion.Timer.Elapsed.TotalMilliseconds;
            currentRegion.Timer.Reset();
        }
        public static void Start(string functionName) 
        {
            if (currentRegion == null)
                return;

            if (!currentRegion.Functions.ContainsKey(functionName))
                currentRegion.Functions.Add(functionName, new ProfilerEntry { Name = functionName });
            currentFunction = currentRegion.Functions[functionName];
            //currentFunction.Timer.Reset();
            currentFunction.Timer.Start();
        }
        public static void Track(string functionName, Action action)
        {
            Start(functionName);
            action.Invoke();
            End(functionName);
        }
        public static void End() 
        {
            if (currentFunction == null)
                return;
            currentFunction.Timer.Stop();
            currentFunction.RecordedTime = currentFunction.Timer.Elapsed.TotalMilliseconds;
            currentFunction.Timer.Reset();
        }
        public static void End(string funcname)
        {
            currentFunction = currentRegion.Functions[funcname];
            currentFunction.Timer.Stop();
            currentFunction.RecordedTime = currentFunction.Timer.Elapsed.TotalMilliseconds;
            currentFunction.Timer.Reset();
        }

        const int percent_accuracy = 0;

        public static void Draw(GraphicsEngine GFX)
        {
            
            float renderHeight = 0;
            foreach (var dataset in Regions)
            {

                var totalMS = dataset.Value.RecordedTime;
                double timeAccountedFor = totalMS;
                GFX.Text($"{dataset.Key} : {Math.Round(totalMS, 2)}ms", new Vector2(50, 100 + renderHeight));
                renderHeight += 12;
                foreach (var subset in dataset.Value.Functions)
                {
                    var frac = subset.Value.RecordedTime / totalMS;
                    timeAccountedFor -= subset.Value.RecordedTime;

                    GFX.Text($"{subset.Key} : {Math.Round(frac * 100)}% {Math.Round(subset.Value.RecordedTime, 2)}ms", new Vector2(70, 100 + renderHeight));
                    renderHeight += 12;
                }
                var unaccountedFrac = timeAccountedFor / totalMS;
                if (unaccountedFrac > 0) {
                    GFX.Text($"Other : {Math.Round(unaccountedFrac*100)}% {Math.Round(timeAccountedFor, 2)}ms", new Vector2(70, 100 + renderHeight));
                    renderHeight += 12;
                }
            }
        }
    }

}
