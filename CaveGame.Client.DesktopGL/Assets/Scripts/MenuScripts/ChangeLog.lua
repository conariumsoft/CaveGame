

local compiledLogs = {};
for _,update in pairs(Globals.UpdateLog) do

	compiledLogs.Add(">>" .. update.UpdateName);
	compiledLogs.Add("version " .. update.VersionString);
	compiledLogs.Add("" .. update.Date);

    compiledLogs.Add(">Change log:");
    for t in list(update.ChangeLog) do
        compiledLogs.Add(t);
    end
	--update.ChangeLog.ForEach(t => compiledLogs.Add(t));
	compiledLogs.Add(">Additional notes:");
    --update.Notes.ForEach(t => compiledLogs.Add(t));
    for tn in list(update.Notes) do
        compiledLogs.Add(tn);
    end
end

return compiledLogs;