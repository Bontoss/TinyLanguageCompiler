using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        
        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    public class Parser
    {
        int InputPointer = 0; // keep trake of token stream 
        List<Token> TokenStream;
        public  Node root; // parse tree
        List<string> FunctionCalls = new List<string>();

        private bool isDatatype(int InputPointer)
        {
            bool isInt = TokenStream[InputPointer].token_type == TokenClass.DATATYPEINT;
            bool isFloat = TokenStream[InputPointer].token_type == TokenClass.DATATYPEFLOAT;
            bool isString = TokenStream[InputPointer].token_type == TokenClass.DATATYPESTRING;
            return (isInt || isFloat || isString);
        }
        private bool isStatement(int InputPointer)
        {

            bool ok = false;

            if (TokenStream[InputPointer].token_type == TokenClass.WRITE || TokenStream[InputPointer].token_type == TokenClass.READ
                || TokenStream[InputPointer].token_type == TokenClass.RETURN || TokenStream[InputPointer].token_type == TokenClass.REPEAT
                || TokenStream[InputPointer].token_type == TokenClass.IF || TokenStream[InputPointer].token_type == TokenClass.ELSEIF
                || TokenStream[InputPointer].token_type == TokenClass.ELSE || (TokenStream[InputPointer].token_type == TokenClass.DATATYPEINT
                && TokenStream[InputPointer + 1].token_type != TokenClass.MAIN)
                || TokenStream[InputPointer].token_type == TokenClass.DATATYPEFLOAT || TokenStream[InputPointer].token_type == TokenClass.DATATYPESTRING
                || TokenStream[InputPointer].token_type == TokenClass.IDENTIFIER)
                ok = true;
            return ok;
        }
        private bool isBooleanOp(int InputPointer)
        {
            bool isOr = TokenStream[InputPointer].token_type == TokenClass.OR;
            bool isAnd = TokenStream[InputPointer].token_type == TokenClass.AND;
            return (isAnd || isOr);
        }

        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
        }
        Node Program()
        {
            Node program = new Node("Program");
            program.Children.Add(Functions());
            program.Children.Add(MainFunction());
            MessageBox.Show("Success");
            return program;
        }

        private Node MainFunction()
        {
            Node mainfunction = new Node("MainFunction");
            mainfunction.Children.Add(DataType());
            mainfunction.Children.Add(match(TokenClass.MAIN));
            mainfunction.Children.Add(match(TokenClass.LEFTPARENTHESES));
            mainfunction.Children.Add(match(TokenClass.RIGHTPARENTHESES));
            mainfunction.Children.Add(FunctionBody());
            return mainfunction;
        }

        private Node Functions()
        {
            Node functions = new Node("Program'");
            if (InputPointer + 1 < TokenStream.Count && isDatatype(InputPointer) && TokenStream[InputPointer + 1].token_type != TokenClass.MAIN)
            {
                functions.Children.Add(Function());
                functions.Children.Add(Functions());
                return functions;
            }
            else
                return null;
        }
        private Node Function()
        {
            Node function = new Node("Function");
            function.Children.Add(FunctionDeclaration());
            function.Children.Add(FunctionBody());
            return function;
        }


        private Node FunctionDeclaration()
        {
            Node functiondeclaration = new Node("FunctionDeclaration");
            functiondeclaration.Children.Add(DataType());
            FunctionCalls.Add(TokenStream[InputPointer].lex);
            functiondeclaration.Children.Add(Identifier());
            functiondeclaration.Children.Add(match(TokenClass.LEFTPARENTHESES));
            functiondeclaration.Children.Add(Parameters());
            functiondeclaration.Children.Add(match(TokenClass.RIGHTPARENTHESES));
            return functiondeclaration;
        }
        private Node DataType()
        {
            Node datatype = new Node("DataType");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.DATATYPEINT)
                datatype.Children.Add(match(TokenClass.DATATYPEINT));
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.DATATYPESTRING)
                datatype.Children.Add(match(TokenClass.DATATYPESTRING));
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.DATATYPEFLOAT)
                datatype.Children.Add(match(TokenClass.DATATYPEFLOAT));
            return datatype;
        }
        private Node Identifier()
        {
            Node identifier = new Node("Identifier");
            identifier.Children.Add(match(TokenClass.IDENTIFIER));
            return identifier;
        }
        private Node Parameters()
        {
            Node parameters = new Node("Parameters");
            if (InputPointer < TokenStream.Count && isDatatype(InputPointer))
            {
                parameters.Children.Add(Parameter());
                parameters.Children.Add(ParametersDash());
                return parameters;
            }
            else
                return null;
        }
        private Node Parameter()
        {
            Node parameter = new Node("Parameter");
            parameter.Children.Add(DataType());
            parameter.Children.Add(match(TokenClass.IDENTIFIER));
            return parameter;
        }
        private Node ParametersDash()
        {

            Node parametersdash = new Node("ParametersDash");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.COMMA)
            {
                parametersdash.Children.Add(match(TokenClass.COMMA));
                parametersdash.Children.Add(Parameter());
                parametersdash.Children.Add(ParametersDash());
                return parametersdash;
            }
            else
                return null;
        }
        private Node FunctionBody()
        {
            Node functionbody = new Node("FunctionBody");
            functionbody.Children.Add(match(TokenClass.LEFTCURLYBRACKETS));
            functionbody.Children.Add(Statements());
            functionbody.Children.Add(match(TokenClass.RIGHTCURLYBRACKETS));
            return functionbody;
        }
        Node Statements()
        {
            Node statements = new Node("Statements");
            if (InputPointer < TokenStream.Count && isStatement(InputPointer))
            {
                statements.Children.Add(Statement());
                statements.Children.Add(State());
                return statements;
            }
            else
                return null;
        }
        Node Statement()
        {
            Node statement = new Node("Statement");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.READ)
                statement.Children.Add(ReadStatement());
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.WRITE)
                statement.Children.Add(WriteStatement());
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.REPEAT)
                statement.Children.Add(RepeatStatement());
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.RETURN)
                statement.Children.Add(ReturnStatement());
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.IF)
                statement.Children.Add(IfStatement());
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.ELSEIF)
                statement.Children.Add(ElseIfStatement());
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.ELSE)
                statement.Children.Add(ElseStatement());
            else if (InputPointer < TokenStream.Count && isDatatype(InputPointer))
                statement.Children.Add(DeclarationStatement());
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.IDENTIFIER)
            {
                if (InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type == TokenClass.ASSIGNMENTOPERATOR)
                {
                    statement.Children.Add(AssignmentStatement());
                    statement.Children.Add(match(TokenClass.SEMICOLON));
                }
                else if (InputPointer + 1 < TokenStream.Count && (TokenStream[InputPointer + 1].token_type == TokenClass.GREATERTHAN
                    || TokenStream[InputPointer + 1].token_type == TokenClass.LESSTHAN
                    || TokenStream[InputPointer + 1].token_type == TokenClass.EQUALTO
                    || TokenStream[InputPointer + 1].token_type == TokenClass.NOTEQUAL))
                    statement.Children.Add(ConditionStatement());
                else if (InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type == TokenClass.LEFTPARENTHESES)
                {
                    statement.Children.Add(FunctionCall());
                    statement.Children.Add(match(TokenClass.SEMICOLON));
                }
            }
            return statement;
        }
        Node State()
        {
            Node state = new Node("State");
            if (InputPointer < TokenStream.Count && isStatement(InputPointer))
            {
                state.Children.Add(Statement());
                state.Children.Add(State());
                return state;
            }
            else
                return null;
        }
        private Node ReadStatement()
        {
            Node read = new Node("ReadStatement");
            read.Children.Add(match(TokenClass.READ));
            read.Children.Add(match(TokenClass.IDENTIFIER));
            read.Children.Add(match(TokenClass.SEMICOLON));
            return read;
        }
        private Node WriteStatement()
        {
            Node write = new Node("WriteStatement");
            write.Children.Add(match(TokenClass.WRITE));
            if (TokenStream[InputPointer].token_type == TokenClass.ENDLINE)
                write.Children.Add(match(TokenClass.ENDLINE));
            else if (TokenStream[InputPointer].token_type == TokenClass.STRING)
                write.Children.Add(match(TokenClass.STRING));
            else
                write.Children.Add(Expression());
            write.Children.Add(match(TokenClass.SEMICOLON));
            return write;
        }
        private Node RepeatStatement()
        {
            Node repeatStatement = new Node("RepeatStatement");
            repeatStatement.Children.Add(match(TokenClass.REPEAT));
            repeatStatement.Children.Add(Statements());
                repeatStatement.Children.Add(match(TokenClass.UNTIL));
            if (TokenStream[InputPointer - 1].token_type == TokenClass.UNTIL)
                repeatStatement.Children.Add(ConditionStatement());
            return repeatStatement;
        }

        private Node ReturnStatement()
        {
            Node returnstatement = new Node("ReturnStatement");
            returnstatement.Children.Add(match(TokenClass.RETURN));
            returnstatement.Children.Add(Expression());
            returnstatement.Children.Add(match(TokenClass.SEMICOLON));
            return returnstatement;
        }
        private Node IfStatement()
        {
            Node ifstatement = new Node("IfStatement");
            ifstatement.Children.Add(match(TokenClass.IF));
            ifstatement.Children.Add(ConditionStatement());
            ifstatement.Children.Add(match(TokenClass.THEN));
            ifstatement.Children.Add(Statement());
            ifstatement.Children.Add(ElseClause());
            return ifstatement;
        }

        private Node ElseClause()
        {
            Node elseClause = new Node("ElseClause");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.END)
                elseClause.Children.Add(match(TokenClass.END));
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.ELSEIF)
                elseClause.Children.Add(ElseIfStatement());
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == TokenClass.ELSE)
                elseClause.Children.Add(ElseStatement());
            return elseClause;
        }

        private Node ElseIfStatement()
        {
            Node ElseIf = new Node("ElseIfStatement");
            ElseIf.Children.Add(match(TokenClass.ELSEIF));
            ElseIf.Children.Add(ConditionStatement());
            ElseIf.Children.Add(match(TokenClass.THEN));
            ElseIf.Children.Add(Statement());
            ElseIf.Children.Add(ElseClause());
            return ElseIf;
        }
        private Node ElseStatement()
        {
            Node elseState = new Node("ElseState");
            elseState.Children.Add(match(TokenClass.ELSE));
            elseState.Children.Add(Statement());
            elseState.Children.Add(match(TokenClass.END));
            return elseState;
        }
        private Node DeclarationStatement()
        {
            Node declarationstatement = new Node("DeclarationStatement");
            declarationstatement.Children.Add(DataType());
            declarationstatement.Children.Add(Identifier());
            if(TokenStream[InputPointer].token_type == TokenClass.SEMICOLON)
                declarationstatement.Children.Add(match(TokenClass.SEMICOLON));
            else
            {
                declarationstatement.Children.Add(match(TokenClass.EQUALTO));
                declarationstatement.Children.Add(Expression());
                declarationstatement.Children.Add(match(TokenClass.SEMICOLON));
            }
            return declarationstatement;
        }
        private Node AssignmentStatement()
        {
            Node assignmentstatement = new Node("AssignmentStatement");
            assignmentstatement.Children.Add(match(TokenClass.IDENTIFIER));
            assignmentstatement.Children.Add(match(TokenClass.ASSIGNMENTOPERATOR));
            assignmentstatement.Children.Add(Expression());
            return assignmentstatement;
        }
        private Node ConditionStatement()
        {
            Node condition_statement = new Node("Condition_Statement");
            condition_statement.Children.Add(Condition());
            condition_statement.Children.Add(ConditionDash());
            return condition_statement;
        }

        private Node Condition()
        {
            Node condition = new Node("Condition");
            condition.Children.Add(match(TokenClass.IDENTIFIER));
            condition.Children.Add(ConditionOp());
            condition.Children.Add(Term());
            return condition;
        }

        private Node ConditionOp()
        {
            Node conditionOp = new Node("ConditionOP");
            if (TokenStream[InputPointer].token_type == TokenClass.LESSTHAN)
                conditionOp.Children.Add(match(TokenClass.LESSTHAN));
            else if (TokenStream[InputPointer].token_type == TokenClass.GREATERTHAN)
                conditionOp.Children.Add(match(TokenClass.GREATERTHAN));
            else if (TokenStream[InputPointer].token_type == TokenClass.EQUALTO)
                conditionOp.Children.Add(match(TokenClass.EQUALTO));
            else if (TokenStream[InputPointer].token_type == TokenClass.NOTEQUAL)
                conditionOp.Children.Add(match(TokenClass.NOTEQUAL));
            return conditionOp;
        }
        private Node ConditionDash()
        {
            Node conditiondash = new Node("ConditionDash");
            if (isBooleanOp(InputPointer))
            {
                conditiondash.Children.Add(BooleanOP());
                conditiondash.Children.Add(Condition());
                conditiondash.Children.Add(ConditionDash());
                return conditiondash;
            }
            else
                return null;
        }
        private Node BooleanOP()
        {
            Node boolop = new Node("BooleanOP");

            if (TokenStream[InputPointer].token_type == TokenClass.AND)
                boolop.Children.Add(match(TokenClass.AND));
            else if (TokenStream[InputPointer].token_type == TokenClass.OR)
                boolop.Children.Add(match(TokenClass.OR));
            return boolop;
        }
        private Node FunctionCall()
        {
            Node functioncall = new Node("FunctionCall");
            functioncall.Children.Add(match(TokenClass.IDENTIFIER));
            functioncall.Children.Add(ArgList());
            functioncall.Children.Add(Factor());
            return functioncall;
        }
        Node ArgList()
        {
            // completed
            Node arglist = new Node("Arglist");
            if (TokenStream[InputPointer].token_type == TokenClass.LEFTPARENTHESES)
            {
                arglist.Children.Add(match(TokenClass.LEFTPARENTHESES));
                arglist.Children.Add(Arguments());
                arglist.Children.Add(match(TokenClass.RIGHTPARENTHESES));
                return arglist;
            }
            return null;
        }

        Node Arguments()
        {
            // completed
            Node argument = new Node("Argument");
            argument.Children.Add(match(TokenClass.IDENTIFIER));
            argument.Children.Add(Arg());
            return argument;
        }

        Node Arg()
        {
            // completed
            Node arg = new Node("Arg");
            if (TokenStream[InputPointer].token_type == TokenClass.COMMA)
            {
                arg.Children.Add(match(TokenClass.COMMA));
                arg.Children.Add(match(TokenClass.IDENTIFIER));
                arg.Children.Add(Arg());
                return arg;
            }
            return null;
        }
        Node Expression()
        {
            // completed
            Node expression = new Node("Expression");
            expression.Children.Add(Term());
            expression.Children.Add(Exp());
            return expression;
        }
        Node Term()
        {
            // completed 
            Node term = new Node("term");
            term.Children.Add(Factor());
            term.Children.Add(Ter());
            return term;
        }
        Node Ter()
        {
            // completed 
            Node ter = new Node("Ter");
            if (TokenStream[InputPointer].token_type == TokenClass.MULTIPLICATIONOPERATOR || TokenStream[InputPointer].token_type == TokenClass.DIVISIONOPERATOR)
            {
                ter.Children.Add(MultOp());
                ter.Children.Add(Factor());
                ter.Children.Add(Ter());
                return ter;
            }
            return null;
        }
        Node MultOp()
        {
            // completed
            Node multop = new Node("MultOP");
            if (TokenStream[InputPointer].token_type == TokenClass.MULTIPLICATIONOPERATOR)
                multop.Children.Add(match(TokenClass.MULTIPLICATIONOPERATOR));
            else if (TokenStream[InputPointer].token_type == TokenClass.DIVISIONOPERATOR)
                multop.Children.Add(match(TokenClass.DIVISIONOPERATOR));
            return multop;
        }
        Node AddOp()
        {
            // completed
            Node addop = new Node("AddOP");
            if (TokenStream[InputPointer].token_type == TokenClass.PLUSOPERATOR)
                addop.Children.Add(match(TokenClass.PLUSOPERATOR));
            else if (TokenStream[InputPointer].token_type == TokenClass.MINUSOPERATOR)
               addop.Children.Add(match(TokenClass.MINUSOPERATOR));
            return addop;
        }
        Node Exp()
        {
            // completed
            Node exp = new Node("Exp");
            if (TokenStream[InputPointer].token_type == TokenClass.PLUSOPERATOR || TokenStream[InputPointer].token_type == TokenClass.MINUSOPERATOR)
            {
                exp.Children.Add(AddOp());
                exp.Children.Add(Term());
                exp.Children.Add(Exp());
                return exp;
            }
            else
                return null;
        }
        Node Factor()
        {
            // completed
            Node factor = new Node("Factor");
            if (TokenStream[InputPointer].token_type == TokenClass.IDENTIFIER)
            {
                if (FunctionCalls.Contains(TokenStream[InputPointer].lex))
                {
                    factor.Children.Add(FunctionCall());
                    return factor;
                }
                factor.Children.Add(match(TokenClass.IDENTIFIER));
                factor.Children.Add(Factor());
            }
            else if (TokenStream[InputPointer].token_type == TokenClass.NUMBER)
            {
                factor.Children.Add(match(TokenClass.NUMBER));
                factor.Children.Add(Factor());
            }
            else if(TokenStream[InputPointer].token_type == TokenClass.LEFTPARENTHESES)
            {
                factor.Children.Add(match(TokenClass.LEFTPARENTHESES));
                factor.Children.Add(Factor());
            }
            else if (TokenStream[InputPointer].token_type == TokenClass.RIGHTPARENTHESES)
            {
                factor.Children.Add(match(TokenClass.RIGHTPARENTHESES));
                factor.Children.Add(Factor());
            }
            else if (TokenStream[InputPointer].token_type == TokenClass.STRING)
                factor.Children.Add(match(TokenClass.STRING));
            return factor;
        }
        public Node match(TokenClass ExpectedToken)
        {
            // check of token stream
            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());
                    return newNode;
                }
                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + " and " +
                        TokenStream[InputPointer].token_type.ToString() +
                        "  found\r\n");
                    InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString()  + "\r\n");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
