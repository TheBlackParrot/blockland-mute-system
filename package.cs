function GameConnection::isMuted(%this) {
	%row = %this.getMuteRow();

	if(isObject(%row)) {
		if(%row.expires !$= "never") {
			if(strReplace(getTimestamp(getDateTime())," ","") >= %row.expires) {
				%file = new FileObject();
				%file.openForWrite($Mute::Core::SaveDir @ "/" @ %this.bl_id);
				%file.writeLine("");
				%file.close();
				%file.delete();
			} else {
				return 1;
			}
		} else {
			return 1;
		}
	}
	return 0;
}

function GameConnection::getMuteRow(%this) {
	for(%i=0;%i<MuteDB.getCount();%i++) {
		%row = MuteDB.getObject(%i);
		if(%row.clientID == %this.bl_id) {
			return %row;
		}
	}
	return 0;
}

function GameConnection::decreaseSpamMessageCount(%client) {
	%client.spamMessageCount--;
	if(%client.spamMessageCount < 0) {
		%client.spamMessageCount = 0;
	}
}

package MuteServerPackage {
	function serverCmdMessageSent(%client,%msg) {
		if(!%client.isMuted()) {
			%client.last_msg = %msg;
			return parent::serverCmdMessageSent(%client,%msg);
		} else {
			if(!$Mute::Server::Shadow) {
				%client.play2D("errorSound");
				%row = %client.getMuteRow();
				if(%row.expires $= "never") {
					messageClient(%client,'',"\c6You have been permanently muted.");
				} else {
					messageClient(%client,'',"\c6You have been muted. It expires on" SPC convertTimestamp(%client.getMuteRow().expires));
				}
				%client.last_msg = %msg;
				return 0;
			} else {
				// essentially had to recode chat here, pls
				if(%client.spamMessageCount < 5) {
					%client.spamMessageCount++;
					// seems like 5 seconds
					%client.schedule(5000,decreaseSpamMessageCount);
				}
				if(%msg $= %client.last_msg || getSimTime() - %client.flooded_at < 5000 || %client.spamMessageCount > 4) {
					if(getSimTime() - %client.flooded_at < 5000) {
						%time = mCeil(((%client.flooded_at+5000) - getSimTime())/1000);
					} else {
						%time = 5;
						%client.flooded_at = getSimTime();
					}
					commandToClient(%client,'serverMessage',58,"\c5Do not repeat yourself.");
					// this doesn't have a tagged string?
					commandToClient(%client,'serverMessage',"","\c3FLOOD PROTECTION: You must wait another" SPC %time SPC "seconds.");
					return 0;
				}
				%all = '\c7%1\c3%2\c7%3\c6: %4';
				%name = %client.getPlayerName();
				%pre = %client.clanPrefix;
				%suf = %client.clanSuffix;

				commandToClient(%client,'chatMessage',%client,'','',%all,%pre,%name,%suf,%msg);
				%client.last_msg = %msg;
			}
		}
	}

	function serverCmdStartTalking(%client) {
		if(!%client.isMuted()) {
			return parent::serverCmdStartTalking(%client);
		} else {
			if($Mute::Server::AllowIsTyping || $Mute::Server::Shadow) {
				// it's assumed you want shadow mutes to work correctly with just $Mute::Server::Shadow set.
				return parent::serverCmdStartTalking(%client);
			}
		}
		return 0;
	}

	function onServerDestroyed() {
		export("$Mute::Server::*",$Mute::Core::Config);
		$Mute::Core::DBInit = 0;
		return parent::onServerDestroyed();
	}
};
activatePackage(MuteServerPackage);