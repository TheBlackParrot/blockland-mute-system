function serverCmdMute(%client,%victim,%time) {
	if(!isObject(MuteDB)) {
		return;
	}

	%time = mCeil(%time);

	switch$($Mute::Server::AllowRank) {
		case "Moderator":
			// it would help to have moderator add-ons to double check with, this is just here to support what should be the standard
			if(!%client.isModerator) {
				%client.play2D("errorSound");
				messageClient(%client,'',"\c6You must be a moderator to use this command.");	
			}
		case "Admin":
			if(!%client.isAdmin) {
				%client.play2D("errorSound");
				messageClient(%client,'',"\c6You must be an admin to use this command.");
				return 0;
			}
		case "Super Admin":
			if(!%client.isSuperAdmin) {
				%client.play2D("errorSound");
				messageClient(%client,'',"\c6You must be a super admin to use this command.");
				return 0;
			}
		case "Host":
			if(!%client.bl_id == getNumKeyID()) {
				%client.play2D("errorSound");
				messageClient(%client,'',"\c6You must be the host to use this command.");
				return 0;
			}
	}

	if(%victim < 0 || %victim $= "") {
		%client.play2D("errorSound");
		messageClient(%client,'',"\c6You must specify a valid victim. Use their name or BL_ID.");
		return 0;
	}

	%victim_obj = findClientByName(%victim);
	if(!isObject(%victim_obj)) {
		%victim_obj = findClientByBL_ID(%victim);
	}

	if(!isObject(%victim_obj)) {
		%name = "";
		if(isInt(%victim)) {
			%bl_id = %victim;
		} else {
			%client.play2D("errorSound");
			messageClient(%client,'',"\c6\"" @ %victim @ "\" does not exist.");
			return 0;
		}
	} else {
		%name = %victim_obj.name;
		%bl_id = %victim_obj.bl_id;
		if(!$Mute::Server::AllowIsTyping) {
			serverCmdStopTalking(%victim_obj);
		}
	}

	if(%time < -1 || %time $= "") {
		%client.play2D("errorSound");
		messageClient(%client,'',"\c6You must specify a valid amount of minutes or -1 for a permanent mute. Use the /timecalc [unit] [amount] to convert units of time to minutes.");
		return 0;
	}

	for(%i=0;%i<MuteDB.getCount();%i++) {
		%tmp_row = MuteDB.getObject(%i);
		if(%tmp_row.clientID == %bl_id) {
			%tmp_row.delete();
			break;
		}
	}

	if(%time == -1) {
		%expires = "never";
	} else {
		%expires = getTimestamp(getDateTime(),%time);
	}
	%row = new ScriptObject(MuteVictimRow) {
		clientID = %bl_id;
		clientName = %name;
		date = getDateTime();
		expires = strReplace(%expires," ","");
	};
	MuteDB.add(%row);
	saveMuteRow(%row);

	if(%name $= "") {
		%who_str = "\c1BL_ID:" SPC %victim SPC "\c2(ID:" SPC %victim @ ")";
	} else {
		%who_str = "\c1" @ %name SPC "\c2(ID:" SPC %victim_obj.bl_id @ ")";
	}

	if(%expires $= "never") {
		messageAll('MsgAdminForce',"\c3" @ %client.name SPC "\c2permanently muted" SPC %who_str);
	} else {
		messageAll('MsgAdminForce',"\c3" @ %client.name SPC "\c2muted" SPC %who_str SPC "\c2for" SPC %time SPC "minutes");
	}
}

function serverCmdUnmute(%client,%victim) {
	if(!isObject(MuteDB)) {
		return;
	}

	switch$($Mute::Server::AllowRank) {
		case "Moderator":
			// it would help to have moderator add-ons to double check with, this is just here to support what should be the standard
			if(!%client.isModerator) {
				%client.play2D("errorSound");
				messageClient(%client,'',"\c6You must be a moderator to use this command.");	
			}
		case "Admin":
			if(!%client.isAdmin) {
				%client.play2D("errorSound");
				messageClient(%client,'',"\c6You must be an admin to use this command.");
				return 0;
			}
		case "Super Admin":
			if(!%client.isSuperAdmin) {
				%client.play2D("errorSound");
				messageClient(%client,'',"\c6You must be a super admin to use this command.");
				return 0;
			}
		case "Host":
			if(!%client.bl_id == getNumKeyID()) {
				%client.play2D("errorSound");
				messageClient(%client,'',"\c6You must be the host to use this command.");
				return 0;
			}
	}

	if(%victim < 0 || %victim $= "") {
		%client.play2D("errorSound");
		messageClient(%client,'',"\c6You must specify a valid victim. Use their name or BL_ID.");
		return 0;
	}

	for(%i=0;%i<MuteDB.getCount();%i++) {
		%row = MuteDB.getObject(%i);
		if(stripos(%row.clientName,%victim) != -1 || %row.clientID == %victim) {
			%found = 1;
			%name = %row.clientName;
			%bl_id = %row.clientID;
			deleteMuteSave(%bl_id);
			%row.delete();
			break;
		}
	}
	if(!%found) {
		%client.play2D("errorSound");
		messageClient(%client,'',"\c6\"" @ %victim @ "\" is not muted.");
		return 0;
	}

	if(%name $= "") {
		%who_str = "\c1BL_ID:" SPC %bl_id SPC "\c2(ID:" SPC %bl_id @ ")";
	} else {
		%who_str = "\c1" @ %name SPC "\c2(ID:" SPC %bl_id @ ")";
	}

	messageAll('MsgAdminForce',"\c3" @ %client.name SPC "\c2unmuted" SPC %who_str);
}

function saveMuteRow(%row) {
	%file = new FileObject();
	%file.openForWrite($Mute::Core::SaveDir @ "/" @ %row.clientID);

	%file.writeLine(%row.clientID @ "\t" @ %row.clientName @ "\t" @ %row.date @ "\t" @ %row.expires);

	%file.close();
	%file.delete();
}
function deleteMuteSave(%bl_id) {
	%file = new FileObject();
	%file.openForWrite($Mute::Core::SaveDir @ "/" @ %bl_id);

	%file.writeLine("");

	%file.close();
	%file.delete();
}

function serverCmdTimeCalc(%client,%unit,%amount) {
	%amount = mCeil(%amount);

	switch$(%unit) {
		case "minute" or "minutes" or "min" or "mn":
			%unit_str = "minute(s)";
			%conv = %amount;
		case "hour" or "hours" or "hr" or "h":
			%unit_str = "hour(s)";
			%conv = mFloor(%amount*60);
		case "day" or "days" or "d":
			%unit_str = "day(s)";
			%conv = mFloor(%amount*60*24);
		case "week" or "weeks" or "wk" or "w":
			%unit_str = "week(s)";
			%conv = mFloor(%amount*60*(24*7));
		case "month" or "months" or "mo":
			messageClient(%client,'',"todo");
			return 0;
		case "year" or "years" or "yrs" or "yr" or "y":
			// todo: count for leap years
			%unit_str = "year(s)";
			%conv = mFloor(%amount*60*(24*365));
	}

	messageClient(%client,'',"\c3" @ %amount SPC %unit_str SPC "\c6is approximately\c2" SPC %conv SPC "minutes");
}