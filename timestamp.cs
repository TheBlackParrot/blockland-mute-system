function leapYear(%year) {
	// A leap year is divisable by 4, but not by 100 (except if divisable by 400.)
	// 100 and 400 shouldn't matter for what we can get from getDateTime()
	if(%year % 4 == 0) {
		// (!%year % 4) wasn't working. why? i don't know
		return 1;
	}
	return 0;
}

function getDaysInMonth(%month,%year) {
	if(%month > 12) {
		%month -= 12;
		%year++;
	}

	switch$(%month) {
		case 1 or "Jan" or 3 or "Mar" or 5 or "May" or 7 or "Jul" or "July" or 8 or "Aug" or 10 or "Oct" or 12 or "Dec":
			return 31;
		case 4 or "Apr" or 6 or "Jun" or "June" or 9 or "Sep" or 11 or "Nov":
			return 30;
		case 2 or "Feb":
			if(leapYear(%year)) {
				return 29;
			} else {
				return 28;
			}
	}
}

function convertTimestamp(%timestamp) {
	if(%timestamp <= 0 || %timestamp $= "") {
		return 0;
	}
	
	%year = getSubStr(%timestamp,0,2);
	%month = getSubStr(%timestamp,2,2);
	%day = getSubStr(%timestamp,4,2);
	%hour = getSubStr(%timestamp,6,2);
	%minute = getSubStr(%timestamp,8,2);
	%second = getSubStr(%timestamp,10,2);

	return %month @ "/" @ %day @ "/" @ %year SPC %hour @ ":" @ %minute @ ":" @ %second;
}

function getTimestamp(%time,%offset) {
	if(%time $= "" || %offset < 0) {
		return -1;
	}

	// MM/DD/YY HH:MM:SS
	// padding for all, 24 hour time
	%month = mFloor(getSubStr(%time,0,2));
	%day = mFloor(getSubStr(%time,3,2));
	%year = mFloor(getSubStr(%time,6,2));
	%hour = mFloor(getSubStr(%time,9,2));
	%minute = mFloor(getSubStr(%time,12,2));
	%second = mFloor(getSubStr(%time,15,2));

	if(%offset) {
		%minute_o = (%offset % 60);
		if(%minute + %minute_o >= 60) {
			%hour++;
			%minute = (%minute + %minute_o) - 60;
		} else {
			%minute += %minute_o;
		}

		%hour_o = (mFloor((%offset - %minute_o)/60)) % 24;
		if(%hour + %hour_o >= 24) {
			%day++;
			%hour = (%hour + %hour_o) - 24;
		} else {
			%hour += %hour_o;
		}

		if(leapYear(%year)) {
			%day_o = mFloor((%offset - (%hour_o*60))/(24*60)) % 366;
		} else {
			%day_o = mFloor((%offset - (%hour_o*60))/(24*60)) % 365;
		}

		%day += %day_o;
		while(%day > getDaysInMonth(%month,%year)) {
			%day -= getDaysInMonth(%month,%year);
			%month++;
			if(%month > 12) {
				%month -= 12;
				%year++;
			}
		}
	}

	// yy mm dd hh mm ss
	// leaving like this in case anyone wants to use it for something, do a strReplace on the spaces if you want to compare it to another timestamp
	%timestamp = getSubStr("00",strLen(%year),2-strLen(%year)) @ %year;
	%timestamp = %timestamp SPC getSubStr("00",strLen(%month),2-strLen(%month)) @ %month;
	%timestamp = %timestamp SPC getSubStr("00",strLen(%day),2-strLen(%day)) @ %day;
	%timestamp = %timestamp SPC getSubStr("00",strLen(%hour),2-strLen(%hour)) @ %hour;
	%timestamp = %timestamp SPC getSubStr("00",strLen(%minute),2-strLen(%minute)) @ %minute;
	%timestamp = %timestamp SPC getSubStr("00",strLen(%second),2-strLen(%second)) @ %second;
	return %timestamp;
}