
using CaveGame.Common.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using CaveGame.Common;

namespace CaveGame.Client.DebugTools
{
    public interface GraphSample { 
    
        double Value { get; set; }
    }
    public interface GraphDataset<TSample> where TSample: GraphSample {
        Color Color { get; set; }
        int SampleCount { get; set; }
        List<TSample> Data { get; set; }
    }

    public class GraphRecorder<TSample>: GraphDataset<TSample> where TSample : GraphSample
    {
        public float Boldness { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
        public int SampleCount { get; set; }
        public List<TSample> Data { get; set; }
        public TSample Average;

        public GraphRecorder()
        {
            Data = new List<TSample>();
        }


        public void Push(TSample sample)
        {
            Data.Add(sample);
            if (Data.Count > SampleCount)
            {
                Data.RemoveAt(0);
            }
        }

    }


    public class GraphRenderer<TSample> where TSample : GraphSample
    {
        public Vector2 ScreenPosition { get; set; }
        public Vector2 GraphSize { get; set; }
        public float XAxisMin { get; set; }
        public float XAxisMax { get; set; }
        public float YAxisMin { get; set; }
        public float YAxisMax { get; set; }
        public string GraphName { get; set; }

        public float Scale { get; set; }
        public Color BackgroundColor { get; set; }
        //public GraphRecorder<TSample> DataSet { get; set; }
        private void DrawBackground(GraphicsEngine GFX)
        {
            GFX.Rect(BackgroundColor, ScreenPosition, GraphSize);
            GFX.Text(GraphName, ScreenPosition - GFX.Fonts.Arial10.MeasureString(GraphName).GetY());
        }

        private double ScaleDatapoint(TSample datapoint)
        {

            return datapoint.Value * Scale;
        }

        private void DrawReferenceLine(GraphicsEngine GFX, float value, string label, Color color)
        {
            Vector2 datapoint = new Vector2(0, GraphSize.Y - (float)Scale * (value));
            Vector2 linePos = ScreenPosition + datapoint;
            GFX.Line(color, linePos, linePos + GraphSize.GetX());
            GFX.Text(GFX.Fonts.Arial10, label, ScreenPosition + datapoint.GetY(), color, TextXAlignment.Right, TextYAlignment.Center);
        }

        private void DrawReferenceLines(GraphicsEngine GFX)
        {
            DrawReferenceLine(GFX, 120, "120FPS", Color.Gray);
            DrawReferenceLine(GFX, 60, "60FPS", Color.Gray);


           /* Vector2 datapoint30fps = new Vector2(0, GraphSize.Y - (float)Scale * (30.0f));
            Vector2 linePos30fps = ScreenPosition + datapoint30fps;
            GFX.Line(new Color(0.7f, 0.7f, 0.7f), linePos30fps, linePos30fps + GraphSize.GetX());
            GFX.Text(GFX.Fonts.Arial10, "30fps", ScreenPosition + datapoint30fps.GetY(), Color.White, TextXAlignment.Right, TextYAlignment.Center);*/
        }

        public void DrawLineGraph(GraphicsEngine GFX, GraphRecorder<TSample> data) 
        {
            Vector2 lastDatapoint = GraphSize.GetY();
            float spacing = GraphSize.X / data.SampleCount;

            int start = Math.Max(data.Data.Count - data.SampleCount, 0);


            


            for (int idx = start; idx < data.Data.Count; idx++)
            {
                float clamped = Scale * (float)Math.Clamp(data.Data[idx].Value, YAxisMin, YAxisMax);
                Vector2 datapoint = new Vector2(spacing * idx, GraphSize.Y - clamped);

                GFX.Line(data.Color, ScreenPosition + lastDatapoint, ScreenPosition + datapoint, data.Boldness);

                lastDatapoint = datapoint;
            }    



        }


        public void Draw(GraphicsEngine GFX)
        {
            DrawBackground(GFX);
            DrawReferenceLines(GFX);
           //DrawLineGraph(GFX, DataSet);
        }
    }
}
