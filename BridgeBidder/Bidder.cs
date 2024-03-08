using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

//using static BridgeBidding.CallFeature;

namespace BridgeBidding
{



    public abstract class Bidder
	{
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

		private static PartnerCalls _PartnerBids(Call call,
											PositionCallsFactory choicesFactory, 
											params StaticConstraint[] constraints)
		{
			return new PartnerCalls(call, choicesFactory, constraints);
		}

		public static CallFeature Forcing(Call call, params Constraint[] constraints)
		{
			return Rule(call, BidForce.Forcing1Round, constraints);
		}

		public static CallFeature Semiforcing(Call call, params Constraint[] constraints)
		{
			// TODO: What do do about semi-forcing?  
			return Rule(call, BidForce.Nonforcing, constraints);
		}

		public static CallFeature ForcingToGame(Call call, params Constraint[] constraints)
		{
			return Rule(call, BidForce.ForcingToGame, constraints);
		}

		// TODO: Need a non-forcing BidMessage...


		public static CallFeature Nonforcing(Call call, params Constraint[] constraints)
		{
			return Rule(call, BidForce.Nonforcing, constraints);
		}



		public static CallFeature Invitational(Call call, params Constraint[] constraints)
		{
			return Rule(call, BidForce.Invitational, constraints);
		}
	

		public static CallFeature Signoff(Call call, params Constraint[] constraints)
		{
			return Rule(call, BidForce.Signoff, constraints);
		}


		public static BidRule Rule(Call call, BidForce force, params Constraint[] constraints)
		{
			return new BidRule(call, force, constraints);
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
		
		// ************************************************************ STATIC CONSTRAINTS ***

		public static StaticConstraint Seat(params int[] seats)
		{
			return new SimpleStaticConstraint((call, ps) => seats.Contains(ps.Seat), getDescription: (call, ps) => $"seat {string.Join(",", seats)}");
		}
		public static StaticConstraint LastBid(Call call)
		{
			return new BidHistory(0, call);
		}
		public static StaticConstraint LastBid(int level, Suit suit)
		{
			return new BidHistory(0, new Bid(level, suit));
		}
		public static StaticConstraint LastBid(int level, Strain strain)
		{
			return new BidHistory(0, new Bid(level, strain));
		}
		public static StaticConstraint OpeningBid(Bid bid)
		{
			return new SimpleStaticConstraint((call, ps) => ps.BiddingState.OpeningBid == bid);
		}


		public static StaticConstraint CueBid = new IsCueBid(null);
		public static StaticConstraint NotCueBid = Not(CueBid);

		public static StaticConstraint Rebid = new BidHistory(0, null);
		public static StaticConstraint NotRebid = Not(Rebid);

		public static Constraint NewSuit = And(NotCueBid, new NewSuit(null));


//		public static StaticConstraint IsNewSuit(Suit suit) { return new NewSuit(suit); }

		public static StaticConstraint ID(string id)
		{
			return new LogID(id);
		}
		
		public static StaticConstraint Jump(params int[] jumpLevels)
		{
			return new JumpBid(jumpLevels);
		}

		public static StaticConstraint NonJump = Jump(0);
		public static StaticConstraint SingleJump = Jump(1);
		public static StaticConstraint DoubleJump = Jump(2);

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
		private static StaticConstraint IsReverseBid = new SimpleStaticConstraint((call, ps) => ps.IsReverse(call), description: "reverse");
		public static Constraint Reverse = And(IsReverseBid, new ShowsReverseShape());
		public static StaticConstraint NotReverse = Not(IsReverseBid);
		public static StaticConstraint ForcedToBid = new SimpleStaticConstraint((call, ps) => ps.ForcedToBid);


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

		public static StaticConstraint PassEndsAuction()
		{
			return new SimpleStaticConstraint((call, ps) => ps.BiddingState.Contract.PassEndsAuction, description: "pass ends auction");
		}

		public static StaticConstraint BidAvailable(int level, Suit suit)
		{ 
			return new SimpleStaticConstraint((call, ps) => ps.IsValidNextCall(new Bid(level, suit)));
	 	}

		public static StaticConstraint OppsContract()
		{ 
			return new SimpleStaticConstraint((call, ps) => ps.IsOpponentsContract, description: "opps contract"); 
		}


		public static StaticConstraint IsAgreedStrain = new AgreedStrain();
		public static StaticConstraint AgreedStrain(params Strain[] strains)
		{
			return new AgreedStrain(strains);
		}

		public static StaticConstraint ContractIsAgreedStrain = 
				new SimpleStaticConstraint((call, ps) =>  
					(ps.BiddingState.Contract.Bid is Bid contractBid &&
                    ps.BiddingState.Contract.IsOurs(ps.Direction) && 
                    contractBid.Strain == ps.PairState.Agreements.AgreedStrain));


		public static StaticConstraint HasShownSuit(Suit? suit = null, bool eitherPartner = false)
		{
			return new HasShownSuit(suit, eitherPartner);
		}
		
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
		public static Constraint DummyPoints((int min, int max) range)
		{
			return DummyPoints(range.min, range.max); 
		}

		public static Constraint DummyPoints(Suit? trumpSuit, (int min, int max) range)
		{
			// TODO: Rename this too????  SuitPoints
			return new ShowsPoints(trumpSuit, range.min, range.max, HasPoints.PointType.Dummy);
		}

		public static Constraint Shape(int min) { return new ShowsShape(null, min, min); }
		public static Constraint Shape(Suit suit, int count) { return new ShowsShape(suit, count, count); }
		public static Constraint Shape(int min, int max) { return new ShowsShape(null, min, max); }
		public static Constraint Shape(Suit suit, int min, int max) { return new ShowsShape(suit, min, max); }
		public static HandConstraint Balanced = new ShowsBalanced(true);
		public static HandConstraint NotBalanced = new ShowsBalanced(false);

		public static Constraint Flat(bool desired = true) { return new ShowsFlat(desired); }



		public static StaticConstraint RHO(Constraint constraint)
		{
			return new PositionProxy(PositionProxy.RelativePosition.RHO, constraint);
		}

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

		public static HandConstraint Quality(SuitQuality min, SuitQuality max) {
			return new ShowsQuality(null, min, max);
		}
		public static HandConstraint Quality(Suit suit, SuitQuality min, SuitQuality max)
		{ return new ShowsQuality(suit, min, max); }

		public static Constraint And(params Constraint[] constraints)
		{
			return new ConstraintGroup(constraints);
		}


		public static HandConstraint ExcellentPlusSuit = IsExcellentPlusSuit(null);
        public static HandConstraint IsExcellentPlusSuit(Suit? suit = null)
        { return new ShowsQuality(suit, SuitQuality.Excellent, SuitQuality.Solid); }

		public static HandConstraint BadSuit = IsBadSuit(null);
		public static HandConstraint IsBadSuit(Suit? suit)
		{ return new ShowsQuality(suit, SuitQuality.Poor, SuitQuality.Poor);
		}
        public static HandConstraint GoodPlusSuit = IsGoodPlusSuit(null);
		public static HandConstraint IsGoodPlusSuit(Suit? suit)
		{ return new ShowsQuality(suit, SuitQuality.Good, SuitQuality.Solid); }

		public static HandConstraint DecentPlusSuit = IsDecentPlusSuit(null);
		public static HandConstraint IsDecentPlusSuit(Suit? suit)
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

		public static HandConstraint LongestSuit = IsLongestSuit(null);
		public static HandConstraint IsLongestSuit(Suit? suit)
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


		public static Constraint KeyCards(Suit suit, int a, int b, bool? hasQueen = null)
		{
			return new KeyCards(suit, hasQueen, a, b);
		}

		public static Constraint PairKeyCards(Suit suit, bool? hasQueen, params int[] count)
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
			return And(new TakeoutSuit(suit), NotCueBid);
		}



		public static Constraint Fit(int count = 8, Suit? suit = null, bool desiredValue = true)
		{
			return new PairShowsMinShape(suit, count, desiredValue);
			//return And(HasShownSuit(suit, eitherPartner: true), new PairShowsMinShape(suit, count, desiredValue));
		}

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

		public static HandConstraint PairPoints(Suit? suit, (int Min, int Max) range)
		{
			return new PairShowsPoints(suit, range.Min, range.Max);
		}

		public static Constraint AgreedStrainPoints((int Min, int Max) range)
		{
			return new PairShowsPoints(range.Min, range.Max);
		}


		public static Constraint OppsStopped(bool desired = true)
		{
			return new ShowsOppsStopped(desired);
		}




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
            return And(Partner(HasShownSuit(suit)), Fit(fit, suit), Jump(jump), ShowsTrumpSuit(suit));
        }
   


    }
};

