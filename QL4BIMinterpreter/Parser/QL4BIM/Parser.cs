
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

public delegate void PartParsedEventHandler(object sender, PartParsedEventArgs e);
public event PartParsedEventHandler PartParsed;
 
protected virtual void OnParsed(ParserParts context, string currentToken = "", string lookAhead = "") 
{	
	var e = new PartParsedEventArgs(context, currentToken, lookAhead);
    Console.WriteLine(e.ToString());
    PartParsed?.Invoke(this, e);
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
			OnParsed(ParserParts.GlobalBlock); 
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
		OnParsed(ParserParts.Statement); 
		Expect(3);
		OnParsed(ParserParts.Variable, t.val); 
		while (la.kind == 8 || la.kind == 9) {
			if (la.kind == 8) {
				Get();
				OnParsed(ParserParts.EmptyRelAtt); 
			} else {
				Get();
				Expect(3);
				OnParsed(ParserParts.RelAtt); 
				Expect(10);
				Expect(3);
				OnParsed(ParserParts.RelAtt); 
				while (la.kind == 10) {
					Get();
					Expect(3);
				}
				OnParsed(ParserParts.RelAtt); 
				Expect(11);
			}
		}
		Expect(12);
		expression();
	}

	void func_() {
		OnParsed(ParserParts.FuncDefBlock); 
		Expect(26);
		Expect(4);
		if (la.kind == 7) {
			Get();
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
		if (la.kind == 3 || la.kind == 9) {
			setRelAttLongShort();
			if (StartOf(1)) {
				if (la.kind == 25) {
					exType();
				} else if (la.kind == 24) {
					exAtt();
					if (StartOf(2)) {
						attPredicate();
					}
				} else {
					countPredicate();
				}
			}
		} else if (StartOf(3)) {
			constant();
		} else SynErr(29);
	}

	void setRelAttLongShort() {
		if (la.kind == 3) {
			Get();
			OnParsed(ParserParts.SetRelArg, t.val); 
			if (la.kind == 9) {
				relAtt();
				OnParsed(ParserParts.RelAtt, t.val); 
			}
		} else if (la.kind == 9) {
			relAtt();
			OnParsed(ParserParts.RelAtt, t.val); 
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
		OnParsed(ParserParts.AttPredicate); 
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
		} else if (la.kind == 21) {
			Get();
		} else if (la.kind == 22) {
			Get();
		} else if (la.kind == 23) {
			Get();
		} else if (la.kind == 12) {
			Get();
		} else SynErr(34);
		Expect(2);
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
		} else if (la.kind == 1) {
			Get();
		} else if (la.kind == 3 || la.kind == 9) {
			setRelAttPredEnd();
		} else SynErr(39);
	}

	void relAtt() {
		Expect(9);
		Expect(3);
		OnParsed(ParserParts.RelAtt, t.val); 
		Expect(11);
	}

	void setRelFormalArg() {
		Expect(3);
		if (la.kind == 9) {
			Get();
			Expect(3);
			Expect(10);
			Expect(3);
			while (la.kind == 10) {
				Get();
				Expect(3);
			}
			Expect(11);
		}
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