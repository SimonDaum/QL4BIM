
using System;

namespace QL4BIMinterpreter.QL4BIM {



public class Parser {
	public const int _EOF = 0;
	public const int _float = 1;
	public const int _exAtt = 2;
	public const int _exType = 3;
	public const int _number = 4;
	public const int _literal = 5;
	public const int _operator = 6;
	public const int _conststring = 7;
	public const int _compare = 8;
	public const int _minust = 9;
	public const int maxT = 21;

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
		while (la.kind == 5) {
			statement_();
		}
	}

	void statement_() {
		Expect(5);
		while (la.kind == 10 || la.kind == 11) {
			if (la.kind == 10) {
				Get();
			} else {
				Get();
				Expect(5);
				Expect(12);
				Expect(5);
				while (la.kind == 12) {
					Get();
					Expect(5);
				}
				Expect(13);
			}
		}
		Expect(14);
		expression();
	}

	void expression() {
		Expect(6);
		Expect(15);
		argument();
		while (la.kind == 16) {
			Get();
			argument();
		}
		Expect(17);
	}

	void argument() {
		if (la.kind == 5) {
			Get();
			if (la.kind == 11) {
				relAtt();
			}
			if (la.kind == 2) {
				Get();
				if (la.kind == 3 || la.kind == 16 || la.kind == 17) {
					if (la.kind == 3) {
						Get();
					}
				} else if (la.kind == 8) {
					predicate();
				} else SynErr(22);
			}
		} else if (la.kind == 11) {
			Get();
			Expect(5);
			Expect(13);
			if (la.kind == 2) {
				Get();
				if (la.kind == 3 || la.kind == 16 || la.kind == 17) {
					if (la.kind == 3) {
						Get();
					}
				} else if (la.kind == 8) {
					predicate();
				} else SynErr(23);
			}
		} else if (StartOf(1)) {
			constant();
		} else SynErr(24);
	}

	void relAtt() {
		Expect(11);
		Expect(5);
		Expect(13);
	}

	void predicate() {
		Expect(8);
		if (StartOf(1)) {
			constant();
		} else if (la.kind == 5 || la.kind == 11) {
			if (la.kind == 5) {
				Get();
				if (la.kind == 11) {
					relAtt();
				}
			} else {
				relAtt();
			}
			Expect(2);
		} else SynErr(25);
	}

	void constant() {
		if (la.kind == 7) {
			Get();
		} else if (la.kind == 4) {
			Get();
		} else if (la.kind == 1) {
			Get();
		} else if (la.kind == 18 || la.kind == 19 || la.kind == 20) {
			bool_();
		} else SynErr(26);
	}

	void bool_() {
		if (la.kind == 18) {
			Get();
		} else if (la.kind == 19) {
			Get();
		} else if (la.kind == 20) {
			Get();
		} else SynErr(27);
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		QL4BIM();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,x,x, T,x,x,T, x,x,x,x, x,x,x,x, x,x,T,T, T,x,x}

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
			case 2: s = "exAtt expected"; break;
			case 3: s = "exType expected"; break;
			case 4: s = "number expected"; break;
			case 5: s = "literal expected"; break;
			case 6: s = "operator expected"; break;
			case 7: s = "conststring expected"; break;
			case 8: s = "compare expected"; break;
			case 9: s = "minust expected"; break;
			case 10: s = "\"[]\" expected"; break;
			case 11: s = "\"[\" expected"; break;
			case 12: s = "\"|\" expected"; break;
			case 13: s = "\"]\" expected"; break;
			case 14: s = "\"=\" expected"; break;
			case 15: s = "\"(\" expected"; break;
			case 16: s = "\",\" expected"; break;
			case 17: s = "\")\" expected"; break;
			case 18: s = "\"true\" expected"; break;
			case 19: s = "\"false\" expected"; break;
			case 20: s = "\"unknown\" expected"; break;
			case 21: s = "??? expected"; break;
			case 22: s = "invalid argument"; break;
			case 23: s = "invalid argument"; break;
			case 24: s = "invalid argument"; break;
			case 25: s = "invalid predicate"; break;
			case 26: s = "invalid constant"; break;
			case 27: s = "invalid bool_"; break;

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