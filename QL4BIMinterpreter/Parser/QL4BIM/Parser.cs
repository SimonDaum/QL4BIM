/*
Copyright (c) 2017 Chair of Computational Modeling and Simulation (CMS), 
Prof. André Borrmann, 
Technische Universität München, 
Arcisstr. 21, D-80333 München, Germany

This file is part of QL4BIMinterpreter.

QL4BIMinterpreter is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
any later version.

QL4BIMinterpreter is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with QL4BIMinterpreter. If not, see <http://www.gnu.org/licenses/>.

*/


using System;

namespace QL4BIMinterpreter.QL4BIM {



public class Parser {
	public const int _EOF = 0;
	public const int _float = 1;
	public const int _number = 2;
	public const int _lowString = 3;
	public const int _upString = 4;
	public const int _string_ = 5;
	public const int _minust = 6;
	public const int _alias = 7;
	public const int maxT = 27;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public event PartParsedEventHandler PartParsed;
public event ContextChangedEventHandler ContextChanged;
 
protected virtual void OnParsed(ParserParts parsePart, string currentToken = "")
{
    var e = new PartParsedEventArgs(parsePart, currentToken);
    //Console.WriteLine(e.ToString());
    PartParsed?.Invoke(this, e);
}

protected virtual void OnContext(ParserContext context)
{
    var e = new ContextChangedEventArgs(context);
    //Console.WriteLine(e.ToString());
    ContextChanged?.Invoke(this, e);
}



	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void QL4BIM() {
		if (la.kind == 3) {
			OnContext(ParserContext.GlobalBlock); 
			statement_();
			while (la.kind == 3) {
				statement_();
			}
			while (la.kind == 26) {
				func_();
			}
		} else if (la.kind == 26) {
			func_();
			while (la.kind == 26) {
				func_();
			}
		} else SynErr(28);
	}

	void statement_() {
		OnContext(ParserContext.Statement); 
		OnContext(ParserContext.Variable); 
		Expect(3);
		OnParsed(ParserParts.SetRelVar, t.val); 
		while (la.kind == 8 || la.kind == 9) {
			if (la.kind == 8) {
				Get();
				OnParsed(ParserParts.EmptyRelAtt); 
			} else {
				Get();
				Expect(3);
				OnParsed(ParserParts.RelAttStr, t.val); 
				Expect(10);
				Expect(3);
				OnParsed(ParserParts.RelAttStr, t.val); 
				while (la.kind == 10) {
					Get();
					Expect(3);
					OnParsed(ParserParts.RelAttStr, t.val); 
				}
				Expect(11);
			}
		}
		Expect(12);
		expression();
	}

	void func_() {
		OnContext(ParserContext.UserFuncBlock); 
		Expect(26);
		Expect(4);
		OnParsed(ParserParts.DefOp,t.val); 
		if (la.kind == 7) {
			Get();
			OnParsed(ParserParts.DefAlias,t.val); 
		}
		Expect(13);
		setRelFormalArg();
		while (la.kind == 14) {
			Get();
			setRelFormalArg();
		}
		Expect(15);
		Expect(13);
		statement_();
		while (la.kind == 3) {
			statement_();
		}
		Expect(15);
	}

	void expression() {
		OnContext(ParserContext.Operator ); 
		Expect(4);
		OnParsed(ParserParts.Operator, t.val); 
		Expect(13);
		argument();
		while (la.kind == 14) {
			Get();
			argument();
		}
		Expect(15);
	}

	void argument() {
		OnContext(ParserContext.Argument); 
		if (la.kind == 3 || la.kind == 9) {
			setRelAttLongShort();
			if (StartOf(1)) {
				if (la.kind == 25) {
					exType();
					OnContext(ParserContext.TypePredicate); 
				} else if (la.kind == 24) {
					exAtt();
					if (StartOf(2)) {
						OnContext(ParserContext.AttPredicate); 
						attPredicate();
					}
				} else {
					OnContext(ParserContext.CountPredicate); 
					countPredicate();
				}
			}
		} else if (StartOf(3)) {
			constant();
		} else SynErr(29);
		OnContext(ParserContext.ArgumentEnd); 
	}

	void setRelAttLongShort() {
		if (la.kind == 3) {
			Get();
			OnParsed(ParserParts.SetRelArg, t.val); 
			if (la.kind == 9) {
				relAtt();
			}
		} else if (la.kind == 9) {
			relAtt();
		} else SynErr(30);
	}

	void exType() {
		Expect(25);
		if (la.kind == 3) {
			Get();
		} else if (la.kind == 4) {
			Get();
		} else SynErr(31);
		OnParsed(ParserParts.ExType, t.val); 
	}

	void exAtt() {
		Expect(24);
		if (la.kind == 3) {
			Get();
		} else if (la.kind == 4) {
			Get();
		} else SynErr(32);
		OnParsed(ParserParts.ExAtt, t.val); 
	}

	void attPredicate() {
		switch (la.kind) {
		case 12: {
			equalsPred();
			break;
		}
		case 19: {
			inPred();
			break;
		}
		case 20: {
			morePred();
			break;
		}
		case 21: {
			moreEqulPred();
			break;
		}
		case 22: {
			lessPred();
			break;
		}
		case 23: {
			lessEqulPred();
			break;
		}
		default: SynErr(33); break;
		}
	}

	void countPredicate() {
		if (la.kind == 20) {
			Get();
			OnParsed(ParserParts.MorePred); 
		} else if (la.kind == 21) {
			Get();
			OnParsed(ParserParts.MoreEqualPred); 
		} else if (la.kind == 22) {
			Get();
			OnParsed(ParserParts.LessPred); 
		} else if (la.kind == 23) {
			Get();
			OnParsed(ParserParts.LessEqualPred); 
		} else if (la.kind == 12) {
			Get();
			OnParsed(ParserParts.EqualsPred); 
		} else SynErr(34);
		Expect(2);
		OnParsed(ParserParts.Number, t.val); 
	}

	void constant() {
		if (la.kind == 5) {
			Get();
			OnParsed(ParserParts.String, t.val); 
		} else if (la.kind == 2) {
			Get();
			OnParsed(ParserParts.Number, t.val); 
		} else if (la.kind == 1) {
			Get();
			OnParsed(ParserParts.Float, t.val); 
		} else if (la.kind == 16 || la.kind == 17 || la.kind == 18) {
			bool_();
		} else SynErr(35);
	}

	void bool_() {
		if (la.kind == 16) {
			Get();
			OnParsed(ParserParts.Bool,t.val); 
		} else if (la.kind == 17) {
			Get();
			OnParsed(ParserParts.Bool,t.val); 
		} else if (la.kind == 18) {
			Get();
			OnParsed(ParserParts.Bool,t.val); 
		} else SynErr(36);
	}

	void equalsPred() {
		OnParsed(ParserParts.EqualsPred); 
		Expect(12);
		if (StartOf(3)) {
			constant();
		} else if (la.kind == 3 || la.kind == 9) {
			setRelAttPredEnd();
		} else SynErr(37);
	}

	void inPred() {
		OnParsed(ParserParts.InPred); 
		Expect(19);
		if (la.kind == 5) {
			Get();
			OnParsed(ParserParts.String, t.val); 
		} else if (la.kind == 3 || la.kind == 9) {
			setRelAttPredEnd();
		} else SynErr(38);
	}

	void morePred() {
		OnParsed(ParserParts.MorePred); 
		Expect(20);
		numericOrSetRelAtt();
	}

	void moreEqulPred() {
		OnParsed(ParserParts.MoreEqualPred); 
		Expect(21);
		numericOrSetRelAtt();
	}

	void lessPred() {
		OnParsed(ParserParts.LessPred); 
		Expect(22);
		numericOrSetRelAtt();
	}

	void lessEqulPred() {
		OnParsed(ParserParts.LessEqualPred); 
		Expect(23);
		numericOrSetRelAtt();
	}

	void setRelAttPredEnd() {
		OnParsed(ParserParts.SetRelAttPredEnd); 
		setRelAttLongShort();
		exAtt();
	}

	void numericOrSetRelAtt() {
		OnParsed(ParserParts.NumericOrSetRelAtt); 
		if (la.kind == 2) {
			Get();
			OnParsed(ParserParts.Number, t.val); 
		} else if (la.kind == 1) {
			Get();
			OnParsed(ParserParts.Float, t.val); 
		} else if (la.kind == 3 || la.kind == 9) {
			setRelAttPredEnd();
		} else SynErr(39);
	}

	void relAtt() {
		Expect(9);
		if (la.kind == 3) {
			Get();
			OnParsed(ParserParts.RelAttStr, t.val); 
		} else if (la.kind == 2) {
			Get();
			OnParsed(ParserParts.RelAttIndex, t.val); 
		} else SynErr(40);
		Expect(11);
	}

	void setRelFormalArg() {
		OnParsed(ParserParts.SetRelFormalArg); 
		Expect(3);
		OnParsed(ParserParts.SetRelArg, t.val); 
		if (la.kind == 8) {
			Get();
			OnParsed(ParserParts.EmptyRelAtt); 
			Expect(9);
			Expect(3);
			OnParsed(ParserParts.RelAttStr, t.val); 
			Expect(10);
			Expect(3);
			OnParsed(ParserParts.RelAttStr, t.val); 
			while (la.kind == 10) {
				Get();
				Expect(3);
				OnParsed(ParserParts.RelAttStr, t.val); 
			}
			Expect(11);
		}
		OnParsed(ParserParts.SetRelFormalArgEnd); 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		QL4BIM();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,x, T,T,T,T, T,T,x,x, x},
		{x,x,x,x, x,x,x,x, x,x,x,x, T,x,x,x, x,x,x,T, T,T,T,T, x,x,x,x, x},
		{x,T,T,x, x,T,x,x, x,x,x,x, x,x,x,x, T,T,T,x, x,x,x,x, x,x,x,x, x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "float expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "lowString expected"; break;
			case 4: s = "upString expected"; break;
			case 5: s = "string_ expected"; break;
			case 6: s = "minust expected"; break;
			case 7: s = "alias expected"; break;
			case 8: s = "\"[]\" expected"; break;
			case 9: s = "\"[\" expected"; break;
			case 10: s = "\"|\" expected"; break;
			case 11: s = "\"]\" expected"; break;
			case 12: s = "\"=\" expected"; break;
			case 13: s = "\"(\" expected"; break;
			case 14: s = "\",\" expected"; break;
			case 15: s = "\")\" expected"; break;
			case 16: s = "\"true\" expected"; break;
			case 17: s = "\"false\" expected"; break;
			case 18: s = "\"unknown\" expected"; break;
			case 19: s = "\"~\" expected"; break;
			case 20: s = "\">\" expected"; break;
			case 21: s = "\">=\" expected"; break;
			case 22: s = "\"<\" expected"; break;
			case 23: s = "\"<=\" expected"; break;
			case 24: s = "\".\" expected"; break;
			case 25: s = "\"is\" expected"; break;
			case 26: s = "\"func\" expected"; break;
			case 27: s = "??? expected"; break;
			case 28: s = "invalid QL4BIM"; break;
			case 29: s = "invalid argument"; break;
			case 30: s = "invalid setRelAttLongShort"; break;
			case 31: s = "invalid exType"; break;
			case 32: s = "invalid exAtt"; break;
			case 33: s = "invalid attPredicate"; break;
			case 34: s = "invalid countPredicate"; break;
			case 35: s = "invalid constant"; break;
			case 36: s = "invalid bool_"; break;
			case 37: s = "invalid equalsPred"; break;
			case 38: s = "invalid inPred"; break;
			case 39: s = "invalid numericOrSetRelAtt"; break;
			case 40: s = "invalid relAtt"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}
