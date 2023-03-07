import ('MonoGame.Framework', 'Microsoft.Xna.Framework')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Common')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Client')
import ('CaveGame.Client.DesktopGL', 'CaveGame.Client.UI')

import ('System', 'System.Collections.Generic')

local compiledLogs = {};
for update in list(Globals.UpdateLog) do

	table.insert(compiledLogs, ">>" .. update.UpdateName);
	table.insert(compiledLogs, "version " .. update.VersionString);
	table.insert(compiledLogs, "" .. update.Date);

    table.insert(compiledLogs, ">Change log:");
    for t in list(update.ChangeLog) do
        table.insert(compiledLogs, t);
    end
	--update.ChangeLog.ForEach(t => compiledLogs.Add(t));
	table.insert(compiledLogs, ">Additional notes:");
    --update.Notes.ForEach(t => compiledLogs.Add(t));
    for tn in list(update.Notes) do
        table.insert(compiledLogs, tn);
    end
end

return compiledLogs;