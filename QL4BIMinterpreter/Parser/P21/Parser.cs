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

using System.Text;
using QL4BIMinterpreter.QL4BIM;



using System;

namespace QL4BIMinterpreter.P21 {



public class Parser {
	public const int _EOF = 0;
	public const int _stringText = 1;
	public const int _entityId = 2;
	public const int _equal = 3;
	public const int _obracket = 4;
	public const int _cbracket = 5;
	public const int _float = 6;
	public const int _number = 7;
	public const int _indent = 8;
	public const int _star = 9;
	public const int _semicolonT = 10;
	public const int maxT = 15;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public QLExchangeFile QLExchangeFile { get; } = new QLExchangeFile();



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

	
	void classname(out string name ) {
		Expect(8);
		name = t.val; 
	}

	void list(out QLList QLList ) {
		QLList = new QLList(); 
		Expect(4);
		QLPart QLPart; 
		part(out QLPart);
		QLList.Add(QLPart); 
		while (la.kind == 11) {
			Get();
			part(out QLPart);
			QLList.Add(QLPart); 
		}
		Expect(5);
	}

	void part(out QLPart QLPart) {
		QLPart  = new QLPart();  
		QLClass QLClass; 
		QLList QLList; 
		string QLstring; 
		string enumstring; 
		switch (la.kind) {
		case 12: {
			myenum(out enumstring);
			QLPart.QLEnum = new QLEnum(enumstring); 
			break;
		}
		case 1: {
			mystring(out QLstring);
			QLPart.QLString = new QLString(QLstring); 
			break;
		}
		case 13: {
			Get();
			QLPart.IsNull = true; 
			break;
		}
		case 2: {
			Get();
			QLPart.QLEntityId = new QLEntityId(t.val); 
			break;
		}
		case 4: {
			list(out QLList);
			QLPart.QLList = QLList; 
			break;
		}
		case 6: {
			Get();
			QLPart.SetFloat(t.val); 
			break;
		}
		case 7: {
			Get();
			QLPart.SetNumber(t.val); 
			break;
		}
		case 9: {
			Get();
			QLPart.IsNull = true; 
			break;
		}
		case 8: {
			myclass(out QLClass);
			QLPart.QLClass = QLClass; 
			break;
		}
		case 14: {
			Get();
			QLPart.IsEmptyList = true; 
			break;
		}
		default: SynErr(16); break;
		}
	}

	void myenum(out string enumstring) {
		var sb = new StringBuilder(); 
		Expect(12);
		sb.Append('.'); 
		while (StartOf(1)) {
			Get();
			sb.Append(t.val); 
		}
		Expect(12);
		sb.Append('.'); 
		enumstring = sb.ToString(); 
	}

	void mystring(out string QLstring) {
		var sb = new StringBuilder(); 
		Expect(1);
		sb.Append(t.val); 
		QLstring = sb.ToString();	
	}

	void myclass(out QLClass QLClass) {
		QLClass  = new QLClass();  
		string name; 
		classname(out name);
		QLList QLList; 
		QLClass.ClassName = name; 
		list(out QLList);
		QLClass.QLDirectList = QLList;   
	}

	void entity(out QLEntity QLEntity) {
		QLEntity  = new QLEntity();   
		Expect(2);
		var value = t.val; 
		Expect(3);
		QLClass QLClass;   
		myclass(out QLClass);
		Expect(10);
		QLEntity.SetEntityAndClass(value, QLClass);  
	}

	void EXCHANGEFILE() {
		QLEntity QLEntity;   
		entity(out QLEntity);
		QLExchangeFile.Add(QLEntity); 
		while (la.kind == 2) {
			entity(out QLEntity);
			QLExchangeFile.Add(QLEntity); 
		}
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		EXCHANGEFILE();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x},
		{x,T,T,T, T,T,T,T, T,T,T,T, x,T,T,T, x}

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
			case 1: s = "stringText expected"; break;
			case 2: s = "entityId expected"; break;
			case 3: s = "equal expected"; break;
			case 4: s = "obracket expected"; break;
			case 5: s = "cbracket expected"; break;
			case 6: s = "float expected"; break;
			case 7: s = "number expected"; break;
			case 8: s = "indent expected"; break;
			case 9: s = "star expected"; break;
			case 10: s = "semicolonT expected"; break;
			case 11: s = "\",\" expected"; break;
			case 12: s = "\".\" expected"; break;
			case 13: s = "\"$\" expected"; break;
			case 14: s = "\"()\" expected"; break;
			case 15: s = "??? expected"; break;
			case 16: s = "invalid part"; break;

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
