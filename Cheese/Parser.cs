
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Cheese
{

	class ParseNode {
		public enum EType {
			ERROR,
			TERMINAL,
			UNOP,
			BINOP,
			UN_OP_WRAP,
			BIN_OP_WRAP,
			FIELD_SEP,
			FIELD,
			FIELD_LIST,
			TABLE_CONS,
			PAR_LIST,
			FUNC_BODY,
			FUNCTION,
			ARGS,
			VAR_SUFFIX,
			NAME_AND_ARGS,
			VAR_OR_EXP,
			FUNC_CALL,
			PREFIX_EXP,
			VAR, 
			EXP,
			EXP_LIST,
			NAME_LIST,
			VAR_LIST,
			FUNC_NAME,
			ASSIGN_STAT,
			LAST_STAT,
			BREAK_STAT, 
			RETURN_STAT,
			DO_STAT,
			WHILE_STAT,
			REPEAT_STAT,
			IF_STAT,
			FOR_NUM_STAT,
			FOR_ITER_STAT,
			FUNC_STAT,
			LOCAL_FUNC_STAT,
			LOCAL_ASSIGN_STAT,
			STATEMENT,
			BLOCK,
			CHUNK
		};

		internal EType Type;
		internal Token Token;  // if leaf
		internal List<ParseNode> Children; // or not leaf

		public ParseNode() {
			Children = null;
			;//Children = new List<ParseNode>();
		}
		public ParseNode(EType Type) {
			this.Type = Type;
			Children = null;
			;//Children = new List<ParseNode>();
		}

		public ParseNode(Token Tok) {
			Assign(Tok);
		}

		public void Add(ParseNode Node) {
			if(Children == null)
				Children = new List<ParseNode>();
			Children.Add(Node);
		}

		public void Add(Token Tok) {
			ParseNode Child = new ParseNode(Tok);
			Add(Child);
		}

		public void Assign(Token Tok) {
			Token = Tok;
			Type = EType.TERMINAL;
		}

		public override string ToString()
		{
			StringBuilder Builder = new StringBuilder();
			Builder.Append(Type.ToString()).Append(":");
			if(Token != null)
				Builder.Append(Token.ToString());
			else if(Children != null)
				Builder.Append("C").Append(Children.Count);
			return Builder.ToString();
		}
	
		// True if this node is a terminal, or chains singularly down to a terminal
		public bool IsTerminal() {
			if(Type == EType.TERMINAL)
				return true;
			else if(Children != null) {
				if(Children.Count == 1) {
					return Children[0].IsTerminal();
				}
			}
			return false;
		}
		public Token GetTerminal() {
			if(Type == EType.TERMINAL)
				return Token;
			else if(Children != null) {
				if(Children.Count == 1) {
					return Children[0].GetTerminal();
				}
			}
			return null;
		}
		public ParseNode Dig() {
			if(Children == null)
				return this;
			else {
				if(Children.Count == 1)
					return Children[0].Dig();
				else
					return this;
			}
		}
	}

	class Parser {
		//TextReader Reader;
		Scanner Scanner;
		Token Curr, Look, LookFar;

		SortedSet<string> UnOps = new SortedSet<string>(){ "-", "not", "#" }, 
		BinOps = new SortedSet<string>(){ "+", "-", "*", "/", "^", "%", "..", 
			"<", "<=", ">", ">=", "==", "~=", "and", "or" },
		FieldSeps = new SortedSet<string>(){ ",", ";" };

		public Parser(TextReader Reader) {
			//this.Reader = Reader;
			Scanner = new Scanner(Reader);

		}

		public void Advance() {
			Curr = Look;
			Look = LookFar;
			LookFar = Scanner.ReadToken();

			//if(Curr != null)
			//	Console.WriteLine(" {0} : {1} : {2} : {3} : {4} : {5} ", Curr.Counter, Curr.Position, Curr.Line, Curr.Column, Curr.Type, Curr.Value);

			//if(Curr != null && Curr.Counter == 3677) {
			//	Console.WriteLine("BREAKPOINT");
			//}
		}

		public ParseNode Parse() {
			Advance(); // prime both lookaheads
			Advance(); 

			ParseNode RootChunk = ParseChunk();
			//Console.WriteLine("Parse Complete");
			return RootChunk;
			//Console.WriteLine("Parse Complete");
			//PrintTree(RootChunk, 0);
			/*
			while(true) {
				Token Tok = Scanner.ReadToken();
				if(Tok == null)
					break;
				Console.WriteLine(" {0} : {1} : {2} : {3} : {4} : {5} ", Tok.Counter, Tok.Position, Tok.Line, Tok.Column, Tok.Type, Tok.Value);
				if(Tok.Type == Token.ERROR)
					break;
				if(Tok.Type == Token.EOF) 
					break;
			}
			*/
		}

		public void PrintTree(ParseNode Node, int Level=0) {

			for(int i = 0; i < Level; i++) {
				if(Level % 2 == 0)
					Console.Write("  ");
				else 
					Console.Write("..");
			}

			Console.Write("{0} : ", Node.Type);
			if(Node.Token != null)
				Console.Write("{0} : ", Node.Token.ToString());
			if(Node.Children != null)
				Console.Write("{0}", Node.Children.Count);
			//else
			//	Console.Write("0");
			Console.WriteLine();

			if(Node.Children != null) {
				foreach(ParseNode Child in Node.Children) {
					PrintTree(Child, Level + 1);
				}
			}
		}



		public class ParseException : Exception {
			string Expectation;
			Token Found;

			public ParseException(string Expectation, Token Found) {
				this.Expectation = Expectation;
				this.Found = Found;
			}

			override public string ToString() {
				StringBuilder Message = new StringBuilder();
				Message.Append("Parsing Error: ");
				Message.AppendFormat(" At location {0}, {1}, ", Found.Line, Found.Column);
				Message.AppendFormat(" Expected \"{0}\" , Got \"{1}\" .", Expectation, Found.Value);
				return Message.ToString();
			}
		}

		internal void Match(string Expectation, ParseNode Parent) {
			if(Curr.Value == Expectation) {
				Parent.Add(Curr);
				Advance();
				return;
			} 
			throw new ParseException(Expectation, Curr);
		}

		internal void Match(Token.EType Expectation, ParseNode Parent) {
			if(Curr.Type == Expectation) {
				Parent.Add(Curr);
				Advance();
				return;
			} 
			throw new ParseException(Expectation.ToString(), Curr);
		}

		internal void Match(SortedSet<string> Expectation, ParseNode Parent) {
			if(Expectation.Contains(Curr.Value)) {
				Parent.Add(Curr);
				Advance();
				return;
			} 
			throw new ParseException(Expectation.ToString() /* FIXME */, Curr);
		}

		internal ParseNode ParseChunk() {
			Advance();

			ParseNode Block = ParseBlock();
			// if not EOF, error

			ParseNode Chunk = new ParseNode(ParseNode.EType.CHUNK);
			Chunk.Add(Block);
			return Chunk;
		}

		internal ParseNode ParseBlock() {
			ParseNode Block = new ParseNode(ParseNode.EType.BLOCK);

			while(Curr.Type != Token.EType.EOF && 
			      !(Curr.IsKeyword("else")  || Curr.IsKeyword("elseif") ||
			  Curr.IsKeyword("end") || Curr.IsKeyword("until") )) {
				if(Curr.IsKeyword("return")) {
					Block.Add(ParseReturnStatement());
					// prepend return, set to LAST_STAT?
					break;
				} else if(Curr.IsKeyword("break")) {
					Block.Add(ParseBreakStatement());
					break;
				}

				ParseNode Statement = ParseStatement();
				if(Statement != null) 
					Block.Add(Statement);
			}

			if(Curr.IsOperator(";")) {
				Advance(); 
			}

			return Block;
		}

		internal ParseNode ParseStatement() {
			if(Curr.IsKeyword("break")) 
				return ParseBreakStatement(); 
			if(Curr.IsKeyword("do"))
				return ParseDoStatement();
			else if(Curr.IsKeyword("for"))
				return ParseForStatement();
			else if(Curr.IsKeyword("function")) 
				return ParseFunctionStatement();
			else if(Curr.IsKeyword("if"))
				return ParseIfStatement();
			else if(Curr.IsKeyword("local"))
				return ParseLocalStatement();
			else if(Curr.IsKeyword("repeat"))
				return ParseRepeatStatement();
			else if(Curr.IsKeyword("return"))
				return ParseReturnStatement();
			else if(Curr.IsKeyword("while"))
				return ParseWhileStatement();

			if(Curr.IsOperator(";")) {
				Advance();
				return null;
			}

			return ParseAssignOrCallStatement();
		}

		internal ParseNode ParseAssignOrCallStatement() {
			// varlist1 '=' explist1 |  var (, var) => NAME | ( 
			// functioncall |     (varOrExp => var | ( => NAME | ( 

			Token ErrorStore = Curr;
			ParseNode First = ParsePrefixExp();

			// if Look is ',' or '=', first type?
			if(First != null && (Curr.IsOperator(",") || Curr.IsOperator("="))) {
				ParseNode Assign = new ParseNode(ParseNode.EType.ASSIGN_STAT);
				if(Curr.IsOperator(",")) {
					Token Comma = Curr;
					ParseNode VarList = ParseVarList();
					VarList.Children.Insert(0, new ParseNode(Comma));
					VarList.Children.Insert(0, First.Children[0].Children[0]);
					Assign.Add(VarList);
				} else {
					ParseNode VarList = new ParseNode(ParseNode.EType.VAR_LIST);
					VarList.Add(First.Children[0].Children[0]);
					Assign.Add(VarList);
				}

				Match("=", Assign);

				Assign.Add(ParseExpList());
				return Assign;
			} // assignment  
			else if(First != null && First.Children != null && 
			        First.Children[0].Type == ParseNode.EType.VAR_OR_EXP) {
				// function call
				//return ParseFuncCall();
				ParseNode FuncCall = new ParseNode(ParseNode.EType.FUNC_CALL);
				foreach(ParseNode part in First.Children) {
					FuncCall.Add(part);
				}
				return FuncCall;
			} else {
				throw new ParseException("Assignment or Function Call", ErrorStore);
			}
		}

		internal ParseNode ParseBreakStatement() {
			ParseNode Node = new ParseNode(ParseNode.EType.LAST_STAT);
			Match("break", Node);
			return Node;
		}

		internal ParseNode ParseDoStatement() {
			// 'do' block 'end' 
			ParseNode Do = new ParseNode(ParseNode.EType.DO_STAT);
			Match("do", Do);

			ParseNode Body = ParseBlock();
			Do.Add(Body);

			Match("end", Do);

			return Do;
		}

		internal ParseNode ParseForStatement() {
			// 'for' NAME '=' exp ',' exp (',' exp)? 'do' block 'end' | 
			// 'for' namelist 'in' explist1 'do' block 'end' | 

			if(Look.IsOperator("=")) {
				ParseNode For = new ParseNode(ParseNode.EType.FOR_NUM_STAT);
				Match("for", For);			

				Match(Token.EType.NAME, For);
				Match("=", For);

				For.Add(ParseExp());
				Match(",", For);

				For.Add(ParseExp());

				if(Curr.IsOperator(",")) {
					Match(",", For);
					For.Add(ParseExp());
				} 

				Match("do", For);

				For.Add(ParseBlock());

				Match("end", For);

				return For;
			}  // numerical 
			else if(Look.IsOperator(",") || Look.IsKeyword("in")) {
				ParseNode For = new ParseNode(ParseNode.EType.FOR_ITER_STAT);
				Match("for", For);			

				For.Add(ParseNameList());

				Match("in", For);

				For.Add(ParseExpList());

				Match("do", For);

				For.Add(ParseBlock());

				Match("end", For);

				return For;
			} // for .. in 
			else {
				throw new ParseException("'=', ',', or 'in'", Look);
			}

			//return null;
		}

		internal ParseNode ParseFunctionStatement() {
			// 'function' funcname funcbody | 
			ParseNode Func = new ParseNode(ParseNode.EType.FUNC_STAT);
			Match("function", Func);

			ParseNode Name = ParseFunctionName();
			Func.Add(Name);

			ParseNode Body = ParseFuncBody();
			Func.Add(Body);

			return Func;
		}

		internal ParseNode ParseIfStatement() {
			// 'if' exp 'then' block ('elseif' exp 'then' block)* ('else' block)? 'end' |
			ParseNode If = new ParseNode(ParseNode.EType.IF_STAT);
			Match("if", If);

			If.Add(ParseExp());

			Match("then", If);

			If.Add(ParseBlock());

			while(Curr.IsKeyword("elseif")) {
				Match("elseif", If);

				If.Add(ParseExp());

				Match("then", If);

				If.Add(ParseBlock());
			} // end elseif*


			if(Curr.IsKeyword("else")) {
				Match("else", If);
				If.Add(ParseBlock());
			} // end else?

			Match("end", If);

			return If;
		}

		internal ParseNode ParseLocalStatement() {
			// 'local' 'function' NAME funcbody | 
			// 'local' namelist ('=' explist1)? ;

			if(Curr.IsKeyword("local") && Look.IsKeyword("function")) {
				ParseNode Local = new ParseNode(ParseNode.EType.LOCAL_FUNC_STAT);
				Match("local", Local);

				Match("function", Local);
				Match(Token.EType.NAME, Local);
				Local.Add(ParseFuncBody());

				return Local;
			} // end local func
			else if(Curr.IsKeyword("local") && Look.Type == Token.EType.NAME) {
				ParseNode Local = new ParseNode(ParseNode.EType.LOCAL_ASSIGN_STAT);
				Match("local", Local);

				Local.Add(ParseNameList());

				if(Curr.IsOperator("=")) {
					Match("=", Local);
					Local.Add(ParseExpList());
				}

				return Local;
			} // end local var
			else {
				throw new ParseException("'function' or NAME", Curr);
			}

			//return null;
		}

		internal ParseNode ParseRepeatStatement() {
			// 'repeat' block 'until' exp | 
			ParseNode Repeat = new ParseNode(ParseNode.EType.REPEAT_STAT);
			Match("repeat", Repeat);

			ParseNode Body = ParseBlock();
			Repeat.Add(Body);

			Match("until", Repeat);

			ParseNode Exp = ParseExp();
			Repeat.Add(Exp);

			return Repeat;
		}

		internal ParseNode ParseReturnStatement() {
			ParseNode Return = new ParseNode(ParseNode.EType.LAST_STAT);
			Match("return", Return);

			ParseNode Body = ParseExpList();
			Return.Add(Body);

			return Return;
		}

		internal ParseNode ParseWhileStatement() {
			// 'while' exp 'do' block 'end' 
			ParseNode While = new ParseNode(ParseNode.EType.WHILE_STAT);

			Match("while", While);

			ParseNode Exp = ParseExp();
			While.Add(Exp);

			Match("do", While);

			ParseNode Body = ParseBlock();
			While.Add(Body);

			Match("end", While);

			return While;
		}

		internal bool LeadFunctionName() {
			return (Curr.Type == Token.EType.NAME);
		}
		internal ParseNode ParseFunctionName() {
			// funcname : NAME ('.' NAME)* (':' NAME)? ;
			ParseNode FuncName = new ParseNode(ParseNode.EType.FUNC_NAME);

			Match(Token.EType.NAME, FuncName);

			while(Curr.IsOperator(".")) {
				Match(".", FuncName);
				Match(Token.EType.NAME, FuncName);
			}

			if(Curr.IsOperator(":")) {
				Match(":", FuncName);
				Match(Token.EType.NAME, FuncName);
			}

			return FuncName;
		}

		internal bool LeadVarList() {
			return LeadVar();
		}
		internal ParseNode ParseVarList() {
			// varlist1 : var (',' var)*;
			ParseNode VarList = new ParseNode(ParseNode.EType.VAR_LIST);

			ParseNode First = ParseVar();
			VarList.Add(First);

			while(Curr.IsOperator(",")) {
				Match(",", VarList);
				ParseNode SubVar =  ParseVar();
				VarList.Add(SubVar);
			}

			return VarList;
		}

		internal bool LeadNameList() {
			return (Curr.Type == Token.EType.NAME);
		}
		internal ParseNode ParseNameList() {
			// namelist : NAME (',' NAME)*;
			ParseNode NameList = new ParseNode(ParseNode.EType.NAME_LIST);

			Match(Token.EType.NAME, NameList);

			while(Curr.IsOperator(",")) {
				Match(",", NameList);
				Match(Token.EType.NAME, NameList);
			}

			return NameList;
		}

		internal bool LeadExpList() {
			return LeadExp();
		}
		internal ParseNode ParseExpList() {
			// explist1 : (exp ',')* exp;
			ParseNode ExpList = new ParseNode(ParseNode.EType.EXP_LIST);

			ParseNode Head = ParseExp();
			ExpList.Add(Head);

			while(Curr.IsOperator(",")) {
				Match(",", ExpList);
				ExpList.Add(ParseExp());
			}

			return ExpList;
		}

		internal int OperatorPrecedence(Token Tok) {
			// http://www.lua.org/pil/3.5.html
			// 8    ^
			// 7    not  - (unary)
			// 6	*   /
			// 5	+   -
			// 4	..
			// 3	<   >   <=  >=  ~=  ==
			// 2	and
			// 1	or
			char First = Tok.Value[0];
			int Length = Tok.Value.Length;

			if(Length == 1) {
				if(First == '^') 
					return 8;
				else if(First == '*' || First == '/')
					return 6;
				else if(First == '+' || First == '-')
					return 5;
				else if(First == '<' || First == '>')
					return 3;
			} else if(Length == 2) {
				if(First == '.')
					return 4;
				else if(First == '<' || First == '>' || First == '~' || First == '=')
					return 3;
				else if(First == 'o')
					return 1;
			} else if(Length == 3) {
				if(First == 'a')
					return 2;
				else if(First == 'n')
					return 7;
			}
			return 0;
		}

		internal bool LeadExp() {
			if(Curr.Type == Token.EType.NUMBER ||
			   Curr.Type == Token.EType.STRING ||
			   Curr.IsKeyword("nil") ||
			   Curr.IsKeyword("false") ||
			   Curr.IsKeyword("true") ||
			   Curr.IsOperator("...") || 
			   Curr.IsKeyword("function") ||
			   Curr.IsBracket("{") ||
			   LeadPrefixExp() || 
			   UnOps.Contains(Curr.Value) ) {
				return true;
			}
			return false;
		}
		internal ParseNode ParseExp() {
			// exp :  ('nil' | 'false' | 'true' | number | string | '...' | function | prefixexp | tableconstructor | unop exp) (binop exp)* ;
			ParseNode Exp = new ParseNode(ParseNode.EType.EXP);

			if(Curr.IsKeyword("nil") ||
			   Curr.IsKeyword("false") ||
			   Curr.IsKeyword("true") ||
			   Curr.IsOperator("...")) {
				Exp.Add(Curr);
				Advance();
			} else if(Curr.Type == Token.EType.NUMBER) {
				Match(Token.EType.NUMBER, Exp);
			} else if(Curr.Type == Token.EType.STRING) {
				Match(Token.EType.STRING, Exp);
			} else if(Curr.IsKeyword("function")) {
				Exp.Add(ParseFunction());
			} else if(Curr.IsBracket("{")) {
				Exp.Add(ParseTableConstructor());
			} else if(Curr.Type == Token.EType.NAME ||
			          Curr.IsBracket("(")) {
				Exp.Add(ParsePrefixExp());
			} else if(UnOps.Contains(Curr.Value)) {
				ParseNode UnOpWrap = new ParseNode(ParseNode.EType.UN_OP_WRAP);
				UnOpWrap.Add(ParseUnOp());
				UnOpWrap.Add(ParseExp());
				Exp.Add(UnOpWrap);
			}

			while (BinOps.Contains(Curr.Value)) {
				ParseNode BinOpWrap = new ParseNode(ParseNode.EType.BIN_OP_WRAP);

				BinOpWrap.Children = new List<ParseNode>(Exp.Children);
				Exp.Children.Clear();
				BinOpWrap.Add(ParseBinOp());
				BinOpWrap.Add(ParseExp());

				Exp.Add(BinOpWrap);
			}

			return Exp;
		}

		internal bool LeadVar() {
			if(Curr.Type == Token.EType.NAME ||
			   Curr.IsBracket("("))
				return true;
			else
				return false;
		}
		internal ParseNode ParseVar() {
			// var: (NAME | '(' exp ')' varSuffix) varSuffix*;
			ParseNode Var = new ParseNode(ParseNode.EType.VAR);

			if(Curr.Type == Token.EType.NAME) {
				Match(Token.EType.NAME, Var);
			} else if(Curr.IsBracket("(")) {
				Match("(", Var);

				ParseNode Exp = ParseExp();
				Var.Add(Exp);

				Match(")", Var);

				ParseNode VarSuffix = ParseVarSuffix();
				Var.Add(VarSuffix);
			}

			//while(LeadVarSuffix()) {
			while(Curr.IsBracket("[") ||
			      Curr.IsOperator(".") ) {
				ParseNode VarSuffix = ParseVarSuffix();
				Var.Add(VarSuffix);
			}

			return Var;
		}

		internal bool LeadPrefixExp() {
			return LeadVarOrExp();
		}
		internal ParseNode ParsePrefixExp() {
			// prefixexp: varOrExp nameAndArgs*;
			ParseNode PrefixExp = new ParseNode(ParseNode.EType.PREFIX_EXP);

			PrefixExp.Add(ParseVarOrExp());
			while(LeadNameAndArgs()) {
				PrefixExp.Add(ParseNameAndArgs());
			}

			return PrefixExp;
		}

		internal bool LeadFuncCall() {
			return LeadVarOrExp();
		}
		internal ParseNode ParseFuncCall() {
			// functioncall: varOrExp nameAndArgs+;
			ParseNode FuncCall = new ParseNode(ParseNode.EType.FUNC_CALL);

			FuncCall.Add(ParseVarOrExp());
			FuncCall.Add(ParseNameAndArgs()); // the one

			// the rest
			while(LeadNameAndArgs()) {
				FuncCall.Add(ParseNameAndArgs());
			}

			return FuncCall;
		}

		internal bool LeadVarOrExp() {
			if(LeadVar() || Curr.IsBracket("(")) 
				return true;
			else
				return false;
		}
		internal ParseNode ParseVarOrExp() {
			// varOrExp: var | '(' exp ')';
			ParseNode VOE = new ParseNode(ParseNode.EType.VAR_OR_EXP);

			if(Curr.IsBracket("(")) {
				Match("(", VOE);

				ParseNode Exp = ParseExp();
				VOE.Add(Exp);

				Match(")", VOE);
			} else {
				ParseNode Var = ParseVar();
				VOE.Add(Var);
			}

			return VOE;
		}

		internal bool LeadNameAndArgs() {
			if(Curr.IsOperator(":") || LeadArgs())
				return true;
			return false;
		}
		internal ParseNode ParseNameAndArgs() {
			// nameAndArgs: (':' NAME)? args;
			ParseNode NAA = new ParseNode(ParseNode.EType.NAME_AND_ARGS);

			if(Curr.IsOperator(":")) {
				Match(":", NAA);
				Match(Token.EType.NAME, NAA);
			}

			ParseNode Args = ParseArgs();
			NAA.Add(Args);

			return NAA;
		}

		internal bool LeadVarSuffix() {
			if (Curr.IsBracket("[") ||
			    Curr.IsOperator(".") ||
			    LeadNameAndArgs())
				return true;
			else
				return false;
		}
		internal ParseNode ParseVarSuffix() {
			//  varSuffix: nameAndArgs* ('[' exp ']' | '.' NAME);
			ParseNode VarSuffix = new ParseNode(ParseNode.EType.VAR_SUFFIX);

			while (LeadNameAndArgs()) {
				ParseNode NAA = ParseNameAndArgs();
				VarSuffix.Add(NAA);
			}

			if(Curr.IsBracket("[")) {
				Match("[", VarSuffix);

				ParseNode Exp = ParseExp();
				VarSuffix.Add(Exp);

				Match("]", VarSuffix);
			} else if(Curr.IsOperator(".")) {
				Match(".", VarSuffix);
				Match(Token.EType.NAME, VarSuffix);
			} else {
				throw new ParseException("'[' or '.'", Curr);
			}

			return VarSuffix;
		}

		internal bool LeadArgs() {
			if (Curr.Type == Token.EType.STRING ||
			    Curr.IsBracket("{") ||
			    Curr.IsBracket("(")) {
				return true;
			}
			return false;
		}
		internal ParseNode ParseArgs() {
			// args :  '(' (explist1)? ')' | tableconstructor | string ;
			ParseNode Args = new ParseNode(ParseNode.EType.ARGS);

			if(Curr.Type == Token.EType.STRING) {
				Match(Token.EType.STRING, Args);
			} else if(Curr.IsBracket("{")) {
				ParseNode TableCtor = ParseTableConstructor();
				Args.Add(TableCtor);
			} else if(Curr.IsBracket("(")) {
				Match("(", Args);

				if(!Curr.IsBracket(")")) {
					ParseNode ExpList = ParseExpList();
					Args.Add(ExpList);
				}

				Match(")", Args);
			} else {
				throw new ParseException("STRING, '{', or '('", Curr);
			}

			return Args;
		}

		internal bool LeadFunction() {
			return Curr.IsKeyword("function");
		}
		internal ParseNode ParseFunction() {
			// function : 'function' funcbody;
			ParseNode Function = new ParseNode(ParseNode.EType.FUNCTION);

			Match("function", Function);

			ParseNode FuncBody = ParseFuncBody();
			Function.Add(FuncBody);

			return Function;
		}

		internal bool LeadFuncBody() {
			return Curr.IsOperator("(");
		}
		internal ParseNode ParseFuncBody() {
			// funcbody : '(' (parlist1)? ')' block 'end';
			ParseNode FuncBody = new ParseNode(ParseNode.EType.FUNC_BODY);

			Match("(", FuncBody);

			if(!Curr.IsBracket(")")) {
				ParseNode ParList = ParseParList();
				FuncBody.Add(ParList);
			}

			Match(")", FuncBody);

			ParseNode Block = ParseBlock();
			FuncBody.Add(Block);

			Match("end", FuncBody);

			return FuncBody;
		}

		internal bool LeadParList() {
			if (LeadNameList() ||
			    Curr.IsOperator("..."))
				return true;
			else
				return false;
		}
		internal ParseNode ParseParList() {
			// parlist1 : namelist (',' '...')? | '...';
			ParseNode ParList = new ParseNode(ParseNode.EType.PAR_LIST);

			if(Curr.IsOperator("...")) {
				Match("...", ParList);
			} else {
				ParseNode NameList = ParseNameList();
				ParList.Add(NameList);

				if(Curr.IsOperator(",")) {
					Match(",", ParList);
					Match("...", ParList);
				}
			}

			return ParList;
		}

		internal ParseNode ParseTableConstructor() {
			// tableconstructor : '{' (fieldlist)? '}';
			ParseNode TableCtor = new ParseNode(ParseNode.EType.TABLE_CONS);

			Match("{", TableCtor);

			if(!Curr.IsBracket("}")) {
				ParseNode FieldList = ParseFieldList();
				TableCtor.Add(FieldList);
			}

			Match("}", TableCtor);

			return TableCtor;
		}

		internal bool LeadFieldList() {
			return LeadField();
		}
		internal ParseNode ParseFieldList() {
			// fieldlist : field (fieldsep field)* (fieldsep)?;
			ParseNode FieldList = new ParseNode(ParseNode.EType.FIELD_LIST);

			ParseNode FirstField = ParseField();
			FieldList.Add(FirstField);

			while(FieldSeps.Contains(Curr.Value)) {
				ParseNode FieldSep = ParseFieldSep();
				FieldList.Add(FieldSep);
				FieldList.Add(ParseField());
			}

			return FieldList;
		}

		internal bool LeadField() {
			if (Curr.IsBracket("[") ||
			    Curr.Type == Token.EType.NAME ||
			    LeadExp())
				return true;
			return false;
		}
		internal ParseNode ParseField() {
			// field : '[' exp ']' '=' exp | NAME '=' exp | exp;
			ParseNode Field = new ParseNode(ParseNode.EType.FIELD);

			if(Curr.IsBracket("[")) {
				Match("[", Field);

				ParseNode DestExp = ParseExp();
				Field.Add(DestExp);

				Match("]", Field);

				Match("=", Field);

				ParseNode SrcExp = ParseExp();
				Field.Add(SrcExp);

				return Field;
			} else if(Curr.Type == Token.EType.NAME && Look.IsOperator("=")) {
				Match(Token.EType.NAME, Field);

				Match("=", Field);

				ParseNode SrcExp = ParseExp();
				Field.Add(SrcExp);

				return Field;
			} else if(LeadExp()) {
				Field.Add(ParseExp());
			} else {
				throw new ParseException("'[', NAME, or Expression", Curr);
			}

			return Field;
		}

		internal ParseNode ParseFieldSep() {
			if(FieldSeps.Contains(Curr.Value)) {
				ParseNode Sep = new ParseNode(Curr);
				Sep.Type = ParseNode.EType.FIELD_SEP;
				Advance();
				return Sep;
			} else {
				throw new ParseException("',' or ';'", Curr);
			}
		}

		internal ParseNode ParseBinOp() {
			/*	binop : '+' | '-' | '*' | '/' | '^' | '%' | '..' | 
		 			'<' | '<=' | '>' | '>=' | '==' | '~=' | 
		 			'and' | 'or';
			*/
			if (BinOps.Contains(Curr.Value)) {
				ParseNode BinOp = new ParseNode(Curr);
				BinOp.Type = ParseNode.EType.BINOP;
				Advance();
				return BinOp;
			} else {
				throw new ParseException("binary operator", Curr);
			}
		}

		internal ParseNode ParseUnOp() {
			if(UnOps.Contains(Curr.Value)) {
				ParseNode UnOp = new ParseNode(Curr);
				UnOp.Type = ParseNode.EType.UNOP;
				Advance();
				return UnOp;
			} else {
				throw new ParseException("unary operator", Curr);
			}
		}

		internal ParseNode ParseNumber() {
			if(Curr.Type == Token.EType.NUMBER) {
				ParseNode NumberNode = new ParseNode(Curr);
				Advance();
				return NumberNode;
			} else {
				throw new ParseException("NUMBER", Curr);
			}
		}

		internal ParseNode ParseString() {
			if(Curr.Type == Token.EType.STRING) {
				ParseNode StringNode = new ParseNode(Curr);
				Advance();
				return StringNode;
			} else {
				throw new ParseException("STRING", Curr);
			}
		}

	} // end Parser
}
