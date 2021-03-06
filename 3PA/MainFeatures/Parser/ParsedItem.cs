﻿#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (ParsedItem.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace _3PA.MainFeatures.Parser {
    /// <summary>
    /// base abstract class for ParsedItem
    /// </summary>
    internal abstract class ParsedItem {

        /// <summary>
        /// Name of the parsed item
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// full file path in which this item has been parsed
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// When including a file, each item parsed has a definition line that corresponds to the line in the file where the item was parsed,
        /// but we also need to need to know where, in the current file parsed, this include is, so we can know filter the items correctly... 
        /// The default value is -1 
        /// </summary>
        public int IncludeLine { get; set; }

        /// <summary>
        /// The starting position of the first keyword of the statement where the item is found
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// Line of the first keyword of the statement where the item is found
        /// <remarks>THE LINE COUNT START AT 0 NOT 1!!</remarks>
        /// </summary>
        public int Line { get; private set; }

        /// <summary>
        /// Column of the first keyword of the statement where the item is found
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// The ending position of the first keyword of the statement where the item is found
        /// OR the ending position of the EOS of the statement (be careful to verify this 
        /// before using this property...)
        /// </summary>
        public int EndPosition { get; set; }

        /// <summary>
        /// Scope in which this item has been parsed
        /// </summary>
        public ParsedScopeItem Scope { get; set; }

        public abstract void Accept(IParserVisitor visitor);

        protected ParsedItem(string name, Token token) {
            Name = name;
            Line = token.Line;
            Column = token.Column;
            Position = token.StartPosition;
            EndPosition = token.EndPosition;
            IncludeLine = -1;
        }
    }

    /// <summary>
    /// Parent class for procedure, function and OnEvent Items
    /// </summary>
    internal abstract class ParsedScopeItem : ParsedItem {
        /// <summary>
        /// line of the "end" keyword that ends the block
        /// </summary>
        public int EndBlockLine { get; set; }

        /// <summary>
        /// end position of the EOS that closes the block, initiated to -1 by default
        /// </summary>
        public int EndBlockPosition { get; set; }

        /// <summary>
        /// If true, the block contains too much characters and will not be openable in the
        /// appbuilder
        /// </summary>
        public bool TooLongForAppbuilder { get; set; }

        /// <summary>
        /// Allows faster comparison against ParsedScopeItems
        /// </summary>
        public ParsedScopeType ScopeType { get; private set; }

        protected ParsedScopeItem(string name, Token token, ParsedScopeType scopeType)
            : base(name, token) {
            ScopeType = scopeType;
            EndBlockPosition = -1;
        }
    }

    /// <summary>
    /// Allows faster comparison against ParsedScopeItems
    /// </summary>
    internal enum ParsedScopeType {
        File,
        Block,
        Procedure,
        Function,
        OnStatement
    }

    /// <summary>
    /// The "root" scope of a file
    /// </summary>
    internal class ParsedFile : ParsedScopeItem {

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }
        public ParsedFile(string name, Token token) : base(name, token, ParsedScopeType.File) { }

    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    internal class ParsedPreProcBlock : ParsedScopeItem {

        /// <summary>
        /// type of this block
        /// </summary>
        public ParsedPreProcBlockType Type { get; set; }

        /// <summary>
        /// Everything after ANALYZE-SUSPEND
        /// </summary>
        public string BlockDescription { get; set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }
        public ParsedPreProcBlock(string name, Token token) : base(name, token, ParsedScopeType.Block) {
        }
    }

    internal enum ParsedPreProcBlockType {
        Unknown,
        FunctionForward,
        MainBlock,
        Definitions,
        UibPreprocessorBlock,
        Xftr,
        ProcedureSettings,
        CreateWindow,
        RunTimeAttributes,

        // before that, this is an ANALYSE-SUSPEND block, below are the other pre-processed block
        IfEndIf,
    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    internal class ParsedProcedure : ParsedScopeItem {
        public string Left { get; private set; }

        /// <summary>
        /// Has the external flag in its definition
        /// </summary>
        public bool IsExternal { get; private set; }
        /// <summary>
        /// Has the private flag
        /// </summary>
        public bool IsPrivate { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedProcedure(string name, Token token, string left, bool isExternal, bool isPrivate)
            : base(name, token, ParsedScopeType.Procedure) {
            Left = left;
            IsExternal = isExternal;
            IsPrivate = isPrivate;
        }
    }

    /// <summary>
    /// Function parsed item
    /// Flag : private
    /// </summary>
    internal abstract class ParsedFunction : ParsedScopeItem {
        public ParsedPrimitiveType ReturnType { get; set; }
        /// <summary>
        /// Parsed string for the return type, use ReturnType instead!
        /// </summary>
        public string ParsedReturnType { get; private set; }
        /// <summary>
        /// is the return-type "EXTENT [x]" (0 if not extented) / should be a string representing an integer
        /// </summary>
        public string Extend { get; set; }
        public bool IsExtended { get; set; }
        public string Parameters { get; set; }
        public bool IsPrivate { get; set; }

        protected ParsedFunction(string name, Token token, string parsedReturnType) : base(name, token, ParsedScopeType.Function) {
            ParsedReturnType = parsedReturnType;
            Extend = String.Empty;
        }
    }

    /// <summary>
    /// Function parsed item
    /// </summary>
    internal class ParsedPrototype : ParsedFunction {

        /// <summary>
        /// true if it's a simple FORWARD and the implementation is in the same proc,
        /// false otherwise (meaning we matched a IN)
        /// </summary>
        public bool SimpleForward { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedPrototype(string name, Token token, string parsedReturnType) : base(name, token, parsedReturnType) {
            Extend = String.Empty;
        }
    }

    /// <summary>
    /// Function parsed item
    /// </summary>
    internal class ParsedImplementation : ParsedFunction {

        /// <summary>
        /// true if this function is an implementation AND has a prototype
        /// </summary>
        public bool HasPrototype { get; set; }
        public int PrototypeLine { get; set; }
        public int PrototypeColumn { get; set; }
        public int PrototypePosition { get; set; }
        public int PrototypeEndPosition { get; set; }
        /// <summary>
        /// Boolean to know if the prototype is correct compared to the implementation
        /// </summary>
        public bool PrototypeUpdated { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedImplementation(string name, Token token, string parsedReturnType)
            : base(name, token, parsedReturnType) {
            Extend = String.Empty;
        }
    }

    /// <summary>
    /// Procedure parsed item
    /// </summary>
    internal class ParsedOnStatement : ParsedScopeItem {
        public string EventList { get; private set; }
        public string WidgetList { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedOnStatement(string name, Token token, string eventList, string widgetList)
            : base(name, token, ParsedScopeType.OnStatement) {
            EventList = eventList;
            WidgetList = widgetList;
        }
    }

    /// <summary>
    /// found table in program
    /// </summary>
    internal class ParsedFoundTableUse : ParsedItem {
        public bool IsTempTable { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedFoundTableUse(string name, Token token, bool isTempTable)
            : base(name, token) {
            IsTempTable = isTempTable;
        }
    }

    /// <summary>
    /// Label
    /// </summary>
    internal class ParsedLabel : ParsedItem {
        public int UndefinedLine { get; set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedLabel(string name, Token token) : base(name, token) {
        }
    }

    /// <summary>
    /// dynamic function calls parsed item
    /// </summary>
    internal class ParsedFunctionCall : ParsedItem {
        /// <summary>
        /// true if the called function is not defined in the program
        /// </summary>
        public bool ExternalCall { get; private set; }

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedFunctionCall(string name, Token token, bool externalCall)
            : base(name, token) {
            ExternalCall = externalCall;
        }
    }

    /// <summary>
    /// Run parsed item
    /// </summary>
    internal class ParsedRun : ParsedItem {
        /// <summary>
        /// true if the Run statement is based on a evaluating a VALUE()
        /// </summary>
        public bool IsEvaluateValue { get; private set; }
        public bool HasPersistent { get; private set; }
        public string Left { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedRun(string name, Token token, string left, bool isEvaluateValue, bool hasPersistent) : base(name, token) {
            Left = left;
            IsEvaluateValue = isEvaluateValue;
            HasPersistent = hasPersistent;
        }
    }

    /// <summary>
    /// include file parsed item
    /// </summary>
    internal class ParsedIncludeFile : ParsedItem {

        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedIncludeFile(string name, Token token) : base(name, token) {}
    }

    /// <summary>
    /// Pre-processed var parsed item
    /// </summary>
    internal class ParsedPreProc : ParsedItem {
        public string Value { get; private set; }
        public int UndefinedLine { get; set; }
        public ParsedPreProcType Type { get; set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedPreProc(string name, Token token, int undefinedLine, ParsedPreProcType type, string value) : base(name, token) {
            UndefinedLine = undefinedLine;
            Type = type;
            Value = value;
        }
    }

    internal enum ParsedPreProcType {
        Scope = 2,
        Global = 4
    }

    /// <summary>
    /// Define parsed item
    /// </summary>
    internal class ParsedDefine : ParsedItem {
        /// <summary>
        /// can contains (separated by 1 space) :
        /// global, shared, private, new
        /// </summary>
        public string LcFlagString { get; private set; }
        /// <summary>
        /// contains as or like in lowercase
        /// (for buffers, it contains the table it buffs)
        /// </summary>
        public ParsedAsLike AsLike { get; private set; }
        /// <summary>
        /// In case of a buffer, contains the references table (BUFFER name FOR xxx)
        /// </summary>
        public string BufferFor { get; private set; }
        /// <summary>
        /// if the variable is "EXTENT [x]"
        /// </summary>
        public bool IsExtended { get; private set; }
        /// <summary>
        /// if the variable was CREATE'd instead of DEFINE'd
        /// </summary>
        public bool IsDynamic { get; private set; }
        public string Left { get; private set; }
        /// <summary>
        /// The "Type" is what succeeds the DEFINE word of the statement (VARIABLE, BUFFER....)
        /// </summary>
        public ParseDefineType Type { get; private set; }
        /// <summary>
        /// When parsing, we store the value of the "primitive-type" in there, 
        /// with the visitor, we convert this to a ParsedPrimitiveType later
        /// </summary>
        public string TempPrimitiveType { get; private set; } 
        /// <summary>
        /// (Used for variables) contains the primitive type of the variable
        /// </summary>
        public ParsedPrimitiveType PrimitiveType { get; set; }
        /// <summary>
        /// first word after "view-as"
        /// </summary>
        public string ViewAs { get; private set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedDefine(string name, Token token, string lcFlagString, ParsedAsLike asLike, string left, ParseDefineType type, string tempPrimitiveType, string viewAs, string bufferFor, bool isExtended, bool isDynamic)
            : base(name, token) {
            LcFlagString = lcFlagString;
            AsLike = asLike;
            Left = left;
            Type = type;
            TempPrimitiveType = tempPrimitiveType;
            ViewAs = viewAs;
            BufferFor = bufferFor;
            IsExtended = isExtended;
            IsDynamic = isDynamic;
        }
    }

    internal enum ParsedAsLike {
        None,
        As,
        Like
    }

    /// <summary>
    /// Enumeration of DEFINE types
    /// </summary>
    internal enum ParseDefineType {
        [Description("PARAMETER")]
        Parameter,
        [Description("DATA-SOURCE")]
        DataSource,
        [Description("EVENT")]
        Event,
        [Description("BUFFER")]
        Buffer,
        [Description("VARIABLE")]
        Variable,
        [Description("BROWSE")]
        Browse,
        [Description("STREAM")]
        Stream,
        [Description("BUTTON")]
        Button,
        [Description("DATASET")]
        Dataset,
        [Description("IMAGE")]
        Image,
        [Description("MENU")]
        Menu,
        [Description("FRAME")]
        Frame,
        [Description("QUERY")]
        Query,
        [Description("RECTANGLE")]
        Rectangle,
        [Description("PROPERTY")]
        Property,
        [Description("SUB-MENU")]
        SubMenu,
        [Description("NONE")]
        None
    }

    internal enum ParsedPrimitiveType {
        Character = 0,
        Comhandle,
        Date,
        Datetime,
        Datetimetz,
        Decimal,
        Handle,
        Int64,
        Integer,
        Logical,
        Longchar,
        Memptr,
        Raw,
        Recid,
        Rowid,
        // Below are the types allowed for the parameters
        Buffer = 20,
        Table,
        TableHandle,
        Dataset,
        DatasetHandle,
        // below are the types that are not considered as primitive (they will appear in the VariableComplex category)
        Clob = 30,
        WidgetHandle,
        Blob,
        Widget,
        Unknow,
        Class,
        // below, are the types for the .dll
        Long = 50,
        Short,
        Byte,
        Float,
        Double,
        UnsignedShort,
        UnsignedLong
    }

    /// <summary>
    /// data base parsed item
    /// </summary>
    internal class ParsedDataBase {
        public string LogicalName { get; private set; }
        public string PhysicalName { get; private set; }
        public string ProgressVersion { get; private set; }
        public List<ParsedTable> Tables { get; private set; }

        public ParsedDataBase(string logicalName, string physicalName, string progressVersion, List<ParsedTable> tables) {
            LogicalName = logicalName;
            PhysicalName = physicalName;
            ProgressVersion = progressVersion;
            Tables = tables;
        }
    }

    /// <summary>
    /// Table or temp table parsed item
    /// </summary>
    internal class ParsedTable : ParsedItem {
        public string Id { get; private set; }
        public string Crc { get; private set; }
        public string DumpName { get; private set; }
        /// <summary>
        /// To know if the table is a temptable
        /// </summary>
        public bool IsTempTable { get; private set; }
        /// <summary>
        /// From database, represents the description of the table
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// contains the table "LIKE TABLE" name in lowercase
        /// </summary>
        public string LcLikeTable { get; private set; }
        /// <summary>
        /// if temptable and temptable is "like" another table, contains the USE-INDEX 
        /// </summary>
        public string UseIndex { get; private set; }
        /// <summary>
        /// In case of a temp table, can contains the eventuals :
        /// NEW [ GLOBAL ] ] SHARED ] | [ PRIVATE | PROTECTED ] [ STATIC ] flags
        /// </summary>
        public string LcFlagString { get; private set; }
        public List<ParsedField> Fields { get; set; }
        public List<ParsedIndex> Indexes { get; set; }
        public List<ParsedTrigger> Triggers { get; set; }
        public override void Accept(IParserVisitor visitor) {
            visitor.Visit(this);
        }

        public ParsedTable(string name, Token token, string id, string crc, string dumpName, string description, string lcLikeTable, bool isTempTable, List<ParsedField> fields, List<ParsedIndex> indexes, List<ParsedTrigger> triggers, string lcFlagString, string useIndex) : base(name, token) {
            Id = id;
            Crc = crc;
            DumpName = dumpName;
            Description = description;
            LcLikeTable = lcLikeTable;
            IsTempTable = isTempTable;
            Fields = fields;
            Indexes = indexes;
            Triggers = triggers;
            LcFlagString = lcFlagString;
            UseIndex = useIndex;
        }
    }

    /// <summary>
    /// describes a field of a table
    /// </summary>
    internal class ParsedField {
        public string Name { get; private set; }
        /// <summary>
        /// When parsing, we store the value of the "primitive-type" in there, 
        /// with the visitor, we convert this to a ParsedPrimitiveType later
        /// </summary>
        public string TempType { get; set; } 
        public ParsedPrimitiveType Type { get; set; } 
        public string Format { get;  set; }
        public int Order { get;  set; }
        public ParsedFieldFlag Flag { get;  set; }
        public string InitialValue { get;  set; }
        public string Description { get;  set; }
        /// <summary>
        /// contains as or like in lowercase
        /// </summary>
        public ParsedAsLike AsLike { get; set; }
        public ParsedField(string name, string lcTempType, string format, int order, ParsedFieldFlag flag, string initialValue, string description, ParsedAsLike asLike) {
            Name = name;
            TempType = lcTempType;
            Format = format;
            Order = order;
            Flag = flag;
            InitialValue = initialValue;
            Description = description;
            AsLike = asLike;
        }
    }

    [Flags]
    internal enum ParsedFieldFlag {
        None = 1,
        Extent = 2,
        Index = 4,
        Primary = 8,
        Mandatory = 16
    }

    /// <summary>
    /// defines a index of a table
    /// </summary>
    internal class ParsedIndex {
        public string Name { get; private set; }
        public ParsedIndexFlag Flag { get; private set; }
        public List<string> FieldsList { get; private set; }
        public ParsedIndex(string name, ParsedIndexFlag flag, List<string> fieldsList) {
            Name = name;
            Flag = flag;
            FieldsList = fieldsList;
        }
    }

    [Flags]
    internal enum ParsedIndexFlag {
        None = 1,
        Unique = 2,
        Primary = 4
    }

    /// <summary>
    /// defines a trigger of a table
    /// </summary>
    internal class ParsedTrigger {
        public string Event { get; private set; }
        public string ProcName { get; private set; }
        public ParsedTrigger(string @event, string procName) {
            Event = @event;
            ProcName = procName;
        }
    }
}
