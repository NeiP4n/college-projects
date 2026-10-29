using System;
using Microsoft.Office.Interop.Word;

namespace WordAIAgent.Services
{
    public class CursorGuard
    {
        private readonly WordService _ws;

        public CursorGuard(WordService ws) => _ws = ws;

        public bool EnsureSafeForInsert()
        {
            try
            {
                var app = Globals.ThisAddIn.Application;
                var sel = app.Selection;
                if (sel == null) return false;

                if (sel.Tables.Count > 0)
                {
                    sel.MoveRight(WdUnits.wdCharacter, 1);
                    sel.TypeParagraph();
                }

                if (sel.StoryType != WdStoryType.wdMainTextStory)
                {
                    app.ActiveDocument.Content.Select();
                    sel = app.Selection;
                }

                sel.Collapse(WdCollapseDirection.wdCollapseEnd);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
