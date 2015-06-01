$Mute::Core::Root = "Add-Ons/Server_Mute";
$Mute::Core::Config = "config/server/mute/config.cs";
$Mute::Core::SaveDir = "config/server/mute/saves";

$Mute::Core::Version = "1.1.0-1";

exec("./support.cs");
exec("./timestamp.cs");
exec("./commands.cs");

function initMuteConfig() {
	%filename = $Mute::Core::Config;
	if(!isFile(%filename)) {
		$Mute::Server::Announce = 1;
		$Mute::Server::AllowRank = "Admin";
		$Mute::Server::Shadow = 0;
		$Mute::Server::AllowIsTyping = 0;
		export("$Mute::Server::*",%filename);
	}
	exec(%filename);
}
initMuteConfig();

function initMuteDB() {
	if(isObject(MuteDB)) {
		for(%i=0;%i<MuteDB.getCount();%i++) {
			MuteDB.getObject(0).delete();
		}
	} else {
		new SimSet(MuteDB);
	}

	%pattern = $Mute::Core::SaveDir @ "/*";
	%files = findFirstFile(%pattern);
	%file = new FileObject();

	while(isFile(%files)) {
		echo("found mute save" SPC %files);
		%file.openForRead(%files);

		%line = "";
		%line = %file.readLine();

		if(%line $= "") {
			echo("skipped, blank");
			%file.close();
			%files = findNextFile(%pattern);
			continue;
		}

		%clientID = getField(%line,0);
		%clientName = getField(%line,1);
		%date = getField(%line,2);
		%expires = getField(%line,3);

		if(%expires !$= "never") {
			if(strReplace(getTimestamp(getDateTime())," ","") >= %expires) {
				%file = new FileObject();
				%file.openForWrite($Mute::Core::SaveDir @ "/" @ %this.bl_id);
				%file.writeLine("");
				%file.close();
				%file.delete();
				echo("mute passed for" SPC %clientID @ ", it expired at" SPC convertTimestamp(%expires) @ ". deleting entry and moving on...");
				continue;
			}
		}

		%row = new ScriptObject(MuteVictimRow) {
			clientID = %clientID;
			clientName = %clientName;
			date = %date;
			expires = %expires;
		};
		MuteDB.add(%row);
		echo("muted" SPC %clientID @ ", expires" SPC convertTimestamp(%expires));

		%file.close();
		%files = findNextFile(%pattern);
	}

	%file.delete();
}
if(!$Mute::Core::DBInit) {
	$Mute::Core::DBInit = 1;
	initMuteDB();	
}

exec("./package.cs");