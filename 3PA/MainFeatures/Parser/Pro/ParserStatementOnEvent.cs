﻿using System.Text;
using _3PA.Lib;

namespace _3PA.MainFeatures.Parser.Pro {
    internal partial class Parser {

        /// <summary>
        /// Creates parsed item for ON CHOOSE OF XXX events
        /// (choose or anything else)
        /// </summary>
        /// <param name="onToken"></param>
        /// <returns></returns>
        private void CreateParsedOnEvent(Token onToken) {
            /*
            ON event-list 
            {     ANYWHERE
                |  { OF widget-list 
                    [ OR event-list OF widget-list]...
                    [ ANYWHERE ]
                }
            }
            {     trigger-block 
                |  REVERT
                |  { PERSISTENT RUN procedure 
                    [ ( input-parameters ) ]
                }
            }
            
            ON event OF database-object 
            [referencing-phrase]
            [ OVERRIDE ]
            {trigger-block | REVERT }

            ON key-labelkey-function 

            ON "WEB-NOTIFY" ANYWHERE  {trigger-block}
            */

            // info we will extract from the current statement :
            var eventList = new StringBuilder();
            var widgetList = new StringBuilder();
            int state = 0;
            do {
                var token = PeekAt(1); // next token
                if (token is TokenEos) break;
                if (token is TokenComment) continue;
                if (state == 99) break;
                switch (state) {
                    case 0:
                        // matching event type
                        if (token is TokenWord || token is TokenString || token is TokenSymbol) {
                            eventList.Append((eventList.Length == 0 ? "" : ", ") + GetTokenStrippedValue(token));
                            state++;
                        }
                        break;
                    case 1:
                        // matching an event list?
                        if (token is TokenSymbol && token.Value.Equals(",")) {
                            state--;
                            break;
                        }
                        if (!(token is TokenWord)) break;

                        if (token.Value.EqualsCi("anywhere")) {
                            // we match anywhere, need to return to match a block start
                            widgetList.Append("anywhere");
                            var new1 = new ParsedOnStatement(eventList + " " + widgetList, onToken, eventList.ToString(), widgetList.ToString());
                            AddParsedItem(new1, onToken.OwnerNumber);
                            _context.Scope = new1;
                            return;
                        }
                        // if not anywhere, we expect an "of"
                        if (token.Value.EqualsCi("of")) {
                            state++;
                            break;
                        }

                        // we matched a 'ON key-label key-function'
                        widgetList.Append(token.Value);
                        var new3 = new ParsedOnStatement(eventList + " " + widgetList, onToken, eventList.ToString(), widgetList.ToString());
                        AddParsedItem(new3, onToken.OwnerNumber);
                        _context.Scope = new3;
                        return;

                    case 2:
                        // matching widget name
                        if (token is TokenWord || token is TokenString) {
                            // ON * OF FRAME fMain, on ne prend pas en compte le FRAME
                            if (!token.Value.EqualsCi("frame")) {
                                widgetList.Append((widgetList.Length == 0 ? "" : ", ") + GetTokenStrippedValue(token));
                                state++;
                            }
                        }
                        break;
                    case 3:
                        // matching a widget list?
                        if (token is TokenSymbol && token.Value.Equals(",")) {
                            state--;
                            break;
                        }
                        if (!(token is TokenWord)) break;

                        // matching a widget IN FRAME
                        if (token.Value.EqualsCi("in")) {
                            var nextNonSpace = PeekAtNextNonSpace(1);
                            if (!(nextNonSpace is TokenWord && nextNonSpace.Value.Equals("frame"))) {
                                // skip the whole IN FRAME XX
                                MoveNext();
                                MoveNext();
                                MoveNext();
                                MoveNext();
                                break;
                            }
                        }

                        var new2 = new ParsedOnStatement(eventList + " " + widgetList, onToken, eventList.ToString(), widgetList.ToString());
                        AddParsedItem(new2, onToken.OwnerNumber);
                        _context.Scope = new2;

                        // matching a OR
                        if (token.Value.EqualsCi("or")) {
                            widgetList.Clear();
                            eventList.Clear();
                            state = 0;
                            break;
                        }

                        // end here
                        return;
                }
            } while (MoveNext());
        }

    }
}