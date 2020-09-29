using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;

namespace PythonUtils
{
    public abstract class AbstractPythonExecutorBuilder
    {
        /// <summary>
        /// The scope manager
        /// </summary>
        protected PythonScopeManager scopeManager;
        /// <summary>
        /// The engine (singleton)
        /// </summary>
        protected static ScriptEngine engine;
        /// <summary>
        /// The scope
        /// </summary>
        protected dynamic scope;

        /// <summary>
        /// Gets the engine (instantiates it if necessary)
        /// </summary>
        /// <value>
        /// The engine.
        /// </value>
        /// <remarks>There is one engine that serves all of the scripts in the program. (Singleton)</remarks>
        protected ScriptEngine Engine
        {
            get
            {
                if (engine == null)
                {
                    engine = IronPython.Hosting.Python.CreateEngine();
                }

                return engine;
            }
        }

        /// <summary>
        /// Gets the scope (instantiates it if necessary)
        /// </summary>
        /// <value>
        /// The scope.
        /// </value>
        protected ScriptScope Scope
        {
            get
            {
                if (scope == null)
                    scope = Engine.CreateScope();

                return (ScriptScope)scope;
            }
        }

        /// <summary>
        /// Gets the scope manager (instantiates it if necessary)
        /// </summary>
        /// <value>
        /// The scope manager.
        /// </value>
        protected PythonScopeManager ScopeManager
        {
            get
            {
                if (this.scopeManager == null)
                    this.scopeManager = new PythonScopeManager(Scope);

                return scopeManager;
            }

        }

        /// <summary>
        /// Sets the value of a variable
        /// </summary>
        /// <typeparam name="T">The data type of the variable</typeparam>
        /// <param name="name">The name of the variable</param>
        /// <param name="value">The value of the variable</param>
        /// <remarks>The variable will be added to the scope.</remarks>
        public void SetVariableValue<T>(string name, T value)
        {
            ScopeManager.SetPyhtonVariableValue(name, value);
        }


        /// <summary>
        /// Builds the resulting instance.
        /// </summary>
        /// <returns>The resulting instance.</returns>
        public abstract AbstractPythonExecutor Build();
    }
}
