
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
	public const int maxT = 25;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public FuncNode GlobalFunc { get; private set; }
 
 


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
		while (la.kind == 3) {
			statement_();
		}
	}

	void statement_() {
		Expect(3);
		while (la.kind == 7 || la.kind == 8) {
			if (la.kind == 7) {
				Get();
			} else {
				Get();
				Expect(3);
				Expect(9);
				Expect(3);
				while (la.kind == 9) {
					Get();
					Expect(3);
				}
				Expect(10);
			}
		}
		Expect(11);
		expression();
	}

	void expression() {
		Expect(4);
		Expect(12);
		argument();
		while (la.kind == 13) {
			Get();
			argument();
		}
		Expect(14);
	}

	void argument() {
		if (la.kind == 3 || la.kind == 8) {
			setRelAttLongShort();
			if (StartOf(1)) {
				if (la.kind == 24) {
					exType();
				} else if (la.kind == 23) {
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
		} else SynErr(26);
	}

	void setRelAttLongShort() {
		if (la.kind == 3) {
			Get();
			if (la.kind == 8) {
				relAtt();
			}
		} else if (la.kind == 8) {
			relAtt();
		} else SynErr(27);
	}

	void exType() {
		Expect(24);
		if (la.kind == 3) {
			Get();
		} else if (la.kind == 4) {
			Get();
		} else SynErr(28);
	}

	void exAtt() {
		Expect(23);
		if (la.kind == 3) {
			Get();
		} else if (la.kind == 4) {
			Get();
		} else SynErr(29);
	}

	void attPredicate() {
		switch (la.kind) {
		case 11: {
			equalsPred();
			break;
		}
		case 18: {
			inPred();
			break;
		}
		case 19: {
			morePred();
			break;
		}
		case 20: {
			moreEqulPred();
			break;
		}
		case 21: {
			lessPred();
			break;
		}
		case 22: {
			lessEqulPred();
			break;
		}
		default: SynErr(30); break;
		}
	}

	void countPredicate() {
		if (la.kind == 19) {
			Get();
		} else if (la.kind == 20) {
			Get();
		} else if (la.kind == 21) {
			Get();
		} else if (la.kind == 22) {
			Get();
		} else if (la.kind == 11) {
			Get();
		} else SynErr(31);
		Expect(2);
	}

	void constant() {
		if (la.kind == 5) {
			Get();
		} else if (la.kind == 2) {
			Get();
		} else if (la.kind == 1) {
			Get();
		} else if (la.kind == 15 || la.kind == 16 || la.kind == 17) {
			bool_();
		} else SynErr(32);
	}

	void bool_() {
		if (la.kind == 15) {
			Get();
		} else if (la.kind == 16) {
			Get();
		} else if (la.kind == 17) {
			Get();
		} else SynErr(33);
	}

	void equalsPred() {
		Expect(11);
		if (StartOf(3)) {
			constant();
		} else if (la.kind == 3 || la.kind == 8) {
			setRelAttPredEnd();
		} else SynErr(34);
	}

	void inPred() {
		Expect(18);
		if (la.kind == 5) {
			Get();
		} else if (la.kind == 3 || la.kind == 8) {
			setRelAttPredEnd();
		} else SynErr(35);
	}

	void morePred() {
		Expect(19);
		numericOrSetRelAtt();
	}

	void moreEqulPred() {
		Expect(20);
		numericOrSetRelAtt();
	}

	void lessPred() {
		Expect(21);
		numericOrSetRelAtt();
	}

	void lessEqulPred() {
		Expect(22);
		numericOrSetRelAtt();
	}

	void setRelAttPredEnd() {
		setRelAttLongShort();
		exAtt();
	}

	void numericOrSetRelAtt() {
		if (la.kind == 2) {
			Get();
		} else if (la.kind == 1) {
			Get();
		} else if (la.kind == 3 || la.kind == 8) {
			setRelAttPredEnd();
		} else SynErr(36);
	}

	void relAtt() {
		Expect(8);
		Expect(3);
		Expect(10);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		QL4BIM();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,x,T, T,T,T,T, T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,T, x,x,x,x, x,x,T,T, T,T,T,x, x,x,x},
		{x,T,T,x, x,T,x,x, x,x,x,x, x,x,x,T, T,T,x,x, x,x,x,x, x,x,x}

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
			case 7: s = "\"[]\" expected"; break;
			case 8: s = "\"[\" expected"; break;
			case 9: s = "\"|\" expected"; break;
			case 10: s = "\"]\" expected"; break;
			case 11: s = "\"=\" expected"; break;
			case 12: s = "\"(\" expected"; break;
			case 13: s = "\",\" expected"; break;
			case 14: s = "\")\" expected"; break;
			case 15: s = "\"true\" expected"; break;
			case 16: s = "\"false\" expected"; break;
			case 17: s = "\"unknown\" expected"; break;
			case 18: s = "\"~\" expected"; break;
			case 19: s = "\">\" expected"; break;
			case 20: s = "\">=\" expected"; break;
			case 21: s = "\"<\" expected"; break;
			case 22: s = "\"<=\" expected"; break;
			case 23: s = "\".\" expected"; break;
			case 24: s = "\"is\" expected"; break;
			case 25: s = "??? expected"; break;
			case 26: s = "invalid argument"; break;
			case 27: s = "invalid setRelAttLongShort"; break;
			case 28: s = "invalid exType"; break;
			case 29: s = "invalid exAtt"; break;
			case 30: s = "invalid attPredicate"; break;
			case 31: s = "invalid countPredicate"; break;
			case 32: s = "invalid constant"; break;
			case 33: s = "invalid bool_"; break;
			case 34: s = "invalid equalsPred"; break;
			case 35: s = "invalid inPred"; break;
			case 36: s = "invalid numericOrSetRelAtt"; break;

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