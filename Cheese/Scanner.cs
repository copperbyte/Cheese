using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Cheese
{
	class Token 
	{
		internal enum EType {
			ERROR,
			NUMBER,
			STRING,
			NAME,
			OPERATOR,
			BRACKET,
			KEYWORD,
			COMMENT,
			EOF
		};

		internal EType Type;
		internal string Value;
		internal int Position;
		internal int Counter;
		internal int Line;
		internal int Column;

		public override string ToString()
		{
			return Type.ToString() + ":" + Value;
		}

		bool Is(EType ExpType, string ExpValue) {
			if(Type != ExpType)
				return false;
			return (Value == ExpValue);
		}

		public bool IsKeyword(string Expected) {
			return Is(EType.KEYWORD, Expected);
		}

		public bool IsOperator(string Expected) {
			return Is(EType.OPERATOR, Expected);
		}

		public bool IsBracket(string Expected) {
			return Is(EType.BRACKET, Expected);
		}
	}

	class Scanner
	{
		TextReader Reader;
		private int Position;
		private int LineNumber;
		private int ColumnNumber;
		private int TokenCounter;

		private char CurrChar;
		private Token.EType TokType;
		private StringBuilder ValueAccum;
		private int StartPosition, StartLine, StartColumn;

		private SortedSet<string> Operators, Keywords;
		private SortedSet<char> Brackets, Symbols, HexDigits;

		public Scanner(TextReader Reader) {
			this.Reader = Reader;
			Position = 0;
			LineNumber = 1;
			ColumnNumber = 1;
			TokenCounter = 0;

			CurrChar = (char)Reader.Peek();
			TokType = Token.EType.ERROR;
			ValueAccum = new StringBuilder(16);
			StartPosition = Position;

			Operators = new SortedSet<string>() { "+", "-", "*", "/",
				"^", "%", "<", ">",
				"=", "#", ":", ",", ";", ".",
				"..", "<=", ">=", "==", "~=", 
				"..."};

			Keywords = new SortedSet<string>() { 
				"and", "break", "do", "else", "elseif", "end", 
				"false", "for", "function", "if", "in", "local", "nil", "not", "or",
				"repeat", "return", "then", "true", "until", "while"
			};

			Brackets = new SortedSet<char>() { '(', '{', '[', ']', '}', ')' };
			Symbols = new SortedSet<char>() { '+', '-', '*', '/', '^', '%',
				'<', '>', '=', '#', ':', '.',
				',', ';', '~' };
			HexDigits = new SortedSet<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
				'a', 'b', 'c', 'd', 'e', 'f',
				'A', 'B', 'C', 'D', 'E', 'F' };

		}

		public bool IsEOF { 
			get {
				return Reader.Peek() == -1;
			}
			//set;
		}

		internal void Advance() {
			Reader.Read();
			CurrChar = (char)Reader.Peek();
			Position++;
			if(CurrChar == '\n') {
				LineNumber++;
				ColumnNumber = 1;
			} else {
				ColumnNumber++;
			}
		}

		internal Token MakeToken() {
			Token Result = new Token();
			Result.Type = TokType;
			Result.Value = ValueAccum.ToString();
			Result.Position = StartPosition;
			Result.Counter = TokenCounter;
			Result.Line = StartLine;
			Result.Column = StartColumn;
			TokenCounter++;
			return Result;
		}


		public Token ReadToken() {
			TokType = Token.EType.ERROR;
			ValueAccum.Clear();

			while(Char.IsWhiteSpace(CurrChar))
				Advance();

			StartPosition = Position;
			StartLine = LineNumber;
			StartColumn = ColumnNumber;

			if(IsEOF) {
				TokType = Token.EType.EOF;
				return MakeToken();
			}

			return ReadAnyToken();		
		}


		internal Token ReadAnyToken()  {
			// Only ever called on the first character
			if(CurrChar == '\"' || CurrChar == '\'')
				return ReadQuotedString();
			else if(Char.IsDigit(CurrChar))
				return ReadNumber();
			else if(Char.IsLetter(CurrChar) || CurrChar == '_')
				return ReadName();
			else if(CurrChar == '[')
				return ReadBracketOrLongString();
			else if(Brackets.Contains(CurrChar))
				return ReadBracket();
			else if(CurrChar == '-')
				return ReadCommentOrOperator();
			else if(Symbols.Contains(CurrChar))
				return ReadOperator();
			else {
				TokType = Token.EType.ERROR;
				return MakeToken();
			}
		}

		internal Token ReadName() {
			// Assumes we are on the first character of the name

			while(true) {
				if(Char.IsLetterOrDigit(CurrChar) || CurrChar == '_') {
					ValueAccum.Append(CurrChar);
				} else {
					break;
				}
				Advance();
			}

			TokType = Token.EType.NAME;
			Token Result = MakeToken();
			if(Keywords.Contains(Result.Value))
				Result.Type = Token.EType.KEYWORD;
			return Result;
		}

		internal Token ReadQuotedString() {
			// Assumes Curr is on the starting quote
			char QuoteMarker = CurrChar;

			Advance();

			// eof error?
			while(CurrChar != QuoteMarker) {
				if(CurrChar == '\\') {
					Advance(); 
					// MOre stypes of escapes
					if(CurrChar == 'n')
						CurrChar = '\n';
					else if(CurrChar == 't')
						CurrChar = '\t';
				}
				ValueAccum.Append(CurrChar);
				Advance();
			}
			Advance(); // eat the closing quote

			TokType = Token.EType.STRING;
			return MakeToken();
		}

		internal Token ReadBracketOrLongString() {
			char StoredChar = CurrChar;

			Advance();

			if( !(CurrChar == '[' || CurrChar == '=') ) {
				ValueAccum.Append(StoredChar);
				TokType = Token.EType.BRACKET;
				return MakeToken();
			}

			// Dont store the inital '['
			return ReadLongString();
		}

		internal Token ReadLongString() {
			// Assumes the initial '[' has been burned, and we 
			// are currently on either the second '[' or the first '=' 
			int EqualCount = 0;

			while(CurrChar == '=') {
				EqualCount++;
				Advance();
			}

			//int MaxEqual = EqualCount;

			if(CurrChar != '[') {
				TokType = Token.EType.ERROR;
				return MakeToken();
			}
			Advance(); // burn the second '['

			// eof error?
			while(CurrChar != ']') {
				if(CurrChar == '\\') {
					Advance(); 
					// MOre stypes of escapes
					if(CurrChar == 'n')
						CurrChar = '\n';
					else if(CurrChar == 't')
						CurrChar = '\t';
				}
				ValueAccum.Append(CurrChar);
				Advance();
			}
			Advance(); // eat first ']' 
			while(CurrChar == '=') {
				EqualCount--;
				Advance();
			}
			if(CurrChar != ']') {
				TokType = Token.EType.ERROR;
				return MakeToken();
			}
			if(EqualCount != 0) {
				TokType = Token.EType.ERROR;
				return MakeToken();
			}
			Advance(); // eat the closing ']'

			TokType = Token.EType.STRING;
			return MakeToken();
		}

		internal Token ReadNumber() {
			// Assumes we are on or before any possible decimal point

			bool DotSeen = false;
			while(true) {
				if(CurrChar == '.') {
					if(!DotSeen) {
						ValueAccum.Append(CurrChar);
						DotSeen = true;
					} else {
						; // error
					}
				} else if(CurrChar == 'x' && ValueAccum.Length == 1 && 
				          ValueAccum[0] == '0' && DotSeen == false) {
					return ReadHexNumber();
				} else if(CurrChar == 'e' || CurrChar == 'E') {
					return ReadExpNumber();
				}else if(Char.IsDigit(CurrChar)) {
					ValueAccum.Append(CurrChar);
				} else {
					break;
				}
				Advance();
			}

			TokType = Token.EType.NUMBER;
			return MakeToken();
		}

		internal Token ReadHexNumber() {
			// Assumes we are on the 'x' 
			ValueAccum.Append(CurrChar);
			Advance(); 

			while(true) {
				if(HexDigits.Contains(CurrChar)) {
					ValueAccum.Append(CurrChar);
					Advance();
				} else {
					break;
				}
			}

			if(ValueAccum.Length < 3) {// too short to be a valid 0xHEX
				TokType = Token.EType.ERROR;
				return MakeToken();
			}

			TokType = Token.EType.NUMBER;
			return MakeToken();
		}

		internal Token ReadExpNumber() {
			// Assems we are on the 'e'
			ValueAccum.Append(CurrChar);
			Advance();

			if(CurrChar == '-') {
				ValueAccum.Append(CurrChar);
				Advance();
			}

			bool DigitFound = false;
			do {
				if(Char.IsDigit(CurrChar)) {
					DigitFound = true;
					ValueAccum.Append(CurrChar);
					Advance();
				} else {
					break;
				}
			} while(true);

			if(!DigitFound) {// too short to be a valid NUMeINT
				TokType = Token.EType.ERROR;
				return MakeToken();
			}

			TokType = Token.EType.NUMBER;
			return MakeToken();
		}

		internal Token ReadBracket() {
			ValueAccum.Append(CurrChar);
			Advance();
			TokType = Token.EType.BRACKET;
			return MakeToken();
		}

		internal Token ReadOperator() {
			// Assumes we are on the first character of the symbol

			// Special Operator
			if(CurrChar == '~') {
				ValueAccum.Append(CurrChar);
				Advance();
				if(CurrChar == '=') {
					ValueAccum.Append(CurrChar);
					Advance();
					TokType = Token.EType.OPERATOR;
					return MakeToken();
				} else {
					TokType = Token.EType.ERROR;
					return MakeToken();
				}
			}

			while(true) {
				ValueAccum.Append(CurrChar);
				if(Operators.Contains(ValueAccum.ToString())) {
					Advance();
					continue;
				} else {
					ValueAccum.Remove(ValueAccum.Length-1, 1);
					break;
				}
			}

			TokType = Token.EType.OPERATOR;
			return MakeToken();
		}

		internal Token ReadCommentOrOperator() {
			// On Inital '-'
			//ValueAccum.Append(CurrChar);
			Advance();
			if(CurrChar != '-') {
				ValueAccum.Append('-');
				TokType = Token.EType.OPERATOR;
				return MakeToken();
			}
			else
				return ReadComment();
		}

		internal Token ReadComment() {
			// is one of the two types
			ValueAccum.Append("--");
			Advance();

			if(CurrChar == '[') {
				ValueAccum.Append(CurrChar);
				Advance();
				if(CurrChar == '[') {
					ValueAccum.Append(CurrChar);
					Advance();
					// loop until "]]"
					int CloserCount = 0;
					while(CloserCount < 2) {
						if(CurrChar == ']')
							CloserCount++;
						else 
							CloserCount = 0;
						ValueAccum.Append(CurrChar);
						Advance();
					}
					TokType = Token.EType.COMMENT;
					//return MakeToken();
					return ReadToken();
				}
			}  

			// loop until '\n'
			while(CurrChar != '\n') {
				ValueAccum.Append(CurrChar);
				Advance();
			}

			TokType = Token.EType.COMMENT;
			//return MakeToken();
			return ReadToken();
		}

	} // end Scanner

}
