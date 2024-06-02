using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

//using static BridgeBidding.CallFeature;

namespace BridgeBidding
{

    public abstract class Bidder
	{
		// TODO: These need to implement IShowAgreements, not simple constraint.

		public static CallFeature PartnerBids(CallFeaturesFactory cff)
		{
			return PartnerBids(PositionCalls.FromCallFeaturesFactory(cff));
		}

        public static CallFeature PartnerBids(PositionCallsFactory pcf)
        {
            return _PartnerBids(null, pcf, new StaticConstraint[0]);
        }

	

		public static CallFeature PartnerBids(Call call, CallFeaturesFactory cff, params StaticConstraint[] constraints)
		{
			Debug.Assert(call != null);
			return _PartnerBids(call, PositionCalls.FromCallFeaturesFactory(cff), constraints);
		}
		public static CallFeature PartnerBids(Call call, PositionCallsFactory choicesFactory)
		{
			Debug.Assert(call != null);
			return _PartnerBids(call, choicesFactory, new StaticConstraint[0]);
		}

		private static CallProperties _PartnerBids(Call call,
											PositionCallsFactory choicesFactory, 
											params StaticConstraint[] constraints)
		{
			return new CallProperties(call, choicesFactory, false, false, false, null, constraints);
		}




		public static BidRule Shows(Call call, params Constraint[] constraints)
		{
			return new BidRule(call, constraints);
		}

		public static CallFeature Alert(Call call, string text, params StaticConstraint[] constraints)
		{
			return new CallAnnotation(call, CallAnnotation.AnnotationType.Alert, text, constraints);
		}
		public static CallFeature Announce(Call call, string text, params StaticConstraint[] constraints)
		{
			return new CallAnnotation(call, CallAnnotation.AnnotationType.Announce, text, constraints);
		}

		public static CallFeature Convention(Call call, string text, params StaticConstraint[] constraints)
		{
			return new CallAnnotation(call, CallAnnotation.AnnotationType.Convention, text, constraints);
		}

		public static CallAnnotation Convention(string text, params StaticConstraint[] constraints)
		{
			return new CallAnnotation(null, CallAnnotation.AnnotationType.Convention, text, constraints);
		}
		
		public static CallFeatureGroup Properties(Call call, PositionCallsFactory partnerBids = null, bool forcing1Round = false, bool forcingToGame = false, bool agreeTrump = false, Suit? trump = null, 
				 	string alert = null, string announce = null, string convention = null, 
					StaticConstraint onlyIf = null)
		{
			var group = new CallFeatureGroup();
			group.Features.Add(new CallProperties(call, partnerBids, forcing1Round, forcingToGame, agreeTrump, trump, onlyIf));
			if (alert != null)
			{
				group.Features.Add(Alert(call, alert, onlyIf));
			}
			if (announce != null)
			{
				group.Features.Add(Announce(call, announce, onlyIf));
			}
			if (convention != null)
			{
				group.Features.Add(Convention(call, convention, onlyIf));
			}
			return group;
		}	

		public static CallFeatureGroup Properties(IEnumerable<Call> calls, PositionCallsFactory partnerBids = null, bool forcing1Round = false, bool forcingToGame = false, bool agreeTrump = false, Suit? trump = null, 
				    string alert = null, string announce = null, string convention = null, 
					StaticConstraint onlyIf = null)
		{
			var group = new CallFeatureGroup();
			foreach (Call call in calls)
			{
				group.Features.Add(Properties(call, partnerBids, forcing1Round, forcingToGame, agreeTrump, trump, alert, announce, convention, onlyIf));
			}
			return group;			
		}


		// ************************************************************ STATIC CONSTRAINTS ***

		public static StaticConstraint IsSeat(params int[] seats)
		{
			return new SimpleStaticConstraint((call, ps) => seats.Contains(ps.Seat), getDescription: (call, ps) => $"seat {string.Join(",", seats)}");
		}
		public static StaticConstraint IsLastBid(Call call)
		{
			return new BidHistory(0, call);
		}
		public static StaticConstraint IsLastBid(int level, Suit suit)
		{
			return new BidHistory(0, new Bid(level, suit));
		}
		public static StaticConstraint IsLastBid(int level, Strain strain)
		{
			return new BidHistory(0, new Bid(level, strain));
		}
		public static StaticConstraint IsOpeningBid(Bid bid)
		{
			return new SimpleStaticConstraint((call, ps) => ps.BiddingState.OpeningBid.Equals(bid));
		}


		public static readonly StaticConstraint IsCueBid = new IsCueBid(null);
		public static readonly StaticConstraint IsNotCueBid = Not(IsCueBid);

		public static readonly StaticConstraint IsRebid = new BidHistory(0, null);
		public static readonly StaticConstraint IsNotRebid = Not(IsRebid);

		public static readonly StaticConstraint IsNewSuit = new ConstraintGroup(IsNotCueBid, new NewSuit(null));


//		public static StaticConstraint IsNewSuit(Suit suit) { return new NewSuit(suit); }

		public static StaticConstraint ID(string id)
		{
			return new LogID(id);
		}
		
		public static StaticConstraint IsJump(params int[] jumpLevels)
		{
			return new JumpBid(jumpLevels);
		}

		public static StaticConstraint IsNonJump = IsJump(0);
		public static StaticConstraint IsSingleJump = IsJump(1);
		public static StaticConstraint IsDoubleJump = IsJump(2);

		// Various vulnerability constraints.  Be careful with Not()
		public static StaticConstraint IsVul = new SimpleStaticConstraint((call, ps) => ps.IsVulnerable, description: "vul");
		public static StaticConstraint IsNotVul = new SimpleStaticConstraint((call, ps) => !ps.IsVulnerable, description: "not vul");
		public static StaticConstraint IsFavVul = new SimpleStaticConstraint((call, ps) => !ps.IsVulnerable && ps.RHO.IsVulnerable, description: "favorable vul");
		public static StaticConstraint IsUnfavVul = new SimpleStaticConstraint((call, ps) => ps.IsVulnerable && !ps.RHO.IsVulnerable, description: "unfavorable vul");
		public static StaticConstraint BothVul = new SimpleStaticConstraint((call, ps) => ps.IsVulnerable && ps.RHO.IsVulnerable, description: "all vul");
		public static StaticConstraint BothNotVul = new SimpleStaticConstraint((call, ps) => !ps.IsVulnerable && !ps.RHO.IsVulnerable, description: "none vul");
		
		/*	-- TODO Figure out what we want to do about constraint groups.  Specifically static constraint groups.
		public static StaticConstraint StaticAnd(params StaticConstraint[] constraints)
		{
			return new StaticConstraint((call, ps) => {
				foreach (var constraint in constraints)
				{
					if (!constraint.Conforms(call, ps) retrn false;
				}
				return true;
			});
		}
		*/ 

	
		// The static constaint "IsReverseBid" simply checks if the call is a reverse bid.  The IsReverse also shows the shape of the reverse bid.
		public static StaticConstraint IsReverseBid = new SimpleStaticConstraint((call, ps) => ps.IsReverse(call), description: "reverse");
	
		public static StaticConstraint IsNotReverse = Not(IsReverseBid);
		public static StaticConstraint IsForcedToBid = new SimpleStaticConstraint((call, ps) => ps.ForcedToBid);

		public static StaticConstraint IsForcedToGame = new SimpleStaticConstraint((call, ps) => ps.PairState.ForcedToGame);


		public static StaticConstraint Not(StaticConstraint c)
		{
			return new SimpleStaticConstraint(eval: (call, ps) => !c.Conforms(call, ps),
										getDescription: (call, ps) => { 
											if (c is IDescribeConstraint dc)
											{
												var desc = dc.Describe(call, ps);
												return string.IsNullOrEmpty(desc) ? null : $"not {desc}";
											}
											return null;
										});
		}

		public static StaticConstraint Partner(Constraint constraint)
		{
			return new PositionProxy(PositionProxy.RelativePosition.Partner, constraint);
		}

		public static StaticConstraint RHO(Constraint constraint)
		{
			return new PositionProxy(PositionProxy.RelativePosition.RHO, constraint);
		}

		public static readonly StaticConstraint IsFinalCall = new SimpleStaticConstraint((call, ps) => ps.BiddingState.Contract.PassEndsAuction, description: "pass ends auction");

		public static readonly StaticConstraint IsNotFinalCall = Not(IsFinalCall);

		public static StaticConstraint IsBidAvailable(int level, Suit suit)
		{ 
			return new SimpleStaticConstraint((call, ps) => ps.IsValidNextCall(new Bid(level, suit)));
	 	}

		public static readonly StaticConstraint IsOppsContract = new SimpleStaticConstraint((call, ps) => ps.IsOpponentsContract, description: "opps contract"); 
	

		public static StaticConstraint ContractIsAgreedStrain = 
				new SimpleStaticConstraint((call, ps) =>  
					(ps.BiddingState.Contract.Bid is Bid contractBid &&
                    ps.BiddingState.Contract.IsOurs(ps.Direction) && 
                    contractBid.Suit == ps.PairState.LastShownSuit));


		public static StaticConstraint HasShownSuit(Suit? suit = null, bool eitherPartner = false)
		{
			return new HasShownSuit(suit, eitherPartner);
		}

		public static readonly StaticConstraint IsJumpShift = new ConstraintGroup(IsSingleJump, IsNewSuit);

		public static readonly StaticConstraint IsPartnersSuit = Partner(HasShownSuit(eitherPartner: false));
	
		// ************************************  DYNAMIC CONSTRAINTS ***
		public static HandConstraint PassIn4thSeat()
		{
			return new PassIn4thSeat();
		}
		public static HandConstraint HighCardPoints(int min, int max)
		{
			 return new ShowsPoints(null, min, max, HasPoints.PointType.HighCard); 
		
		}
		public static HandConstraint HighCardPoints((int min, int max) range)
		{
			return HighCardPoints(range.min, range.max);
		}

		public static HandConstraint Points(int min, int max)
		{
			return new ShowsPoints(null, min, max, HasPoints.PointType.Starting);
		}

		public static HandConstraint Points((int min, int max) range) {
			return Points(range.min, range.max); }

		public static HandConstraint DummyPoints(int min, int max)
		{
			// TODO: Rename this??? SuitPoints???  
			return new ShowsPoints(null, min, max, HasPoints.PointType.Dummy);
		}
		public static HandConstraint DummyPoints((int min, int max) range)
		{
			return DummyPoints(range.min, range.max); 
		}

		public static HandConstraint DummyPoints(Suit? trumpSuit, (int min, int max) range)
		{
			// TODO: Rename this too????  SuitPoints
			return new ShowsPoints(trumpSuit, range.min, range.max, HasPoints.PointType.Dummy);
		}

		public static HandConstraint Shape(int min) { return new ShowsShape(null, min, min); }
		public static HandConstraint Shape(Suit suit, int count) { return new ShowsShape(suit, count, count); }
		public static HandConstraint Shape(int min, int max) { return new ShowsShape(null, min, max); }
		public static HandConstraint Shape(Suit suit, int min, int max) { return new ShowsShape(suit, min, max); }
		public static readonly HandConstraint Balanced = new ShowsBalanced(true);
		public static readonly HandConstraint NotBalanced = new ShowsBalanced(false);

		public static readonly HandConstraint Flat = new ShowsFlat(true);

		public static readonly HandConstraint NotFlat = new ShowsFlat(false);




		public static HandConstraint HasShape(int count)
		{
			return HasShape(count, count);
		}

		public static HandConstraint HasMinShape(int count)
		{
			return HasMinShape(null, count);
		}

		public static HandConstraint HasMinShape(Suit? suit, int count)
		{
			return new HasMinShape(suit, count);
		}


		public static HandConstraint HasShape(int min, int max)
		{
			return new HasShape(null, min, max);
		}

		public static readonly HandConstraint ReverseShape = new HasReverseShape();


		public static HandConstraint Quality(SuitQuality min, SuitQuality max) {
			return new ShowsQuality(null, min, max);
		}
		public static HandConstraint Quality(Suit suit, SuitQuality min, SuitQuality max)
		{ return new ShowsQuality(suit, min, max); }

		public static Constraint And(params Constraint[] constraints)
		{
			return new ConstraintGroup(constraints);
		}


		public static readonly HandConstraint ExcellentPlusSuit = ShowsExcellentPlusSuit(null);
        public static HandConstraint ShowsExcellentPlusSuit(Suit? suit = null)
        { return new ShowsQuality(suit, SuitQuality.Excellent, SuitQuality.Solid); }

		public static HandConstraint BadSuit = ShowsBadSuit(null);
		public static HandConstraint ShowsBadSuit(Suit? suit)
		{ return new ShowsQuality(suit, SuitQuality.Poor, SuitQuality.Poor);
		}
        public static HandConstraint GoodPlusSuit = ShowsGoodPlusSuit(null);
		public static HandConstraint ShowsGoodPlusSuit(Suit? suit)
		{ return new ShowsQuality(suit, SuitQuality.Good, SuitQuality.Solid); }

		public static HandConstraint DecentPlusSuit = ShowsDecentPlusSuit(null);
		public static HandConstraint ShowsDecentPlusSuit(Suit? suit)
		{ return new ShowsQuality(suit, SuitQuality.Decent, SuitQuality.Solid); }

		public static HandConstraint SuitLosers(int min, int max, Suit? suit = null)
		{
			return new ShowsLosers(false, suit, min, max);
		}

	
		public static HandConstraint Better(Suit better, Suit worse) { return new ShowsBetterSuit(better, worse, worse, false); }

		public static HandConstraint BetterOrEqual(Suit better, Suit worse) { return new ShowsBetterSuit(better, worse, better, false); }

		public static HandConstraint BetterThan(Suit worse) { return new ShowsBetterSuit(null, worse, worse, false); }

		public static HandConstraint BetterOrEqualTo(Suit worse) { return new ShowsBetterSuit(null, worse, null, false); }

		public static HandConstraint LongerThan(Suit shorter) { return new ShowsBetterSuit(null, shorter, shorter, true); }

		public static HandConstraint LongerOrEqualTo(Suit shorter) { return new ShowsBetterSuit(null, shorter, null, true); }
		public static HandConstraint Longer(Suit longer, Suit shorter) { return new ShowsBetterSuit(longer, shorter, shorter, true); }

		public static HandConstraint LongerOrEqual(Suit longer, Suit shorter) { return new ShowsBetterSuit(longer, shorter, longer, true); }

		public static HandConstraint LongestSuit = ShowsLongestSuit(null);
		public static HandConstraint ShowsLongestSuit(Suit? suit)
		{
			return new ShowsLongestSuit(suit);
		}


		public static HandConstraint DummyPoints(Suit trumpSuit, (int min, int max) range)
		{
			return new ShowsPoints(trumpSuit, range.min, range.max, HasPoints.PointType.Dummy);
		}

		public static Constraint LongestMajor(int max)
		{
			return And(Shape(Suit.Hearts, 0, max), Shape(Suit.Spades, 0, max));
		}




/*
		public static Constraint ShowsTrump = new ShowsTrump(null);

		public static Constraint AgreeOnStrain(Strain trumpStrain)
		{
			return new ShowsTrump(trumpStrain);
		}

		public static Constraint ShowsTrumpSuit(Suit? trumpSuit)
		{
			return (trumpSuit is Suit s) ? AgreeOnStrain(s.ToStrain()) : ShowsTrump;
		}


		public static Constraint AgreedAnySuit =  AgreedStrain(Strain.Clubs, Strain.Diamonds, Strain.Hearts, Strain.Spades);
*/

		public static HandConstraint KeyCards(Suit suit, int a, int b, bool? hasQueen = null)
		{
			return new KeyCards(suit, hasQueen, a, b);
		}

		public static HandConstraint PairKeyCards(Suit suit, bool? hasQueen, params int[] count)
		{
			return new PairKeyCards(suit, hasQueen, count);
		}

		public static Constraint Aces(params int[] count)
		{
			return new KeyCards(null, null, count);
		}

		public static Constraint PairAces(params int[] count)
		{
			return new PairKeyCards(null, null, count);
		}

		public static Constraint Kings(params int[] count)
		{
			return new Kings(count);
		}

		public static Constraint PairKings(params int[] count)
		{
			return new PairKings(count);
		}

		// Perhaps rename this.  Perhaps move this to takeout...
		public static Constraint TakeoutSuit(Suit? suit = null)
		{
			return And(new TakeoutSuit(suit), IsNotCueBid);
		}



		public static HandConstraint Fit(int count = 8, Suit? suit = null, bool desiredValue = true)
		{
			return new PairShowsMinShape(suit, count, desiredValue);
			//return And(HasShownSuit(suit, eitherPartner: true), new PairShowsMinShape(suit, count, desiredValue));
		}

		public static readonly HandConstraint Fit8Plus = Fit(8);
		public static readonly HandConstraint Fit9Plus = Fit(9);

		public static Constraint Fit(Suit suit, bool desiredValue = true)
		{
			return Fit(8, suit, desiredValue);
		}

		public static Constraint Fit(bool desiredValue)
		{
			return Fit(8, null, desiredValue);
		}



		public static HandConstraint PairPoints((int Min, int Max) range)
		{
			return PairPoints(null, range);
		}

		public static HandConstraint PairPoints(int min, int max) => PairPoints((min, max));
		public static HandConstraint PairPoints(Suit? suit, (int Min, int Max) range)
		{
			return new PairShowsPoints(suit, range.Min, range.Max);
		}

		public static Constraint AgreedStrainPoints((int Min, int Max) range)
		{
			return new PairShowsPoints(range.Min, range.Max);
		}


		public static readonly HandConstraint OppsStopped = new ShowsOppsStopped(true);
		public static readonly HandConstraint OppsNotStopped = new ShowsOppsStopped(false);




		public static Constraint RuleOf17(Suit? suit = null)
		{
			return new RuleOf17(suit);
		}

		public static Constraint Break(bool isStatic, string name)
		{	
			if (isStatic)
			{
				return new StaticBreak(name);
			}
			return new Break(name);
		}

		public static HandConstraint BetterMinor(Suit? suit = null)
		{
			return new BetterMinor(suit);
		}

		public static HandConstraint RuleOf9()
		{
			return new RuleOf9();
		}




		// THE FOLLOWING CONSTRAINTS ARE GROUPS OF CONSTRAINTS
        public static Constraint RaisePartner(Suit? suit = null, int jump = 0, int fit = 8)
        {
            return And(Partner(HasShownSuit(suit)), Fit(fit, suit), IsJump(jump));
        }
   


    }
};

