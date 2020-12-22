using CaveGame.Core;
using DataManagement;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

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
        public GraphRecorder<TSample> DataSet { get; set; }
        private void DrawBackground(GraphicsEngine GFX)
        {
            GFX.Rect(BackgroundColor, ScreenPosition, GraphSize);
            GFX.Text(GraphName, ScreenPosition - GFX.Fonts.Arial10.MeasureString(GraphName).GetY());
        }

        private double ScaleDatapoint(TSample datapoint)
        {

            return datapoint.Value * Scale;
        }

        private void DrawReferenceLines(GraphicsEngine GFX)
        {

            Vector2 datapoint = new Vector2(0, GraphSize.Y - (float)Scale * (1 / 60.0f));
            Vector2 linePos = ScreenPosition + datapoint;
            GFX.Line(new Color(0.7f, 0.7f, 0.7f), linePos, linePos+GraphSize.GetX());
            GFX.Text(GFX.Fonts.Arial10, "60fps", ScreenPosition + datapoint.GetY(), Color.White, TextXAlignment.Right, TextYAlignment.Center );
        }

        private void DrawLineGraph(GraphicsEngine GFX, GraphDataset<TSample> data) 
        {
            Vector2 lastDatapoint = GraphSize.GetY();
            float spacing = GraphSize.X / data.SampleCount;

            int start = Math.Max(data.Data.Count - data.SampleCount, 0);


            


            for (int idx = start; idx < data.Data.Count; idx++)
            {
                Vector2 datapoint = new Vector2(spacing * idx, GraphSize.Y - (float)ScaleDatapoint(data.Data[idx]));

                GFX.Line(data.Color, ScreenPosition + lastDatapoint, ScreenPosition + datapoint);

                lastDatapoint = datapoint;
            }    



        }


        public void Draw(GraphicsEngine GFX)
        {
            DrawBackground(GFX);
            DrawReferenceLines(GFX);
            DrawLineGraph(GFX, DataSet);
        }
    }
}
