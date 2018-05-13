using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using Microsoft.VisualBasic;
using Microsoft.CSharp;

namespace DataStreams.ETL
{
    public class Evaluator
    {
        private static Dictionary<string, Delegate> methodLookup = new Dictionary<string, Delegate>();

        private string expression;
        private EvaluationType evalType;
        private Language language;

        // using SortedLists to guarantee same order during class text generation,
        // even if Add methods are called out of order
        private SortedList<string, Variable> variables = null;
        private SortedList<string, string> methods = null;
        private SortedList<string, object> references = null;

        public Evaluator(
            string expression,
            EvaluationType evalType) :
            this(
                expression,
                evalType,
                Language.CSharp)
        {

        }

        public Evaluator(
            string expression,
            EvaluationType evalType,
            Language language)
        {
            this.expression = expression;
            this.evalType = evalType;
            this.language = language;
        }

        public void AddVariable(
            string variableName,
            object variableValue)
        {
            AddVariable(
                variableName,
                variableValue,
                null);
        }

        public void AddVariable(
            string variableName,
            object variableValue,
            Type variableType)
        {
            // don't take the constructor hit unless required
            if (variables == null)
            {
                variables = new SortedList<string, Variable>();
            }
            Variable variable = new Variable();
            variable.VariableValue = variableValue;
            if (variableType != null)
            {
                variable.VaribleType = variableType;
            }
            variables.Add(variableName, variable);
        }

        public void AddCustomMethod(string methodContent)
        {
            // don't take the constructor hit unless required
            if (methods == null)
            {
                methods = new SortedList<string, string>();
            }
            methods.Add(methodContent, null);
        }

        public void AddReference(string reference)
        {
            // don't take the constructor hit unless required
            if (references == null)
            {
                references = new SortedList<string, object>();
            }
            references.Add(reference, null);
        }

        public EvaluationResult Eval()
        {
            string className = "CustomEvaluator";

            StringBuilder classText = new StringBuilder();

            if (language == Language.CSharp)
            {
                classText.AppendLine("using System;\n");

                classText.Append("class ");
                classText.Append(className);
                classText.AppendLine("\n{");

                #region add delegate

                classText.Append("\tpublic delegate ");

                if (evalType == EvaluationType.NoReturn)
                {
                    classText.Append("void");
                }
                else
                {
                    classText.Append("object");
                }

                classText.AppendLine(" EvalDelegate(");

                #region add variable definitions

                if (variables != null)
                {
                    bool firstVariable = true;
                    foreach (string variableName in variables.Keys)
                    {
                        if (!firstVariable)
                        {
                            classText.AppendLine(", ");
                        }
                        firstVariable = false;

                        Variable variable = variables[variableName];

                        // passing by reference so variables can be updated and their new values captured
                        if (variable.VaribleType != null)
                        {
                            classText.Append("\t\tref ");
                            classText.Append(variable.VaribleType.FullName);
                            classText.Append(" ");
                            classText.Append(variableName);
                        }
                        else
                        {
                            classText.Append("\t\tref object ");
                            classText.Append(variableName);
                        }
                    }
                }

                #endregion

                classText.AppendLine(");\n");

                #endregion

                #region add Eval method

                classText.Append("\tpublic static ");

                if (evalType == EvaluationType.NoReturn)
                {
                    classText.Append("void");
                }
                else
                {
                    classText.Append("object");
                }

                classText.AppendLine(" Eval(");

                #region add variable definitions

                if (variables != null)
                {
                    bool firstVariable = true;
                    foreach (string variableName in variables.Keys)
                    {
                        if (!firstVariable)
                        {
                            classText.AppendLine(", ");
                        }
                        firstVariable = false;

                        Variable variable = variables[variableName];

                        // passing by reference so variables can be updated and their new values captured
                        if (variable.VaribleType != null)
                        {
                            classText.Append("\t\tref ");
                            classText.Append(variable.VaribleType.FullName);
                            classText.Append(" ");
                            classText.Append(variableName);
                        }
                        else
                        {
                            classText.Append("\t\tref object ");
                            classText.Append(variableName);
                        }
                    }
                }

                #endregion

                classText.AppendLine(")\n\t{\n");

                if (evalType == EvaluationType.SingleLineReturn)
                {
                    classText.Append("\t\treturn ");
                    classText.Append(expression);
                    classText.AppendLine(";");
                }
                else
                {
                    classText.AppendLine(expression);
                }

                classText.AppendLine("\n\t}");

                #endregion

                if (methods != null)
                {
                    foreach (string customMethod in methods.Keys)
                    {
                        classText.AppendLine(customMethod);
                    }
                }

                classText.AppendLine("}");
            }
            else
            {
                // can't specify option strict or we would need to know all the variable data types beforehand

                classText.AppendLine("Imports System\n");

                classText.Append("Class ");
                classText.AppendLine(className);

                #region add delegate

                classText.Append("\tPublic Delegate ");

                if (evalType == EvaluationType.NoReturn)
                {
                    classText.Append("Sub");
                }
                else
                {
                    classText.Append("Function");
                }

                classText.AppendLine(" EvalDelegate( _");

                #region add variable definitions

                if (variables != null)
                {
                    bool firstVariable = true;
                    foreach (string variableName in variables.Keys)
                    {
                        if (!firstVariable)
                        {
                            classText.AppendLine(", _");
                        }
                        firstVariable = false;

                        Variable variable = variables[variableName];

                        // passing by reference so variables can be updated and their new values captured
                        if (variable.VaribleType != null)
                        {
                            classText.Append("\t\tByRef ");
                            classText.Append(variableName);
                            classText.Append(" As ");
                            classText.Append(variable.VaribleType.FullName);
                        }
                        else
                        {
                            classText.Append("\t\tByRef ");
                            classText.Append(variableName);
                            classText.Append(" As Object");
                        }
                    }
                }

                #endregion

                classText.Append(")");

                if (evalType != EvaluationType.NoReturn)
                {
                    classText.Append(" As Object");
                }

                classText.AppendLine("\n");

                #endregion

                #region add Eval method

                classText.Append("\tPublic Shared ");

                if (evalType == EvaluationType.NoReturn)
                {
                    classText.Append("Sub");
                }
                else
                {
                    classText.Append("Function");
                }

                classText.AppendLine(" Eval( _");

                #region add variable definitions

                if (variables != null)
                {
                    bool firstVariable = true;
                    foreach (string variableName in variables.Keys)
                    {
                        if (!firstVariable)
                        {
                            classText.AppendLine(", _");
                        }
                        firstVariable = false;

                        Variable variable = variables[variableName];

                        // passing by reference so variables can be updated and their new values captured
                        if (variable.VaribleType != null)
                        {
                            classText.Append("\t\tByRef ");
                            classText.Append(variableName);
                            classText.Append(" As ");
                            classText.Append(variable.VaribleType.FullName);
                        }
                        else
                        {
                            classText.Append("\t\tByRef ");
                            classText.Append(variableName);
                            classText.Append(" As Object");
                        }
                    }
                }

                #endregion

                classText.Append(")");

                if (evalType != EvaluationType.NoReturn)
                {
                    classText.Append(" As Object");
                }

                classText.AppendLine("\n");

                if (evalType == EvaluationType.SingleLineReturn)
                {
                    classText.Append("\t\tReturn ");
                }

                classText.AppendLine(expression);

                classText.Append("\n\tEnd ");

                if (evalType == EvaluationType.NoReturn)
                {
                    classText.AppendLine("Sub");
                }
                else
                {
                    classText.AppendLine("Function");
                }

                #endregion

                if (methods != null)
                {
                    foreach (string customMethod in methods.Keys)
                    {
                        classText.AppendLine(customMethod);
                    }
                }

                classText.AppendLine("End Class");
            }

            string finalClass = classText.ToString();

            // have to use full class text since cache exists over entire application domain
            // custom method implementations might have changed or passed in variable list might
            // have changed
            if (!methodLookup.ContainsKey(finalClass))
            {
                CodeDomProvider provider = null;

                if (language == Language.CSharp)
                {
                    provider = new CSharpCodeProvider();
                }
                else
                {
                    provider = new VBCodeProvider();
                }

                using (provider)
                {
                    CompilerParameters options = new CompilerParameters();
                    if (references != null)
                    {
                        foreach (string reference in references.Keys)
                        {
                            options.ReferencedAssemblies.Add(reference);
                        }
                    }
                    options.CompilerOptions = "/t:library";
                    options.GenerateInMemory = true;
                    options.IncludeDebugInformation = false;
                    options.TreatWarningsAsErrors = false;

                    // doesn't need to be globally unique since we're only searching
                    // for the class in the assembly that gets compiled
                    CompilerResults compileResults = provider.CompileAssemblyFromSource(options, finalClass);

                    if (compileResults.Errors.HasErrors)
                    {
                        EvaluationResult errorResult = new EvaluationResult();
                        errorResult.ThrewException = true;
                        string exceptionMessage = "";
                        foreach (CompilerError error in compileResults.Errors)
                        {
                            exceptionMessage += "\n\tLine " + error.Line.ToString() + ": " + error.ErrorText;
                        }
                        errorResult.Exception = new Exception(exceptionMessage);
                        errorResult.GeneratedClassCode = finalClass;
                        return errorResult;
                    }

                    Type evalClass = compileResults.CompiledAssembly.GetType(className);
                    Type delegateType = evalClass.GetNestedType("EvalDelegate");
                    Delegate method = Delegate.CreateDelegate(
                        delegateType,
                        evalClass.GetMethod("Eval"),
                        true);

                    methodLookup[finalClass] = method;
                }
            }

            object[] variableValues = null;

            if (variables != null)
            {
                variableValues = new object[variables.Count];

                int variableIndex = 0;
                foreach (Variable variable in variables.Values)
                {
                    variableValues[variableIndex++] = variable.VariableValue;
                }
            }

            try
            {
                object expressionResult = methodLookup[finalClass].DynamicInvoke(variableValues);

                if (variables != null)
                {
                    int variableIndex = 0;

                    // create a clone of the keys otherwise this would throw an exception saying
                    // the enumeration was modified
                    string[] keys = new string[variables.Keys.Count];
                    variables.Keys.CopyTo(keys, 0);

                    foreach (string variableName in keys)
                    {
                        Variable variable = variables[variableName];
                        variable.VariableValue = variableValues[variableIndex++];
                        variables[variableName] = variable;
                    }
                }

                EvaluationResult result = new EvaluationResult();
                result.Result = expressionResult;
                result.Variables = variables;
                result.GeneratedClassCode = finalClass;

                return result;
            }
            catch (ArgumentException ex)
            {
                EvaluationResult errorResult = new EvaluationResult();
                errorResult.ThrewException = true;
                errorResult.Exception = new ArgumentException(ex.Message);
                errorResult.GeneratedClassCode = finalClass;
                return errorResult;
            }
            catch (TargetInvocationException ex)
            {
                EvaluationResult errorResult = new EvaluationResult();
                errorResult.ThrewException = true;
                errorResult.Exception = new Exception(ex.InnerException.Message);
                errorResult.GeneratedClassCode = finalClass;
                return errorResult;
            }
        }

        public struct EvaluationResult
        {
            public object Result;
            public SortedList<string, Variable> Variables;
            public bool ThrewException;
            public Exception Exception;
            public string GeneratedClassCode;
        }

        public enum EvaluationType
        {
            SingleLineReturn,
            MultiLineReturn,
            NoReturn
        }

        public enum Language
        {
            CSharp,
            VB
        }

        public struct Variable
        {
            public object VariableValue;
            public Type VaribleType;
        }
    }
}