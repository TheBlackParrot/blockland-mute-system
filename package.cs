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

package MuteServerPackage {
	function serverCmdMessageSent(%client,%msg) {
		if(!%client.isMuted()) {
			return parent::serverCmdMessageSent(%client,%msg);
		} else {
			%client.play2D("errorSound");
			messageClient(%client,'',"\c6You have been muted. It expires on" SPC convertTimestamp(%client.getMuteRow().expires));
			return 0;
		}
	}

	function serverCmdStartTalking(%client) {
		if(!%client.isMuted()) {
			return parent::serverCmdStartTalking(%client);
		} else {
			if($Mute::Server::AllowIsTyping) {
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