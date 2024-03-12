using Grammar;
using System.Collections.Generic;

namespace Lab3
{
	public class GrammarOps
	{
		public GrammarOps(IGrammar g)
		{
			this.g = g;
			compute_empty();
		}

		public ISet<Nonterminal> EmptyNonterminals { get; } = new HashSet<Nonterminal>();
		private void compute_empty()
		{
			///TODO: Add your code here...

		}

		private IGrammar g;
	}
}
