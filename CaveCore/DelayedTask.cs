using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
	public enum TimeStepProcedure
	{
		SetToZero,
		SubtractIncrement
	}

	public class DelayedTask
	{
		public TimeStepProcedure TimeStepProcedure { get; set; }
		public bool Active { get; set; }
		public float TimeIncrement => increment;


		private float currentTime;
		private readonly float increment;
		private Action job;


		public DelayedTask(Action action, float timeIncrement, TimeStepProcedure procedure = TimeStepProcedure.SetToZero) 
		{
			job = action;
			increment = timeIncrement;
			TimeStepProcedure = procedure;
		}

		public void Update(GameTime gt) {
			if (!Active)
				return;

			currentTime += (float)gt.ElapsedGameTime.TotalSeconds;
			if (TimeStepProcedure == TimeStepProcedure.SubtractIncrement)
			{
				while (currentTime >= increment)
				{
					currentTime -= increment;
					job.Invoke();
				}

			}

			if (TimeStepProcedure == TimeStepProcedure.SetToZero)
			{
				if (currentTime >= increment)
				{
					currentTime = 0;
					job.Invoke();
				}
			}
		}

		public void Force()
		{
			currentTime = 0;
			job.Invoke();
		}


	}
}
