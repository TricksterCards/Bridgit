using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BridgeBidding
{
	public class CallAnnotation : CallFeature
	{
		public enum AnnotationType { Alert, Announce, Convention }
		public string Text { get; }

		public AnnotationType Type { get; }
		public CallAnnotation(Call call, AnnotationType type, string text, params StaticConstraint[] constraints) :
			base(call, constraints)
		{
			this.Type = type;
			this.Text = text;
		}
	}

}
