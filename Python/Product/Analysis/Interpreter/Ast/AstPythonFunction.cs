﻿// Python Tools for Visual Studio
// Copyright(c) Microsoft Corporation
// All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the License); you may not use
// this file except in compliance with the License. You may obtain a copy of the
// License at http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED ON AN  *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS
// OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY
// IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE,
// MERCHANTABLITY OR NON-INFRINGEMENT.
//
// See the Apache Version 2.0 License for specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PythonTools.Analysis;
using Microsoft.PythonTools.Analysis.Infrastructure;
using Microsoft.PythonTools.Parsing.Ast;

namespace Microsoft.PythonTools.Interpreter.Ast {
    class AstPythonFunction : IPythonFunction, ILocatedMember {
        private readonly List<IPythonFunctionOverload> _overloads;
        private readonly string _doc;

        public AstPythonFunction(
            PythonAst ast,
            IPythonModule declModule,
            IPythonType declType,
            FunctionDefinition def,
            LocationInfo loc
        ) {
            DeclaringModule = declModule ?? throw new ArgumentNullException(nameof(declModule));
            DeclaringType = declType;

            Name = def.Name;
            if (Name == "__init__") {
                _doc = declType?.Documentation;
            }

            foreach (var dec in (def.Decorators?.DecoratorsInternal).MaybeEnumerate().OfType<NameExpression>()) {
                if (dec.Name == "classmethod") {
                    IsClassMethod = true;
                } else if (dec.Name == "staticmethod") {
                    IsStatic = true;
                }
            }

            _overloads = new List<IPythonFunctionOverload>();

            Locations = loc != null ? new[] { loc } : Array.Empty<LocationInfo>();
        }

        internal void AddOverload(IPythonFunctionOverload overload) {
            _overloads.Add(overload);
        }

        internal static IEnumerable<IParameterInfo> MakeParameters(PythonAst ast, FunctionDefinition def) {
            foreach (var p in def.ParametersInternal) {
                yield return new AstPythonParameterInfo(ast, p);
            }
        }

        public IPythonModule DeclaringModule {get;}
        public IPythonType DeclaringType {get;}
        public string Name { get; }
        public string Documentation => _doc ?? _overloads.FirstOrDefault()?.Documentation;
        public bool IsBuiltin => true;

        public bool IsClassMethod { get; }
        public bool IsStatic { get; }

        public PythonMemberType MemberType => DeclaringType == null ? PythonMemberType.Function : PythonMemberType.Method;

        public IList<IPythonFunctionOverload> Overloads => _overloads.ToArray();

        public IEnumerable<LocationInfo> Locations { get; }
    }
}
