using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using BridgeBidding.PBN;

namespace BridgeBidding
{
	public enum Scoring { MP, IMP };

   	public enum Vulnerable { None = 0, NS = 1, EW = 2, All = 3 }

    public enum Pair { NS, EW }


	public static class BridgeBidder
	{
		/// <summary>
		/// Suggests the next bid in the auction given a specified hand in a deal.  Where applicable, strings
		/// adhere to the Portable Bridge Notation (PBN) format.
		/// </summary>
		/// <param name="deal">Deal string in PBN format of D:h1 h2 h3 h4 where D: is the direction of the
		/// dealer, and h1-4 are full hands in PBN format or "-" to indicate unknown hands.  The hand for the next to act
		/// must be known.</param>
		/// <param name="vulnerable">One of "None", "All", "NS" or "EW"</param>
		/// <param name="auction">Can be null to or empty string to indicate no auction.  Otherwise contains
		/// a space separated string compatible with the PBN auction format.</param>
		/// <param name="bidSystemNS">For future use.</param>
		/// <param name="bidSysttemEW">For future use.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="AuctionException"></exception> 
		public static string SuggestBid(string deal, string vulnerable, string auction, string bidSystemNS = "TwoOverOneGameForce", string bidSystemEW = "TwoOverOneGameForce")
		{
			// For now we will only allow SAYC bidding system.
            if (bidSystemNS != "TwoOverOneGameForce" || bidSystemEW != "TwoOverOneGameForce")
            {
                throw new ArgumentException("Bidding system is limited to 2/1");
            }

			var game = Game.Parse(deal, vulnerable);
			game.ParseAuction(auction);
			var callDetails = SuggestCall(game);
			return callDetails.Call.ToString();
		}

		// TODO: Add bidding system parameters here.  
		public static CallDetails SuggestCall(Game game, bool throwExceptionIfNoBestCall = false)
		{
			var biddingState = new BiddingState(game);
			if (!biddingState.NextToAct.HasHand)
			{
				throw new AuctionException(null, biddingState.NextToAct.Direction, biddingState.Contract,
								"Can not suggest next bid when position has no defined hand.");
			}
			var choices = biddingState.GetCallChoices();
			if (choices.BestCall != null) return choices.BestCall;
			if (throwExceptionIfNoBestCall)
				throw new AuctionException(null, biddingState.NextToAct.Direction, biddingState.Contract,
						"No suggested call given by bidding system.");
			//
			// This is an error, so we will just pass.  If there is a rule for pass then select it,
			// otherwise create one.
			if (!choices.ContainsKey(Call.Pass)) choices.AddPassRule();
			return choices[Call.Pass];
		}

		public static string[] ExplainHistory(string deal, string auction, string nsSystem = "SAYC", string ewSystem = "SAYC")
		{
			throw new NotImplementedException();
		}

		public static HandSummary[] SummarizeHistory(string deal, string auction, string nsSystem = "SAYC", string ewSystem = "SAYC")
		{
			throw new NotImplementedException();
		}







		// PBN defines deal import as D:(hand1) (hand2) (hand3) (hand4)
		// where "D" is be equal to the dealer.  
		

	}
}
