using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Utility
{
	public static class Tennis
	{
		public static bool IsLastSetNotFinished(int scoreWinner, int scoreLoser)
		{
			bool isScoreLessThanSix = scoreWinner < 6 && scoreLoser < 6;
			bool isScoreSixAndFive = scoreWinner == 6 && scoreLoser == 5;
			bool isLastSetNotFinished = isScoreLessThanSix || isScoreSixAndFive;

			return isLastSetNotFinished;
		}
	}
}