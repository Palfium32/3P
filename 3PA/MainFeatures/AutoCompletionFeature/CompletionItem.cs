﻿#region header
// ========================================================================
// Copyright (c) 2017 - Julien Caillon (julien.caillon@gmail.com)
// This file (CompletionItem.cs) is part of 3P.
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
using System.Drawing;
using YamuiFramework.Controls.YamuiList;
using _3PA.Images;
using _3PA.MainFeatures.Parser;

namespace _3PA.MainFeatures.AutoCompletionFeature {

    /// <summary>
    /// class used in the auto completion feature
    /// </summary>
    internal class CompletionItem : FilteredTypeListItem {

        /// <summary>
        /// Type of completion
        /// </summary>
        public CompletionType Type { get; set; }

        /// <summary>
        /// Allows to display small "tag" picture on the left of a completionData in the autocomp list,
        /// see the ParseFlag enumeration for all the possibilities
        /// It works as a Flag, call HasFlag() method to if a certain flag is set and use
        /// Flag = Flag | ParseFlag.Reserved to set a flag!
        /// </summary>
        public ParseFlag Flag { get; set; }

        /// <summary>
        /// Used for sorting the autocompletion list, the higher the ranking, the higher in the list
        /// the item is
        /// </summary>
        public int Ranking { get; set; }

        /// <summary>
        /// A free to use string, can contain :
        /// - keyword = type of keyword
        /// - table = name of the owner database
        /// - field = type
        /// </summary>
        public string SubString { get; set; }

        /// <summary>
        /// Indicates whether or not this completionData is created by the parser Visitor
        /// </summary>
        public bool FromParser { get; set; }

        /// <summary>
        /// When the FromParser is true, contains the ParsedItem extracted by the parser
        /// </summary>
        public ParsedItem ParsedItem { get; set; }

        /// <summary>
        /// This field is only used when Type == CompletionType.Keyword, it contains the keyword type...
        /// </summary>
        public KeywordType KeywordType { get; set; }

        /// <summary>
        /// Use this method to do an action for each flag of the item...
        /// </summary>
        /// <param name="toApplyOnFlag"></param>
        public void DoForEachFlag(Action<string, ParseFlag> toApplyOnFlag) {
            foreach (var name in Enum.GetNames(typeof(ParseFlag))) {
                ParseFlag flag = (ParseFlag)Enum.Parse(typeof(ParseFlag), name);
                if (flag == 0 || !Flag.HasFlag(flag)) continue;
                toApplyOnFlag(name, flag);
            }
        }
        
        /// <summary>
        /// The piece of text displayed in the list
        /// </summary>
        public override string DisplayText { get; set; }

        /// <summary>
        /// return the image to display for this item
        /// If null, the image corresponding to ItemTypeImage will be used instead
        /// </summary>
        public override Image ItemImage { get { return null; } }

        /// <summary>
        /// return this item type (a unique int for each item type)
        /// if the value is strictly inferior to 0, the button for this type will not appear
        /// on the bottom of list
        /// </summary>
        public override int ItemType { get { return (int) Type; } }

        /// <summary>
        /// return the image that will be used to identify this item
        /// type, it will be used for the bottom buttons of the list
        /// All items of a given type should return the same image! The image used for the 
        /// bottom buttons will be that of the first item found for the given type
        /// </summary>
        public override Image ItemTypeImage {
            get {
                Image tryImg = (Image)ImageResources.ResourceManager.GetObject(((CompletionType)ItemType).ToString());
                return tryImg ?? ImageResources.Error;
            }
        }

        /// <summary>
        /// The text that describes this item type
        /// </summary>
        public override string ItemTypeText { 
            get {
                return "Category : <span class='SubTextColor'><b>" + ((CompletionType)ItemType) + "</b></span><br><br>"; 
            }
        }

        /// <summary>
        /// return true if the item is to be highlighted
        /// </summary>
        public override bool IsRowHighlighted { get { return false; } }

        /// <summary>
        /// return a string containing the subtext to display
        /// </summary>
        public override string SubText { get { return SubString; } }

        /// <summary>
        /// return a list of images to be displayed (in reverse order) for the item
        /// </summary>
        public override List<Image> TagImages {
            get {
                var outList = new List<Image>();
                foreach (var name in Enum.GetNames(typeof(ParseFlag))) {
                    ParseFlag flag = (ParseFlag)Enum.Parse(typeof(ParseFlag), name);
                    if (flag == 0 || !Flag.HasFlag(flag)) continue;

                    Image tryImg = (Image)ImageResources.ResourceManager.GetObject(name);
                    if (tryImg != null) {
                        outList.Add(tryImg);
                    }
                }
                return outList;
            }
        }

    }
    
    /// <summary>
    /// Flags applicable for every ParsedItems
    /// </summary>
    [Flags]
    internal enum ParseFlag {
        // indicates that the parsed item is not coming from the originally parsed source (= from .i)
        External = 1,
        // Local/File define the scope of a defined variable...
        LocalScope = 2,
        FileScope = 4,
        Parameter = 8,
        // is used for keywords
        Reserved = 16,
        Abbreviation = 32,
        New = 64,
        // Special flag for DEFINE
        Global = 128,
        Shared = 256,
        Private = 512,
        // flags for fields
        Mandatory = 1024,
        Extent = 2048,
        Index = 4096,
        // is a buffer
        Buffer = 8192,
        // the variable was defined with a CREATE and not a DEFINE
        Dynamic = 16384,
        // the procedure is EXTERNAL
        ExternalProc = 32768,
        // a proc or func was loaded in persistent
        Persistent = 65536,

        // the block has too much characters and the program will not be open-able in the appbuilder
        IsTooLong = 131072,
        // applies for Run statement, the program/proc to run is VALUE(something) so we only guess which one it is
        Uncertain = 262144,
        // if a table found w/o the database name before it
        MissingDbName = 524288,
        // if the .i file is not found in the propath
        NotFound = 1048576,
        // a run file has the keyword PERSISTENT
        LoadPersistent = 2097152,

    }

    internal enum CompletionType {
        Snippet,
        VariablePrimitive,
        VariableComplex,
        Widget,
        Function,
        Procedure,
        Database,
        TempTable,
        Table,
        Sequence,
        Preprocessed,
        Label,
        Keyword,
        KeywordObject,
        FieldPk,
        Field,
    }

}
