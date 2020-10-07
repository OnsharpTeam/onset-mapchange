local webUI

AddEvent("OnPackageStart", function ()
	webUI = CreateWebUI(0, 0, 0, 0, 1, 60)
	LoadWebFile(webUI, "http://asset/" .. GetPackageName() .. "/selector.html")
	SetWebAlignment(webUI, 0.0, 0.0)
	SetWebAnchors(webUI, 0.0, 0.0, 1.0, 1.0)
	SetWebVisibility(webUI, WEB_HIDDEN)
end)

AddEvent("cmc:web:close", function ()
	SetIgnoreLookInput(false)
	SetIgnoreMoveInput(false)
	ShowMouseCursor(false)
	SetInputMode(INPUT_GAME)
	SetWebVisibility(webUI, WEB_HIDDEN)
end)

AddEvent("cmc:web:vote", function (map) 
	CallRemoteEvent("cmc:client:vote", map)
end)

AddEvent("cmc:server:finish", function (map)
	ExecuteWebJS(webUI, "selectMap(`" .. map .. "`)")
end)

AddRemoteEvent("cmc:server:vote", function (steamId, map)
	ExecuteWebJS(webUI, "setVote(" .. steamId .. ", `" .. map .. "`)")
end)

AddRemoteEvent("cmc:server:show", function (seconds, mapsJson) 
	AddPlayerChat("Show CMC: " .. mapsJson)
	ExecuteWebJS(webUI, "startCountdown(" .. tostring(seconds) .. ", `" .. mapsJson .. "`)")
end)